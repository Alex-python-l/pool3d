import bpy,json,time,traceback
from pathlib import Path
OUT=Path(__file__).parent
try:
    s=bpy.data.scenes['PROMO | rigid body laboratory'];bpy.context.window.scene=s
    for o in s.objects:o.hide_render=False;o.hide_set(False)
    if s.rigidbody_world.collection.name not in s.collection.children:
        s.collection.children.link(s.rigidbody_world.collection)
    # Include the original ivory corners and wooden surround: cushion-only
    # proxies leave an escape route across the outer corner shoulders.
    for name in ['Chinese8 | luxury body','Chinese8 | six pocket leather back facings R12']:
        source=bpy.data.objects[name]
        proxy=bpy.data.objects.new('COLLIDER | original surround '+name,source.data)
        proxy.matrix_world=source.matrix_world.copy();s.collection.objects.link(proxy)
        bpy.context.view_layer.objects.active=proxy;proxy.select_set(True)
        bpy.ops.rigidbody.object_add();proxy.select_set(False)
        rb=proxy.rigid_body;rb.type='PASSIVE';rb.collision_shape='MESH';rb.friction=.18;rb.restitution=.65;rb.use_margin=True;rb.collision_margin=.00004
    balls=[bpy.data.objects[f'PHYS | Ball {i:02}'] for i in range(16)]
    s.rigidbody_world.substeps_per_frame=48;s.rigidbody_world.solver_iterations=100
    cue=balls[0];cue.animation_data_clear();cue.rigid_body.kinematic=False;cue.location=(-.9,-.004,.828575)
    # A kinematic tip imparts momentum by actual Bullet contact; toggling an
    # animated sphere alone does not reliably preserve release velocity.
    bpy.ops.mesh.primitive_uv_sphere_add(segments=12,ring_count=6,radius=.008,location=(-.968,-.004,.828575))
    striker=bpy.context.object;striker.name='PHYS | hidden animated cue tip'
    bpy.ops.rigidbody.object_add();rb=striker.rigid_body
    rb.kinematic=True;rb.collision_shape='SPHERE';rb.friction=0;rb.restitution=.25;rb.use_margin=True;rb.collision_margin=.00002
    for f,x in [(1,-.968),(2,-.940),(3,-.912),(4,-.955),(5,-1.1)]:
        striker.location.x=x;striker.keyframe_insert('location',frame=f)
    s.frame_set(1)
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'physics_source.blend'))
    motion=[];t=time.time()
    # Background depsgraph must be explicitly evaluated for the simulation scene.
    bpy.context.view_layer.update()
    dg=bpy.context.evaluated_depsgraph_get()
    print('SIM CONTEXT',bpy.context.scene.name, dg.scene.name, flush=True)
    for f in range(1,2402):
        s.frame_set(f);bpy.context.view_layer.update();dg=bpy.context.evaluated_depsgraph_get()
        motion.append([list(o.evaluated_get(dg).matrix_world.translation)+list(o.evaluated_get(dg).matrix_world.to_quaternion()) for o in balls])
        if f%240==0:(OUT/'simulation_status.json').write_text(json.dumps({'frame':f,'seconds':time.time()-t}))
    (OUT/'motion_240fps.json').write_text(json.dumps(motion))
    (OUT/'simulation_status.json').write_text(json.dumps({'complete':True,'frames':len(motion),'seconds':time.time()-t,'final':motion[-1]}))
except Exception:
    (OUT/'simulation_status.json').write_text(json.dumps({'error':traceback.format_exc()}));raise
