# Setup Guide

## Requirements

- VRChat SDK (Avatar 3.0)
- VRCRaycast component support (VRC Avatar SDK 3.7.0+)
- Unity 2022.3.x

---

## 1. Hierarchy

アバタールート直下に以下の構成を作成します。

```
Avatar Root
└─ LRF_Attachment
    ├─ LRF_Emitter               [VRCRaycast をアタッチ]
    │   └─ LaserBeam             [Quad, LRF_LaserBeam.shader]
    ├─ LRF_HitPoint              [Result Transform 指定先]
    │   └─ HitDot                [Quad, LRF_HitDot.shader]
    └─ LRF_Display               [Quad, LRF_SevenSegDisplay.shader]
```

### LaserBeam Quad 設定

| 項目 | 値 |
|------|----|
| Position | (0, 0, 0) — LRF_Emitter の子 |
| Rotation | (90, 0, 0) |
| Scale X | 0.02 (ビーム幅) |
| Scale Z | BlendTree で LRF_Ratio 駆動 |
| Pivot | Z=0 起点 (根本) |

### HitDot Quad 設定

| 項目 | 値 |
|------|----|
| Scale | (0.05, 0.05, 1) |
| Billboard | LRF_HitPoint に対して常にカメラを向くよう Constraint 推奨 |

---

## 2. VRCRaycast コンポーネント

`LRF_Emitter` に VRCRaycast をアタッチし、以下の通り設定します。

| Property | Value |
|----------|-------|
| Parameter | `LRF` |
| Direction | Forward (0, 0, 1) |
| Distance | 1000 |
| Collision Mode | Hit Worlds And Players |
| Result Transform | `LRF_HitPoint` |
| Behavior On Miss | Snap To End |

---

## 3. Animation Clip 生成

1. Unity メニューから `Tools > LRF > Generate Animations` を実行
2. `Assets/LRF/Animations/` に以下のファイルが生成されます
   - `LRF_Dist_000.anim` 〜 `LRF_Dist_999.anim` (1000 ファイル)
   - `LRF_Hit_On.anim`
   - `LRF_Hit_Off.anim`

> **Note:** `LRFAnimationGenerator.cs` および `LRFBlendTreeSetup.cs` は `Editor/` フォルダに配置してください。

---

## 4. Animator Controller

### Parameters

| Parameter | Type | 設定方法 |
|-----------|------|----------|
| `LRF_Hit` | Bool | VRCRaycast が自動セット |
| `LRF_Distance` | Float | VRCRaycast が自動セット |
| `LRF_Ratio` | Float | VRCRaycast が自動セット |
| `LRF_Active` | Bool | Expression Menu で制御 |

### Layer 構成

#### Layer 1: LRF_HitDrive (手動設定)

BlendTreeではなくステートマシンで構成します。

```
[Idle] --(LRF_Hit = true)--> [Hit_On]  : LRF_Hit_On.anim  (Loop)
       <-(LRF_Hit = false)-- [Hit_Off] : LRF_Hit_Off.anim (Loop)
```

| Transition 設定 | 値 |
|-----------------|----|
| Has Exit Time | OFF |
| Duration | 0 |
| Interruption Source | Any State |

- Write Defaults: **OFF**

#### Layer 2: LRF_DistDrive (自動構築)

```
BlendTree 1D
  Parameter : LRF_Distance
  Range     : 0 – 999
  Clips     : LRF_Dist_000.anim (threshold=0) 〜 LRF_Dist_999.anim (threshold=999)
```

- Write Defaults: **OFF**

#### Layer 3: LRF_BeamScale (自動構築)

```
BlendTree 1D
  Parameter : LRF_Ratio
  Range     : 0 – 1
  threshold 0 → LRF_Beam_Min.anim (Scale Z = 0)
  threshold 1 → LRF_Beam_Max.anim (Scale Z = 1000)
```

- Write Defaults: **OFF**

### BlendTree 自動構築

Layer 2・3 は手動での1000クリップ登録を避けるためEditorスクリプトで自動構築します。

1. `Assets/LRF/` に Animator Controller を作成し、名前を `LRF_AnimatorController` にする
2. 上記の Parameters を追加する
3. Unity メニューから `Tools > LRF > Setup BlendTrees` を実行
   - `LRF_DistDrive` Layer に BlendTree 1D (LRF_Distance, 0–999, 1000クリップ) が構築される
   - `LRF_BeamScale` Layer に BlendTree 1D (LRF_Ratio, 0–1) が構築される

> **Note:** `LRF_BeamScale` に使用する `LRF_Beam_Min.anim` / `LRF_Beam_Max.anim` は手動で作成してください。
> - `LRF_Beam_Min.anim`: LaserBeam の Scale Z = 0
> - `LRF_Beam_Max.anim`: LaserBeam の Scale Z = 1000

---

## 5. Materials

### LRF_SevenSegDisplay

| Property | 推奨値 |
|----------|--------|
| On Color | (1, 0.2, 0.2, 1) — 赤系 |
| Off Color | (0.05, 0.05, 0.05, 1) |
| Background | (0, 0, 0, 1) |
| Segment Width | 0.10 |
| Segment Gap | 0.03 |

### LRF_LaserBeam

| Property | 推奨値 |
|----------|--------|
| Beam Color | On Color と合わせる |
| Core Width | 0.05 |
| Glow Width | 0.25 |
| Glow Falloff | 3.0 |
| Intensity | 2.0 |

### LRF_HitDot

| Property | 推奨値 |
|----------|--------|
| Dot Color | On Color と合わせる |
| Dot Radius | 0.15 |
| Glow Falloff | 4.0 |
| Intensity | 3.0 |

---

## 6. Expression Menu

| 項目 | 設定 |
|------|------|
| Type | Toggle |
| Parameter | `LRF_Active` |

---

## Display Reference

| 状況 | 表示 |
|------|------|
| 5 m | `  5` |
| 42 m | ` 42` |
| 999 m 以内 | `999` |
| 超過 (Miss) | `+999` |
