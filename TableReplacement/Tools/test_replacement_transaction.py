"""Exercise the real payload and rollback on a disposable checkout copy, never the game."""
from pathlib import Path
import contextlib,hashlib,importlib.util,io,json,shutil,sys
from datetime import datetime

HERE=Path(__file__).resolve().parent
spec=importlib.util.spec_from_file_location('replacement',HERE/'apply_replacement.py')
tool=importlib.util.module_from_spec(spec);spec.loader.exec_module(tool)
scratch=HERE.parent/('Verification/TransactionTarget-'+datetime.now().strftime('%Y%m%d-%H%M%S-%f'))
assert not scratch.exists(), 'Keep prior test evidence; choose a fresh scratch directory'
scratch.mkdir(parents=True)
for folder in ['Assets','Packages','ProjectSettings']:shutil.copytree(tool.DEFAULT/folder,scratch/folder)
tool.editor_closed=lambda target:None # This newly copied fixture has never been opened in Unity.
snapshot=json.loads((HERE.parent/'Docs/original_project_snapshot.json').read_text())
baseline={r['path'].removeprefix('UnityProject/'):r['sha256'] for r in snapshot['files']}
def unchanged():
    actual={p.relative_to(scratch).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in scratch.rglob('*') if p.is_file()}
    assert actual==baseline, 'Checkout files differ after rollback'
def run(*args):
    sys.argv=['apply_replacement.py','--target',str(scratch),*args]
    with contextlib.redirect_stdout(io.StringIO()) as output:tool.main()
    return json.loads(output.getvalue())

check=run();assert check['ready'] and not check['modified']
applied=run('--apply');archive=Path(applied['backup'])
payload=json.loads((HERE.parent/'Docs/replacement_payload.json').read_text())['files']
assert all(tool.sha(scratch/r['path'])==r['sha256'] for r in payload)
restored=run('--restore',str(archive));unchanged()

replace=tool.os.replace;counter=[0]
def fail_third(source,target):
    counter[0]+=1
    if counter[0]==3:raise OSError('Injected third-file rename failure')
    return replace(source,target)
tool.os.replace=fail_third
try:
    try:run('--apply')
    except OSError:pass
    else:raise AssertionError('Expected injected error')
finally:tool.os.replace=replace
unchanged()
report={'passed':True,'cases':['read-only preflight','apply complete paired payload','restore all original files and remove added files','mid-copy failure rolls back and removes staging files'],'files':len(payload),'original_project_modified':False}
(HERE.parent/'Docs/transaction_checks.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report))
