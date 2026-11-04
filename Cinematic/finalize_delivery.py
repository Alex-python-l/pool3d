import json,sys,subprocess,hashlib,shutil,re,time
from pathlib import Path
from PIL import Image
OUT=Path(__file__).parent;ROOT=OUT.resolve().parents[2]
sys.path.insert(0,str(ROOT/'tools/promo_video_deps'))
import imageio_ffmpeg
status=json.loads((OUT/'final_status.json').read_text())
assert status.get('complete') and status['completed']==528,status
assert json.loads((OUT/'preview_review.json').read_text())['passed']
frames=sorted((OUT/'final').glob('*.png'));assert len(frames)==528
for i,p in enumerate(frames,1):
    assert p.name==f'{i:04}.png'
    with Image.open(p) as im:
        assert im.size==(1920,804);im.verify()
subprocess.run([sys.executable,str(OUT/'finish_video.py'),'final'],check=True)
ff=imageio_ffmpeg.get_ffmpeg_exe();video=OUT/'break_promo_1080p.mp4'
decode=subprocess.run([ff,'-v','error','-i',str(video),'-f','null','-'],capture_output=True,text=True)
assert decode.returncode==0 and not decode.stderr.strip(),decode.stderr
media=(OUT/'final_media_probe.txt').read_text(encoding='utf-8')
assert '1920x1080' in media and '24 fps' in media and '00:00:22.00' in media
checks=json.loads((OUT/'project_checks.json').read_text());assert checks['passed']
source=Path(json.loads((OUT/'scene_inventory.json').read_text(encoding='utf-8'))['original'])
assert hashlib.sha256(source.read_bytes()).hexdigest()==checks['source_file_sha256'],'Original source changed during rendering'
dest=ROOT/'deliverables/PoolHall-Break-Trailer-20261102';dest.mkdir(exist_ok=True)
for f in ['break_promo_1080p.mp4','break_preview_640.mp4','shot_list.json','production_notes.txt','cue_design.json']:
    shutil.copy2(OUT/f,dest/f)
shutil.copy2(OUT/'final/0036.png',dest/'poster.png')
record={'complete':True,'video':str(dest/'break_promo_1080p.mp4'),'resolution':[1920,1080],'active_picture':[1920,804],'fps':24,'duration_seconds':22,'independently_rendered_frames':528,'engine':'Blender Cycles OptiX','samples':32,'audio':'AAC 48 kHz stereo, original synthesis','full_decode_passed':True,'all_png_crc_checks_passed':True,'original_source_unchanged':True,'original_meshes_unchanged':106,'new_cue_only_in_strike_shot':True,'project':str(OUT/'break_promo_working.blend'),'backup':str(OUT/'original_scene_backup.blend'),'video_sha256':hashlib.sha256(video.read_bytes()).hexdigest(),'video_bytes':video.stat().st_size}
(OUT/'delivery_checks.json').write_text(json.dumps(record,ensure_ascii=False,indent=2),encoding='utf-8')
shutil.copy2(OUT/'delivery_checks.json',dest/'delivery_checks.json')
print(json.dumps(record,ensure_ascii=False))
