# Godot Click-Through Transparent Window Manager (C#)

<p align="center">
  <a href="README.md">English</a> | 
  <a href="README_zh.md">中文</a>
</p>

**Smart pixel-perfect click-through for desktop pets, overlays, or always-on-top transparent Godot applications on Windows.**

This AutoLoad singleton (`ClickThroughManager.cs`) enables your Godot 4.x game/window to:

- Have a **fully transparent background** (only drawn pixels are visible)
- Be **click-through** where the sprite/pet/character has **zero or very low alpha** (≤ configurable threshold)
- Remain **clickable/interactive** exactly on the visible/opaque parts (pixel-accurate)
- Automatically recover clickability when mouse leaves the window
- Minimize performance overhead with mouse-move detection & state caching

### Core Technique
Uses a tiny hidden `SubViewport` + `Camera2D` as an "optical probe" to sample the exact pixel under the mouse cursor every frame — including shader effects, animations, particles, etc.  
Then dynamically toggles `WS_EX_TRANSPARENT` via `user32.dll` to achieve true per-pixel click-through behavior.

### Features
- Adjustable alpha threshold (`AlphaThreshold`)
- Optional performance toggle (`EnableCheck`)
- Only calls expensive WinAPI when clickability actually changes
- Works well for **desktop pets**, floating widgets, screen overlays, live wallpapers, etc.

### Requirements
- Godot 4.x (Mono / C# support)
- **Windows only** (uses `user32.dll`)
- Project settings:
  - `Display > Window > Transparent Background` = On
  - `Display > Window > Per Pixel Transparency` = Enabled (recommended)
  - Best use the **Compatibility** renderer

### Quick Start
1. Add `ClickThroughManager.cs` as an AutoLoad or create a Node2D node in your AutoLoad scene to attach the script.
2. Run → enjoy a window where only the visible pixel blocks mouse events
