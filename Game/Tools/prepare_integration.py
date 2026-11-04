from pathlib import Path
import hashlib
import json
import shutil

ROOT = Path(__file__).resolve().parents[3]
DEST = ROOT / 'deliverables/Pool3D-Integrated-V1'
SPIN = ROOT / 'deliverables/Pool3D-SpinLab-20261026'
ART = ROOT / 'deliverables/Pool3D-MaterialStudy-V3-20261025'
EXCLUDED = {'Library', 'Temp', 'Logs', 'obj', '.git', '.vs', '__pycache__'}


def validate_destination(root, destination):
    base = (Path(root).resolve() / 'deliverables').resolve()
    target = Path(destination).resolve()
    if target == base or base not in target.parents:
        raise ValueError('Destination must be a child of workspace/deliverables')
    if target.exists():
        raise FileExistsError('Destination already exists; never overwrite an integration')
    return target


def files_under(base):
    return (p for p in base.rglob('*') if p.is_file() and not EXCLUDED.intersection(p.relative_to(base).parts))


def main():
    destination = validate_destination(ROOT, DEST)
    protected = []
    for base in (SPIN, ART, ROOT / 'restoration/pool3d-original-source/Recovered'):
        for path in files_under(base):
            protected.append({'path': path.relative_to(ROOT).as_posix(), 'sha256': hashlib.sha256(path.read_bytes()).hexdigest()})
    project = destination / 'UnityProject'
    for name in ('Assets', 'Packages', 'ProjectSettings'):
        shutil.copytree(SPIN / 'UnityProject' / name, project / name, ignore=shutil.ignore_patterns(*EXCLUDED))
    for name in ('SourceReference', 'ThirdPartyNotices'):
        shutil.copytree(SPIN / name, destination / name)
    for name in ('Source', 'Tools', 'Docs'):
        (destination / name).mkdir()
    (destination / 'Docs/baseline_manifest.json').write_text(json.dumps(protected, indent=2), encoding='utf-8')
    shutil.copy2(__file__, destination / 'Tools/prepare_integration.py')
    shutil.copy2(Path(__file__).with_name('test_prepare.py'), destination / 'Tools/test_prepare.py')
    art_in = ART / 'UnityProject/Assets/MaterialStudy'
    art_out = project / 'Assets/IntegratedArt'
    for name in ('Materials', 'Textures', 'Meshes', 'Models', 'Prefabs', 'Room', 'Shaders'):
        shutil.copytree(art_in / name, art_out / name)
        meta = art_in / (name + '.meta')
        if meta.exists(): shutil.copy2(meta, art_out / meta.name)
    shutil.copy2(art_in.with_suffix('.meta'), project / 'Assets/IntegratedArt.meta')
    shutil.copytree(ART / 'UnityProject/Assets/ThirdParty/PoolSet', project / 'Assets/ThirdParty/PoolSet')
    shutil.copy2(ART / 'UnityProject/Assets/ThirdParty/PoolSet.meta', project / 'Assets/ThirdParty/PoolSet.meta')
    # Remove only obsolete entry points from the fresh copy, never source assets.
    for name in ('SpinLabBuild.cs', 'SpinLabBuild.cs.meta'):
        (project / 'Assets/OfflineRuntime/Editor' / name).unlink(missing_ok=True)
    # The whole static sample is not a runtime asset; it carries four static balls and cameras.
    for name in ('MaterialStudyV3.prefab', 'MaterialStudyV3.prefab.meta'):
        (art_out / 'Prefabs' / name).unlink(missing_ok=True)
    manifest_file = project / 'Packages/manifest.json'
    manifest = json.loads(manifest_file.read_text(encoding='utf-8'))
    manifest['dependencies']['com.unity.postprocessing'] = '3.2.2'
    manifest_file.write_text(json.dumps(manifest, indent=2), encoding='utf-8')
    bootstrap = project / 'Assets/OfflineRuntime/OfflineBootstrap.cs'
    bootstrap.write_text(bootstrap.read_text(encoding='utf-8').replace('SelectProfile("local")', 'SelectProfile("integrated-v1")'), encoding='utf-8')
    settings_file = project / 'ProjectSettings/ProjectSettings.asset'
    settings = settings_file.read_text(encoding='utf-8').replace('productName: Pool3DSpinLab', 'productName: Pool3DIntegrated').replace('m_ActiveColorSpace: 0', 'm_ActiveColorSpace: 1')
    settings_file.write_text(settings, encoding='utf-8')
    guids = {}
    for path in (project / 'Assets').rglob('*.meta'):
        for line in path.read_text(encoding='utf-8-sig').splitlines():
            if line.startswith('guid: '):
                guid = line[6:]
                if guid in guids: raise RuntimeError(f'Duplicate GUID: {path}, {guids[guid]}')
                guids[guid] = path
    provenance = {'spin_source': SPIN.relative_to(ROOT).as_posix(), 'art_source': ART.relative_to(ROOT).as_posix(), 'source_files_protected': len(protected), 'duplicate_guids': 0, 'scope': 'M0/M1 only; no gameplay/visual review/lighting bake', 'original_sources_modified': False}
    (destination / 'Docs/preparation.json').write_text(json.dumps(provenance, indent=2), encoding='utf-8')
    print(json.dumps(provenance))


if __name__ == '__main__':
    main()
