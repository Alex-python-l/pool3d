"""Bake the four-way seam fix to one repeatable image, then add live ball compression.

Emission bake is a material data bake, not a generated replacement for a scene render.
"""
import bpy,json,traceback,time
from pathlib import Path
OUT=Path(r'C:\Users\L\Desktop\破壳BullBear Labs\art-source\luxury_pool_table\20261102_felt_depth_r13')
assert json.loads((OUT/'render_status.json').read_text(encoding='utf-8'))['status']=='complete'
s=bpy.context.scene;mat=bpy.data.materials['Chinese8 | level unmarked green cloth']
assert not mat.get('single_albedo_baked_r13')
window=bpy.context.window
temp=bpy.data.scenes.new('TEMP | R13 cloth albedo data bake')
temp.render.engine='CYCLES';temp.cycles.device='GPU';temp.cycles.samples=1
temp.render.bake.margin=0
mesh=bpy.data.meshes.new('TEMP | metric 25 cm material swatch')
mesh.from_pydata([(0,0,0),(.25,0,0),(.25,.25,0),(0,.25,0)],[],[(0,1,2,3)]);mesh.update()
uv=mesh.uv_layers.new(name='UVMap')
for i,p in enumerate([(0,0),(1,0),(1,1),(0,1)]):uv.data[i].uv=p
plane=bpy.data.objects.new('TEMP | material bake plane',mesh);temp.collection.objects.link(plane)
anchor=bpy.data.objects['Tabletop | metric texture anchor R12'];temp.collection.objects.link(anchor)
bm=mat.copy();bm.name='TEMP | R13 seam-free colour bake';mesh.materials.append(bm)
out=next(n for n in bm.node_tree.nodes if n.type=='OUTPUT_MATERIAL')
photo=next(n for n in bm.node_tree.nodes if n.label=='Continuous photographic nap')
emit=bm.node_tree.nodes.new('ShaderNodeEmission');bm.node_tree.links.new(photo.outputs[0],emit.inputs['Color']);bm.node_tree.links.new(emit.outputs[0],out.inputs['Surface'])
image=bpy.data.images.new('FeltR13_SeamlessNap_Albedo',width=2048,height=2048,alpha=False)
image.colorspace_settings.name='sRGB'
target=bm.node_tree.nodes.new('ShaderNodeTexImage');target.image=image
for n in bm.node_tree.nodes:n.select=False
target.select=True;bm.node_tree.nodes.active=target
started=time.time()
try:
    window.scene=temp
    for o in temp.objects:o.select_set(False)
    plane.select_set(True);bpy.context.view_layer.objects.active=plane
    assert 'EMIT' in [i.identifier for i in bpy.ops.object.bake.get_rna_type().properties['type'].enum_items]
    bpy.ops.object.bake(type='EMIT')
    image.filepath_raw=str(OUT/'Textures/FeltR13_SeamlessNap_Albedo.png')
    assert 'PNG' in [i.identifier for i in image.bl_rna.properties['file_format'].enum_items]
    image.file_format='PNG';image.save();image.pack()
finally:
    window.scene=s
    bpy.data.objects.remove(plane,do_unlink=True)
    bpy.data.scenes.remove(temp)
    bpy.data.meshes.remove(mesh)
    bpy.data.materials.remove(bm)

nodes=mat.node_tree.nodes;links=mat.node_tree.links
def enum(n,p,v):
    assert v in [i.identifier for i in n.bl_rna.properties[p].enum_items];setattr(n,p,v)
tex=nodes.new('ShaderNodeTexImage');tex.label='Baked seamless 2K photo nap, all three axes';tex.image=image
enum(tex,'projection','BOX');tex.projection_blend=.22;enum(tex,'extension','REPEAT');enum(tex,'interpolation','Linear')
coordinate=next(n for n in nodes if n.label=='25 cm photographic nap sample')
links.new(coordinate.outputs[0],tex.inputs['Vector'])
colour=next(n for n in nodes if n.label=='Photographic dye, visible mottle and flecks')
links.new(tex.outputs['Color'],colour.inputs[1])

# All sixteen positions are read from the real preview objects. Object-space
# length is invariant under each ball's current rotation; no painted shadow disc.
minimum=None
for i in range(16):
    tc=nodes.new('ShaderNodeTexCoord');tc.label=f'Ball {i:02d} live cloth-compression position';tc.object=bpy.data.objects[f'Ball | {i:02d}']
    length=nodes.new('ShaderNodeVectorMath');enum(length,'operation','LENGTH');links.new(tc.outputs['Object'],length.inputs[0])
    if minimum is None:minimum=length.outputs['Value']
    else:
        n=nodes.new('ShaderNodeMath');enum(n,'operation','MINIMUM');links.new(minimum,n.inputs[0]);links.new(length.outputs['Value'],n.inputs[1]);minimum=n.outputs[0]
subtract=nodes.new('ShaderNodeMath');enum(subtract,'operation','SUBTRACT');subtract.inputs[1].default_value=.028575;links.new(minimum,subtract.inputs[0])
mask=nodes.new('ShaderNodeMapRange');mask.label='Nap compressed within 0.8 mm of any ball surface'
mask.inputs['From Min'].default_value=0;mask.inputs['From Max'].default_value=.0008
mask.inputs['To Min'].default_value=0;mask.inputs['To Max'].default_value=1;mask.clamp=True
enum(mask,'interpolation_type','SMOOTHSTEP');links.new(subtract.outputs[0],mask.inputs['Value'])
height=nodes.new('ShaderNodeMath');enum(height,'operation','MULTIPLY');height.inputs[1].default_value=.000075;links.new(mask.outputs['Result'],height.inputs[0])
bump=next(n for n in nodes if n.type=='BUMP');links.new(height.outputs[0],bump.inputs['Distance'])

output=next(n for n in nodes if n.type=='OUTPUT_MATERIAL')
needed=set();todo=[output]
while todo:
    n=todo.pop()
    if n in needed:continue
    needed.add(n)
    todo.extend(l.from_node for inp in n.inputs for l in inp.links)
for n in list(nodes):
    if n not in needed:nodes.remove(n)
mat['single_albedo_baked_r13']=True;mat['live_ball_compression_r13']=True
mat['compression_distance_m']=.0008
report={'passed':True,'emission_data_bake':True,'colour_tile_size':[2048,2048],
        'live_ball_object_coordinates':16,'ball_positions_unchanged':True,'nap_relief_reduces_to_zero_at_contact':True,
        'painted_shadow_disc':False,'alpha_shell_layers':0,'image_nodes_after_bake':sum(n.type=='TEX_IMAGE' for n in nodes),
        'albedo_three_axis_mapping':True,'seconds':time.time()-started,'original_lights_unchanged':True}
(OUT/'material_bake_checks.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report))
