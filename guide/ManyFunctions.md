# IndexList / PrefixSum 解説書

このファイルには、配列や列挙可能なデータに対して位置情報や累積和を簡単に扱うためのユーティリティがまとめられています。

主に次の2つのクラスがあります。

* `IndexList` — 各要素が出現したインデックス一覧を取得する
* `PrefixSum` — 累積和を効率的に計算する

---

# 目次

| クラス / メソッド | 概要 | 計算量 |
| --- | --- | --- |
| [`IndexList.Counter`](#indexlistcounter) | 要素ごとの出現位置一覧を取得する | `O(N)` |
| [`IndexList.CounterLong`](#indexlistcounterlong) | `long` インデックス版を取得する | `O(N)` |
| [`IndexList.CounterInt`](#indexlistcounterint) | 要素ごとの出現回数を取得する | `O(N)` |
| [`IndexList.CounterLongInt`](#indexlistcounterlongint) | `long` 型の出現回数を取得する | `O(N)` |
| [`IndexList.CounterSet`](#indexlistcounterset) | 各要素に対応する位置を `SortedSet` で返す | `O(N log N)` |
| [`IndexList.CounterSetLong`](#indexlistcountersetlong) | `long` 版の `SortedSet` を返す | `O(N log N)` |
| [`PrefixSum.PSIntArray`](#prefixsumpsintarray) | `int[]` の累積和を作る | `O(N)` |
| [`PrefixSum.PSLongArray`](#prefixsumpslongarray) | `int[]` → `long[]` の累積和を作る | `O(N)` |
| [`PrefixSum.PSIntList`](#prefixsumpsintlist) | `List<int>` の累積和を作る | `O(N)` |
| [`PrefixSum.PSLongList`](#prefixsumpslonglist) | `List<long>` の累積和を作る | `O(N)` |

---

# IndexList

`IndexList` は、要素をキーにして「どの位置に出現したか」をまとめて管理するためのユーティリティです。

頻繁に使うのは、

```csharp
var positions = IndexList.Counter(items);
```

のような形です。

---

# Counter

```csharp
public static Dictionary<T, List<int>> Counter<T>(IEnumerable<T> items)
```

## 意味

各要素が何番目に現れたかを記録します。

## 例

```csharp
var a = new[] { 3, 1, 3, 2, 1 };
var pos = IndexList.Counter(a);

foreach (var kv in pos)
{
    Console.WriteLine($"{kv.Key}: {string.Join(", ", kv.Value)}");
}
```

出力例:

```text
3: 0, 2
1: 1, 4
2: 3
```

つまり、

```text
3 は 0 番目と 2 番目に出現した
1 は 1 番目と 4 番目に出現した
```

という情報を保持しています。

## 計算量

* 時間計算量: `O(N)`
* 空間計算量: `O(N)`

---

# CounterLong

```csharp
public static Dictionary<T, List<long>> CounterLong<T>(IEnumerable<T> items)
```

## 意味

インデックスを `int` ではなく `long` で持ちたい場合に使います。

大きな配列や非常に大きなデータセットで役立ちます。

## 計算量

* 時間計算量: `O(N)`
* 空間計算量: `O(N)`

---

# CounterInt

```csharp
public static Dictionary<T, int> CounterInt<T>(IEnumerable<T> items)
```

## 意味

各要素の出現回数を数えます。

## 例

```csharp
var a = new[] { 1, 2, 1, 3, 1 };
var cnt = IndexList.CounterInt(a);

Console.WriteLine(cnt[1]);
```

出力：

```text
3
```

## 計算量

* 時間計算量: `O(N)`
* 空間計算量: `O(K)`

ここで `K` は種類数です。

---

# CounterLongInt

```csharp
public static Dictionary<T, long> CounterLongInt<T>(IEnumerable<T> items)
```

## 意味

出現回数を `long` で扱いたいときに使います。

大量データのカウントに向いています。

## 計算量

* 時間計算量: `O(N)`
* 空間計算量: `O(K)`

---

# CounterSet

```csharp
public static Dictionary<T, SortedSet<int>> CounterSet<T>(IEnumerable<T> items)
```

## 意味

各要素がどこに出現したかを、`SortedSet` として保存します。

List に重複が入るのではなく、重複を自動で整理したい場合に有効です。

## 例

```csharp
var a = new[] { 5, 2, 5, 7, 2 };
var set = IndexList.CounterSet(a);

Console.WriteLine(string.Join(", ", set[5]));
```

出力：

```text
0, 2
```

## 計算量

* 時間計算量: `O(N log N)`
* 空間計算量: `O(N)`

---

# CounterSetLong

```csharp
public static Dictionary<T, SortedSet<long>> CounterSetLong<T>(IEnumerable<T> items)
```

## 意味

`CounterSet` の `long` 版です。

大きなインデックスや長い列に対応します。

---

# PrefixSum

`PrefixSum` は、累積和を簡単に計算するためのクラスです。

競技プログラミングでは、

```text
配列の区間和を O(1) で求めたい
```

ときに非常に便利です。

---

# PSIntArray

```csharp
public static int[] PSIntArray(IReadOnlyList<int> a)
```

## 意味

`a` の累積和配列を作ります。

たとえば、

```csharp
int[] a = { 1, 3, 5 };
var sum = PrefixSum.PSIntArray(a);
```

のとき、`sum` は次のようになります。

```text
[0, 1, 4, 9]
```

`sum[i]` は元の配列の先頭から `i-1` までの総和を表します。

## 例

```csharp
int[] a = { 2, 4, 6 };
var ps = PrefixSum.PSIntArray(a);

Console.WriteLine(string.Join(" ", ps));
```

出力：

```text
0 2 6 12
```

## 計算量

* 時間計算量: `O(N)`
* 空間計算量: `O(N)`

---

# PSLongArray

```csharp
public static long[] PSLongArray(IReadOnlyList<int> a)
public static long[] PSLongArray(IReadOnlyList<long> a)
```

## 意味

`int` 配列や `long` 配列の累積和を `long[]` で返します。

合計が大きくなる可能性がある場合に使います。

## 計算量

* 時間計算量: `O(N)`
* 空間計算量: `O(N)`

---

# PSIntList

```csharp
public static List<int> PSIntList(IReadOnlyList<int> a)
```

## 意味

`List<int>` で累積和を作ります。

## 例

```csharp
var a = new List<int> { 1, 2, 3 };
var ps = PrefixSum.PSIntList(a);

Console.WriteLine(string.Join(" ", ps));
```

出力：

```text
0 1 3 6
```

## 計算量

* 時間計算量: `O(N)`
* 空間計算量: `O(N)`

---

# PSLongList

```csharp
public static List<long> PSLongList(IReadOnlyList<int> a)
public static List<long> PSLongList(IReadOnlyList<long> a)
```

## 意味

`List<long>` で累積和を作る版です。

オーバーフローを避けたいときに使います。

## 例

```csharp
var a = new List<long> { 1000000000L, 2000000000L };
var ps = PrefixSum.PSLongList(a);

Console.WriteLine(string.Join(" ", ps));
```

## 計算量

* 時間計算量: `O(N)`
* 空間計算量: `O(N)`

---

# まとめ

`IndexList` は、

* 各値の出現位置
* 出現回数
* 位置集合をまとめて管理したい

ときに便利です。

一方 `PrefixSum` は、

* 区間和を求めたい
* 累積の情報を頻繁に参照したい

ときに強いです。

特に競技プログラミングでは、

```csharp
var ps = PrefixSum.PSIntArray(a);
```

のようにして、区間和の計算を高速化する用途が非常に多いです。

---

# 実用例

```csharp
var values = new[] { 5, 1, 3, 1, 4 };
var pos = IndexList.Counter(values);
var ps = PrefixSum.PSIntArray(values);

Console.WriteLine(pos[1][0]);
Console.WriteLine(ps[3]);
```

このように、位置情報と累積和を組み合わせて使うと、問題の解法がかなり整理しやすくなります。
