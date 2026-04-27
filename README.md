# Laser Range Finder w/ VRCRaycast

Avatar gimmick Laser Range Finder (LRF) using VRCRaycast for VRChat.

## Overview

Measures the distance to the nearest object or player in the avatar's line of sight using `VRCRaycast`, and displays the result on a 7-segment style shader display. A visible laser beam and hit dot are rendered via additive shaders.

## Features

- 3-digit integer display (0–999 m)
- `+999` display when out of range (> 999 m)
- Leading zero suppression
- WD ON/OFF compatible (Shader-based display, no BlendShape dependency)
- Visible laser beam with glow effect
- Hit dot rendered at Result Transform position

## Files

| File | Description |
|------|-------------|
| `Shader/LRF_SevenSegDisplay.shader` | 7-segment style distance display shader |
| `Shader/LRF_LaserBeam.shader` | Additive laser beam shader with glow |
| `Shader/LRF_HitDot.shader` | Additive hit dot shader with glow |
| `Editor/LRFAnimationGenerator.cs` | Editor script to generate Animation Clips for BlendTree |

## Quick Start

See [SETUP.md](SETUP.md) for full setup instructions.

## Notes

- Distance display accuracy: ±1 m (1 keyframe per meter)
- 1-frame lag between `LRF_Hit` and `LRF_Distance` is acceptable
- VRCRaycast is local-only; remote players will see positional offset
