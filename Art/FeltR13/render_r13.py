import bpy,json
from pathlib import Path
from mathutils import Vector
OUT=Path(r'C:\Users\L\Desktop\破壳BullBear Labs\art-source\luxury_pool_table\20261102_felt_depth_r13')
def detail(name,eye,at,lens):
    o=bpy.data.objects.get(name)
    if o is None:
        o=bpy.data.objects.new(name,bpy.data.cameras.new(name));bpy.context.scene.collection.objects.link(o)
    o.location=eye;o.rotation_euler=(Vector(at)-o.location).to_track_quat('-Z','Y').to_euler();o.data.lens=lens;o.data.clip_start=.003;o.data.clip_end=50
detail('Detail | R13 fabric wrap',(.94,-.41,1.10),(1.06,-.668,.843),60)
detail('Detail | R13 ball contact',(-.975,-.18,.882),(-.9,0,.818),55)
detail('Detail | R13 cut cloth edge',(.08,-.48,.845),(0,-.618,.800),65)
renderer=(OUT.parent/'20261102_tabletop_realism_r12/render_views.py').read_text(encoding='utf-8')
renderer=renderer.replace('20261102_tabletop_realism_r12','20261102_felt_depth_r13')
exec(compile(renderer,'render_views_r13','exec'))
