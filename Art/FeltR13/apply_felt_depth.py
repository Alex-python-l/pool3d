import bpy,json,math,random
from pathlib import Path
from mathutils import Vector
OUT=Path(r'C:\Users\L\Desktop\破壳BullBear Labs\art-source\luxury_pool_table\20261102_felt_depth_r13')
s=bpy.context.scene;spec=json.loads((OUT/'felt_spec.json').read_text(encoding='utf-8'))
m=bpy.data.materials['Chinese8 | level unmarked green cloth'];assert not m.get('felt_depth_r13')
nodes=m.node_tree.nodes;links=m.node_tree.links;nodes.clear()
def enum(n,p,value):
    assert value in [v.identifier for v in n.bl_rna.properties[p].enum_items],(n.type,p,value)
    setattr(n,p,value)
def node(kind,label,xy):
    n=nodes.new(kind);n.label=label;n.location=xy;return n
def mathnode(op,a=None,b=None,c=None,label=''):
    n=node('ShaderNodeMath',label,(0,0));enum(n,'operation',op)
    for i,value in enumerate([a,b,c]):
        if value is None:continue
        if isinstance(value,(float,int)):n.inputs[i].default_value=value
        else:links.new(value,n.inputs[i])
    return n.outputs[0]
def image_node(im,coordinate,label,xy):
    n=node('ShaderNodeTexImage',label,xy);n.image=im;enum(n,'extension','REPEAT');enum(n,'interpolation','Linear');links.new(coordinate,n.inputs['Vector']);return n
tc=node('ShaderNodeTexCoord','Metric coordinates shared with the table',(-1600,300));tc.object=bpy.data.objects['Tabletop | metric texture anchor R12']
uv=node('ShaderNodeVectorMath','25 cm photographic nap sample',(-1400,500));enum(uv,'operation','SCALE');uv.inputs['Scale'].default_value=4;links.new(tc.outputs['Object'],uv.inputs[0])
xyz=node('ShaderNodeSeparateXYZ','Smooth periodic crossfade coordinates',(-1200,700));links.new(uv.outputs['Vector'],xyz.inputs[0])
wx=mathnode('MULTIPLY_ADD',mathnode('COSINE',mathnode('MULTIPLY',xyz.outputs['X'],math.tau)),.5,.5,label='X seam-free weight')
wy=mathnode('MULTIPLY_ADD',mathnode('COSINE',mathnode('MULTIPLY',xyz.outputs['Y'],math.tau)),.5,.5,label='Y seam-free weight')
albedo=bpy.data.images.load(str(OUT/'Textures/FeltR13_ReferenceNap_Albedo.png'),check_existing=False);albedo.colorspace_settings.name='sRGB';albedo.pack()
samples=[]
for i,offset in enumerate([(0,0,0),(.5,0,0),(0,.5,0),(.5,.5,0)]):
    v=node('ShaderNodeVectorMath','Offset photographic sample '+str(i),(-1200,400-i*220));enum(v,'operation','ADD');v.inputs[1].default_value=offset;links.new(uv.outputs['Vector'],v.inputs[0])
    samples.append(image_node(albedo,v.outputs['Vector'],'Reference mottled short wool '+str(i),(-950,400-i*220)).outputs['Color'])
def mix(a,b,fac,label):
    n=node('ShaderNodeMixRGB',label,(-550,0));links.new(fac,n.inputs[0]);links.new(a,n.inputs[1]);links.new(b,n.inputs[2]);return n.outputs[0]
photo=mix(mix(samples[0],samples[1],wx,'Blend X 1'),mix(samples[2],samples[3],wx,'Blend X 2'),wy,'Continuous photographic nap')

macro_scale=node('ShaderNodeVectorMath','Full-bed unique brushing coordinates',(-1400,-600));enum(macro_scale,'operation','MULTIPLY');macro_scale.inputs[1].default_value=(1/2.70,1/1.42,0);links.new(tc.outputs['Object'],macro_scale.inputs[0])
macro_offset=node('ShaderNodeVectorMath','Table-wide origin',(-1200,-600));enum(macro_offset,'operation','ADD');macro_offset.inputs[1].default_value=(.5,.5,0);links.new(macro_scale.outputs['Vector'],macro_offset.inputs[0])
im=bpy.data.images.load(str(OUT/'Textures/FeltR13_FullBed_NapWear.png'),check_existing=False);im.colorspace_settings.name='Non-Color';im.pack()
macro=image_node(im,macro_offset.outputs[0],'Dye / directional nap / compressed flecks',(-950,-650))
channels=node('ShaderNodeSeparateColor','Full-bed wear data',(-700,-650));enum(channels,'mode','RGB');links.new(macro.outputs['Color'],channels.inputs[0])
micro_scale=node('ShaderNodeVectorMath','64 mm fine-fibre tile',(-1400,-1000));enum(micro_scale,'operation','SCALE');micro_scale.inputs['Scale'].default_value=1/.064;links.new(tc.outputs['Object'],micro_scale.inputs[0])
im=bpy.data.images.load(str(OUT/'Textures/FeltR13_FibrePacked.png'),check_existing=False);im.colorspace_settings.name='Non-Color';im.pack()
micro=image_node(im,micro_scale.outputs[0],'Compacted short fibres in three planes',(-950,-1000));enum(micro,'projection','BOX');micro.projection_blend=.22
small=node('ShaderNodeSeparateColor','Fine dye / roughness / relief',(-700,-1000));enum(small,'mode','RGB');links.new(micro.outputs['Color'],small.inputs[0])
factors=mathnode('MULTIPLY',mathnode('MULTIPLY_ADD',channels.outputs['Red'],.8,.6),mathnode('MULTIPLY_ADD',channels.outputs['Blue'],.32,.68))
factors=mathnode('MULTIPLY',factors,mathnode('MULTIPLY_ADD',small.outputs['Red'],.20,.90))
factors=mathnode('MULTIPLY',factors,.42,label='Albedo exposure, not lighting')
color=node('ShaderNodeMixRGB','Photographic dye, visible mottle and flecks',(100,400));enum(color,'blend_type','MULTIPLY');color.inputs[0].default_value=1;links.new(photo,color.inputs[1]);links.new(factors,color.inputs[2])
bs=node('ShaderNodeBsdfPrincipled','Dense compressible wool felt',(600,150));bs.inputs['Metallic'].default_value=0;bs.inputs['Diffuse Roughness'].default_value=.24
bs.inputs['Specular IOR Level'].default_value=.11;bs.inputs['Sheen Roughness'].default_value=.65;bs.inputs['Sheen Tint'].default_value=(.004,.075,.012,1)
bs.inputs['Anisotropic'].default_value=.20
links.new(color.outputs[0],bs.inputs['Base Color'])
links.new(mathnode('MULTIPLY_ADD',channels.outputs['Green'],.10,.82),bs.inputs['Roughness'])
links.new(mathnode('MULTIPLY_ADD',channels.outputs['Green'],.12,.10),bs.inputs['Sheen Weight'])
bump=node('ShaderNodeBump','75 micron fibre relief, no table waves',(350,-200));bump.inputs['Distance'].default_value=.000075;bump.inputs['Strength'].default_value=1;bump.inputs['Filter Width'].default_value=.13
links.new(small.outputs['Blue'],bump.inputs['Height']);links.new(bump.outputs['Normal'],bs.inputs['Normal'])
output=node('ShaderNodeOutputMaterial','Cloth surface',(900,150));links.new(bs.outputs['BSDF'],output.inputs['Surface'])
m.diffuse_color=(.002,.10,.012,1);m['felt_depth_r13']=True;m['nominal_physics_contact_z']=.800;m['uncompressed_nap_top_z']=.80018

# A real downward cloth sheet; the 0.18 mm top pile lets the unchanged balls sit
# into the nap instead of touching a mathematically zero-thickness colour plane.
bed=bpy.data.objects['Chinese8 | level cloth with six open pockets'];assert not bed.modifiers
for v in bed.data.vertices:v.co.z=.80018
bed.data.update()
solid=bed.modifiers.new('R13 | 1.35 mm cloth sheet, downward','SOLIDIFY');solid.thickness=.00135;solid.offset=-1;solid.use_rim=True
if hasattr(solid,'use_even_offset'):solid.use_even_offset=True
for o in s.objects:o.select_set(False)
bed.select_set(True);bpy.context.view_layer.objects.active=bed
bpy.ops.object.modifier_apply(modifier=solid.name)
bed['physical_contact_reference_m']=.800;bed['visual_uncompressed_nap_height_m']=.00018;bed['cloth_sheet_thickness_m']=.00135
bed['not_a_new_gameplay_collider']=True

rng=random.Random(131102)
def build_nap(name,entries,preview=False):
    vertices=[];faces=[];count=0;compressed=0
    for root,height,width,ball in entries:
        theta=rng.uniform(0,math.tau);tip=root+Vector((math.cos(theta),math.sin(theta),0))*height*.22+Vector((0,0,height))
        if ball is not None:
            rad=.028575;d=(tip.xy-ball.xy).length
            if d<rad:
                ceiling=ball.z-math.sqrt(max(0,rad*rad-d*d))-.000012
                allowed=ceiling-root.z
                if allowed<.000025:continue
                if tip.z>ceiling:tip.z=ceiling;compressed+=1
        for angle in [theta,theta+math.pi/2]:
            side=Vector((math.cos(angle),math.sin(angle),0))*width*.5
            i=len(vertices);vertices.extend([root-side,root+side,tip]);faces.append((i,i+1,i+2))
        count+=1
    me=bpy.data.meshes.new(name);me.from_pydata(vertices,[],faces);me.materials.append(m);me.update()
    obj=bpy.data.objects.new(name,me);s.collection.objects.link(obj)
    obj['no_gameplay_collider']=True;obj['authoring_contact_preview_only']=preview
    return {'object':name,'fibres':count,'triangles':len(faces),'fibres_compressed_beneath_ball':compressed,'exclude_from_static_environment_export':preview}

edge=[]
for o in s.objects:
    if o.type!='MESH' or not o.name.startswith('Chinese8 | cushion '):continue
    n=len(o.data.vertices)//8;assert len(o.data.vertices)==8*n
    ring=[o.matrix_world@v.co for v in o.data.vertices[-n:]]
    for a,b in zip(ring,ring[1:]+ring[:1]):
        length=(b-a).length
        for j in range(max(0,int(length*95))):
            p=a.lerp(b,rng.random())+Vector((0,0,.000012))
            edge.append((p,rng.uniform(.00014,.00032),rng.uniform(.000025,.000045),None))
edge_report=build_nap('Chinese8 | fine cloth edge nap R13',edge)
entries=[]
for i in range(16):
    ball=bpy.data.objects[f'Ball | {i:02d}'].matrix_world.translation
    for j in range(920 if i==0 else 64):
        a=rng.uniform(0,math.tau);r=math.sqrt(rng.uniform(.0028**2,.021**2))
        p=Vector((ball.x+r*math.cos(a),ball.y+r*math.sin(a),.80019))
        entries.append((p,rng.uniform(.00012,.00029),rng.uniform(.000022,.000040),ball))
contact_report=build_nap('Preview | compressed cloth nap at balls R13',entries,True)
bpy.context.view_layer.update()
report={'sheet_thickness_m':.00135,'visual_pile_top_m':.80018,'nominal_contact_plane_m':.800,'ball_transforms_unchanged':True,
        'bed_source_vertices':322,'bed_vertices_after':len(bed.data.vertices),'bed_triangles_after':sum(len(f.vertices)-2 for f in bed.data.polygons),
        'edge_nap':edge_report,'contact_nap':contact_report,
        'scene_triangles':sum(len(f.vertices)-2 for o in s.objects if o.type=='MESH' for f in o.data.polygons),
        'no_particle_hair_system':True,'no_extra_lights':True,'no_unity_changes':True}
(OUT/'geometry_depth_report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(report,ensure_ascii=False))
