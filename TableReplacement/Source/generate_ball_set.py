"""V3 UV-compatible procedural full set; writes only Integrated-V1 ball textures."""
from pathlib import Path
import hashlib
import json
import numpy as np
from PIL import Image, ImageDraw, ImageFont

DELIVERY = Path(__file__).resolve().parent.parent
COLORS = {0: (239, 233, 215), 1: (231, 166, 8), 2: (24, 65, 155), 3: (181, 29, 32),
          4: (95, 42, 135), 5: (226, 96, 12), 6: (20, 102, 58), 7: (112, 28, 38), 8: (12, 15, 17)}


def label_mask(number):
    image = Image.new('L', (1024, 1024))
    draw = ImageDraw.Draw(image)
    size = 730
    font_path = Path('C:/Windows/Fonts/georgiab.ttf')
    font = ImageFont.truetype(str(font_path), size)
    while draw.textbbox((0, 0), str(number), font=font)[2] > 850:
        size -= 5
        font = ImageFont.truetype(str(font_path), size)
    box = draw.textbbox((0, 0), str(number), font=font)
    draw.text(((1024 - (box[2] - box[0])) / 2 - box[0], (1024 - (box[3] - box[1])) / 2 - box[1] - 10), str(number), fill=255, font=font)
    if number in (6, 9): draw.rounded_rectangle((390, 920, 634, 948), radius=10, fill=255)
    return np.asarray(image, dtype=np.float32) / 255


def make_arrays(size=2048):
    theta = np.linspace(0, np.pi, size, dtype=np.float32)[:, None]
    phi = np.linspace(-np.pi/2, np.pi*1.5, size, dtype=np.float32)[None, :]
    x = np.sin(theta) * np.sin(phi)
    y = np.broadcast_to(np.cos(theta), (size, size))
    z = np.sin(theta) * np.cos(phi)
    result = {}
    for number in range(16):
        base_number = number if number < 9 else number - 8
        albedo = np.empty((size, size, 3), np.float32)
        albedo[:] = COLORS[base_number]
        if number >= 9:
            band = np.clip((.47 - np.abs(y)) * 600 + .5, 0, 1)
            albedo = np.array([242, 239, 222], np.float32)[None, None, :] * (1-band[:, :, None]) + albedo * band[:, :, None]
        if number:
            radius = .355
            cap = np.clip((radius - np.sqrt(x*x+y*y))*1500+.5, 0, 1) * (np.abs(z) > .9)
            albedo = albedo*(1-cap[:, :, None]) + np.array([245, 242, 224])*cap[:, :, None]
            stamp = label_mask(number)
            local_x = np.where(z >= 0, -x, x)
            ix = np.int32(np.clip((local_x/(radius*1.65)+.5)*1023, 0, 1023))
            iy = np.int32(np.clip((.5-y/(radius*1.65))*1023, 0, 1023))
            ink = stamp[iy, ix] * cap
            albedo = albedo*(1-ink[:, :, None]) + np.array([13, 15, 15])*ink[:, :, None]
        pixels = np.uint8(np.clip(albedo, 0, 255))
        pixels[:, -1] = pixels[:, 0]
        result[number] = pixels
    return result


def main():
    folder = DELIVERY / 'UnityProject/Assets/IntegratedArt/Textures'
    records = []
    for number, pixels in make_arrays().items():
        path = folder / f'Ball_{number}.png'
        Image.fromarray(pixels).save(path)
        records.append({'number': number, 'file': path.name, 'size': list(pixels.shape), 'sha256': hashlib.sha256(path.read_bytes()).hexdigest()})
    (DELIVERY / 'Docs/ball_set_manifest.json').write_text(json.dumps(records, indent=2), encoding='utf-8')
    print(json.dumps({'balls': len(records), 'dimensions': [2048, 2048], 'gameplay': False}))


if __name__ == '__main__': main()
