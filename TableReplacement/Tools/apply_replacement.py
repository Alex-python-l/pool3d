"""Check/apply/restore the complete paired table change. Default is read-only.
It never starts Unity or terminates a user's Editor. It refuses unexpected checkout changes.
"""
from pathlib import Path,PurePosixPath
from datetime import datetime
import argparse,hashlib,json,os,shutil,subprocess,zipfile

DELIVERY=Path(__file__).resolve().parent.parent
WORKSPACE=DELIVERY.parents[1]
DEFAULT=WORKSPACE/'deliverables/Pool3D-Integrated-V1/UnityProject'

def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def resolve(root,relative):
    p=PurePosixPath(relative)
    if p.is_absolute() or '..' in p.parts or p.parts[0]!='Assets':raise ValueError('Invalid payload path')
    target=(root/Path(*p.parts)).resolve()
    if root.resolve() not in target.parents:raise ValueError('Payload escaped project')
    return target
def remove_empty_payload_dirs(target,records):
    parents={resolve(target,r['path']).parent for r in records}
    for parent in sorted(parents,key=lambda p:len(p.parts),reverse=True):
        while parent!=target/'Assets' and (target/'Assets') in parent.parents:
            try:parent.rmdir()
            except OSError:break
            parent=parent.parent
def editor_closed(target):
    literal=str(target.resolve()).replace("'","''")
    command="Get-CimInstance Win32_Process -Filter \"Name='Unity.exe'\" -ErrorAction Stop | Where-Object { $_.CommandLine -and $_.CommandLine.Contains('"+literal+"') } | ForEach-Object { $_.ProcessId }"
    result=subprocess.run(['powershell','-NoProfile','-NonInteractive','-Command',command],capture_output=True,text=True,creationflags=subprocess.CREATE_NO_WINDOW)
    if result.returncode:raise RuntimeError('Cannot verify Editor state; no files changed')
    if result.stdout.strip():raise RuntimeError('Close the target Editor before applying/restoring; this tool will not stop it')

def baseline_matches(target):
    snapshot=json.loads((DELIVERY/'Docs/original_project_snapshot.json').read_text(encoding='utf-8'))
    expected_assets={r['path'].removeprefix('UnityProject/') for r in snapshot['files'] if r['path'].startswith('UnityProject/Assets/')}
    for record in snapshot['files']:
        p=PurePosixPath(record['path']);file=target/Path(*p.parts[1:])
        if not file.is_file() or sha(file)!=record['sha256']:raise RuntimeError('Original checkout differs from reviewed baseline: '+record['path'])
    actual_assets={p.relative_to(target).as_posix() for p in (target/'Assets').rglob('*') if p.is_file()}
    if actual_assets!=expected_assets:raise RuntimeError('Target has additional/unreviewed asset files; review them before replacement')

def restore(target,archive):
    editor_closed(target)
    with zipfile.ZipFile(archive) as z:
        journal=json.loads(z.read('journal.json'))
        if Path(journal['target']).resolve()!=target.resolve():raise ValueError('Rollback target differs from receipt')
        for record in journal['files']:
            file=resolve(target,record['path'])
            if not file.is_file() or sha(file)!=record['new_sha256']:raise RuntimeError('File changed after replacement; refusing destructive rollback: '+record['path'])
        for record in reversed(journal['files']):
            file=resolve(target,record['path'])
            if record['existed']:file.write_bytes(z.read('before/'+record['path']))
            else:file.unlink() # Only a known new file, checked above, never a recursive deletion.
    remove_empty_payload_dirs(target,journal['files'])
    return len(journal['files'])

def main():
    parser=argparse.ArgumentParser();parser.add_argument('--target',type=Path,default=DEFAULT)
    actions=parser.add_mutually_exclusive_group();actions.add_argument('--apply',action='store_true');actions.add_argument('--restore',type=Path)
    args=parser.parse_args();target=args.target.resolve()
    if not(target/'ProjectSettings/ProjectVersion.txt').is_file():raise ValueError('Target is not a Unity project')
    if args.restore:
        count=restore(target,args.restore);print(json.dumps({'restored':count,'target':str(target)},ensure_ascii=False));return
    if (DELIVERY/'Docs/pending_blender_revision.json').is_file():
        raise RuntimeError('New Blender pocket curves are pending Unity synchronization; do not apply the previous table geometry. No files changed.')
    manifest=json.loads((DELIVERY/'Docs/replacement_payload.json').read_text(encoding='utf-8'))
    for record in manifest['files']:
        source=resolve(DELIVERY/'UnityProject',record['path'])
        if not source.is_file() or sha(source)!=record['sha256']:raise RuntimeError('Candidate payload checksum mismatch: '+record['path'])
        destination=resolve(target,record['path'])
        if not record['existed'] and destination.exists():raise RuntimeError('New asset path already exists: '+record['path'])
    baseline_matches(target)
    if not args.apply:
        print(json.dumps({'ready':True,'action':'read-only preflight','files':len(manifest['files']),'target':str(target),'modified':False},ensure_ascii=False));return
    editor_closed(target)
    backup_dir=target.parent/'ReplacementBackups';backup_dir.mkdir(exist_ok=True)
    backup=backup_dir/('BeforeNewTable-'+datetime.now().strftime('%Y%m%d-%H%M%S-%f')+'.zip')
    records=[{'path':r['path'],'existed':r['existed'],'new_sha256':r['sha256']} for r in manifest['files']]
    journal={'target':str(target),'files':records}
    with zipfile.ZipFile(backup,'x',compression=zipfile.ZIP_DEFLATED) as z:
        z.writestr('journal.json',json.dumps(journal,ensure_ascii=False,indent=2))
        for record in records:
            if record['existed']:z.write(resolve(target,record['path']),'before/'+record['path'])
    changed=[];staged=[]
    try:
        for record in records:
            file=resolve(target,record['path']);file.parent.mkdir(parents=True,exist_ok=True)
            temporary=file.with_name(file.name+'.replacement-tmp')
            if temporary.exists():raise RuntimeError('Unexpected staging file: '+str(temporary))
            staged.append(temporary)
            shutil.copyfile(resolve(DELIVERY/'UnityProject',record['path']),temporary)
            os.replace(temporary,file);changed.append(record)
        if any(sha(resolve(target,r['path']))!=r['new_sha256'] for r in records):raise RuntimeError('Post-copy checksum mismatch')
    except BaseException:
        with zipfile.ZipFile(backup) as z:
            for record in reversed(changed):
                file=resolve(target,record['path'])
                if record['existed']:file.write_bytes(z.read('before/'+record['path']))
                elif file.exists():file.unlink()
        for temporary in staged:
            if temporary.exists():temporary.unlink()
        remove_empty_payload_dirs(target,records)
        raise
    print(json.dumps({'applied':len(records),'backup':str(backup),'target':str(target),'playMode':False,'requires_editor_import_check':True},ensure_ascii=False))

if __name__=='__main__':main()
