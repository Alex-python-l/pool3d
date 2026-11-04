import json,numpy as np
from pathlib import Path
OUT=Path(__file__).parent
a=np.array(json.loads((OUT/'motion_240fps.json').read_text()))
r=.028575;mins=[];bad=[]
for k,p in enumerate(a[:,:,:3]):
    ds=np.linalg.norm(p[:,None]-p[None,:],axis=2);ds+=np.eye(16)*99
    mins.append(ds.min())
    if ds.min()<2*r-.002:bad.append(k)
disp=np.linalg.norm(a[-1,:,:2]-a[0,:,:2],axis=1)
hit=np.flatnonzero(np.linalg.norm(a[:,1,:2]-a[0,1,:2],axis=1)>.001)
report={'simulator':'Blender Bullet','fps':240,'frames':len(a),'substeps_per_frame':48,'solver_iterations':100,
        'all_target_balls_moved_over_3mm':bool(np.all(disp[1:]>.003)),
        'target_balls_moved_over_10cm':int(np.sum(disp[1:]>.1)),
        'first_rack_movement_sim_frame':int(hit[0]+1),
        'max_ball_height_m':float(a[:,:,2].max()),'min_ball_height_m':float(a[:,:,2].min()),
        'min_ball_center_separation_m':float(min(mins)),
        'severe_overlap_frames_over_2mm':bad,
        'balls_pocketed':np.where(a[-1,:,2]<.75)[0].tolist(),
        'no_ball_outside_table':bool(np.all(np.abs(a[:,:,0])<1.37) and np.all(np.abs(a[:,:,1])<.73)),
        'final_displacement_m':disp.tolist()}
(OUT/'motion_checks.json').write_text(json.dumps(report,indent=2));print(json.dumps(report))
