# Setup Guide

## Requirements

- VRChat SDK (Avatar 3.0)
- VRCRaycast component support (VRC Avatar SDK 3.7.0+)
- Unity 2022.3.x

---

## 1. Hierarchy

`LRF_Attachment` 自身に Animator を配置し、MA Merge Animator でアバターに統合します。
Animation のパスはすべて `LRF_Attachment` からの相対パスです。

```
Avatar Root
└─ LRF_Attachment                [Animator (LRF_AnimatorController)]
    ├─ LRF_Emitter               [VRCRaycast をアタッチ]
    │   └─ BeamAnchor            [Scale Z を BlendTree で駆動]
    │       └─ LaserBeam         [Quad, LRF_LaserBeam.shader, Position Z=0.5]
    ├─ LRF_HitPoint              [Result Transform 指定先]
    │   └─ HitDot                [Quad, LRF_HitDot.shader]
    └─ LRF_Display               [Quad, LRF_SevenSegDisplay.shader]
```

### BeamAnchor / LaserBeam 設定

Quad のデフォルト Pivot は中心のため、そのまま Scale Z を伸ばすと前後両方向に伸びます。
`BeamAnchor` の Scale Z を駆動し、その子の `LaserBeam` を Z=0.5 にオフセットすることで
Pivot を根本に揃えます。

| オブジェクト | 設定項目 | 値 |
|-------------|---------|----|
| BeamAnchor | Position | (0, 0, 0) |
| BeamAnchor | Scale Z | `LRF_Beam_Min/Max.anim` で駆動 (0 〜 1000) |
| LaserBeam (Quad) | Position Z | 0.5 |
| LaserBeam (Quad) | Scale X | 0.02 (ビーム幅) |

### HitDot 設定

| 設定項目 | 値 |
|---------|----|
| Scale | (0.05, 0.05, 1) |
| 表示制御 | `LRF_Active_On/Off.anim` で GameObject の Active を切り替え |

`Behavior On Miss: Snap To End` により未ヒット時は前方 1000m に移動しますが、
`LRF_Active=false` 時は非表示にして初期位置に戻ったときの誤表示を防ぎます。

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

| ファイル | 内容 |
|---------|------|
| `LRF_Dist_000.anim` 〜 `LRF_Dist_999.anim` | 距離表示用 (1000 ファイル) |
| `LRF_Hit_On.anim` / `LRF_Hit_Off.anim` | Hit 状態切り替え |
| `LRF_Beam_Min.anim` / `LRF_Beam_Max.anim` | BeamAnchor Scale Z (0 / 1000) |
| `LRF_Active_On.anim` / `LRF_Active_Off.anim` | HitDot 表示制御 |

> **Note:** `Editor/` フォルダに `LRFAnimationGenerator.cs` と `LRFBlendTreeSetup.cs` を配置してください。

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
  threshold 0 → LRF_Beam_Min.anim (BeamAnchor Scale Z = 0)
  threshold 1 → LRF_Beam_Max.anim (BeamAnchor Scale Z = 1000)
```

- Write Defaults: **OFF**

#### Layer 4: LRF_ActiveDrive (手動設定)

```
[Idle] --(LRF_Active = true)--> [Active_On]  : LRF_Active_On.anim  (Loop)
       <-(LRF_Active = false)-- [Active_Off] : LRF_Active_Off.anim (Loop)
```

- Write Defaults: **OFF**

### BlendTree 自動構築

1. `Assets/LRF/` に Animator Controller を作成し、名前を `LRF_AnimatorController` にする
2. 上記の Parameters を追加する
3. Unity メニューから `Tools > LRF > Setup BlendTrees` を実行

---

## 5. MergeAnimator 設定

`LRF_Attachment` に `MA Merge Animator` コンポーネントをアタッチします。

| 設定項目 | 値 |
|---------|----|
| Animator to Merge | LRF_AnimatorController |
| Layer Type | FX |
| Delete Attached Animator | ON |

---

## 6. Materials

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

## 7. Expression Menu

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
