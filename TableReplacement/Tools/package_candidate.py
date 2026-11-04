"""Verify and package the candidate source project without caches or licensing logs."""
from pathlib import Path
import json,hashlib,shutil,zipfile,tarfile

ROOT=Path(__file__).resolve().parent.parent
WORKSPACE=ROOT.parents[1]
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def read(name):return json.loads((ROOT/name).read_text(encoding='utf-8'))
checks={name:read('Docs/'+name) for name in ['geometry_checks.json','presentation_checks.json','replacement_checks.json','transaction_checks.json','model_package_checks.json']}
assert all(r['passed'] for r in checks.values())
assert read('numeric_checks.json')['passed']
assert read('Docs/editor_exit.json')['exitCode']==0
snapshot=read('Docs/original_project_snapshot.json')
original=WORKSPACE/'deliverables/Pool3D-Integrated-V1'
assert all(sha(original/r['path'])==r['sha256'] for r in snapshot['files'])
core_files=list((original/'UnityProject/Assets/OriginalCore').rglob('*.cs'))
assert all(sha(p)==sha(ROOT/p.relative_to(original)) for p in core_files)
payload=read('Docs/replacement_payload.json')
assert all(sha(ROOT/'UnityProject'/r['path'])==r['sha256'] for r in payload['files'])
source=WORKSPACE/'art-source/luxury_pool_table/20261029_cleanup_sizing/luxury_pool_table_clean_metric.blend'
shutil.copyfile(source,ROOT/'Source/authoring_clean_metric.blend')
for name in ['final_asset_audit.json','preparation_report.json']:
    shutil.copyfile(source.parent/name,ROOT/'Docs'/('blender_'+name))
assert read('UnityProject/Assets/TableReplacement/Source/TableMesh.json')['sourceSha256']==sha(ROOT/'Source/authoring_clean_metric.blend')
verification={'date':'2026-10-30','passed':True,'stage':'paired table replacement prepared in isolated candidate; original not activated',
    'unity':'2022.3.62f2 Built-in Linear','editModeExitCode':0,
    'geometryChecks':checks['geometry_checks.json']['cases'],'presentationChecks':checks['presentation_checks.json']['cases'],
    'replacementChecks':checks['replacement_checks.json']['checks'],'numericRegression':True,
    'transactionChecks':checks['transaction_checks.json']['cases'],
    'meshParts':50,'materials':9,'triangles':checks['replacement_checks.json']['triangles'],'blockers':checks['replacement_checks.json']['blockers'],
    'modelPackageAssets':len(checks['model_package_checks.json']['assets']),
    'originalFilesUnchanged':len(snapshot['files']),'candidateOriginalCoreCsUnchanged':len(core_files),
    'payloadFiles':len(payload['files']),'appliedToOriginal':False,'playMode':False,'gameplay':False,'screenshots':False,'rendered':False,'windowsBuild':False}
rail_check=read('Docs/rail_fix_checks.json')
assert rail_check['passed'] and rail_check['r2_rolled_back'] and not rail_check['wood_frame_rebuilt']
assert rail_check['cream_cap_vertex_positions_unchanged'] and len(rail_check['cushion_end_curves'])==6
assert rail_check['minimal_corner_restore']['passed'] and rail_check['minimal_corner_restore']['vertex_and_face_geometry_unchanged']
assert all(v==0 for v in rail_check['body_cushion_triangle_overlaps'].values())
assert rail_check['upper_wood_uv_checks']['outside_clean_uv_crop']==0 and rail_check['upper_wood_uv_checks']['white_texture_samples']==0
verification.update({'upperFrameRevision':'R3b original corner appearance, limited straight-rail UV and curved pocket cloth','r2RolledBack':True,'woodFrameRebuilt':False,'creamCapsRebuilt':False,'originalCornerAppearanceRestored':True,'curvedJawConnectors':12,'bodyCushionTriangleIntersections':0,'observedMissingOriginalAssetsPreserved':snapshot.get('observedMissingFromPreviousBaseline',[])})
(ROOT/'Docs/verification.json').write_text(json.dumps(verification,ensure_ascii=False,indent=2),encoding='utf-8')

files=[ROOT/'README.md',ROOT/'numeric_checks.json',ROOT/'Pool3D-NewTable-Model.unitypackage']
for folder in ['Docs','Source','ThirdPartyNotices']:
    files.extend(p for p in (ROOT/folder).rglob('*') if p.is_file() and '__pycache__' not in p.parts and p.name!='package_receipt.json')
files.extend(p for p in (ROOT/'Tools').iterdir() if p.is_file() and p.suffix in ['.py','.json'])
for folder in ['Assets','Packages','ProjectSettings']:
    files.extend(p for p in (ROOT/'UnityProject'/folder).rglob('*') if p.is_file() and not p.name.endswith('.bak'))
files=sorted(set(files))
assert not any(p.suffix.lower()=='.log' or p.suffix.lower() in ['.pfx','.p12','.pem','.key'] for p in files)
manifest={'files':[{'path':p.relative_to(ROOT).as_posix(),'bytes':p.stat().st_size,'sha256':sha(p)} for p in files]}
(ROOT/'delivery_manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
files.append(ROOT/'delivery_manifest.json')
output=ROOT.parent/'Pool3D-TableReplacement-Prep-20261030.zip'
with zipfile.ZipFile(output,'w',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
    for p in files:z.write(p,'Pool3D-TableReplacement-Prep/'+p.relative_to(ROOT).as_posix())
with zipfile.ZipFile(output) as z:
    assert z.testzip() is None
    assert all('/Library/' not in n and '/LibraryFirstImportStalled/' not in n and '/Verification/' not in n for n in z.namelist())
receipt={'passed':True,'zip':output.name,'bytes':output.stat().st_size,'sha256':sha(output),'files':len(files),'crcChecked':True,'rawLogsIncluded':False,'cachesIncluded':False}
(ROOT/'Docs/package_receipt.json').write_text(json.dumps(receipt,indent=2),encoding='utf-8')
print(json.dumps({'zip':output.name,'bytes':receipt['bytes'],'files':len(files),'crc':'passed','original_unchanged':len(snapshot['files']),'candidate_core_cs_unchanged':len(core_files)}))
