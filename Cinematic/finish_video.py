import sys,json,wave,subprocess,math
from pathlib import Path
import numpy as np
ROOT=Path(__file__).resolve().parents[3]
sys.path.insert(0,str(ROOT/'tools/promo_video_deps'))
import imageio_ffmpeg
OUT=Path(__file__).parent
mode=sys.argv[1] if len(sys.argv)>1 else 'preview'
sr=48000;duration=22.;N=int(sr*duration);rng=np.random.default_rng(20261102)
t=np.arange(N)/sr;audio=np.zeros((N,2),np.float64)
def add(start,signal,level=1,pan=0):
    k=int(start*sr)
    if k<0:return
    n=min(len(signal),N-k)
    if n<=0:return
    audio[k:k+n,0]+=signal[:n]*level*np.sqrt((1-pan)/2)
    audio[k:k+n,1]+=signal[:n]*level*np.sqrt((1+pan)/2)
def impact(start,amp=.3,low=False,pan=0):
    q=np.arange(int(sr*(.45 if low else .12)))/sr
    sound=(np.sin(2*np.pi*(93 if low else 1900)*q)+.45*np.sin(2*np.pi*3100*q))*.5*np.exp(-q/(.10 if low else .012))
    sound+=rng.normal(0,.3,len(q))*np.exp(-q/.006)
    add(start,sound,amp,pan)
# Original synthesized underscore and foley. No third-party recordings/music.
env=np.minimum(t/2,1)*np.minimum((duration-t)/1.6,1)
for freq,amp in [(55,.025),(82.4069,.020),(110,.008),(164.8138,.008)]:
    p=(np.sin(2*np.pi*freq*t)+.12*np.sin(2*np.pi*(freq+.12)*t))*(.8+.2*np.sin(2*np.pi*.11*t))*env*amp
    audio[:,0]+=p;audio[:,1]+=p*.97
# Restrained rise, cue-tip transient, then a slowed break accent.
q=np.arange(int(1.75*sr))/sr
riser=rng.normal(0,1,len(q));riser=np.convolve(riser,np.ones(24)/24,mode='same')*(q/1.75)**2*.10
add(8.15,riser)
impact(207.5/24,.5,False,-.12)
motion=np.array(json.loads((OUT/'motion_240fps.json').read_text()))
hit=int(np.flatnonzero(np.linalg.norm(motion[:,1,:2]-motion[0,1,:2],axis=1)>.001)[0]+1)
hitframe=217+(hit-33)/.6
impact((hitframe-1)/24,.42,True,.08);impact((hitframe-1)/24,.55,False,.08)
# Secondary clacks from the same simulated trajectory, retimed with the image.
vel=np.diff(motion[:,:,:3],axis=0)*240
acc=np.linalg.norm(np.diff(vel,axis=0),axis=2)
events=[];last=-1
for k in range(80,min(2150,len(acc))):
    strength=float(acc[k].max())
    if strength<.65 or k-last<12:continue
    sim=k+2
    if sim<76:continue
    f=289+(sim-76)/10 if sim<=1026 else (385+(sim-1026)/8 if sim<=1594 else 457+(sim-1594)/8)
    sec=(f-1)/24
    if sec>21:continue
    i=int(acc[k].argmax());pan=float(np.clip(motion[k,i,1],-.65,.65))
    impact(sec,min(.24,.04+strength*.018),False,pan);last=k
    events.append({'time':sec,'ball':i,'strength':strength})
audio*=np.minimum(t/.3,1)[:,None]*np.minimum((duration-t)/1,1)[:,None]
audio=np.tanh(audio*1.25)*.90
pcm=(np.clip(audio,-1,1)*32767).astype('<i2')
wav=OUT/'original_trailer_soundtrack.wav'
with wave.open(str(wav),'wb') as w:w.setnchannels(2);w.setsampwidth(2);w.setframerate(sr);w.writeframes(pcm.tobytes())
(OUT/'audio_cue_sheet.json').write_text(json.dumps({'original_synthesis':True,'cue_strike_seconds':207.5/24,'rack_impact_seconds':(hitframe-1)/24,'secondary_contacts':events},indent=2))
ff=imageio_ffmpeg.get_ffmpeg_exe()
if mode=='preview':
    files=sorted((OUT/'preview').glob('*.png'));assert len(files)==264,len(files)
    concat=OUT/'preview_frames.txt'
    concat.write_text(''.join("file '"+str(p).replace('\\','/')+"'\nduration 0.083333333333\n" for p in files)+"file '"+str(files[-1]).replace('\\','/')+"'\n",encoding='utf-8')
    inp=['-f','concat','-safe','0','-i',str(concat)]
    vf='pad=640:360:0:46:black,setsar=1,fade=t=in:st=0:d=0.6,fade=t=out:st=21:d=1'
    target=OUT/'break_preview_640.mp4'
else:
    files=sorted((OUT/'final').glob('*.png'));assert len(files)==528,len(files)
    inp=['-framerate','24','-start_number','1','-i',str(OUT/'final/%04d.png')]
    vf='pad=1920:1080:0:138:black,setsar=1,fade=t=in:st=0:d=0.6,fade=t=out:st=21:d=1'
    target=OUT/'break_promo_1080p.mp4'
vf='scale=iw:ih:out_color_matrix=bt709:out_range=tv,'+vf
subprocess.run([ff,'-y',*inp,'-i',str(wav),'-vf',vf,'-r','24','-t','22','-c:v','libx264','-preset','medium','-crf','18','-pix_fmt','yuv420p','-colorspace','bt709','-color_primaries','bt709','-color_trc','iec61966-2-1','-c:a','aac','-b:a','192k','-movflags','+faststart',str(target)],check=True)
probe=subprocess.run([ff,'-hide_banner','-i',str(target)],capture_output=True,text=True,encoding='utf-8',errors='replace')
(OUT/f'{mode}_media_probe.txt').write_text(probe.stderr,encoding='utf-8')
print(json.dumps({'video':str(target),'bytes':target.stat().st_size,'frames':len(files),'duration':22}))
