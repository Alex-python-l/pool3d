import bpy,json,time,traceback
from pathlib import Path
OUT=Path(r'C:\Users\L\Desktop\破壳BullBear Labs\art-source\luxury_pool_table\20261101_closed_room_balls_cue_r11')
s=bpy.context.scene;default_camera=bpy.data.objects['Shot | front-half cue camera']
prefs=bpy.context.preferences.addons['cycles'].preferences
assert 'OPTIX' in [t[0] for t in prefs.get_device_types(bpy.context)]
prefs.compute_device_type='OPTIX';prefs.get_devices()
for d in prefs.devices:d.use=d.type=='OPTIX'
assert any(d.use for d in prefs.devices)
s.render.engine='CYCLES';s.cycles.device='GPU';s.cycles.samples=96;s.cycles.use_denoising=True
s.cycles.use_adaptive_sampling=True;s.cycles.adaptive_threshold=.02
status=OUT/'render_status.json';status.write_text(json.dumps({'status':'queued'}),encoding='utf-8')
def render():
    started=time.time();views=[]
    try:
        for label,camera,res in [('closed_room',bpy.data.objects['Room | presentation camera'],(1800,1350)),
                                 ('shot_front_half',default_camera,(1600,1000))]:
            s.camera=camera;s.render.resolution_x,s.render.resolution_y=res;s.render.resolution_percentage=100
            s.render.filepath=str(OUT/(label+'.png'))
            status.write_text(json.dumps({'status':'rendering','view':label,'completed_views':len(views)}),encoding='utf-8')
            bpy.ops.render.render(write_still=True,scene=s.name)
            views.append({'file':s.render.filepath,'camera':camera.name,'resolution':list(res)})
        result={'status':'complete','views':views,'seconds':time.time()-started,'actual_blender_scene':True,
                'engine':'CYCLES','backend':'OPTIX','unity_runtime':False,'gameplay_executed':False}
    except BaseException as e:
        result={'status':'failed','error':str(e),'traceback':traceback.format_exc()}
    finally:
        s.camera=default_camera;s.render.resolution_x=1600;s.render.resolution_y=1000
        s.render.filepath=str(OUT/'shot_front_half.png')
    status.write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
    return None
bpy.app.timers.register(render,first_interval=1)
print(json.dumps({'queued':True,'views':['closed_room','shot_front_half'],'samples':96}))
