import bpy,math,json
from pathlib import Path
OUT=Path(__file__).parent
s=bpy.data.scenes['PROMO | cinematic scene'];bpy.context.window.scene=s;s.frame_set(1)
root=bpy.data.objects['Cue | logical tip root'];root.location.y=-.004
for o in list(bpy.data.objects):
    if o.name.startswith('PROMO cue |'):bpy.data.objects.remove(o,do_unlink=True)
def material(name,color,rough,spec=.3):
    m=bpy.data.materials.get(name) or bpy.data.materials.new(name);m.use_nodes=True
    n=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
    n.inputs['Base Color'].default_value=(*color,1);n.inputs['Roughness'].default_value=rough
    n.inputs['Specular IOR Level'].default_value=spec
    return m,n
wood,bs=material('PROMO cue | polished fine ash',(.50,.34,.14),.30,.30)
nt=wood.node_tree;nodes=nt.nodes;links=nt.links
for n in list(nodes):
    if n.type not in {'BSDF_PRINCIPLED','OUTPUT_MATERIAL'}:nodes.remove(n)
coord=nodes.new('ShaderNodeTexCoord');scale=nodes.new('ShaderNodeVectorMath');scale.operation='MULTIPLY';scale.inputs[1].default_value=(2.5,800,800)
links.new(coord.outputs['Object'],scale.inputs[0])
noise=nodes.new('ShaderNodeTexNoise');noise.inputs['Scale'].default_value=1;noise.inputs['Detail'].default_value=2;noise.inputs['Roughness'].default_value=.55
links.new(scale.outputs[0],noise.inputs['Vector'])
ramp=nodes.new('ShaderNodeValToRGB');ramp.color_ramp.elements[0].position=.18;ramp.color_ramp.elements[0].color=(.24,.15,.065,1)
ramp.color_ramp.elements[1].position=.82;ramp.color_ramp.elements[1].color=(.62,.46,.25,1)
links.new(noise.outputs['Fac'],ramp.inputs[0]);links.new(ramp.outputs[0],bs.inputs['Base Color'])
bs.inputs['Coat Weight'].default_value=.16;bs.inputs['Coat Roughness'].default_value=.25
ferrule,_=material('PROMO cue | ivory ferrule',(.72,.68,.55),.27)
tip,_=material('PROMO cue | blue chalk leather tip',(.045,.11,.14),.80,.16)
def tube(name,rings,mat):
    verts=[];faces=[];N=64
    for x,r in rings:
        for j in range(N):
            a=j*math.tau/N;verts.append((x,r*math.cos(a),r*math.sin(a)))
    for i in range(len(rings)-1):
        for j in range(N):faces.append((i*N+j,i*N+(j+1)%N,(i+1)*N+(j+1)%N,(i+1)*N+j))
    faces.extend([tuple(range(N-1,-1,-1)),tuple((len(rings)-1)*N+j for j in range(N))])
    me=bpy.data.meshes.new(name);me.from_pydata(verts,[],faces);me.materials.append(mat)
    o=bpy.data.objects.new(name,me);s.collection.objects.link(o);o.parent=root
    for p in me.polygons:p.use_smooth=len(p.vertices)==4
    return o
shaft=tube('PROMO cue | slender ash front shaft',[(-.735+.720*i/32,.0064-(.0064-.0051)*i/32) for i in range(33)],wood)
fer=tube('PROMO cue | 12mm ferrule',[(-.015,.0051),(-.003,.0051)],ferrule)
end=tube('PROMO cue | 10mm domed leather tip',[(-.003,.0051),(-.0015,.00505),(-.0004,.0044),(0,.0028)],tip)
for o in [shaft,fer,end]:
    for f,h in [(1,True),(144,True),(145,False),(216,False),(217,True)]:
        o.hide_render=h;o.keyframe_insert('hide_render',frame=f)
old=bpy.data.objects['Cue | supplied magic staff model'];old.animation_data_clear();old.hide_render=True
(OUT/'cue_design.json').write_text(json.dumps({'only_film_copy_changed':True,'original_model_retained_hidden':old.name,'logical_full_length_m':1.45,'visible_front_section_m':.735,'tip_diameter_mm':10.2,'front_shaft_diameter_range_mm':[10.2,12.8],'ferrule_length_mm':12,'visible_shot_frames':[145,216],'look':'fine straight ash grain, satin polish; no exaggerated bump'},indent=2))
print('Film-only slender cue front section ready')
