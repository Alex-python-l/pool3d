"""Close the fourth wall and add a static, game-scale 16-ball authoring preview."""
import bpy,bmesh,json,math,hashlib
from pathlib import Path
from mathutils import Vector
OUT=Path(r'C:\Users\L\Desktop\破壳BullBear Labs\art-source\luxury_pool_table\20261101_closed_room_balls_cue_r11')
s=bpy.context.scene
assert bpy.data.objects.get('Room | front wall closed R11') is None
room=bpy.data.collections['Room | walls and floor']
wall=bpy.data.materials['R10 | subtle grey fabric wall'];dark=bpy.data.materials['R10 | satin charcoal trim'];blue=bpy.data.materials['Room | slim cyan-blue accents']
def box(name,center,size,material):
    x,y,z=[a/2 for a in size]
    vertices=[(-x,-y,-z),(x,-y,-z),(x,y,-z),(-x,y,-z),(-x,-y,z),(x,-y,z),(x,y,z),(-x,y,z)]
    faces=[(3,2,1,0),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7),(4,5,6,7)]
    me=bpy.data.meshes.new(name);me.from_pydata(vertices,[],faces);me.materials.append(material);me.update()
    o=bpy.data.objects.new(name,me);room.objects.link(o);o.location=center;return o
box('Room | front wall closed R11',(3.38,0,1.25),(.16,6.32,2.5),wall)
box('Room | front skirting R11',(3.275,0,.05),(.05,6.0,.10),dark)
trim=[]
for i,y in enumerate([-2.80,-1.45,0,1.20,2.70]):
    trim.append(box(f'Room | front dark joint R11 {i}',(3.295,y,1.25),(.008,.028,2.48),dark))
    trim.append(box(f'Room | front blue seam R11 {i}',(3.287,y,1.27),(.010,.010,2.30),blue))
trim.append(box('Room | front horizontal blue seam R11',(3.286,0,1.58),(.010,5.80,.010),blue))
# Combine these few new trims by material; never alter previous room batches.
groups=[(mat,[o for o in trim if o.data.materials[0]==mat]) for mat in [dark,blue]]
for mat,objects in groups:
    for old in bpy.context.selected_objects:old.select_set(False)
    for o in objects:o.select_set(True)
    bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join()
    objects[0].name='Room | front '+('shadow joints' if mat==dark else 'blue accent grid')+' R11'

collection=bpy.data.collections.new('Gameplay preview | Balls - exclude from static room export')
s.collection.children.link(collection)
radius=.028575;longitude=48;latitude=24
vertices=[(0,0,radius)];coordinates=[(.5,1)]
for j in range(1,latitude):
    theta=math.pi*j/latitude
    for i in range(longitude+1):
        u=i/longitude;phi=-math.pi/2+math.tau*u
        vertices.append((radius*math.sin(theta)*math.cos(phi),-radius*math.sin(theta)*math.sin(phi),radius*math.cos(theta)))
        coordinates.append((u,1-j/latitude))
bottom=len(vertices);vertices.append((0,0,-radius));coordinates.append((.5,0))
faces=[]
for i in range(longitude):faces.append((0,1+i,1+i+1))
for j in range(latitude-2):
    a=1+j*(longitude+1);b=a+longitude+1
    for i in range(longitude):faces.append((a+i,b+i,b+i+1,a+i+1))
last=1+(latitude-2)*(longitude+1)
for i in range(longitude):faces.append((last+i,bottom,last+i+1))
mesh=bpy.data.meshes.new('Balls | shared 57.15mm UV sphere');mesh.from_pydata(vertices,[],faces);mesh.update()
bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free()
uv=mesh.uv_layers.new(name='Existing game ball texture UV')
for face in mesh.polygons:
    face.use_smooth=True
    non_poles=[coordinates[mesh.loops[li].vertex_index][0] for li in face.loop_indices if mesh.loops[li].vertex_index not in [0,bottom]]
    for li in face.loop_indices:
        vi=mesh.loops[li].vertex_index;u,v=coordinates[vi]
        if vi in [0,bottom]:u=sum(non_poles)/len(non_poles)
        uv.data[li].uv=(u,v)
mesh.normals_split_custom_set([tuple(mesh.vertices[loop.vertex_index].co.normalized()) for loop in mesh.loops])
materials=[];maps=[]
for number in range(16):
    path=OUT/f'Textures/Balls/Ball_{number}.png';assert path.is_file()
    image=bpy.data.images.load(str(path),check_existing=False);image.pack()
    mat=bpy.data.materials.new(f'Balls | original game ball {number:02d}');mat.use_nodes=True
    bsdf=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
    bsdf.inputs['Roughness'].default_value=.16;bsdf.inputs['IOR'].default_value=1.50
    bsdf.inputs['Coat Weight'].default_value=.28;bsdf.inputs['Coat Roughness'].default_value=.12
    tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=image;mat.node_tree.links.new(tex.outputs['Color'],bsdf.inputs['Base Color'])
    materials.append(mat);maps.append({'ball':number,'texture':path.name,'resolution':list(image.size),'sha256':hashlib.sha256(path.read_bytes()).hexdigest()})
mesh.materials.append(materials[0])
layout={0:(-.9,0)}
order=[1,10,3,4,8,11,14,5,9,2,6,12,13,7,15]
index=0;spacing_radius=radius+.002
for row in range(5):
    for column in range(row+1):
        # Original game rack spacing, deterministic authoring arrangement only.
        x=.635+2*spacing_radius*math.cos(math.pi/6)*row
        y=-spacing_radius*row+2*spacing_radius*column
        layout[order[index]]=(x,y);index+=1
balls=[]
for number in range(16):
    o=bpy.data.objects.new(f'Ball | {number:02d}',mesh);collection.objects.link(o)
    o.material_slots[0].link='OBJECT';o.material_slots[0].material=materials[number]
    o.location=(*layout[number],.8+radius)
    o['ball_number']=number;o['diameter_m']=2*radius;o['authoring_preview_only']=True
    o['unity_export_role']='exclude static copy; existing PoolBall state creates runtime ball views'
    balls.append(o)
bpy.context.view_layer.update()
for i,a in enumerate(balls):
    assert abs(a.location.z-radius-.8)<1e-7
    for b in balls[i+1:]:assert (a.location-b.location).length>=2*radius-1e-6
assert layout[8]==layout[order[4]]
s.camera.location=(2.95,-2.50,1.65)
target=Vector((-.55,0,1.01));s.camera.rotation_euler=(target-s.camera.location).to_track_quat('-Z','Y').to_euler();s.camera.data.lens=24
s.camera.data.clip_start=.02;s.camera.data.clip_end=100
report={'fourth_wall_closed':True,'front_wall_inner_x_m':3.30,'room_inner_x_m':[-4.10,3.30],'room_inner_y_m':[-3,3],
        'room_ceiling_height_m':2.5,'presentation_camera_moved_inside_closed_room':True,
        'balls':16,'ball_diameter_m':2*radius,'ball_center_height_m':.8+radius,
        'shared_sphere_triangles':sum(len(f.vertices)-2 for f in mesh.polygons),
        'ball_positions_m':{str(n):[x,y,.8+radius] for n,(x,y) in layout.items()},
        'existing_game_texture_maps':maps,'static_preview_only':True,'no_rigidbody_or_game_tick':True}
(OUT/'wall_and_balls_checks.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({k:report[k] for k in ['fourth_wall_closed','balls','ball_diameter_m','ball_center_height_m','shared_sphere_triangles','static_preview_only']}))
