import sys,subprocess
from pathlib import Path
OUT=Path(__file__).parent
sys.path.insert(0,str(OUT.resolve().parents[2]/'tools/promo_video_deps'))
import imageio_ffmpeg
vf="select='eq(n,207)+eq(n,239)+eq(n,315)+eq(n,480)',scale=480:270,tile=2x2"
subprocess.run([imageio_ffmpeg.get_ffmpeg_exe(),'-v','error','-y','-i',str(OUT/'break_promo_1080p.mp4'),'-vf',vf,'-frames:v','1','-update','1',str(OUT/'final_encoded_contact_sheet.png')],check=True)
