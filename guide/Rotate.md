# Rotate クラス 解説書

`Rotate` は、**グリッドや二次元配列を回転・反転する**ためのユーティリティクラスです。

競技プログラミングでは、

```text
迷路
盤面
マス目の状態
```

のような二次元構造を回転させる場面がよくあります。

このクラスは、`List<T[]>` や `List<List<T>>` を回転・反転しやすくします。

---

# 目次

| メソッド | 概要 | 計算量 |
| --- | --- | --- |
| [`RotateR90`](#rotater90) | 右回転 90 度 | `O(H × W)` |
| [`RotateR180`](#rotater180) | 右回転 180 度 | `O(H × W)` |
| [`RotateR270`](#rotater270) | 右回転 270 度 | `O(H × W)` |
| [`RotateL90`](#rotatel90) | 左回転 90 度 | `O(H × W)` |
| [`RotateL180`](#rotatel180) | 左回転 180 度 | `O(H × W)` |
| [`RotateL270`](#rotatel270) | 左回転 270 度 | `O(H × W)` |
| [`lrswap`](#lrswap) | 左右反転 | `O(H × W)` |
| [`udswap`](#udswap) | 上下反転 | `O(H × W)` |
| [`naname`](#naname) | 対角反転 | `O(H × W)` |
| [`GridEqual`](#gridequal) | 二つのグリッドが同じか判定 | `O(H × W)` |

---

# 1. 前提

このクラスでは、配列の形が矩形であることを前提にしています。

```csharp
var grid = new List<T[]>
{
    new[] { 'a', 'b', 'c' },
    new[] { 'd', 'e', 'f' },
    new[] { 'g', 'h', 'i' }
};
```

のような形を想定しています。

---

# RotateR90

```csharp
public static List<T[]> RotateR90<T>(List<T[]> a)
public static List<List<T>> RotateR90<T>(List<List<T>> a)
```

## 意味

グリッドを右に 90 度回転させます。

## 例

```csharp
var a = new List<char[]>
{
    new[] { 'a', 'b' },
    new[] { 'c', 'd' }
};

var b = Rotate.RotateR90(a);
```

元の状態:

```text
a b
c d
```

回転後:

```text
b d
a c
```

## 計算量

高さを `H`、幅を `W` とすると、

* 時間計算量: `O(H × W)`
* 空間計算量: `O(H × W)`

---

# RotateR180

```csharp
public static List<T[]> RotateR180<T>(List<T[]> a)
public static List<List<T>> RotateR180<T>(List<List<T>> a)
```

## 意味

180 度回転させます。

```text
abc
def
ghi
```

を

```text
ihg
fed
cba
```

に変えます。

## 計算量

* 時間計算量: `O(H × W)`
* 空間計算量: `O(H × W)`

---

# RotateR270

```csharp
public static List<T[]> RotateR270<T>(List<T[]> a)
public static List<List<T>> RotateR270<T>(List<List<T>> a)
```

## 意味

右回転 270 度、つまり左回転 90 度と同じです。

---

# RotateL90

```csharp
public static List<T[]> RotateL90<T>(List<T[]> a)
public static List<List<T>> RotateL90<T>(List<List<T>> a)
```

## 意味

左に 90 度回転させます。

内部では `RotateR270` を呼び出しています。

---

# RotateL180

```csharp
public static List<T[]> RotateL180<T>(List<T[]> a)
public static List<List<T>> RotateL180<T>(List<List<T>> a)
```

## 意味

180 度回転です。`RotateR180` と同じです。

---

# RotateL270

```csharp
public static List<T[]> RotateL270<T>(List<T[]> a)
public static List<List<T>> RotateL270<T>(List<List<T>> a)
```

## 意味

左に 270 度回転します。`RotateR90` と同じ結果になります。

---

# lrswap

```csharp
public static List<T[]> lrswap<T>(List<T[]> a)
```

## 意味

左右反転します。

### 例

```text
abc
def
ghi
```

→

```text
cba
fed
ihg
```

## 計算量

* 時間計算量: `O(H × W)`
* 空間計算量: `O(H × W)`

---

# udswap

```csharp
public static List<T[]> udswap<T>(List<T[]> a)
```

## 意味

上下反転します。

### 例

```text
abc
def
ghi
```

→

```text
ghi
def
abc
```

## 計算量

* 時間計算量: `O(H × W)`
* 空間計算量: `O(H × W)`

---

# naname

```csharp
public static List<T[]> naname<T>(List<T[]> a)
```

## 意味

斜めに反転したような変換をします。

### 例

```text
abc
def
ghi
```

→

```text
adg
beh
cfi
```

これは対角線を軸にしたような動作です。

## 計算量

* 時間計算量: `O(H × W)`
* 空間計算量: `O(H × W)`

---

# GridEqual

```csharp
public static bool GridEqual<T>(List<T[]> a, List<T[]> b)
```

## 意味

2つのグリッドが同じ形・同じ内容かどうかを判定します。

## 例

```csharp
bool ok = Rotate.GridEqual(grid1, grid2);
```

## 計算量

* 時間計算量: `O(H × W)`
* 空間計算量: `O(1)`

---

# まとめ

`Rotate` は、

* 盤面回転
* マス目の向きを変える
* 反転をまとめて処理したい

ときに非常に便利です。

競技プログラミングでは、数独・迷路・グリッド探索・図形変換などで頻繁に使うので、知っておくと作業がかなり楽になります。
