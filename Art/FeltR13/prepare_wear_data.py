"""Metric full-bed nap / wear data and tangent normals, not an edit of the reference images."""
from pathlib import Path
import json,hashlib
import numpy as np
from PIL import Image,ImageDraw
from scipy.ndimage import gaussian_filter
OUT=Path(__file__).resolve().parent;TEX=OUT/'Textures';TEX.mkdir(exist_ok=True)
rng=np.random.default_rng(131102);H,W=1024,2048
def noise(sigma):
    a=gaussian_filter(rng.normal(size=(H,W)).astype(np.float32),sigma,mode='reflect');return a/(a.std()+1e-8)
field=.5+.095*noise((19,27))+.07*noise((70,95))
nap=np.clip(.5+.19*noise((14,40))+.08*noise((58,38)),.04,.96)
spots=Image.new('L',(W,H),255);draw=ImageDraw.Draw(spots);descriptors=[]
for i in range(47):
    x,y=rng.uniform(.05,.95,2);rx=rng.uniform(.0025,.010)/2.70*W;ry=rx*rng.uniform(.55,1.35)
    val=int(rng.integers(90,205));draw.ellipse((x*W-rx,y*H-ry,x*W+rx,y*H+ry),fill=val)
    descriptors.append({'uv':[float(x),float(y)],'radius_x_mm':float(rx/W*2700),'strength':float(1-val/255)})
for i in range(125):
    x,y=rng.uniform(0,1,2);r=rng.uniform(.35,1.2)
    draw.ellipse((x*W-r,y*H-r,x*W+r,y*H+r),fill=int(rng.integers(120,215)))
wear=gaussian_filter(np.asarray(spots,dtype=np.float32)/255,.65)
brush=Image.new('L',(W,H),128);d=ImageDraw.Draw(brush)
for i in range(9):
    x,y=rng.uniform(-.1,.7,2);a=rng.uniform(.12,.38);b=rng.uniform(.10,.32)
    d.arc((x*W,y*H,(x+a)*W,(y+b)*H),start=int(rng.integers(20,130)),end=int(rng.integers(180,280)),fill=int(rng.integers(95,117)),width=int(rng.integers(2,5)))
field+=((gaussian_filter(np.asarray(brush,dtype=np.float32)/255,2)-128/255)*1.25)
field=np.clip(field,.03,.97)
data=np.stack((field,nap,wear,np.ones_like(field)),axis=-1)
Image.fromarray(np.rint(data*255).astype(np.uint8)).save(TEX/'FeltR13_FullBed_NapWear.png')

packed=np.asarray(Image.open(TEX/'FeltR13_FibrePacked.png'),dtype=np.float32)/255
height=packed[:,:,2];span=.000075;tile=.064;pitch=tile/height.shape[1]
dx=(np.roll(height,-1,1)-np.roll(height,1,1))*span/(2*pitch)
dy=(np.roll(height,-1,0)-np.roll(height,1,0))*span/(2*pitch)
n=np.stack((-dx,dy,np.ones_like(dx)),axis=-1);n/=np.linalg.norm(n,axis=-1,keepdims=True)
encoded=np.rint((n*.5+.5)*255).clip(0,255).astype(np.uint8);encoded[-1]=encoded[0];encoded[:,-1]=encoded[:,0]
Image.fromarray(encoded).save(TEX/'FeltR13_NormalGL.png')
files=[]
for p in sorted(TEX.glob('*.png')):
    with Image.open(p) as im:res=list(im.size)
    files.append({'file':p.name,'resolution':res,'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'bytes':p.stat().st_size})
spec={'revision':'R13','reference_images':['7ad642868466b6208a5a1892734e941.jpg','7a4f74fbbdc39c220a9f25d1a7f11d7.jpg'],
      'albedo_physical_tile_m':.25,'albedo_linear_scale':.42,'micro_tile_m':tile,'micro_height_span_m':span,
      'full_bed_domain_m':[-1.35,-.71,2.70,1.42],'full_bed_dye_multiplier':[.6,.8],
      'mean_roughness':.87,'sheen_weight_range':[.10,.22],'specular_ior_level':.11,
      'sheet_thickness_m':.00135,'nominal_ball_contact_plane_m':.800,'uncompressed_visual_nap_top_m':.80018,
      'edge_nap_height_m':[.00014,.00032],'contact_nap_height_m':[.00012,.00029],
      'no_ball_transform_change':True,'flat_physics_surface_preserved':True,
      'compressed_spots':descriptors,'macro_dye_std':float(field.std()),'files':files,
      'normal_convention':'OpenGL +Y up; PNG row-down sign compensated',
      'ai_asset':'FeltR13_ReferenceNap_Albedo.png, built-in imagegen, two user photos as material references',
      'preview_fibres_excluded_from_static_environment':True,'extra_realtime_lights':0}
(OUT/'felt_spec.json').write_text(json.dumps(spec,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'files':len(files),'albedo_resolution':list(Image.open(TEX/'FeltR13_ReferenceNap_Albedo.png').size),'macro_std':float(field.std()),'cloth_mm':1.35,'visual_nap_offset_mm':.18}))
