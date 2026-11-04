import bpy
s=bpy.data.scenes['PROMO | rigid body laboratory'];bpy.context.window.scene=s
for o in s.objects:o.hide_render=False;o.hide_viewport=False;o.hide_set(False)
if s.rigidbody_world.collection.name not in s.collection.children:s.collection.children.link(s.rigidbody_world.collection)
bpy.context.view_layer.update()
print('DG',len(bpy.context.evaluated_depsgraph_get().objects),bpy.context.scene.name,bpy.context.view_layer.name)
print('WORLD',[(p.identifier,str(getattr(s.rigidbody_world,p.identifier,None))) for p in s.rigidbody_world.bl_rna.properties if p.identifier not in ['rna_type','collection','constraints','effector_weights','point_cache']])
o=bpy.data.objects['PHYS | Ball 01'];o.location.z=1.5
for f in range(1,101):
    s.frame_set(f);bpy.context.view_layer.update()
    if f in [1,2,10,100]:print('FALL',f, list(o.evaluated_get(bpy.context.evaluated_depsgraph_get()).matrix_world.translation))
# Force update by re-add world and ensure actual collection objects are linked.
bpy.ops.rigidbody.world_remove();bpy.ops.rigidbody.world_add()
w=s.rigidbody_world;w.collection=bpy.data.collections['RigidBodyWorld']
for f in range(1,101):
    s.frame_set(f);bpy.context.view_layer.update()
    if f in [1,2,10,100]:print('REWORLD',f,list(o.evaluated_get(bpy.context.evaluated_depsgraph_get()).matrix_world.translation))
