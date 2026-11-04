"""Export a two-dimensional cushion footprint from actual imported V3 triangles.
Authoring-only: shapely + numpy. The Unity player consumes the resulting JSON.
"""
from pathlib import Path
import json
import sys
import numpy as np

DELIVERY = Path(__file__).resolve().parent.parent
WORKSPACE = DELIVERY.parents[1]
sys.path.insert(0, str(WORKSPACE / 'research/3DPoolBall/20261026_integrated_v1/_vendor'))
from shapely.geometry import Polygon, LineString, Point
from shapely.ops import unary_union


def footprint(mesh):
    vertices = np.array([[v['z']*1000, v['x']*1000] for v in mesh['vertices']])
    triangles = np.array(mesh['triangles']).reshape(-1, 3)
    faces = [Polygon(vertices[tri]) for tri in triangles]
    # Weld sub-micrometre import seams; this changes the envelope by <0.01 mm.
    return unary_union([p for p in faces if p.area > .00001]).buffer(.003).buffer(-.003)


def main():
    source = json.loads((DELIVERY / 'Docs/table_mesh_export.json').read_text())
    mesh = next(m for m in source['meshes'] if m['name'] == 'PoolSet_Bumper')
    original = footprint(mesh)
    assert original.geom_type == 'MultiPolygon' and len(original.geoms) == 6
    polygons = [p.simplify(.2, preserve_topology=True) for p in original.geoms]
    main_rails, extras = [], []
    for polygon in polygons:
        coords = list(polygon.exterior.coords)
        nose_points = []
        for a, b in zip(coords, coords[1:]):
            main_rail = (abs(abs(a[0])-1270) < .002 and abs(abs(b[0])-1270) < .002 and a[0]*b[0] > 0) or (abs(abs(a[1])-635) < .002 and abs(abs(b[1])-635) < .002 and a[1]*b[1] > 0)
            if main_rail:
                nose_points.extend((a, b))
            else:
                extras.append([*a, *b])
        pts = np.array(nose_points)
        assert len(pts) >= 2
        axis = int(np.argmax(np.ptp(pts, axis=0)))
        main_rails.append([*pts[np.argmin(pts[:, axis])], *pts[np.argmax(pts[:, axis])]])
    # Keep original blocker IDs 4..9 as six nose rails. Other envelope edges follow.
    main_rails.sort(key=lambda s: (round((s[1]+s[3])/2), round((s[0]+s[2])/2)))
    outside = [-1500, 1500, 865, -865]
    outer_segments = [[-1500,865,1500,865],[1500,865,1500,-865],[1500,-865,-1500,-865],[-1500,-865,-1500,865]]
    blockers = outer_segments + main_rails + extras
    blockers = [[round(float(v), 3) for v in s] for s in blockers]
    holes = []
    pocket_source = []
    for pocket in (m for m in source['meshes'] if m['name'].startswith('PoolSet_Pocket')):
        vertices = np.array([[v['z']*1000, v['x']*1000, v['y']*1000] for v in pocket['vertices']])
        lip = vertices[vertices[:, 2] > vertices[:, 2].max()-.02]
        lower, upper = lip.min(0), lip.max(0)
        centre = (lower+upper)/2
        opening_radius = min(upper[:2]-lower[:2])/2
        capture_radius = opening_radius - 28.575
        holes.append([round(float(centre[0]),3), round(float(centre[1]),3), round(float(capture_radius),3)])
        pocket_source.append({'mesh': pocket['name'], 'openingRadius': float(opening_radius), 'captureRadius': float(capture_radius)})
    # Preserve original pocket ordering: upper-left, upper-side, upper-right, lower-right, lower-side, lower-left.
    holes.sort(key=lambda h: (0 if h[1]>0 else 1, h[0] if h[1]>0 else -h[0]))
    profile = {'schemaVersion': 1, 'geometryId': 'v3-poolset-2540', 'longSide': 2540, 'shortSide': 1270, 'ballRadius': 28.575,
               'metresPerUnit': .001, 'cueStartX': -900, 'rackX': 635, 'headStringX': -635, 'respottedObjectX': 635,
               'outsideCorners': outside, 'insideCorners': [-1270,1270,635,-635], 'freeCorners': [-1270,-635,635,-635],
               'insideBorder': [543.278,-543.278,-1177.247,-78.483,78.483,1177.247],
               'blockers': [v for s in blockers for v in s], 'holes': [v for h in holes for v in h]}
    output = DELIVERY / 'UnityProject/Assets/StreamingAssets/TableGeometry.json'
    output.write_text(json.dumps(profile, indent=2), encoding='utf-8')
    physical = unary_union([LineString([s[:2], s[2:]]) for s in blockers[4:]])
    boundary = original.boundary
    samples = [Point(v['z']*1000, v['x']*1000) for v in mesh['vertices']]
    # Only silhouette vertices participate in this two-dimensional envelope check.
    silhouette_samples = [p for p in samples if boundary.distance(p)<.01]
    error = max(physical.distance(p) for p in silhouette_samples)
    assert error < 1, error
    report = {'source': 'PoolSet_Bumper imported mesh, projected complete cushion silhouette; not a three-dimensional nose model',
              'meshCount': len(source['meshes']), 'cushions': 6, 'blockerCount': len(blockers), 'noseHeightMetres': .0349505,
              'outlineSilhouetteSamples': len(silhouette_samples), 'maxEnvelopeErrorMillimetres': error,
              'pockets': pocket_source, 'capturePolicy': 'top opening inscribed radius minus ball radius; original circular capture retained',
              'gameplay': False, 'visualReview': False}
    (DELIVERY / 'Docs/geometry_mesh_report.json').write_text(json.dumps(report, indent=2), encoding='utf-8')
    print(json.dumps({k:report[k] for k in ('cushions','blockerCount','outlineSilhouetteSamples','maxEnvelopeErrorMillimetres')}))


if __name__ == '__main__': main()
