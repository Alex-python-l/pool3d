"""Read exported Unity-aligned mesh rings; produce the actual 2-D nose collision contour.
No Shapely, gameplay tick or reauthored approximation of the mesh is needed.
"""
from pathlib import Path
import json,math,struct,hashlib

ROOT=Path(__file__).resolve().parent.parent
data=json.loads((ROOT/'UnityProject/Assets/TableReplacement/Source/TableMesh.json').read_text(encoding='utf-8'))
main=[];extra=[];rings=[]
for mesh in (m for m in data['meshes'] if m['role']=='cushion'):
    # Mesh is split at UV/normal seams. Weld only identical samples of the nose-height ring.
    points=sorted({(round(p['z']*1000,5),round(p['x']*1000,5)) for p in mesh['vertices'] if abs(p['y']-.036)<1e-6})
    center=(sum(p[0] for p in points)/len(points),sum(p[1] for p in points)/len(points))
    points.sort(key=lambda p:math.atan2(p[1]-center[1],p[0]-center[0]))
    # Convex footprint: remove collinear intermediate samples without changing the shape.
    for _ in range(3):
        keep=[]
        for i,b in enumerate(points):
            a,c=points[i-1],points[(i+1)%len(points)]
            cross=(b[0]-a[0])*(c[1]-b[1])-(b[1]-a[1])*(c[0]-b[0])
            if abs(cross)>1e-4:keep.append(b)
        points=keep
    nose=[];remainder=[]
    for a,b in zip(points,points[1:]+points[:1]):
        segment=[*a,*b]
        on_nose=(abs(a[0]-b[0])<.001 and abs(abs(a[0])-1270)<.001) or (abs(a[1]-b[1])<.001 and abs(abs(a[1])-635)<.001)
        (nose if on_nose else remainder).append(segment)
    assert len(nose)==1,(mesh['name'],nose)
    main+=nose;extra+=remainder;rings.append({'mesh':mesh['name'],'points':points})
main.sort(key=lambda s:(round((s[1]+s[3])/2),round((s[0]+s[2])/2)))
assert len(main)==6
outer=[[-1500,865,1500,865],[1500,865,1500,-865],[1500,-865,-1500,-865],[-1500,-865,-1500,865]]
blockers=[[round(v,5) for v in segment] for segment in outer+main+extra]
centres=[[-1305,660],[0,665],[1305,660],[1305,-660],[0,-665],[-1305,-660]]
holes=[[*p,(53 if i in [1,4] else 51.5)-28.575] for i,p in enumerate(centres)]
end_y=max(abs(s[k]) for s in main if abs(s[0]-s[2])<.001 for k in [1,3])
end_x=max(abs(s[k]) for s in main if abs(s[1]-s[3])<.001 for k in [0,2])
mouth=min(abs(s[k]) for s in main if abs(s[1]-s[3])<.001 for k in [0,2])
profile={'schemaVersion':1,'geometryId':'luxury-chinese8-20261030-r3','longSide':2540,'shortSide':1270,'ballRadius':28.575,'metresPerUnit':.001,
    'cueStartX':-900,'rackX':635,'headStringX':-635,'respottedObjectX':635,'outsideCorners':[-1500,1500,865,-865],
    'insideCorners':[-1270,1270,635,-635],'freeCorners':[-1270,-635,635,-635],
    'insideBorder':[end_y,-end_y,-end_x,-mouth,mouth,end_x],'blockers':[v for s in blockers for v in s],'holes':[v for h in holes for v in h]}
# Hash IEEE754 single-precision values, matching Unity float storage and little-endian CPUs.
fields=['longSide','shortSide','ballRadius','metresPerUnit','cueStartX','rackX','headStringX','respottedObjectX','outsideCorners','insideCorners','freeCorners','insideBorder','blockers','holes']
values=[v for key in fields for v in (profile[key] if isinstance(profile[key],list) else [profile[key]])]
digest=hashlib.sha256(struct.pack('<'+'f'*len(values),*values)).hexdigest()
(ROOT/'UnityProject/Assets/StreamingAssets/TableGeometry.json').write_text(json.dumps(profile,indent=2),encoding='utf-8')
(ROOT/'Docs/replacement_geometry.json').write_text(json.dumps({'geometryId':profile['geometryId'],'floatPayloadSha256':digest,'blockerCount':len(blockers),'rings':rings,'mainNoses':main,'pockets':holes,'capturePolicy':'Actual slate opening radius minus 28.575 mm ball radius. No second radius is added by the original hole intersection formula.','meshSourceSha256':hashlib.sha256((ROOT/'UnityProject/Assets/TableReplacement/Source/TableMesh.json').read_bytes()).hexdigest(),'playMode':False},indent=2),encoding='utf-8')
print(json.dumps({'cushions':6,'blockerCount':len(blockers),'canonicalFloatSha256':digest,'captureRadiusMm':[h[2] for h in holes]}))
