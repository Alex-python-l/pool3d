from pathlib import Path
import hashlib
import json
import re

DELIVERY = Path(__file__).resolve().parent.parent
ROOT = DELIVERY.parents[1]
PROJECT = DELIVERY / 'UnityProject'
SPIN = ROOT / 'deliverables/Pool3D-SpinLab-20261026/UnityProject'
V3 = ROOT / 'deliverables/Pool3D-MaterialStudy-V3-20261025/UnityProject'


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    protected = json.loads((DELIVERY / 'Docs/baseline_manifest.json').read_text(encoding='utf-8'))
    changed = [item['path'] for item in protected if not (ROOT / item['path']).is_file() or digest(ROOT / item['path']) != item['sha256']]
    if changed: raise AssertionError({'protected_files_changed': changed})
    guids = {}
    for path in (PROJECT / 'Assets').rglob('*.meta'):
        match = re.search(r'^guid: ([0-9a-f]+)$', path.read_text(encoding='utf-8-sig'), re.M)
        if match:
            if match[1] in guids: raise AssertionError('Duplicate asset GUID')
            guids[match[1]] = path
    changes = []
    for path in sorted((PROJECT / 'Assets').rglob('*')):
        if not path.is_file() or path.suffix == '.meta': continue
        relative = path.relative_to(PROJECT)
        source = SPIN / relative
        art_relative = path.relative_to(PROJECT / 'Assets')
        if art_relative.parts[0] == 'IntegratedArt': source = V3 / 'Assets/MaterialStudy' / Path(*art_relative.parts[1:])
        if art_relative.parts[:2] == ('ThirdParty', 'PoolSet'): source = V3 / relative
        sha = digest(path)
        if not source.exists() or digest(source) != sha:
            changes.append({'path': relative.as_posix(), 'kind': 'adapted' if source.exists() else 'new',
                            'source': source.relative_to(ROOT).as_posix() if source.exists() else None, 'sha256': sha})
    (DELIVERY / 'Docs/source_changes.json').write_text(json.dumps(changes, indent=2), encoding='utf-8')
    geometry = json.loads((DELIVERY / 'Docs/geometry_checks.json').read_text())
    presentation = json.loads((DELIVERY / 'Docs/presentation_checks.json').read_text())
    numeric = json.loads((DELIVERY / 'numeric_checks.json').read_text())
    editor = json.loads((DELIVERY / 'Docs/editor_exit.json').read_text())
    assert editor['exitCode'] == 0 and editor['method'] == 'IntegratedPresentationChecks.Run'
    assert geometry['passed'] and presentation['passed'] and numeric['passed']
    assert not any(presentation[k] for k in ('gameplay', 'playMode', 'screenshots', 'rendered'))
    assert (PROJECT / 'Assets/Scenes/IntegratedOffline.unity').is_file()
    assert not (DELIVERY / 'Windows').exists(), 'M3 output is not part of this stage'
    mesh = json.loads((DELIVERY / 'Docs/geometry_mesh_report.json').read_text())
    profile = json.loads((PROJECT / 'Assets/StreamingAssets/TableGeometry.json').read_text())
    geometry_report = {'passed': geometry['passed'], 'checks': geometry['cases'], 'playing_area_simulator_units': [profile['longSide'],profile['shortSide']],
                       'ball_diameter_millimetres': profile['ballRadius']*2, 'metres_per_simulator_unit': profile['metresPerUnit'],
                       'roundtrip_tolerance_simulator_units': .001, 'blockers': len(profile['blockers'])//4, 'pockets': profile['holes'],
                       'source_mesh_projection': mesh, 'adapted_callsites': [c['path'] for c in changes if c['path'].startswith('Assets/OriginalCore/') or c['path'].endswith(('OfflineMatchController.cs','OfflineControls.cs','OfflineSceneView.cs'))]}
    (DELIVERY / 'Docs/geometry_report.json').write_text(json.dumps(geometry_report, indent=2),encoding='utf-8')
    report = {'passed': True, 'date': '2026-10-27', 'scope': 'M0/M1 (plan tasks 1-4)', 'unity': '2022.3.62f2',
              'protected_files_unchanged': len(protected), 'duplicate_asset_guids': 0, 'changed_or_new_assets': len(changes),
              'geometry_checks': geometry['cases'], 'presentation_checks': presentation['cases'], 'full_ball_resources': presentation['ballResources'],
              'spin_regression_passed': numeric['passed'], 'editor_exit_code': editor['exitCode'],
              'gameplay': False, 'play_mode': False, 'screenshots': False, 'render_review': False,
              'lighting_baked_this_stage': False, 'windows_build_created': False,
              'review_important_findings_fixed': 1, 'review_deferred_minors': 0,
              'shader_variants_runtime_verified': False,
              'scene': 'Assets/Scenes/IntegratedOffline.unity',
              'geometry_config_sha256': digest(PROJECT / 'Assets/StreamingAssets/TableGeometry.json')}
    (DELIVERY / 'Docs/verification.json').write_text(json.dumps(report, indent=2), encoding='utf-8')
    print(json.dumps(report))


if __name__ == '__main__': main()
