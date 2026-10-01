"""Import the four-column translation CSV without changing quoted multiline text."""

import argparse
import csv
import hashlib
from pathlib import Path
import re


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "Assets/Mutiny/Localization/Mutiny.csv"
TSV = ROOT / "Assets/Mutiny/Localization/Mutiny.tsv"
RUNTIME = ROOT / "Assets/Mutiny/Resources/Localization/MutinyRuntime.txt"
HEADER = ["# key", "English source", "Simplified Chinese", "Traditional Chinese (HK)"]


def normalized(value):
    return value.replace("\r\n", "\n").replace("\r", "\n").replace("\\n", "\n")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", nargs="?", type=Path, default=SOURCE)
    args = parser.parse_args()
    original = args.source.read_bytes()
    with args.source.open(encoding="utf-8-sig", newline="") as stream:
        rows = list(csv.reader(stream))
    if not rows or rows[0] != HEADER:
        raise ValueError(f"Expected CSV columns: {HEADER}")

    keys = set()
    output = ["# key\ten\tzh-Hans\tzh-HK"]
    for row in rows[1:]:
        if not row:
            continue
        if len(row) != 4:
            raise ValueError(f"Expected four fields: {row[0]}")
        if row[0].startswith("#"):
            output.append(row[0])
            continue
        if not row[0] or row[0] in keys:
            raise ValueError(f"Empty or duplicate key: {row[0]}")
        values = [normalized(value) for value in row]
        if any(not value.strip() for value in values):
            raise ValueError(f"Empty translation: {row[0]}")
        if any("\t" in value for value in values):
            raise ValueError(f"Tab in translation: {row[0]}")
        slots = sorted(re.findall(r"\{\d+(?:[^}]*)\}", values[1]))
        if any(sorted(re.findall(r"\{\d+(?:[^}]*)\}", value)) != slots for value in values[2:]):
            raise ValueError(f"Placeholder mismatch: {row[0]}")
        keys.add(row[0])
        output.append("\t".join(value.replace("\n", "\\n") for value in values))

    current_keys = {line.split("\t")[0] for line in TSV.read_text(encoding="utf-8-sig").splitlines()
                    if line and not line.startswith("#")}
    if current_keys - keys:
        raise ValueError(f"CSV is missing existing keys: {sorted(current_keys - keys)}")

    generated = ("\n".join(output) + "\n").encode("utf-8")
    if args.source.resolve() != SOURCE.resolve():
        SOURCE.write_bytes(original)
    TSV.write_bytes(generated)
    RUNTIME.write_bytes(generated)
    print(f"Imported {len(keys)} keys, three languages. Source SHA256={hashlib.sha256(original).hexdigest()}")
    if keys - current_keys:
        print(f"Added keys: {', '.join(sorted(keys - current_keys))}")
    print("TSV and runtime text match. Rebuild String Tables to refresh Unity authoring assets.")


if __name__ == "__main__":
    main()
