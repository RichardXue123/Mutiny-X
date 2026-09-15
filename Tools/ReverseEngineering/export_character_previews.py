"""Export preview frame (frame 1) for all 27 character types to Assets/Mutiny/Art/Characters/Preview/."""
import csv
import pathlib
import shutil

PROJECT = pathlib.Path(__file__).resolve().parents[2]
ART_DIR = PROJECT / 'Docs/ReverseEngineering/Art'
RASTER_SPRITES = ART_DIR / 'raster/sprites'
DEST_DIR = PROJECT / 'Assets/Mutiny/Art/Characters/Preview'

CHAR_TYPES = [
    'blindPirate', 'blindPirateCaptain', 'bluePirate', 'bluePirateCaptain',
    'bossGuy', 'bossGuyZombie', 'cabinBoy', 'cabinBoyCaptain',
    'crab', 'femalePirate', 'femalePirateCaptain', 'monkey',
    'oldPirate', 'oldPirateCaptain', 'parrot', 'rainbowBeard',
    'rainbowBeardCaptain', 'redPirate', 'redPirateCaptain', 'shark',
    'skeletonPirate', 'skeletonPirateCaptain', 'soldier', 'soldierCaptain',
    'squid', 'tribe', 'tribeChief'
]


def main():
    DEST_DIR.mkdir(parents=True, exist_ok=True)
    symbols = {}
    with open(ART_DIR / 'symbols.csv', 'r', encoding='utf-8') as f:
        for row in csv.DictReader(f):
            for name in row['linkage_names'].split(';'):
                if name:
                    symbols[name] = row

    copied = 0
    for ct in CHAR_TYPES:
        if ct in symbols:
            sym_id = symbols[ct]['symbol_id']
            matches = list(RASTER_SPRITES.glob(f'DefineSprite_{sym_id}_*/1.png')) + \
                      list(RASTER_SPRITES.glob(f'DefineSprite_{sym_id}/1.png'))
            if matches:
                src = matches[0]
                dst = DEST_DIR / f'{ct}.png'
                shutil.copy2(src, dst)
                copied += 1
            else:
                print('No frame 1 for', ct, sym_id)
        else:
            print('Symbol not found for', ct)

    print(f'Copied {copied}/{len(CHAR_TYPES)} character preview frames to {DEST_DIR}')


if __name__ == '__main__':
    main()

