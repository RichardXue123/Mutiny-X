# Built-in imagegen prompts

Date: 2026-10-01. Mode: built-in image_gen, transparent_background=true. Source sheets retained verbatim; project PNGs are mechanically sliced, resized, palette-normalized and aligned for the existing 35-slot character timeline.

## RobotCaptain

```
Use case: stylized-concept. Asset type: production 2D pixel-art character keyframe sprite sheet for the existing Mutiny game.
Input image: reference only, showing actual in-game pirate and captain idle/hit sprites enlarged with nearest-neighbor. Match their extremely low-resolution pixel art, approximately 20 pixels wide and 26 pixels tall per figure, big head with tiny squat body, frontal view with a slight right-facing bias, 1 logical pixel off-white outer keyline, dark inner outline, very few flat colors. NO human realistic proportions, no long legs, no smooth illustration.
Create RobotCaptain, a Daft Punk Thomas Bangalter-inspired cylindrical robot helmet with broad black horizontal visor and squared vented jaw, but deliberately GOLD instead of silver as requested. Gold ochre shadows, amber midtone, pale warm highlight, charcoal black minimal suit/body and tiny gold shoulder accents. Fully helmeted face. Tiny mitten-like hands close to body. Front-facing helmet and body, consistent in all frames.
Produce exactly 8 equally sized cells, 4 columns by 2 rows, on a truly transparent background. Target sheet 1024x512, cell256x256. Each figure occupies the same approximately 160px height, about 26 logical pixel rows, aligned to the same foot baseline at y=208 within EVERY cell. Each pixel of the artwork is a large crisp square, not fine detail. Leave generous transparent cell margins. No visible cell borders, no floor, no shadows, no labels, no text.
Top row four IDLE key poses left to right: neutral idle A; slight down compression B (helmet one logical pixel lower, feet stationary); neutral A again; slight up breath C (helmet one logical pixel higher, feet stationary).
Bottom row four HIT/RECOVERY key poses left to right: shocked impact held pose with helmet slightly tilted backward and short arms raised, knees compressed; early recovery with arms half lowered and body still leaning; late recovery almost upright and arms down; EXACT neutral idle A again.
Keep identical gold helmet shape, visor construction, head-to-body proportion and palette in all eight cells. Hit causes pose changes only; no broken helmet, no new facial features, no flashes or loose particles. Match reference chunky pixels, big head and very short body. All eight figures same identity, same scale, no drift. True transparent PNG.
```

## Robot

```
Use case: stylized-concept. Asset type: production 2D pixel-art character keyframe sprite sheet for Mutiny.
Input image 1 is reference only: existing Mutiny pirate/captain sprites, enlarged. Input image 2 is the newly designed RobotCaptain sheet: use its chunky pixel size, white 1-logical-pixel keyline, tiny squat body and black suit. Create its regular teammate Robot, visibly different helmet.
Robot is a Guy-Manuel de Homem-Christo / Daft Punk inspired smooth rounded wraparound helmet, deliberately SILVER instead of gold. It has a large continuous black curved faceplate reaching over the forehead, a silver horseshoe border around the sides and chin, small silver earpieces, and a small cool pale blue reflection in the black faceplate. No gold, no eyes, no mouth or vent teeth. Silver in 3 flat shades: charcoal shadow, blue-gray midtone, pale silver highlight. Same very short charcoal body with tiny silver shoulder details, narrow waist, tiny feet tucked directly underneath. Slightly smaller/shorter than Captain. Big head, body only about 1/3 total height, no long human legs.
Exactly 8 equal cells, 4 columns by 2 rows, transparent PNG, target1024x512. Each cell256x256. Consistent scale. Foot baseline y208 in each cell; neutral figure about144px tall corresponding to24 logical pixel rows. Large square pixel-art blocks, limited flat palette, no fine texture. No labels/text/borders/grid/floor/shadow/background.
Top row left to right: neutral idle A; down breathing B with helmet one logical pixel down and feet stationary; neutral A again; up breathing C with helmet one pixel up and feet stationary.
Bottom row left to right: hit peak, helmet tilts backward and tiny arms raised; early recovery with arms lowering and body still leaning; late recovery nearly upright; exact same neutral idle A as top-left.
Preserve the same smooth SILVER Guy-Man wraparound black dome faceplate identity in every cell, no Thomas-style gold helmet or mouth vents. Match reference game's tiny sprite proportions and crisp low-resolution pixel art with off-white silhouette keyline. True transparent background. No detached sparks, fragments or glow.
```

