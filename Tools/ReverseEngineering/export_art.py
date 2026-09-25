"""Export SWF art with FFDec; keep extraction outside Unity Assets."""
import csv
import hashlib
import json
import pathlib
import re
import struct
import subprocess
import xml.etree.ElementTree as ET
import zlib
from collections import Counter

PROJECT = pathlib.Path(__file__).resolve().parents[2]
WORKSPACE = PROJECT.parent
OUTPUT = PROJECT / 'Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Art'
SWF = WORKSPACE / 'Mutiny Source/mutiny-flash-game/mutiny.swf'
SWF_XML = PROJECT / 'Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml'


def recorded_arg(argument):
    """Store paths relative to the workspace while running with absolute paths."""
    path = pathlib.Path(argument)
    return path.relative_to(WORKSPACE).as_posix() if path.is_absolute() else argument


def main():
    java = next((WORKSPACE / 'Tools').glob('java-*/**/bin/java.exe'))
    jar = next((WORKSPACE / 'Tools').glob('ffdec-*/ffdec.jar'))
    base = [str(java), '-Djava.awt.headless=true', '-Dfile.encoding=UTF-8', '-Xmx2g', '-jar', str(jar)]
    fingerprint = hashlib.sha256(SWF.read_bytes()).hexdigest()
    OUTPUT.mkdir(parents=True, exist_ok=True)
    logs = OUTPUT / 'logs'
    logs.mkdir(exist_ok=True)
    jobs = [
        ('raster', 'image:png,shape:png,sprite:png,button:png,morphshape:png_frames',
         'image,shape,sprite,button,morphshape'),
        ('vector', 'shape:svg,sprite:svg,button:svg,morphshape:svg',
         'shape,sprite,button,morphshape'),
    ]
    commands = []
    for name, formats, types in jobs:
        args = ['-onerror', 'abort', '-zoom', '1', '-ignorebackground', '-format', formats,
                '-export', types, str(OUTPUT / name), str(SWF)]
        print('Exporting ' + name, flush=True)
        completed = subprocess.run(base + args, capture_output=True, timeout=1800)
        (logs / (name + '.stdout.log')).write_bytes(completed.stdout)
        (logs / (name + '.stderr.log')).write_bytes(completed.stderr)
        commands.append(dict(job=name, argv=[recorded_arg(arg) for arg in base + args],
                             exit_code=completed.returncode))
        (OUTPUT / 'commands.json').write_text(json.dumps(commands, indent=2), encoding='utf8')
        if completed.returncode:
            raise RuntimeError(name + ' export failed; inspect logs.')
    if fingerprint != hashlib.sha256(SWF.read_bytes()).hexdigest():
        raise RuntimeError('Source SWF changed.')
    (OUTPUT / 'metadata.json').write_text(json.dumps(dict(swf_sha256=fingerprint,
        ffdec='26.3.0', zoom=1, transparent_background=True,
        policy='Raw exports; no renaming, Unity import or gameplay-script execution.'), indent=2), encoding='utf8')
    build_symbol_index()
    audit_exports()


def build_symbol_index():
    root = ET.parse(SWF_XML).getroot()
    links = {}
    for tag in root.iter('item'):
        if tag.get('type') == 'ExportAssetsTag':
            for ident, name in zip(tag.findall('tags/item'), tag.findall('names/item')):
                links.setdefault(int(ident.text), []).append(name.text)
    records = []
    frames = []
    placements = []
    bounds = []
    for tag in root.iter('item'):
        kind = tag.get('type', '')
        if not kind.startswith('Define'):
            continue
        ident = next((tag.get(key) for key in ['spriteId','shapeId','characterID','characterId',
                    'buttonId','morphShapeId','fontID','fontId'] if tag.get(key) is not None), None)
        if ident is None:
            continue
        ident = int(ident)
        if kind.startswith(('DefineSound','DefineFont','DefineText','DefineEditText','DefineBinary')):
            continue
        records.append([ident, kind, '|'.join(links.get(ident, [])), tag.get('frameCount',''),
                        tag.get('bitmapWidth',''), tag.get('bitmapHeight','')])
        for name in ['shapeBounds','edgeBounds','startBounds','endBounds']:
            rect = tag.find(name)
            if rect is not None:
                bounds.append([ident, name] + [int(rect.get(key))/20 for key in ['Xmin','Ymin','Xmax','Ymax']])
        sub = tag.find('subTags')
        if sub is None:
            continue
        frame = 1
        for action in sub:
            action_type = action.get('type','')
            if action_type == 'FrameLabelTag':
                frames.append([ident, frame, action.get('name','')])
            if action_type.startswith('PlaceObject'):
                matrix = action.find('matrix')
                placements.append([ident, frame, action_type, action.get('depth',''),
                    action.get('characterId',''), action.get('placeFlagMove',''),
                    json.dumps(matrix.attrib, sort_keys=True) if matrix is not None else '',
                    action.get('name','')])
            if action_type == 'ShowFrameTag':
                frame += 1
    def write(name, header, rows):
        with (OUTPUT / name).open('w', encoding='utf8', newline='') as stream:
            writer = csv.writer(stream); writer.writerow(header); writer.writerows(rows)
    write('symbols.csv', ['symbol_id','tag_type','linkage_names','timeline_frames','bitmap_width','bitmap_height'], sorted(records))
    write('frame-labels.csv', ['symbol_id','frame','label'], sorted(frames))
    write('placements.csv', ['parent_symbol_id','frame','tag_type','depth','child_symbol_id','move','matrix_original','instance_name'], placements)
    write('shape-bounds.csv', ['symbol_id','bounds_type','xmin_px','ymin_px','xmax_px','ymax_px'], sorted(bounds))
    print(f'Indexed {len(records)} art definitions, {len(frames)} frame labels, {len(placements)} placements.', flush=True)


def audit_exports():
    definitions = list(csv.DictReader((OUTPUT / 'symbols.csv').open(encoding='utf8')))
    symbols = {int(row['symbol_id']): row for row in definitions}
    counts = Counter()
    covered = set()
    inventory = []
    origins = []
    errors = []
    for file_index, path in enumerate(sorted((OUTPUT / 'raster').rglob('*.png')), 1):
        if file_index % 500 == 0:
            print(f'Checking PNG {file_index}', flush=True)
        relative = path.relative_to(OUTPUT / 'raster')
        group = relative.parts[0]
        ident_match = re.match(r'(?:Define(?:Sprite|Button2)_)?(\d+)', relative.parts[1])
        if ident_match is None:
            errors.append([str(relative), 'Cannot identify source symbol']); continue
        ident = int(ident_match[1])
        if ident not in symbols:
            errors.append([str(relative), 'Unknown source symbol']); continue
        covered.add(ident)
        counts[group] += 1
        data = path.read_bytes()
        try:
            if data[:8] != b'\x89PNG\r\n\x1a\n': raise ValueError('Invalid PNG signature')
            offset = 8; compressed = bytearray(); color_type = None; has_trns = False
            width = height = 0; ended = False
            while offset < len(data):
                size = struct.unpack_from('>I', data, offset)[0]
                kind = data[offset+4:offset+8]; payload = data[offset+8:offset+8+size]
                crc = struct.unpack_from('>I', data, offset+8+size)[0]
                if zlib.crc32(kind + payload) & 0xffffffff != crc: raise ValueError('PNG CRC mismatch')
                if kind == b'IHDR': width,height,depth,color_type = struct.unpack_from('>IIBB', payload)
                if kind == b'IDAT': compressed.extend(payload)
                if kind == b'tRNS': has_trns = True
                offset += size+12
                if kind == b'IEND': ended = True; break
            if not ended or not width or not height: raise ValueError('Incomplete PNG')
            zlib.decompress(compressed)
            alpha = color_type in (4,6) or has_trns
        except (ValueError,struct.error,zlib.error) as exception:
            errors.append([str(relative), str(exception)]); continue
        inventory.append([ident, symbols[ident]['linkage_names'], relative.as_posix(),
                          width,height,color_type,alpha,len(data),hashlib.sha256(data).hexdigest()])
        if group == 'sprites':
            svg = OUTPUT / 'vector' / relative.with_suffix('.svg')
            if not svg.exists():
                errors.append([str(relative), 'Missing matching SVG']); continue
            with svg.open(encoding='utf8') as stream:
                prefix = stream.read(4096)
            transform = re.search(r'<g\s+transform="matrix\(([^)]+)\)"', prefix)
            matrix = transform
            if matrix:
                values = [float(v) for v in re.split(r'[,\s]+',matrix[1].strip())]
                if len(values)==6 and values[:4]==[1,0,0,1]:
                    ox,oy=values[4:]
                    origins.append([ident,relative.as_posix(),svg.relative_to(OUTPUT).as_posix(),
                                    ox,oy,width,height,ox/width,1-oy/height,
                                    'SVG root translation; verify paired PNG registration'])
                else:
                    errors.append([str(relative), 'Unexpected SVG root matrix; origin requires manual review'])
            else:
                errors.append([str(relative), 'No SVG root matrix; origin requires manual review'])
    def write(name, header, rows):
        with (OUTPUT/name).open('w',encoding='utf8',newline='') as stream:
            writer=csv.writer(stream);writer.writerow(header);writer.writerows(rows)
    write('png-files.csv', ['symbol_id','linkage_names','raster_path','width','height','png_color_type','alpha_capable','bytes','sha256'],inventory)
    write('sprite-origins.csv', ['symbol_id','raster_path','svg_path','origin_x_from_left_px','origin_y_from_top_px',
        'png_width','png_height','candidate_unity_pivot_x','candidate_unity_pivot_y','basis'],origins)
    nonvisual=[row for row in definitions if row['tag_type']=='DefineSpriteTag' and row['timeline_frames']=='0']
    missing=[row for row in definitions if int(row['symbol_id']) not in covered and row not in nonvisual]
    write('nonvisual-symbols.csv',['symbol_id','tag_type','linkage_names','timeline_frames'],
          [[row[k] for k in ['symbol_id','tag_type','linkage_names','timeline_frames']] for row in nonvisual])
    write('missing-symbols.csv',['symbol_id','tag_type','linkage_names','timeline_frames','bitmap_width','bitmap_height'],
          [[row[k] for k in ['symbol_id','tag_type','linkage_names','timeline_frames','bitmap_width','bitmap_height']] for row in missing])
    write('audit-errors.csv',['file','diagnostic'],errors)
    summary=dict(definitions=len(definitions),covered_symbols=len(covered),nonvisual_symbols=len(nonvisual),missing_symbols=len(missing),
                 raster_files=dict(counts),validated_pngs=len(inventory),sprite_origin_candidates=len(origins),
                 pngs_without_alpha=sum(not row[6] for row in inventory),audit_errors=len(errors),
                 sprite_timeline_frames=sum(int(r['timeline_frames']) for r in definitions if r['tag_type']=='DefineSpriteTag'),
                 vector_files=len(list((OUTPUT/'vector').rglob('*.svg'))),
                 limitations='PNG integrity and symbol coverage do not prove animation or visual fidelity. Nested/script-driven animation needs manual review.')
    (OUTPUT/'audit-summary.json').write_text(json.dumps(summary,indent=2),encoding='utf8')
    print(json.dumps(summary,indent=2),flush=True)


if __name__ == '__main__':
    main()
