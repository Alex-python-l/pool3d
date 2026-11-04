import json,time,subprocess,sys,traceback
from pathlib import Path
OUT=Path(__file__).parent
while True:
    if (OUT/'STOP_final').exists():sys.exit(0)
    try:
        state=json.loads((OUT/'final_status.json').read_text())
        if 'error' in state:raise RuntimeError(state['error'])
        if state.get('complete'):
            with (OUT/'delivery_encoding.log').open('w',encoding='utf-8') as log:
                for mode in ['final','preview']:
                    subprocess.run([r'D:\Blender\blender.exe','-b',str(OUT/'break_promo_working.blend'),'-t','6','--python',str(OUT/'render_film.py'),'--',mode],stdout=log,stderr=subprocess.STDOUT,check=True)
                subprocess.run([sys.executable,str(OUT/'finish_video.py'),'preview'],stdout=log,stderr=subprocess.STDOUT,check=True)
                subprocess.run([sys.executable,str(OUT/'finalize_delivery.py')],stdout=log,stderr=subprocess.STDOUT,check=True)
            break
    except (FileNotFoundError,json.JSONDecodeError):pass
    except Exception:
        (OUT/'delivery_error.txt').write_text(traceback.format_exc());raise
    time.sleep(5)
