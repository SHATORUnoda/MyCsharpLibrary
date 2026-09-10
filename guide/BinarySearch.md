# Binary クラス 解説書

`Binary` クラスは、**ソート済み配列に対する二分探索**を行うためのユーティリティクラスです。

---

# 目次

| メソッド                            | 概要                            | 計算量        |
| ------------------------------- | ----------------------------- | ---------- |
| [`MinBinary`](#minbinary)       | `target` より小さい最大の要素のインデックスを探す | `O(log N)` |
| [`UpperBound`](#upperbound)     | `value` 以下の要素数を求める            | `O(log N)` |
| [`BinarySearch`](#binarysearch) | `value` と一致する要素のインデックスを探す     | `O(log N)` |

---

# 前提

このクラスのメソッドは、基本的に**配列が昇順にソートされていること**を前提としています。

例えば、

```csharp
int[] data = { 1, 3, 3, 5, 7, 9 };
```

のような配列です。

一方、

```csharp
int[] data = { 5, 1, 9, 3, 7 };
```

のようなソートされていない配列では、正しい結果が保証されません。

---

# MinBinary

```csharp
public static int MinBinary<T>(T[] data, T target)
    where T : IComparable<T>
```

## 意味

`target` **より小さい要素の中で最大の要素**のインデックスを返します。

つまり、

```text
data[i] < target
```

を満たす `i` のうち、最も大きいものを探します。

該当する要素が存在しない場合は `-1` を返します。

---

## 例

```csharp
int[] data = { 1, 3, 5, 7, 9 };

int index = Binary.MinBinary(data, 6);

Console.WriteLine(index);
```

出力：

```text
2
```

なぜなら、

```text
data = [1, 3, 5, 7, 9]
             ↑
```

`6` より小さい要素は、

```text
1, 3, 5
```

であり、その中で最大なのは `5` だからです。

`5` のインデックスは `2` なので、結果は `2` です。

---

## もう一つの例

```csharp
int[] data = { 10, 20, 30, 40 };

int index = Binary.MinBinary(data, 10);

Console.WriteLine(index);
```

出力：

```text
-1
```

`10` より小さい要素が存在しないためです。

---

## 動作

例えば、

```text
data = [1, 3, 5, 7, 9]
target = 8
```

の場合、

```text
1 < 8 ✓
3 < 8 ✓
5 < 8 ✓
7 < 8 ✓
9 < 8 ✗
```

なので、答えは `7` のインデックスである `3` です。

このメソッドは二分探索によって、

> **条件 `data[i] < target` を満たす最後の位置**

を探しています。

---

## 使いどころ

例えば、

> `x` 未満で最大の値がどこにあるか知りたい

という場合に使用できます。

```csharp
int[] a = { 10, 20, 30, 40, 50 };

int i = Binary.MinBinary(a, 35);

if (i != -1)
{
    Console.WriteLine(a[i]);
}
```

出力：

```text
30
```

---

## 計算量

要素数を `N` とすると、

* 時間計算量: `O(log N)`
* 空間計算量: `O(1)`

です。

---

# UpperBound

```csharp
public static int UpperBound<T>(T[] data, T value)
    where T : IComparable<T>
```

## 意味

`value` **以下の要素が何個あるか**を返します。

言い換えると、

```text
data[i] <= value
```

を満たす要素の個数です。

同時に、

> `value` より大きい最初の要素のインデックス

でもあります。

---

## 例

```csharp
int[] data = { 1, 3, 3, 5, 7 };

int count = Binary.UpperBound(data, 3);

Console.WriteLine(count);
```

出力：

```text
3
```

配列は、

```text
index:  0  1  2  3  4
value:  1  3  3  5  7
            └──┘
```

となっています。

`3` 以下の要素は、

```text
1, 3, 3
```

の3個なので、結果は `3` です。

---

## 「個数」と「インデックス」の関係

例えば、

```text
data = [1, 3, 3, 5, 7]
value = 3
```

なら、

```text
index:  0  1  2  3  4
value:  1  3  3  5  7
                ↑
              答え
```

`3` より大きい最初の要素 `5` はインデックス `3` にあります。

したがって、

```csharp
Binary.UpperBound(data, 3)
```

は `3` を返します。

これは同時に、

```text
3 以下の要素数 = 3
```

という意味にもなります。

---

## 使用例：個数を数える

例えば、

```csharp
int[] scores = { 10, 20, 20, 30, 40, 50 };

int count = Binary.UpperBound(scores, 30);

Console.WriteLine(count);
```

出力：

```text
4
```

`30` 以下は、

```text
10, 20, 20, 30
```

の4個だからです。

---

## 使用例：`value` より大きい最初の位置を求める

```csharp
int[] a = { 1, 2, 2, 2, 5, 8 };

int index = Binary.UpperBound(a, 2);

if (index < a.Length)
{
    Console.WriteLine(a[index]);
}
```

出力：

```text
5
```

つまり、

```text
2 より大きい最初の要素 = 5
```

です。

---

## `value` が最大値以上の場合

```csharp
int[] a = { 1, 2, 3 };

int index = Binary.UpperBound(a, 100);

Console.WriteLine(index);
```

出力：

```text
3
```

これは、

```text
100 以下の要素が3個
```

という意味です。

また、

```text
index == a.Length
```

なので、

> `value` より大きい要素が存在しない

とも判断できます。

---

## `BinarySearch` との違い

```csharp
Binary.BinarySearch(data, 3);
```

は、

> `3` が存在するか？

を調べます。

一方、

```csharp
Binary.UpperBound(data, 3);
```

は、

> `3` 以下の要素が何個あるか？

を調べます。

例えば、

```text
data = [1, 3, 3, 3, 5]
```

なら、

```text
BinarySearch(3)
    → 1～3のどこか

UpperBound(3)
    → 4
```

となります。

---

## 計算量

要素数を `N` とすると、

* 時間計算量: `O(log N)`
* 空間計算量: `O(1)`

です。

---

# BinarySearch

```csharp
public static int BinarySearch<T>(T[] data, T value)
    where T : IComparable<T>
```

## 意味

配列から `value` と一致する要素を探し、その**インデックス**を返します。

見つからなかった場合は `-1` を返します。

---

## 例

```csharp
int[] data = { 1, 3, 5, 7, 9 };

int index = Binary.BinarySearch(data, 7);

Console.WriteLine(index);
```

出力：

```text
3
```

なぜなら、

```text
index:  0  1  2  3  4
value:  1  3  5  7  9
                  ↑
```

`7` はインデックス `3` にあるからです。

---

## 見つからない場合

```csharp
int[] data = { 1, 3, 5, 7, 9 };

int index = Binary.BinarySearch(data, 6);

Console.WriteLine(index);
```

出力：

```text
-1
```

`6` が配列に存在しないためです。

---

## 使用例：存在判定

```csharp
int[] a = { 2, 4, 6, 8, 10 };

if (Binary.BinarySearch(a, 6) != -1)
{
    Console.WriteLine("存在します");
}
else
{
    Console.WriteLine("存在しません");
}
```

出力：

```text
存在します
```

競技プログラミングでは、

```csharp
if (Binary.BinarySearch(a, x) != -1)
```

のように書くことで、

> `x` が配列に存在するか

を高速に判定できます。

---

## 使用例：インデックスを利用する

```csharp
int[] a = { 10, 20, 30, 40, 50 };

int index = Binary.BinarySearch(a, 30);

if (index != -1)
{
    Console.WriteLine($"インデックス: {index}");
    Console.WriteLine($"値: {a[index]}");
}
```

出力：

```text
インデックス: 2
値: 30
```

---

## 重複要素について

例えば、

```text
data = [1, 3, 3, 3, 5]
```

に対して、

```csharp
Binary.BinarySearch(data, 3);
```

を実行した場合、`3` のインデックスとして、

```text
1
2
3
```

の**どれが返るかは保証されません**。

ただし、返されたインデックスについて、

```csharp
data[index] == 3
```

であることは保証されます。

「最初の `3` の位置」や「最後の `3` の位置」が必要な場合は、境界探索を利用します。

---

## 計算量

要素数を `N` とすると、

* 時間計算量: `O(log N)`
* 空間計算量: `O(1)`

です。

---

# 3つのメソッドの比較

例えば、

```text
data = [1, 3, 3, 5, 7, 9]
```

に対して `3` を調べるとします。

| メソッド                    |              結果 | 意味                   |
| ----------------------- | --------------: | -------------------- |
| `BinarySearch(data, 3)` | `1, 2, 3` のいずれか | `3` の位置              |
| `MinBinary(data, 3)`    |             `0` | `3` より小さい最大値 `1` の位置 |
| `UpperBound(data, 3)`   |             `3` | `3` 以下の個数            |

---

# イメージ

```text
data = [1, 3, 3, 5, 7, 9]
        ↑     ↑
        │     │
        │     └─ UpperBound(3) → 3
        │
        └─ MinBinary(3) → 0
```

より正確には、

```text
MinBinary(data, 3)
    ↓
「3 より小さい最後の位置」
    ↓
index 0

UpperBound(data, 3)
    ↓
「3 より大きい最初の位置」
    ↓
index 3
```

となります。

そのため、`MinBinary` と `UpperBound` を組み合わせると、ある値の**直前・直後の境界**を簡単に求められます。

---

# 型について

各メソッドは、

```csharp
<T>
where T : IComparable<T>
```

となっているため、比較可能な型であれば利用できます。

例えば `int` だけでなく `string` でも使用できます。

```csharp
string[] data =
{
    "apple",
    "banana",
    "cherry",
    "orange"
};

int index = Binary.BinarySearch(data, "cherry");

Console.WriteLine(index);
```

出力：

```text
2
```

`string` は `IComparable<string>` を実装しているため、二分探索で比較できます。

---

# まとめ

このクラスでは、3種類の二分探索を提供しています。

### `MinBinary`

```text
target より小さい最大の要素
```

を探します。

### `UpperBound`

```text
value 以下の要素数
```

または、

```text
value より大きい最初の位置
```

を求めます。

### `BinarySearch`

```text
value と一致する要素の位置
```

を探します。

すべて二分探索を利用しているため、配列の要素数を `N` とすると、基本的に

```text
時間計算量: O(log N)
空間計算量: O(1)
```

です。

競技プログラミングでは、

```text
「一致するものを探す」
    → BinarySearch

「x未満の最後を探す」
    → MinBinary

「x以下が何個あるか・xより大きい最初を探す」
    → UpperBound
```

と覚えておくと使いやすいです。
