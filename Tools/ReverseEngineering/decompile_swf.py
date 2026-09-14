"""Export original SWF scripts and evidence with the workspace portable tools."""
import csv
import hashlib
import json
import pathlib
import re
import subprocess
import sys

PROJECT = pathlib.Path(__file__).resolve().parents[2]
WORKSPACE = PROJECT.parent
SWF = WORKSPACE / 'Mutiny Source/mutiny-flash-game/mutiny.swf'
TOOLS = WORKSPACE / 'Tools'
OUTPUT = PROJECT / 'Docs/ReverseEngineering/Swf'


def audit():
    expected = {line.lstrip('\\').replace('\\', '/') + '.as'
                for line in (OUTPUT / 'logs/scripts-index.stdout.log').read_text(encoding='utf8').splitlines()
                if line.strip()}
    report = {'indexed_scripts': len(expected), 'formats': {}}
    for folder in ('source', 'deobfuscated', 'pcode'):
        root = OUTPUT / folder / 'scripts'
        extension = '*.pcode' if folder == 'pcode' else '*.as'
        paths = {p.relative_to(root).with_suffix('.as').as_posix(): p for p in root.rglob(extension)}
        report['formats'][folder] = dict(files=len(paths), missing=sorted(expected - paths.keys()),
            unexpected=sorted(paths.keys() - expected),
            empty=sorted(name for name, p in paths.items() if p.stat().st_size == 0))
    classes = []
    for path in sorted((OUTPUT / 'deobfuscated/scripts/__Packages').rglob('*.as')):
        text = path.read_text(encoding='utf8')
        matches = re.findall(r'^class\s+([\w.]+)(?:\s+extends\s+([\w.]+))?', text, re.M)
        classes.append([path.relative_to(OUTPUT).as_posix(), matches[0][0] if matches else '',
                        matches[0][1] if matches else '', text.count('invalid_utf8'),
                        text.count('\ufffd')])
    with (OUTPUT / 'class-readability.csv').open('w', encoding='utf8', newline='') as stream:
        writer = csv.writer(stream)
        writer.writerow(['path', 'class', 'extends', 'invalid_utf8_markers', 'replacement_characters'])
        writer.writerows(classes)
    report['package_scripts'] = len(classes)
    report['recovered_class_declarations'] = sum(bool(row[1]) for row in classes)
    (OUTPUT / 'coverage.json').write_text(json.dumps(report, indent=2), encoding='utf8')
    print(json.dumps(report, indent=2), flush=True)


def main():
    jars = sorted(TOOLS.glob('ffdec-*/ffdec.jar'))
    java_paths = sorted(TOOLS.glob('java-*/**/bin/java.exe'))
    if len(jars) != 1 or len(java_paths) != 1:
        raise RuntimeError('Expected exactly one portable FFDec and Java under workspace Tools.')
    base = [str(java_paths[0]), '-Djava.awt.headless=true', '-Dfile.encoding=UTF-8',
            '-Xmx2g', '-jar', str(jars[0])]
    OUTPUT.mkdir(parents=True, exist_ok=True)
    logs = OUTPUT / 'logs'
    logs.mkdir(exist_ok=True)
    before = hashlib.sha256(SWF.read_bytes()).hexdigest()
    commands = []
    jobs = [
        ('help', ['-help', '-all']),
        ('config', ['-listconfigs']),
        ('scripts-index', ['-dumpAS2', '-exportNames', str(SWF)]),
        ('tags', ['-dumpSWF', str(SWF)]),
        ('source', ['-onerror', 'abort', '-timeout', '120', '-exportTimeout', '1200',
                    '-format', 'script:as', '-export', 'script', str(OUTPUT / 'source'), str(SWF)]),
        ('deobfuscated', ['-config', 'autoDeobfuscate=true', '-onerror', 'abort',
                          '-timeout', '120', '-exportTimeout', '1200', '-format', 'script:as',
                          '-export', 'script', str(OUTPUT / 'deobfuscated'), str(SWF)]),
        ('pcode', ['-onerror', 'abort', '-format', 'script:pcodehex', '-export', 'script',
                   str(OUTPUT / 'pcode'), str(SWF)]),
        ('xml', ['-swf2xml', str(SWF), str(OUTPUT / 'mutiny.swf.xml')]),
        ('symbols', ['-export', 'symbolClass', str(OUTPUT / 'symbols'), str(SWF)]),
    ]
    for name, arguments in jobs:
        print('Running ' + name, flush=True)
        completed = subprocess.run(base + arguments, capture_output=True, timeout=1500)
        (logs / (name + '.stdout.log')).write_bytes(completed.stdout)
        (logs / (name + '.stderr.log')).write_bytes(completed.stderr)
        commands.append(dict(job=name, argv=base + arguments, exit_code=completed.returncode))
        (OUTPUT / 'commands.json').write_text(json.dumps(commands, indent=2), encoding='utf8')
        if completed.returncode:
            raise RuntimeError(f'{name} failed ({completed.returncode}); inspect logs.')

    after = hashlib.sha256(SWF.read_bytes()).hexdigest()
    if before != after:
        raise RuntimeError('Source SWF changed.')
    inventory = []
    for folder in ('source', 'deobfuscated', 'pcode'):
        for path in sorted((OUTPUT / folder).rglob('*')):
            if path.is_file():
                data = path.read_bytes()
                inventory.append([folder, path.relative_to(OUTPUT).as_posix(), len(data),
                                  hashlib.sha256(data).hexdigest()])
    with (OUTPUT / 'script-files.csv').open('w', encoding='utf8', newline='') as stream:
        writer = csv.writer(stream)
        writer.writerow(['format', 'path', 'bytes', 'sha256'])
        writer.writerows(inventory)

    findings = []
    pattern = re.compile(r'(?i)(decompilation (?:error|failed)|decompilation skipped|'
                         r'timeout|timed out|unknown instruction|exception|severe|warning)')
    for folder in ('source', 'deobfuscated'):
        for path in sorted((OUTPUT / folder).rglob('*.as')):
            for line_number, line in enumerate(path.read_text(encoding='utf8', errors='replace').splitlines(), 1):
                if pattern.search(line):
                    findings.append([path.relative_to(OUTPUT).as_posix(), line_number, line.strip()[:500]])
    for path in sorted(logs.glob('*.stderr.log')):
        for line_number, line in enumerate(path.read_text(encoding='utf8', errors='replace').splitlines(), 1):
            if pattern.search(line):
                findings.append([path.relative_to(OUTPUT).as_posix(), line_number, line.strip()])
    with (OUTPUT / 'diagnostic-candidates.csv').open('w', encoding='utf8', newline='') as stream:
        writer = csv.writer(stream)
        writer.writerow(['file', 'line', 'text'])
        writer.writerows(findings)
    metadata = dict(swf=str(SWF), swf_sha256=before,
                    ffdec=str(jars[0]), ffdec_sha256=hashlib.sha256(jars[0].read_bytes()).hexdigest(),
                    java=str(java_paths[0]), source_files=sum(r[0] == 'source' for r in inventory),
                    pcode_files=sum(r[0] == 'pcode' for r in inventory),
                    deobfuscated_files=sum(r[0] == 'deobfuscated' for r in inventory),
                    diagnostic_candidates=len(findings))
    (OUTPUT / 'metadata.json').write_text(json.dumps(metadata, indent=2), encoding='utf8')
    audit()
    print(json.dumps(metadata, indent=2), flush=True)


if __name__ == '__main__':
    main()
