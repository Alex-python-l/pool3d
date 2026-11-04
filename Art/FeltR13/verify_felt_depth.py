import bpy,bmesh,json,math,hashlib,ast
import numpy as np
from pathlib import Path
from mathutils import Vector
from mathutils.bvhtree import BVHTree
OUT=Path(r'C:\Users\L\Desktop\破壳BullBear Labs\art-source\luxury_pool_table\20261102_felt_depth_r13')
a=ast.parse((OUT.parent/'20261030_felt_lite/apply_felt_lite.py').read_text(encoding='utf-8'))
exec(compile(ast.Module(body=[n for n in a.body if isinstance(n,ast.FunctionDef) and n.name in ['mesh_hash','material_state']],type_ignores=[]),'protection','exec'))
b=json.loads((OUT/'before_snapshot.json').read_text(encoding='utf-8'));s=bpy.context.scene
bed=bpy.data.objects['Chinese8 | level cloth with six open pockets'];cloth=bpy.data.materials['Chinese8 | level unmarked green cloth']
protected={n:h for n,h in b['meshes'].items() if n!=bed.name}
assert all(mesh_hash(bpy.data.objects[n])==h for n,h in protected.items())
materials={n:h for n,h in b['materials'].items() if n!=cloth.name}
assert all(material_state(bpy.data.materials[n])==h for n,h in materials.items())
assert all([list(r) for r in bpy.data.objects[n].matrix_world]==t for n,t in b['transforms'].items())
assert b['lights']=={o.name:[o.data.energy,list(o.data.color)] for o in s.objects if o.type=='LIGHT'}
p=np.array([list(v.co) for v in bed.data.vertices]);old=np.array(b['bed_points']);n=len(old)
assert len(p)==2*n
assert np.array_equal(p[:n,:2],old[:,:2])
assert np.max(np.abs(p[:n,2]-.80018))<1e-7
assert np.max(np.abs(p[n:,2]-.79883))<1e-7
bm=bmesh.new();bm.from_mesh(bed.data);assert all(e.is_manifold for e in bm.edges);bm.free()
assert bed.get('physical_contact_reference_m')==.800
assert len([n for n in cloth.node_tree.nodes if n.type=='TEX_COORD' and n.object and n.object.name.startswith('Ball |')])==16
assert sum(n.type=='TEX_IMAGE' for n in cloth.node_tree.nodes)==3

table=[o for o in s.objects if o.type=='MESH' and (o.name.startswith('Chinese8 |') or o.name[:2] in ['01','02','03','04','05','06'])]
points=[];faces=[];owner=[]
for o in table:
    start=len(points);points.extend(o.matrix_world@v.co for v in o.data.vertices)
    for f in o.data.polygons:faces.append([start+i for i in f.vertices]);owner.append(o.name)
tree=BVHTree.FromPolygons(points,faces)
centres=[(-1.305,-.660),(-1.305,.660),(1.305,-.660),(1.305,.660),(0,-.665),(0,.665)]
drop=0
for cx,cy in centres:
    for dx,dy in [(0,0)]+[(r*math.cos(math.tau*i/32),r*math.sin(math.tau*i/32)) for r in [.014,.028575,.0295] for i in range(32)]:
        hit,normal,index,d=tree.ray_cast(Vector((cx+dx,cy+dy,.87)),Vector((0,0,-1)),.33)
        assert hit is None,(cx,cy,dx,dy,owner[index]);drop+=1
r6=json.loads((OUT.parent/'20261030_seam_roughness/seam_roughness_checks.json').read_text(encoding='utf-8'))
seams=0
for r in r6['profiles']:
    for u in np.linspace(r['rear_straight_range_m'][0]+.0002,r['rear_straight_range_m'][1]-.0002,65):
        for offset in [-.002,-.001,-.0004,-.0001,0,.0001,.0003,.001,.002]:
            q=Vector((0,0,.87));q[r['axis']]=r['side']*(r['wood_inner_plane_m']+offset);q[1-r['axis']]=u
            hit,normal,index,d=tree.ray_cast(q,Vector((0,0,-1)),.04)
            assert hit is not None and hit.z>.845,(r['object'],u,offset);seams+=1
level=0
for x in np.linspace(-1.1,1.1,5):
    for y in np.linspace(-.5,.5,5):
        hit,normal,index,d=tree.ray_cast(Vector((x,y,1)),Vector((0,0,-1)),.25)
        assert hit is not None and abs(hit.z-.80018)<1e-7,(x,y,hit);level+=1
balls=np.array([list(bpy.data.objects[f'Ball | {i:02d}'].matrix_world.translation) for i in range(16)])
assert np.max(np.abs(balls[:,2]-.828575))<1e-7
nap=bpy.data.objects['Preview | compressed cloth nap at balls R13']
v=np.array([list(nap.matrix_world@v.co) for v in nap.data.vertices])
gap=np.linalg.norm(v[:,None,:]-balls[None,:,:],axis=2).min(axis=1)-.028575
assert gap.min()>-.000003,float(gap.min())
cue=bpy.data.objects['Cue | supplied magic staff model']
ct=BVHTree.FromPolygons([cue.matrix_world@v.co for v in cue.data.vertices],[list(f.vertices) for f in cue.data.polygons])
assert len(tree.overlap(ct))==0
triangles=sum(len(f.vertices)-2 for o in s.objects if o.type=='MESH' for f in o.data.polygons)
contact_radius=math.sqrt(.028575**2-(.028575-.00018)**2)
report={'passed':True,'revision':'R13 felt depth, mottling and ball contact',
        'protected_r12_meshes':len(protected),'protected_other_materials':len(materials),
        'all_original_transforms_and_ball_centres_unchanged':True,'original_lights_unchanged':True,
        'cloth_sheet_thickness_mm':1.35,'uncompressed_visual_nap_offset_mm':.18,
        'nominal_physics_contact_height_m':.800,'render_pile_top_m':.80018,'sheet_bottom_m':.79883,
        'ball_radius_m':.028575,'ball_centre_z_m':.828575,'sphere_pile_contact_radius_mm':contact_radius*1000,
        'contact_nap_minimum_sphere_clearance_m':float(gap.min()),'bed_xy_and_pocket_edges_unchanged':True,
        'pocket_drop_samples':drop,'wood_seam_samples':seams,'flat_render_pile_points':level,
        'cue_table_intersections':0,'before_triangles':b['triangles'],'after_triangles':triangles,
        'increase_vs_r12_percent':100*(triangles/b['triangles']-1),
        'mesh_objects':sum(o.type=='MESH' for o in s.objects),
        'no_particle_hair_simulation':True,'no_alpha_shell_layers':True,'new_texture_samplers':3,
        'shader_live_ball_coordinate_inputs':16,'contact_fibre_mesh_authoring_only':True,
        'no_added_realtime_lights':True,'no_unity_execution':True,'game_fps_not_measured':True,
        'physics_floor_must_not_be_recalibrated_to_visual_nap_height':True}
(OUT/'felt_depth_checks.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(report,ensure_ascii=False))
