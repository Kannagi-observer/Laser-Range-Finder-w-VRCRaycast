# Laser Range Finder w/ VRCRaycast

Avatar gimmick Laser Range Finder (LRF) using VRCRaycast for VRChat.

## Overview

Measures the distance to the nearest object or player in the avatar's line of sight using `VRCRaycast`, and displays the result on a 7-segment style shader display.

## Features

- 3-digit integer display (0–999 m)
- `+999` display when out of range (>999 m)
- Leading zero suppression
- WD ON/OFF compatible (Shader-based display, no BlendShape dependency)

## Files

| File | Description |
|------|-------------|
| `Shader/LRF_SevenSegDisplay.shader` | 7-segment style distance display shader |
| `Editor/LRFAnimationGenerator.cs` | Editor script to generate 1000 Animation Clips for BlendTree |

## Setup

### VRCRaycast Component

| Property | Value |
|----------|-------|
| Parameter | `LRF` |
| Direction | Forward (0, 0, 1) |
| Distance | 1000 |
| Collision Mode | Hit Worlds And Players |
| Result Transform | `LRF_HitPoint` |
| Behavior On Miss | Snap To End |

### Animator Parameters

| Parameter | Type | Source |
|-----------|------|--------|
| `LRF_Hit` | Bool | VRCRaycast (auto) |
| `LRF_Distance` | Float | VRCRaycast (auto) |
| `LRF_Active` | Bool | Expression Menu |

### Animator Layers

1. **LRF_HitDrive** — Drives `material._Hit` via `Hit_On` / `Hit_Off` clips
2. **LRF_DistDrive** — BlendTree 1D (`LRF_Distance` 0–999) driving `material._Distance`

### Hierarchy

```
Avatar Root
└─ LRF_Attachment
    ├─ [VRCRaycast] LRF_Emitter
    ├─ LaserDot
    ├─ LRF_HitPoint          ← Result Transform
    └─ LRF_Display           ← MeshRenderer with LRF_SevenSegDisplay.shader
```

## Notes

- Distance display accuracy: ±1 m (1 keyframe per meter)
- 1-frame lag between `LRF_Hit` and `LRF_Distance` is acceptable
- Remote players will see positional offset due to local-only VRCRaycast calculation
