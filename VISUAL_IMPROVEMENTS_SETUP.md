# Visual Improvements Setup Guide

## New Scripts Created

1. **TileLayoutManager.cs** - Spreads tiles out in a beautiful pattern
2. **StarFieldGenerator.cs** - Creates a starfield around the scene
3. **TileVisualEffects.cs** - Adds particles and glow effects to tiles
4. **Updated MosaicGazeManager1.cs** - Fixed canvas display issue

## Setup Steps

### 1. Spread Out Tiles

**In Unity:**
1. Create empty GameObject: `GameObject > Create Empty`
2. Name it: `TileLayoutManager`
3. Add component: `TileLayoutManager`
4. Configure settings:
   - **Radius**: 3-4 (distance tiles are from center)
   - **Height Range**: 2-3 (vertical spread)
   - **Tiles Per Row**: 3 (if using grid layout)
   - **Tile Spacing**: 1.5-2 (space between tiles)
   - **Arrange In Sphere**: ✅ Checked (recommended)

The tiles will automatically arrange themselves when you press Play!

### 2. Add Starfield

**In Unity:**
1. Create empty GameObject: `GameObject > Create Empty`
2. Name it: `StarField`
3. Add component: `StarFieldGenerator`
4. Configure settings:
   - **Star Count**: 200-300 (more = prettier but slower)
   - **Min Distance**: 5
   - **Max Distance**: 20
   - **Twinkle Speed**: 2
   - **Rotate Stars**: ✅ Checked

**Optional:** Assign a material to `Star Material`:
- Create new material: `Assets > Create > Material`
- Set shader to `Unlit/Color` or `Unlit/Texture`
- Set color to white or light blue
- Drag to `Star Material` field

### 3. Add Visual Effects to Tiles

**For each tile:**
1. Select a tile GameObject
2. Add component: `TileVisualEffects`
3. The script will auto-create particles and lights!

**Or add to all tiles at once:**
- Select all tiles in Hierarchy
- `Component > Tile Visual Effects`

### 4. Fix Canvas Display

The canvas should now show properly! It will:
- Appear in front of you when session ends
- Show tiles completed count
- Show average dwell time
- Face you automatically

**To test:**
- Set `Session Duration` to 10 seconds (temporary)
- Press Play
- Wait 10 seconds
- Canvas should appear!

### 5. Improve Materials

**Make tiles more beautiful:**

1. **Idle Material** (purple):
   - Create/select material
   - Set color to deep purple/blue
   - Enable **Emission** (makes it glow!)
   - Set emission color to purple

2. **Active Material** (yellow):
   - Create/select material  
   - Set color to bright yellow/orange
   - Enable **Emission**
   - Set emission color to yellow
   - Increase emission intensity

**Quick Material Setup:**
- Select a tile
- In `GazeTile` component:
  - Assign `Idle Material` → Purple glowing material
  - Assign `Active Material` → Yellow glowing material

### 6. Add Butterflies (You mentioned you can add these)

**When you add butterflies:**
1. Position them around the scene
2. They'll automatically be part of the immersive experience!
3. Consider adding them as children of `StarField` or a new `AmbientObjects` parent

## Quick Test Checklist

- [ ] Tiles spread out nicely (not bunched together)
- [ ] Stars visible around scene
- [ ] Tiles glow when looked at
- [ ] Particles appear when tile completes
- [ ] Canvas shows after session ends
- [ ] Materials look good (purple idle, yellow active)

## Troubleshooting

**Tiles don't spread out:**
- Make sure `TileLayoutManager` is in the scene
- Check that tiles have `GazeTile` component
- Press Play - arrangement happens automatically

**No stars visible:**
- Check `StarField` GameObject is active
- Increase `Star Count` if too few
- Check `Min Distance` isn't too far

**Canvas still not showing:**
- Make sure `Summary Canvas` is assigned in `MosaicGazeManager1`
- Check canvas is not disabled in hierarchy
- Try setting `Session Duration` to 5 seconds for quick test

**Materials not glowing:**
- Enable **Emission** in material settings
- Set emission color and intensity
- Make sure material uses a shader that supports emission

## Next Steps

After this works, you can:
1. Create a "Star Version" where stars light up instead of tiles
2. Add more ambient effects
3. Improve particle effects
4. Add more tiles (they'll auto-arrange!)

Enjoy your beautiful immersive VR experience! ✨
