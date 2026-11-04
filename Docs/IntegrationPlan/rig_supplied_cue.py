"""Explicit tip/butt anchors and a first-person view of the supplied cue's front half."""
import bpy,json,math
import numpy as np
from pathlib import Path
from mathutils import Vector,Matrix
from mathutils.bvhtree import BVHTree
OUT=Path(r'C:\Users\L\Desktop\破壳BullBear Labs\art-source\luxury_pool_table\20261101_closed_room_balls_cue_r11')
s=bpy.context.scene;obj=bpy.data.objects['Cue | supplied magic staff model']
assert not obj.get('cue_rig_r11')
collection=bpy.data.collections['Gameplay preview | balls and supplied cue']
world_matrix=obj.matrix_world.copy()
points=np.asarray([world_matrix@v.co for v in obj.data.vertices],dtype=np.float64)
center=points.mean(axis=0);values,axes=np.linalg.eigh((points-center).T@(points-center))
principal=axes[:,-1];q=(points-center)@principal
radial=(points-center)-q[:,None]*principal
ends=[]
for sign in [-1,1]:
    mask=q<q.min()+.04*np.ptp(q) if sign==-1 else q>q.max()-.04*np.ptp(q)
    ends.append((float(np.quantile(np.linalg.norm(radial[mask],axis=1),.9)),sign))
tip_sign=min(ends)[1]
forward=principal*tip_sign;p=(points-center)@forward
def endpoint(front):
    endpoint=p.max() if front else p.min()
    mask=p>endpoint-.001 if front else p<endpoint+.001
    result=points[mask].mean(axis=0)
    result+=forward*(endpoint-np.dot(result-center,forward))
    return Vector(result)
tip_original=endpoint(True);butt_original=endpoint(False)
axis=(tip_original-butt_original).normalized();scale=1.45/(tip_original-butt_original).length
align=axis.rotation_difference(Vector((1,0,0))).to_matrix().to_4x4()
local=align@Matrix.Scale(scale,4)@Matrix.Translation(-tip_original)@world_matrix
root=bpy.data.objects.new('Cue | logical tip root',None);collection.objects.link(root)
root.empty_display_size=.015;obj.parent=root
tip_position=Vector((-.900-.028575-.003,0,.828575))
positions=[];faces=[]
for o in s.objects:
    if o.type!='MESH' or not (o.name=='Chinese8 | luxury body' or 'cushion ' in o.name or o.name=='Chinese8 | level cloth with six open pockets'):continue
    start=len(positions);positions.extend(o.matrix_world@v.co for v in o.data.vertices)
    faces.extend([start+i for i in f.vertices] for f in o.data.polygons)
table=BVHTree.FromPolygons(positions,faces)
tested=[];angle=None
for degrees in np.arange(3,13.1,.5):
    transform=Matrix.Translation(tip_position)@Matrix.Rotation(math.radians(float(degrees)),4,'Y')
    root.matrix_world=transform;obj.matrix_world=transform@local;bpy.context.view_layer.update()
    cue_tree=BVHTree.FromPolygons([obj.matrix_world@v.co for v in obj.data.vertices],[list(f.vertices) for f in obj.data.polygons])
    overlaps=len(table.overlap(cue_tree));tested.append({'elevation_degrees':float(degrees),'intersections':overlaps})
    if not overlaps:angle=float(degrees);break
assert angle is not None,tested
anchors={}
for name,x in [('CueTip',0),('CueButt',-1.45),('CueMidpoint',-.725)]:
    anchor=bpy.data.objects.new(name,None);collection.objects.link(anchor);anchor.parent=root
    anchor.location=(x,0,0);anchor.empty_display_size=.010;anchors[name]=anchor
obj['cue_rig_r11']=True;obj['unity_export_role']='export separately as cuePrefab; use explicit CueTip and CueButt anchors'
root['logical_length_m']=1.45;root['forward_axis']='local +X in Blender; Unity adapter aligns prefab forward'
root['preview_only_not_runtime_shot_binding']=True
data=bpy.data.cameras.new('Shot | front-half cue camera');camera=bpy.data.objects.new(data.name,data);collection.objects.link(camera)
camera.location=(-1.66,-.025,1.08);target=Vector((-.48,0,.825))
camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler()
data.lens=43;data.clip_start=.015;data.clip_end=50
bpy.context.view_layer.update()
tip=anchors['CueTip'].matrix_world.translation;butt=anchors['CueButt'].matrix_world.translation
assert abs((tip-butt).length-1.45)<1e-5
depth_tip=(camera.matrix_world.inverted()@tip).z*-1
depth_butt=(camera.matrix_world.inverted()@butt).z*-1
visible_fraction=(depth_tip-data.clip_start)/(depth_tip-depth_butt)
assert .40<visible_fraction<.65,(depth_tip,depth_butt,visible_fraction)
# Keep original materials and original full geometry. Downsample this new cue's maps to 2K.
texture_report=[]
TEX=OUT/'Textures/Cue';TEX.mkdir(exist_ok=True)
for mat in obj.data.materials:
    for n in mat.node_tree.nodes:
        if n.type=='TEX_IMAGE' and n.image:
            im=n.image
            if max(im.size)>2048:im.scale(2048,2048)
            im.filepath_raw=str(TEX/(Path(im.name).stem+'.png'));im.file_format='PNG';im.save();im.pack()
            texture_report.append({'name':im.name,'resolution':list(im.size),'packed':bool(im.packed_file)})
report={'passed':True,'source_model':str(OUT/'Source/magic staff 3d model.glb'),'full_model_retained':True,
        'cue_length_m':(tip-butt).length,'tip_world_m':list(tip),'butt_world_m':list(butt),
        'anchors':['CueTip','CueButt','CueMidpoint'],'elevation_degrees':angle,
        'table_intersections_at_selected_elevation':0,'elevations_tested':tested,
        'camera':camera.name,'camera_position_m':list(camera.location),'visible_axis_fraction':visible_fraction,
        'front_half_visible_by_camera_composition':True,'geometry_not_cut_in_half':True,
        'new_model_triangles':sum(len(f.vertices)-2 for f in obj.data.polygons),'textures':texture_report,
        'blender_aim_preview_only':True,'unity_runtime_cue_not_replaced_yet':True}
(OUT/'cue_rig_checks.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
s.camera=camera
print(json.dumps({k:report[k] for k in ['passed','cue_length_m','elevation_degrees','table_intersections_at_selected_elevation',
      'visible_axis_fraction','full_model_retained','new_model_triangles','unity_runtime_cue_not_replaced_yet']}))
