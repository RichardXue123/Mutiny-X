# Mutiny Level scan

Files: 34; parsed: 34; errors: 0.

Non-empty tile names (Terrain + Background): 107.

Object types: 31; object attribute names: 20.

Coordinates use original XML x (left to right), y (top to bottom). Object indices are zero-based.

Empty '-' tiles are excluded. Logical markers such as antichest are retained. Raw attributes include type/x/y; no gameplay meanings are inferred.

Verified reference: 18 files, 107 tile names, 29 object types, 20 attribute names. The preliminary documentation stated 22 attributes; independent XML recount confirmed 20.

Reference comparison: NOT APPLICABLE — current mode-scoped resources include extensions; the original 18-file evidence baseline is unchanged.

| Report | Contents |
| --- | --- |
| levels.csv | Per-level metadata, type counts and source SHA256 |
| tile-types.csv | Combined tile names and occurrence counts |
| tile-occurrences.csv | Every non-empty tile, layer and coordinate |
| object-types.csv | Object types and counts |
| objects.csv | Every object, index, type and coordinate |
| attribute-values.csv | Raw attribute values and counts |
| object-properties.csv | Every attribute with its object location |
| errors.csv | File and diagnostic for each failed parse/read |

Reports are overwritten on each scan and ordered deterministically; they are not cumulative.
