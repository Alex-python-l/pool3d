import bpy, json, math, hashlib
from pathlib import Path
from mathutils import Vector
OUT=Path(__file__).parent
render=bpy.context.scene
assert not bpy.data.scenes.get('PROMO | rigid body laboratory')
render.name='PROMO | cinematic scene'
def meshhash(o):
    h=hashlib.sha256()
    for v in o.data.vertices:h.update(bytes(str(tuple(v.co)),'utf8'))
    for p in o.data.polygons:h.update(bytes(str((tuple(p.vertices),p.material_index)),'utf8'))
    return h.hexdigest()
snapshot={o.name:meshhash(o) for o in render.objects if o.type=='MESH'}
(OUT/'original_mesh_hashes.json').write_text(json.dumps(snapshot,indent=2))
sim=bpy.data.scenes.new('PROMO | rigid body laboratory')
bpy.context.window.scene=sim
sim.render.fps=240;sim.frame_start=1;sim.frame_end=2401
def body(obj,typ,shape,friction,bounce):
    sim.collection.objects.link(obj)
    bpy.context.view_layer.objects.active=obj;obj.select_set(True)
    bpy.ops.rigidbody.object_add()
    obj.select_set(False)
    rb=obj.rigid_body;rb.type=typ;rb.collision_shape=shape
    rb.friction=friction;rb.restitution=bounce;rb.use_margin=True;rb.collision_margin=.00004
    obj.hide_render=True
    return rb
bed=bpy.data.objects['Chinese8 | level cloth with six open pockets']
slate=bpy.data.objects.new('COLLIDER | slate with six real openings',bed.data.copy())
slate.matrix_world=bed.matrix_world.copy()
slate.location.z-=.00018
body(slate,'PASSIVE','MESH',.19,.02)
for o in list(render.objects):
    if o.name.startswith('Chinese8 | cushion '):
        proxy=bpy.data.objects.new('COLLIDER | '+o.name,o.data.copy());proxy.matrix_world=o.matrix_world.copy()
        body(proxy,'PASSIVE','MESH',.12,.78)
# Catch pocketed balls within the existing hanging bags, without changing visible nets.
for i,(x,y) in enumerate([(-1.305,-.66),(-1.305,.66),(1.305,-.66),(1.305,.66),(0,-.665),(0,.665)]):
    bpy.ops.mesh.primitive_cylinder_add(vertices=32,radius=.055,depth=.012,location=(x,y,.579))
    o=bpy.context.object;sim.collection.objects.unlink(o);o.name=f'COLLIDER | pocket bottom {i+1}'
    body(o,'PASSIVE','MESH',.5,.05)
    # Open cylindrical pocket liner: physical catch walls only, no visible geometry.
    vs=[];fs=[]
    for z in [.585,.800]:
        for j in range(48):
            a=j*math.tau/48;vs.append((x+.0525*math.cos(a),y+.0525*math.sin(a),z))
    for j in range(48):fs.append((j,(j+1)%48,(j+1)%48+48,j+48))
    me=bpy.data.meshes.new('Pocket collision shell');me.from_pydata(vs,[],fs)
    body(bpy.data.objects.new(f'COLLIDER | pocket shell {i+1}',me),'PASSIVE','MESH',.3,.1)
order=[1,10,3,4,8,11,14,5,9,2,6,12,13,7,15]
positions={0:(-.9,0,.828575)};k=0;d=.05720
for row in range(5):
    for j in range(row+1):
        positions[order[k]]=(.635+row*d*math.sqrt(3)/2,(j-row/2)*d,.828575);k+=1
for i in range(16):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=16,ring_count=8,radius=.028575,location=positions[i])
    o=bpy.context.object;sim.collection.objects.unlink(o);o.name=f'PHYS | Ball {i:02}'
    rb=body(o,'ACTIVE','SPHERE',.19,.94)
    rb.mass=.17;rb.linear_damping=.27;rb.angular_damping=.20
    rb.use_deactivation=False
    if i==0:
        rb.kinematic=True;rb.keyframe_insert('kinematic',frame=1);rb.keyframe_insert('kinematic',frame=2)
        o.keyframe_insert('location',frame=1)
        o.location.x+=6.8/240;o.location.y+=.001/240
        o.keyframe_insert('location',frame=2)
        rb.kinematic=False;rb.keyframe_insert('kinematic',frame=3)
world=sim.rigidbody_world
world.substeps_per_frame=12;world.solver_iterations=50
world.point_cache.frame_start=1;world.point_cache.frame_end=2401
bpy.context.window.scene=render
render['promo_source_preserved']=True
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'break_promo_working.blend'))
print(json.dumps({'film':bpy.data.filepath,'simulation_balls':16,'simulation_fps':240,'speed_m_s':6.8,'hidden_collision_proxies':19,'source_meshes':len(snapshot)}))
