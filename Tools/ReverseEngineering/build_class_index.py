"""Build deterministic lexical AS2 indices; does not execute ActionScript."""
import csv
import hashlib
import json
import pathlib
import re
from collections import defaultdict

PROJECT = pathlib.Path(__file__).resolve().parents[2]
INPUT = PROJECT / 'Docs/ReverseEngineering/Swf/deobfuscated/scripts'
OUTPUT = PROJECT / 'Docs/ReverseEngineering/AS2'


def mask(text):
    # Preserve offsets/newlines while excluding strings and comments from analysis.
    pattern = r'"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'|//[^\n]*|/\*[\s\S]*?\*/'
    return re.sub(pattern, lambda m: ''.join('\n' if c == '\n' else ' ' for c in m[0]), text)


def write_csv(name, header, rows):
    with (OUTPUT / name).open('w', encoding='utf8', newline='') as stream:
        writer = csv.writer(stream)
        writer.writerow(header)
        writer.writerows(rows)


def main():
    OUTPUT.mkdir(parents=True, exist_ok=True)
    scripts = []
    classes = {}
    methods = []
    fields = []
    for path in sorted(INPUT.rglob('*.as')):
        text = path.read_text(encoding='utf8')
        code = mask(text)
        relative = path.relative_to(INPUT).as_posix()
        declaration = re.search(r'^class\s+([\w.]+)(?:\s+extends\s+([\w.]+))?', code, re.M)
        owner = declaration[1] if declaration else ''
        scripts.append((relative, text, code, owner))
        if not declaration:
            continue
        if owner in classes:
            raise RuntimeError('Duplicate class: ' + owner)
        classes[owner] = dict(parent=declaration[2] or '', path=relative,
                              line=code[:declaration.start()].count('\n') + 1)
        depth = 0
        in_class = False
        for line_number, line in enumerate(code.splitlines(), 1):
            if line_number == classes[owner]['line']:
                in_class = True
            if in_class and depth == 1:
                method = re.match(r'\s*(static\s+)?function\s+(?:(get|set)\s+)?([\w$]+)\s*\(([^)]*)\)', line)
                if method:
                    methods.append([owner, method[3], bool(method[1]), method[3] == owner.split('.')[-1],
                                    method[4].strip(), relative, line_number, method[2] or ''])
                field = re.match(r'\s*(static\s+)?var\s+([\w$]+)', line)
                if field:
                    fields.append([owner, field[2], bool(field[1]), relative, line_number,
                                   text.splitlines()[line_number - 1].strip()])
            if in_class:
                depth += line.count('{') - line.count('}')
                if depth == 0 and line_number > classes[owner]['line'] + 1:
                    in_class = False

    known = sorted(classes, key=lambda c: (-len(c), c))
    reference_pattern = re.compile(r'(?<![\w$.])(' + '|'.join(re.escape(c) for c in known) + r')(?![\w$])')
    references = []
    bindings = []
    registrations = []
    timelines = []
    for relative, text, code, owner in scripts:
        if not owner:
            kind = 'root-timeline' if relative.startswith('frame_') else (
                'sprite-timeline' if relative.startswith('DefineSprite_') else (
                'button' if relative.startswith('DefineButton') else 'linked-symbol'))
            timelines.append([relative, kind, len(text), hashlib.sha256(text.encode('utf8')).hexdigest()])
        for line_number, (raw, line) in enumerate(zip(text.splitlines(), code.splitlines()), 1):
            for match in reference_pattern.finditer(line):
                target = match[1]
                if owner == target:
                    continue
                before, after = line[:match.start()], line[match.end():]
                called = re.match(r'\.([\w$]+)\s*\(', after)
                if re.search(r'\bnew\s*$', before):
                    kind, member = 'constructor', ''
                elif re.search(r'\bextends\s*$', before):
                    kind, member = 'inheritance', ''
                elif called:
                    kind, member = 'qualified-call', called[1]
                elif re.match(r'\s*\(', after):
                    kind, member = 'cast-or-call', ''
                else:
                    kind, member = 'reference', ''
                references.append([owner or relative, target, kind, member, relative, line_number, raw.strip()[:300]])
            if re.search(r'\bon(?:EnterFrame|Load|MouseDown|MouseUp|Press|Release|RollOver|RollOut)\s*=', line):
                bindings.append([owner or relative, relative, line_number, raw.strip()[:300]])
            if 'Object.registerClass(' in line:
                match = re.search(r'Object\.registerClass\("([^"]+)",\s*([\w.]+)', raw)
                registrations.append([match[1] if match else '', match[2] if match else '', relative, line_number, raw.strip()[:300]])

    write_csv('classes.csv', ['class', 'package', 'extends', 'source', 'line'],
              [[name, name.rpartition('.')[0], data['parent'], data['path'], data['line']]
               for name, data in sorted(classes.items())])
    write_csv('methods.csv', ['class', 'method', 'static', 'constructor', 'parameters', 'source', 'line', 'accessor'], methods)
    write_csv('fields.csv', ['class', 'field', 'static', 'source', 'line', 'declaration'], fields)
    write_csv('references.csv', ['owner', 'target', 'kind', 'member', 'source', 'line', 'evidence'], references)
    write_csv('event-bindings.csv', ['owner', 'source', 'line', 'evidence'], bindings)
    write_csv('symbol-registrations.csv', ['symbol', 'class', 'source', 'line', 'evidence'], registrations)
    write_csv('timeline-scripts.csv', ['source', 'kind', 'characters', 'sha256'], timelines)

    children = defaultdict(list)
    for name, data in classes.items():
        children[data['parent']].append(name)
    roots = sorted(name for name, data in classes.items() if data['parent'] not in classes)
    tree = ['# AS2 inheritance tree', '', 'Generated from deobfuscated class declarations. Full names are preserved.', '', '```text']
    visited = set()

    def visit(name, depth):
        if name in visited:
            raise RuntimeError('Inheritance cycle or repeated node: ' + name)
        visited.add(name)
        data = classes[name]
        suffix = ' (extends external ' + data['parent'] + ')' if data['parent'] and data['parent'] not in classes else ''
        tree.append('  ' * depth + name + suffix)
        for child in sorted(children[name]):
            visit(child, depth + 1)

    for name in roots:
        visit(name, 0)
    if len(visited) != len(classes):
        raise RuntimeError('Some classes are unreachable in inheritance tree.')
    tree.extend(['```', ''])
    (OUTPUT / 'class-tree.md').write_text('\n'.join(tree), encoding='utf8')
    metadata = dict(scripts=len(scripts), classes=len(classes), methods=len(methods), fields=len(fields),
                    references=len(references), event_bindings=len(bindings), registrations=len(registrations),
                    timeline_scripts=len(timelines), external_parents=sorted({d['parent'] for d in classes.values()
                        if d['parent'] and d['parent'] not in classes}),
                    limitations='Lexical index, not a complete resolved call graph. Dynamic dispatch and AS2 coercion require manual analysis.')
    (OUTPUT / 'index-summary.json').write_text(json.dumps(metadata, indent=2), encoding='utf8')
    print(json.dumps(metadata, indent=2))


if __name__ == '__main__':
    main()
