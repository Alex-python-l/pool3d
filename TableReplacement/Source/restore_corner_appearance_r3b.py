"""Restore original pocket-corner UVs/materials only, with zero vertex movement."""
import bpy,json,math,hashlib
from pathlib import Path
from mathutils import Vector
from mathutils.bvhtree import BVHTree

OUT=Path(r'C:\Users\L\Desktop\破壳BullBear Labs\art-source\luxury_pool_table\20261030_uv_curve_fix')
SOURCE=OUT.parent/'20261029_cleanup_sizing/luxury_pool_table_clean_metric.blend'
BACKUP=OUT.parent/'20261030_rail_fix/before_rectangular_rail_fix.blend'
body=bpy.data.objects['Chinese8 | luxury body'];mesh=body.data
assert body.get('uv_curve_r3') and not body.get('pocket_appearance_restored')
if not (OUT/'before_corner_appearance_restore.blend').exists():
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'before_corner_appearance_restore.blend'),copy=True)
geometry_before=hashlib.sha256(repr(([tuple(v.co) for v in mesh.vertices],[tuple(f.vertices) for f in mesh.polygons])).encode()).hexdigest()
with bpy.data.libraries.load(str(BACKUP),link=False) as (source,destination):
    destination.meshes=['Chinese8 | woodwork with four open corner bores']
original=destination.meshes[0];original.calc_loop_triangles();original_uv=original.uv_layers.active.data
triangles=[];loops=[]
for triangle in original.loop_triangles:
    if triangle.material_index!=0:continue
    p=[original.vertices[k].co for k in triangle.vertices]
    if (p[1]-p[0]).cross(p[2]-p[0]).length<1e-14:continue
    triangles.append(list(triangle.vertices));loops.append(list(triangle.loops))
tree=BVHTree.FromPolygons([v.co for v in original.vertices],triangles,all_triangles=True)
def pocket_region(p):
    if abs(p.x)>1.16 and abs(p.y)>.515:return True
    return abs(p.x)<.085 and abs(abs(p.y)-.665)<.130
def bary_uv(p,index):
    ia,ib,ic=triangles[index];a,b,c=[original.vertices[k].co for k in [ia,ib,ic]]
    e1=b-a;e2=c-a;d=p-a
    aa=e1.dot(e1);ab=e1.dot(e2);bb=e2.dot(e2);da=d.dot(e1);db=d.dot(e2)
    denominator=aa*bb-ab*ab
    u=(da*bb-db*ab)/denominator;v=(db*aa-da*ab)/denominator
    ua,ub,uc=[original_uv[k].uv.copy() for k in loops[index]]
    return ua+(ub-ua)*u+(uc-ua)*v
uv=mesh.uv_layers.active.data;changed=0;max_distance=0
for face in mesh.polygons:
    if face.material_index not in [0,4] or min(mesh.vertices[k].co.z for k in face.vertices)<.790-1e-6:continue
    center=sum((mesh.vertices[k].co for k in face.vertices),Vector())/len(face.vertices)
    if not pocket_region(center):continue
    normal=face.normal
    candidates=tree.find_nearest_range(center,.00005)
    if not candidates:candidates=[tree.find_nearest(center)]
    hit,hit_normal,index,distance=max(candidates,key=lambda item:item[1].dot(normal))
    assert distance<.005, (face.index,distance)
    max_distance=max(max_distance,distance)
    for li in face.loop_indices:uv[li].uv=bary_uv(mesh.vertices[mesh.loops[li].vertex_index].co,index)
    face.material_index=0;changed+=1
assert changed>100
geometry_after=hashlib.sha256(repr(([tuple(v.co) for v in mesh.vertices],[tuple(f.vertices) for f in mesh.polygons])).encode()).hexdigest()
assert geometry_after==geometry_before
mesh.update();body['pocket_appearance_restored']=True
report=json.loads((OUT/'uv_curve_fix_report.json').read_text(encoding='utf-8'))
report['revision']='R3b original pocket appearance restored; minimal UV/jaw correction'
report['minimal_corner_restore']={'passed':True,'restored_faces':changed,'reference':'pre-R2 original body mesh UV/materials','maximum_reference_surface_distance_m':max_distance,'vertex_and_face_geometry_unchanged':True,'geometry_sha256':geometry_after,'new_colors_added':False,'new_corner_design_added':False,'protected_regions':'All four corners and both middle-pocket surroundings are excluded from straight-rail UV remapping.'}
(OUT/'uv_curve_fix_report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
bpy.data.meshes.remove(original)
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE))
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'luxury_pool_table_minimal_uv_curve_r3b.blend'),copy=True)
print(json.dumps(report['minimal_corner_restore'],ensure_ascii=False))
