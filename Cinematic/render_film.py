import bpy,json,time,traceback,sys
from pathlib import Path
OUT=Path(__file__).parent
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else ['stills']
mode=args[0];s=bpy.data.scenes['PROMO | cinematic scene'];bpy.context.window.scene=s
prefs=bpy.context.preferences.addons['cycles'].preferences
prefs.compute_device_type='OPTIX';prefs.get_devices()
for d in prefs.devices:d.use=d.type=='OPTIX'
s.cycles.device='GPU';s.render.use_persistent_data=True
s.cycles.denoising_use_gpu=True
if mode=='stills':frames=[36,100,180,208,231,250,315,370,420,490];res=(960,402);samples=16
elif mode=='preview':frames=list(range(1,529,2));res=(640,268);samples=8
elif mode.startswith('benchmark'):frames=[36,231,315];res=(1920,804);samples=32
else:frames=list(range(1,529));res=(1920,804);samples=32
folder=OUT/mode;folder.mkdir(exist_ok=True)
s.render.resolution_x,s.render.resolution_y=res;s.render.resolution_percentage=100;s.cycles.samples=samples
s.render.image_settings.file_format='PNG';s.render.image_settings.color_mode='RGB'
start=time.time();count=0
try:
    for f in frames:
        if (OUT/f'STOP_{mode}').exists():break
        path=folder/f'{f:04}.png'
        if path.exists():count+=1;continue
        s.frame_set(f)
        # Deterministic camera selection independent of UI timeline marker evaluation.
        s.camera=next(m.camera for m in sorted(s.timeline_markers,key=lambda m:m.frame,reverse=True) if m.camera and m.frame<=f)
        s.render.filepath=str(path)
        bpy.ops.render.render(write_still=True,scene=s.name)
        count+=1
        (OUT/f'{mode}_status.json').write_text(json.dumps({'completed':count,'total':len(frames),'last_frame':f,'elapsed':time.time()-start}))
    (OUT/f'{mode}_status.json').write_text(json.dumps({'complete':True,'completed':count,'total':len(frames),'elapsed':time.time()-start,'resolution':res,'samples':samples}))
except Exception:
    (OUT/f'{mode}_status.json').write_text(json.dumps({'error':traceback.format_exc(),'completed':count}));raise
