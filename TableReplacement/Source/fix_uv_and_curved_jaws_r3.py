"""Start from pre-R2 authoring. Remap upper UVs, trim old protrusions and curve jaw ends.
No new rectangular wood frame or replacement cream caps are constructed.
"""
import bpy,bmesh,json,math,hashlib
from pathlib import Path
from collections import Counter
from mathutils import Vector
from mathutils.geometry import tessellate_polygon
from mathutils.bvhtree import BVHTree

OUT=Path(r'C:\Users\L\Desktop\破壳BullBear Labs\art-source\luxury_pool_table\20261030_uv_curve_fix')
SOURCE=OUT.parent/'20261029_cleanup_sizing/luxury_pool_table_clean_metric.blend'
body=bpy.data.objects['Chinese8 | luxury body']
assert not body.get('rectangular_rails_r2') and not body.get('uv_curve_r3')
assert len(body.data.polygons)==14493
original_mesh=body.data
def signature(mesh,select):
    uv=mesh.uv_layers.active.data
    rows=[]
    for f in mesh.polygons:
        if not select(f):continue
        vertices=[tuple(round(float(x),7) for x in mesh.vertices[mesh.loops[li].vertex_index].co)+tuple(round(float(x),7) for x in uv[li].uv) for li in f.loop_indices]
        rows.append((tuple(vertices),f.material_index,f.use_smooth))
    return hashlib.sha256(repr(sorted(rows)).encode()).hexdigest(),len(rows)
ivory_id=next(i for i,m in enumerate(original_mesh.materials) if 'ivory leather' in m.name)
cloth_id=next(i for i,m in enumerate(original_mesh.materials) if m.name=='Chinese8 | level unmarked green cloth')
ivory_before=signature(original_mesh,lambda f:f.material_index==ivory_id)
lower_before=signature(original_mesh,lambda f:max(original_mesh.vertices[k].co.z for k in f.vertices)<=.790)

# Retain the existing corner rounding and straight noses. Only the formerly straight jaw
# connectors are resampled as smooth curves, with zero slope displacement at each endpoint.
cushions=sorted((o for o in bpy.data.objects if o.type=='MESH' and 'cushion ' in o.name),key=lambda o:o.name)
curves=[]
for obj in cushions:
    me=obj.data;assert len(me.vertices)==108
    ring=[me.vertices[i].co.copy() for i in range(36,72)]
    def local(p):return Vector((p.x,abs(p.y)-.635)) if 'Long' in obj.name else Vector((p.y,abs(p.x)-1.270))
    old=[local(p) for p in ring]
    # Both tangential end boundaries in the CCW local four-corner outline.
    new=[];curve_info=[]
    for i,p in enumerate(old):
        new.append(p)
        if i not in [17,35]:continue
        q=old[(i+1)%len(old)];d=q-p;normal=Vector((-d.y,d.x)).normalized()
        sag=.009 if abs(d.x)>.04 else .0045
        for j in range(1,33):
            t=j/33
            new.append(p.lerp(q,t)+normal*(sag*math.sin(math.pi*t)**2))
        curve_info.append({'from':list(p),'to':list(q),'max_inward_sag_m':sag,'segments':33})
    width=.063 if 'Long' in obj.name else .075
    sign=1 if obj.name.endswith('+1') else -1
    def world(p,z,shift):
        depth=p.y+shift*(1-p.y/width)
        return (p.x,sign*(.635+depth),z) if 'Long' in obj.name else (sign*(1.270+depth),p.x,z)
    vertices=[world(p,z,shift) for z,shift in [(.800,.018),(.836,0),(.842,.010)] for p in new]
    faces=[];n=len(new)
    for layer in range(2):
        for j in range(n):faces.append((layer*n+j,layer*n+(j+1)%n,(layer+1)*n+(j+1)%n,(layer+1)*n+j))
    # Curved jaw outlines can be concave: triangulate the caps instead of a fan.
    tris=tessellate_polygon([[Vector((p.x,p.y,0)) for p in new]])
    faces.extend(tuple(reversed(t)) for t in tris);faces.extend(tuple(2*n+i for i in t) for t in tris)
    mesh=bpy.data.meshes.new(obj.name+' | curved jaw ends')
    mesh.from_pydata(vertices,[],faces);mesh.materials.append(me.materials[0]);mesh.update()
    bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free()
    uv=mesh.uv_layers.new(name='UV metres')
    for f in mesh.polygons:
        for li in f.loop_indices:
            p=mesh.vertices[mesh.loops[li].vertex_index].co;uv.data[li].uv=(p.x,p.y)
    obj.data=mesh
    curves.append({'mesh':obj.name,'connectors':curve_info,'nose_xy':[list(Vector(world(p,.836,0)).xy) for p in new]})

# Expanded convex envelope hides only obsolete scan fragments that protrude into the newly
# authored cloth. It does not reshape the woodwork outside this boundary.
def cross(a,b):return a.x*b.y-a.y*b.x
def hull(points):
    points=sorted({(round(p.x,8),round(p.y,8)) for p in points})
    chains=[]
    for sequence in [points,list(reversed(points))]:
        chain=[]
        for p in sequence:
            p=Vector(p)
            while len(chain)>1 and cross(chain[-1]-chain[-2],p-chain[-1])<=1e-10:chain.pop()
            chain.append(p)
        chains.append(chain[:-1])
    return chains[0]+chains[1]
def expanded(poly,amount):
    result=[]
    for i,b in enumerate(poly):
        d1=(b-poly[i-1]).normalized();d2=(poly[(i+1)%len(poly)]-b).normalized()
        p=b+Vector((d1.y,-d1.x))*amount;q=b+Vector((d2.y,-d2.x))*amount
        det=cross(d1,d2);result.append(p+d1*(cross(q-p,d2)/det) if abs(det)>1e-8 else (p+q)*.5)
    return result
cutters=[expanded(hull([v.co.xy for v in o.data.vertices]),.0004) for o in cushions]

def partition(poly,distance):
    inside=[];outside=[]
    for i,(a,au) in enumerate(poly):
        b,bu=poly[(i+1)%len(poly)];da,db=distance(a),distance(b)
        if da>=-1e-10:inside.append((a,au))
        if da<=1e-10:outside.append((a,au))
        if da>1e-10 and db< -1e-10 or da< -1e-10 and db>1e-10:
            t=da/(da-db);pair=(a.lerp(b,t),au.lerp(bu,t));inside.append(pair);outside.append(pair)
    def valid(poly):
        clean=[]
        for item in poly:
            if not clean or (item[0]-clean[-1][0]).length>1e-9:clean.append(item)
        if len(clean)>1 and (clean[0][0]-clean[-1][0]).length<1e-9:clean.pop()
        return clean if len(clean)>=3 else []
    return valid(inside),valid(outside)
def subtract(poly,cutter):
    if (max(p.x for p,u in poly)<min(p.x for p in cutter) or min(p.x for p,u in poly)>max(p.x for p in cutter) or
        max(p.y for p,u in poly)<min(p.y for p in cutter) or min(p.y for p,u in poly)>max(p.y for p in cutter)):return [poly]
    above,below=partition(poly,lambda p:p.z-.795)
    outside=[below] if below else [];remainder=above
    for a,b in zip(cutter,cutter[1:]+cutter[:1]):
        if not remainder:break
        edge=b-a
        remainder,part=partition(remainder,lambda p:cross(edge,p.xy-a))
        if part:outside.append(part)
    return outside

source_uv=original_mesh.uv_layers.active.data
records=[];affected=0
for f in original_mesh.polygons:
    polygon=[(original_mesh.vertices[original_mesh.loops[li].vertex_index].co.copy(),source_uv[li].uv.copy()) for li in f.loop_indices]
    if f.material_index!=0 or max(p.z for p,u in polygon)<=.790:
        records.append((polygon,f.material_index,f.use_smooth,False));continue
    affected+=1
    # Split seams on the projected material boundary. No original corner is moved.
    for triangle in tessellate_polygon([[p for p,u in polygon]]):
        pieces=[[polygon[i] for i in triangle]]
        for cutter in cutters:pieces=[part for piece in pieces for part in subtract(piece,cutter)]
        for piece in pieces:
            upper,lower=partition(piece,lambda p:p.z-.790)
            if lower:records.append((lower,0,f.use_smooth,False))
            if not upper:continue
            zones=[upper]
            for axis,plane in [(0,-1.345),(0,1.345),(1,-.698),(1,.698)]:
                split=[]
                for zone in zones:
                    a,b=partition(zone,lambda p:p[axis]-plane)
                    if a:split.append(a)
                    if b:split.append(b)
                zones=split
            for zone in zones:
                center=sum((p for p,u in zone),Vector())/len(zone)
                is_wood=abs(center.x)>=1.345-1e-7 or abs(center.y)>=.698-1e-7
                if is_wood:
                    if abs(center.y)>=.698-1e-7:
                        a,b=(-1.270,-.055) if center.x<0 else (.055,1.270)
                        def map_uv(p):return Vector((.450+.025*max(0,min(1,(abs(p.y)-.698)/.082)),.650+.140*max(0,min(1,(p.x-a)/(b-a)))))
                    else:
                        def map_uv(p):return Vector((.450+.025*max(0,min(1,(abs(p.x)-1.345)/.075)),.650+.140*max(0,min(1,(p.y+.60)/1.20))))
                    records.append(([(p,map_uv(p)) for p,u in zone],0,f.use_smooth,True))
                else:records.append((zone,cloth_id,f.use_smooth,True))

vertices=[];faces=[];uvs=[];materials=[];smooth=[];lookup={};remapped=0
for poly,mat,sm,edited in records:
    indices=[]
    for p,u in poly:
        key=tuple(round(float(x),8) for x in p)
        if key not in lookup:lookup[key]=len(vertices);vertices.append(tuple(p))
        indices.append(lookup[key])
    if len(set(indices))<3:continue
    faces.append(indices);uvs.append([tuple(u) for p,u in poly]);materials.append(mat);smooth.append(sm);remapped+=edited
mesh=bpy.data.meshes.new('Chinese8 | original frame with straight upper UV borders')
mesh.from_pydata(vertices,[],faces);mesh.update()
for m in original_mesh.materials:mesh.materials.append(m)
uv=mesh.uv_layers.new(name='Original frame with upper UV corrections')
for face,coords,mat,sm in zip(mesh.polygons,uvs,materials,smooth):
    face.material_index=mat;face.use_smooth=sm
    for li,value in zip(face.loop_indices,coords):uv.data[li].uv=value
bm=bmesh.new();bm.from_mesh(mesh)
degenerate=[f for f in bm.faces if f.calc_area()<1e-11]
if degenerate:bmesh.ops.delete(bm,geom=degenerate,context='FACES')
loose=[v for v in bm.verts if not v.link_faces]
if loose:bmesh.ops.delete(bm,geom=loose,context='VERTS')
for _ in range(8):
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
    bad=[e for e in bm.edges if e.is_manifold and not e.is_contiguous]
    if not bad:break
    bmesh.ops.split_edges(bm,edges=bad)
bm.to_mesh(mesh);bm.free();mesh.update();body.data=mesh
ivory_after=signature(mesh,lambda f:f.material_index==ivory_id)
lower_after=signature(mesh,lambda f:max(mesh.vertices[k].co.z for k in f.vertices)<=.790-1e-6)
# Loop order may change when normals are repaired; compare the original cream surface
# by material-tagged positions separately as well, without rebuilding a single cream cap.
def material_positions(me,mat):return {tuple(round(float(c),7) for c in me.vertices[i].co) for f in me.polygons if f.material_index==mat for i in f.vertices}
assert material_positions(mesh,ivory_id)==material_positions(original_mesh,ivory_id)
def tree(o):return BVHTree.FromPolygons([o.matrix_world@v.co for v in o.data.vertices],[list(f.vertices) for f in o.data.polygons],all_triangles=False)
bpy.context.view_layer.update();bt=tree(body)
overlaps={o.name:len(bt.overlap(tree(o))) for o in cushions}
assert not any(overlaps.values()),overlaps
body['uv_curve_r3']=True
report={'passed':True,'revision':'R3 original frame UV and curved pocket cloth','r2_rolled_back':True,
    'wood_frame_rebuilt':False,'cream_caps_rebuilt':False,'cream_cap_vertex_positions_unchanged':True,
    'upper_source_faces_processed':affected,'uv_material_fragments_remapped':remapped,
    'wood_rectangle_threshold_m':{'end_x':1.345,'side_y':.698},'upper_wood_uv_box':[.450,.650,.475,.790],
    'texture_white_remnants_removed_from_cloth':True,'cushion_end_curves':curves,
    'body_cushion_triangle_overlaps':overlaps,'main_nose_planes_mm':[2540,1270],
    'ivory_before':ivory_before,'ivory_after':ivory_after,'lower_before':lower_before,'lower_after':lower_after,
    'screenshots':False,'rendered':False}
(OUT/'uv_curve_fix_report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE))
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'luxury_pool_table_uv_curved_pockets_r3.blend'),copy=True)
print(json.dumps({'r2_rolled_back':True,'wood_frame_rebuilt':False,'caps_unchanged':True,'upper_uv_fragments':remapped,'curved_jaw_connectors':sum(len(c['connectors']) for c in curves),'body_cushion_overlaps':overlaps,'saved':str(SOURCE)},ensure_ascii=False))
