"""Export evaluated geometry with explicit axis conversion and loop UV/normals.
Live Blender scene, objects, selections and the saved authoring file are not changed.
"""
import bpy,json,math,hashlib
from pathlib import Path
from mathutils import Vector

ROOT=Path(r'C:\Users\L\Desktop\破壳BullBear Labs')
OUT=ROOT/'deliverables/Pool3D-TableReplacement-Prep'
source=ROOT/'art-source/luxury_pool_table/20261029_cleanup_sizing/luxury_pool_table_clean_metric.blend'
assert Path(bpy.data.filepath)==source and not bpy.data.is_dirty, 'Save or inspect user edits before exporting'
body=bpy.data.objects['Chinese8 | luxury body']
objects=[body]+[o for name in ['Chinese8 | calibrated playing assembly','Pocket Collectors | corrected'] for o in bpy.data.collections[name].objects if o.type=='MESH']
assert len(objects)==50
materials=[];material_ids={};meshes=[];discarded=0
body_only=bool(globals().get('REFRESH_BODY_ONLY',False))
previous=None
if body_only:
    previous=json.loads((OUT/'UnityProject/Assets/TableReplacement/Source/TableMesh.json').read_text(encoding='utf-8'))
    assert len(previous['meshes'])==50
    materials=previous['materials'];material_ids={m['name']:i for i,m in enumerate(materials)}
    objects=[body]
for obj in objects:
    me=obj.data;me.calc_loop_triangles()
    normal_matrix=obj.matrix_world.to_3x3().inverted().transposed()
    for mat in me.materials:
        if mat.name in material_ids:continue
        shader=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
        color=list(shader.inputs['Base Color'].default_value)
        materials.append({'name':mat.name,'color':color,'metallic':float(shader.inputs['Metallic'].default_value),'smoothness':1-float(shader.inputs['Roughness'].default_value),'atlas':any(n.type=='TEX_IMAGE' for n in mat.node_tree.nodes),'cloth':mat.name=='Chinese8 | level unmarked green cloth'})
        material_ids[mat.name]=len(materials)-1
    vertices=[];normals=[];uvs=[];lookup={};groups={}
    for triangle in me.loop_triangles:
        wp=[obj.matrix_world@me.vertices[i].co for i in triangle.vertices]
        if (wp[1]-wp[0]).cross(wp[2]-wp[0]).length<2e-14:
            discarded+=1;continue
        indices=[]
        for vi,li in zip(triangle.vertices,triangle.loops):
            p=obj.matrix_world@me.vertices[vi].co
            n=(normal_matrix@me.corner_normals[li].vector).normalized()
            uv=me.uv_layers.active.data[li].uv if me.uv_layers.active else Vector((0,0))
            # One metre, cloth-centred, project-aligned. Existing environment root scales 100.
            position=(p.y,p.z-.800,p.x);normal=(n.y,n.z,n.x)
            key=tuple(round(float(v),8) for v in (*position,*normal,*uv))
            if key not in lookup:
                lookup[key]=len(vertices)
                vertices.append(dict(zip('xyz',position)));normals.append(dict(zip('xyz',normal)));uvs.append(dict(zip('xy',uv)))
            indices.append(lookup[key])
        mat_id=material_ids[me.materials[triangle.material_index].name]
        groups.setdefault(mat_id,[]).extend(indices)
    role='body'
    if 'cushion ' in obj.name:role='cushion'
    elif 'level cloth with' in obj.name:role='cloth'
    elif 'lining' in obj.name:role='lining'
    elif obj!=body:role='collector'
    meshes.append({'name':obj.name.replace('|','-'),'role':role,'vertices':vertices,'normals':normals,'uv':uvs,'groups':[{'material':m,'indices':v} for m,v in groups.items()]})
image=next(n.image for n in body.data.materials[0].node_tree.nodes if n.type=='TEX_IMAGE' and n.image)
assert image.packed_file, 'Original body atlas must remain packed; pixel loading is not required'
texture=OUT/'UnityProject/Assets/TableReplacement/Textures/BaseColor.jpg'
texture.write_bytes(image.packed_file.data)
payload={'schema':1,'sourceSha256':hashlib.sha256(source.read_bytes()).hexdigest(),'units':'metres; unity XYZ = blender Y, Z-0.800, X','materials':materials,'meshes':meshes}
if body_only:
    assert len(meshes)==1 and previous['meshes'][0]['role']=='body'
    payload['meshes']=meshes+previous['meshes'][1:]
    meshes=payload['meshes']
target=OUT/'UnityProject/Assets/TableReplacement/Source/TableMesh.json'
target.write_text(json.dumps(payload,ensure_ascii=False,separators=(',',':'),allow_nan=False),encoding='utf-8')
print(json.dumps({'objects':len(meshes),'materials':len(materials),'mesh_json_bytes':target.stat().st_size,'triangles':sum(len(g['indices'])//3 for m in meshes for g in m['groups']),'discarded_degenerate_triangles':discarded,'scene_modified':False},ensure_ascii=False))
