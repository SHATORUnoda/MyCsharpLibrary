# Treap<T> クラス 解説書

## 目次

### 基本

| メソッド / プロパティ                                        | 概要           |           計算量 |
| --------------------------------------------------- | ------------ | ------------: |
| [Treap()](#constructor)                             | 空の Treap を作成 |          O(1) |
| [Treap(T value)](#constructor-value)                | 1要素から作成      |   期待 O(log N) |
| [Treap(IEnumerable<T> values)](#constructor-values) | 列挙可能な要素から作成  | 期待 O(N log N) |
| [Count](#count)                                     | 要素数を取得       |          O(1) |
| [IsEmpty](#isempty)                                 | 空か判定         |          O(1) |

### 要素操作

| メソッド / プロパティ          | 概要         |         計算量 |
| --------------------- | ---------- | ----------: |
| [Add](#add)           | 要素を追加      | 期待 O(log N) |
| [Remove](#remove)     | 指定値を1つ削除   | 期待 O(log N) |
| [RemoveAt](#removeat) | 指定位置の要素を削除 | 期待 O(log N) |
| [Contains](#contains) | 値が存在するか判定  | 期待 O(log N) |
| [Clear](#clear)       | 全要素を削除     |        O(1) |
| [Min](#min)           | 最小値を取得     | 期待 O(log N) |
| [Max](#max)           | 最大値を取得     | 期待 O(log N) |

### インデックス操作

| メソッド / プロパティ                        | 概要             |         計算量 |
| ----------------------------------- | -------------- | ----------: |
| [this[int index]](#indexer-int)     | インデックスで要素取得・変更 | 期待 O(log N) |
| [this[Index index]](#indexer-index) | `^1` などで要素取得   | 期待 O(log N) |
| [TryGetAt](#trygetat)               | 範囲外例外を出さずに取得   | 期待 O(log N) |

### 二分探索

| メソッド / プロパティ                    | 概要                     |         計算量 |
| ------------------------------- | ---------------------- | ----------: |
| [BinarySearch](#binarysearch)   | 指定値の位置を検索              | 期待 O(log N) |
| [MinBinary](#minbinary)         | `value` 未満の要素数から位置を求める | 期待 O(log N) |
| [LowerBound](#lowerbound)       | `value` 以上の最小要素        | 期待 O(log N) |
| [UpperBound](#upperbound)       | `value` より大きい最小要素      | 期待 O(log N) |
| [TryLowerBound](#trylowerbound) | LowerBound の安全版        | 期待 O(log N) |
| [TryUpperBound](#tryupperbound) | UpperBound の安全版        | 期待 O(log N) |

### 列挙

| メソッド / プロパティ                    | 概要          |  計算量 |
| ------------------------------- | ----------- | ---: |
| [GetEnumerator](#getenumerator) | ソート順に全要素を列挙 | O(N) |

---

# 1. Treap とは

`Treap<T>` は、

* **二分探索木（BST）**
* **ヒープ（Heap）**

を組み合わせた平衡二分探索木です。

このクラスでは、

```text
Value    → 二分探索木として昇順
Priority → ヒープとして優先度順
```

となるように管理しています。

例えば、

```text
        5
       / \
      2   8
     / \   \
    1   3   10
```

のような木を作ります。

ただし、通常の BST と違い、各ノードにランダムな `Priority` を持たせています。

```text
Value:    5
Priority: 827364
```

のようにランダムな優先度を割り当てることで、木が極端に偏る可能性を低くしています。

そのため、各種操作が**期待 O(log N)** で実行できます。

---

# 2. この実装の特徴

この `Treap<T>` には以下の特徴があります。

* 重複要素を扱える
* 同じ値を1つのノードにまとめる
* `Count` で重複を含めた総要素数を取得できる
* 負のインデックスに対応
* `Min` / `Max` に対応
* 二分探索に対応
* `LowerBound` / `UpperBound` に対応
* `^1` などの `Index` に対応
* `IEnumerable<T>` を実装しているため `foreach` が使える

特に重要なのが、**同じ値を `Count` でまとめている**ことです。

例えば、

```csharp
var treap = new Treap<int>();

treap.Add(5);
treap.Add(5);
treap.Add(5);
```

とすると、

```text
Node
Value = 5
Count = 3
Size = 3
```

という1つのノードとして管理されます。

---

# 3. 基本的な使い方

<a id="basic-usage"></a>

## 基本例

```csharp
var treap = new Treap<int>();

treap.Add(5);
treap.Add(2);
treap.Add(8);
treap.Add(5);

Console.WriteLine(treap.Count);   // 4
Console.WriteLine(treap.Min);     // 2
Console.WriteLine(treap.Max);     // 8
```

内部では値がソートされた状態で管理されます。

```text
2 5 5 8
```

---

## foreach

`IEnumerable<T>` を実装しているため、

```csharp
foreach (var x in treap)
{
    Console.WriteLine(x);
}
```

と書けます。

出力順は常に昇順です。

```text
2
5
5
8
```

---

# 4. コンストラクタ

<a id="constructor"></a>

## `Treap()`

空の Treap を作成します。

```csharp
var treap = new Treap<int>();
```

初期状態：

```text
Count = 0
IsEmpty = true
```

### 計算量

**O(1)**

---

<a id="constructor-value"></a>

## `Treap(T value)`

1つの値を持つ Treap を作成します。

```csharp
var treap = new Treap<int>(10);
```

これは、

```csharp
var treap = new Treap<int>();
treap.Add(10);
```

とほぼ同じです。

### 計算量

期待 **O(log N)**

ただし最初の要素なので実際には O(1) です。

---

<a id="constructor-values"></a>

## `Treap(IEnumerable<T> values)`

列挙可能な要素から Treap を作成します。

```csharp
var treap = new Treap<int>(
    new[] { 5, 1, 8, 3, 3 }
);
```

結果：

```text
1 3 3 5 8
```

内部では、

```csharp
foreach (var value in values)
    Add(value);
```

としているため、各要素を順番に追加しています。

### 計算量

要素数を `N` とすると、

**期待 O(N log N)**

---

# 5. Count

<a id="count"></a>

## `Count`

Treap に含まれる**総要素数**を返します。

重複も別々に数えます。

```csharp
var treap = new Treap<int>();

treap.Add(3);
treap.Add(3);
treap.Add(5);

Console.WriteLine(treap.Count);
```

出力：

```text
3
```

内部では各ノードの

```csharp
Size
```

を利用しています。

### 計算量

**O(1)**

---

# 6. IsEmpty

<a id="isempty"></a>

## `IsEmpty`

Treap が空かどうかを返します。

```csharp
if (treap.IsEmpty)
{
    Console.WriteLine("空です");
}
```

以下とほぼ同じ意味です。

```csharp
treap.Count == 0
```

### 計算量

**O(1)**

---

# 7. Add

<a id="add"></a>

## `Add(T value)`

値を Treap に追加します。

```csharp
var treap = new Treap<int>();

treap.Add(10);
treap.Add(5);
treap.Add(20);
```

結果：

```text
5 10 20
```

---

## 重複

重複も追加できます。

```csharp
treap.Add(10);
treap.Add(10);
treap.Add(10);
```

同じ値の場合、新しい Node を作るのではなく、

```csharp
n.Count++;
```

として同じ Node の `Count` を増やします。

例えば、

```text
Value = 10
Count = 4
```

となります。

### 計算量

期待 **O(log N)**

---

# 8. Remove

<a id="remove"></a>

## `Remove(T value)`

指定した値を1つ削除します。

```csharp
treap.Remove(5);
```

成功した場合は `true`、存在しなかった場合は `false` を返します。

```csharp
if (treap.Remove(5))
{
    Console.WriteLine("削除しました");
}
```

---

## 重複している場合

例えば、

```text
2 5 5 5 8
```

に対して、

```csharp
treap.Remove(5);
```

を実行すると、

```text
2 5 5 8
```

になります。

内部では `Count` を1減らします。

```csharp
if (n.Count > 1)
{
    n.Count--;
}
```

`Count == 1` の場合だけ Node 自体を削除します。

### 計算量

期待 **O(log N)**

---

# 9. RemoveAt

<a id="removeat"></a>

## `RemoveAt(int index)`

ソート順で `index` 番目の要素を削除して、その値を返します。

```csharp
var x = treap.RemoveAt(2);
```

例えば、

```text
index:  0  1  2  3
value:  1  3  5  8
```

なら、

```csharp
treap.RemoveAt(2);
```

によって `5` が削除されます。

---

## 負のインデックス

負の値にも対応しています。

```csharp
treap.RemoveAt(-1);
```

は最後の要素を削除します。

```text
-1 → 最後
-2 → 最後から2番目
```

### 計算量

期待 **O(log N)**

---

# 10. Contains

<a id="contains"></a>

## `Contains(T value)`

指定した値が存在するか判定します。

```csharp
if (treap.Contains(10))
{
    Console.WriteLine("存在します");
}
```

存在すれば `true`、存在しなければ `false` です。

二分探索木なので、比較結果によって

```text
value < 現在の値 → 左
value > 現在の値 → 右
value == 現在の値 → 発見
```

と探索します。

### 計算量

期待 **O(log N)**

---

# 11. Min

<a id="min"></a>

## `Min`

Treap 内の最小値を取得します。

```csharp
int minimum = treap.Min;
```

BST では最小値は最も左にあるため、

```text
        5
       /
      3
     /
    1 ← Min
```

のように左へ進み続けます。

空の Treap に対して使用すると `InvalidOperationException` が発生します。

### 計算量

期待 **O(log N)**

---

# 12. Max

<a id="max"></a>

## `Max`

Treap 内の最大値を取得します。

```csharp
int maximum = treap.Max;
```

最大値は最も右にあるため、右へ進み続けます。

空の場合は `InvalidOperationException` が発生します。

### 計算量

期待 **O(log N)**

---

# 13. int インデクサ

<a id="indexer-int"></a>

## `this[int index]`

ソート順で `index` 番目の要素を取得できます。

```csharp
int x = treap[3];
```

例えば、

```text
1 3 3 5 8
```

なら、

```csharp
treap[0] // 1
treap[1] // 3
treap[2] // 3
treap[3] // 5
treap[4] // 8
```

となります。

---

## 負のインデックス

負のインデックスにも対応しています。

```csharp
treap[-1]
```

は最後の要素です。

```csharp
treap[-2]
```

は最後から2番目です。

これは、

```csharp
if (index < 0)
    index += Count;
```

によって実現されています。

---

## 値の変更

setter も用意されています。

```csharp
treap[2] = 100;
```

内部では、

```csharp
EraseAt(ref root, index);
Add(value);
```

を行っています。

つまり、元の値を削除してから新しい値を追加しています。

そのため、変更後も Treap のソート順が維持されます。

### 計算量

期待 **O(log N)**

---

# 14. Index インデクサ

<a id="indexer-index"></a>

## `this[Index index]`

C# の `Index` を利用できます。

特に `^1` が便利です。

```csharp
int last = treap[^1];
```

これは、

```csharp
treap[treap.Count - 1]
```

と同じです。

例えば、

```csharp
treap[^1] // 最後
treap[^2] // 最後から2番目
```

とできます。

### 計算量

期待 **O(log N)**

---

# 15. BinarySearch

<a id="binarysearch"></a>

## `BinarySearch(T value)`

指定した値の位置を返します。

```csharp
int index = treap.BinarySearch(5);
```

例えば、

```text
1 3 5 5 8
```

の場合、

```csharp
BinarySearch(5)
```

は `2` を返します。

重複がある場合は、**最初に探索で見つかった `5` の位置**が返ります。

必ず左端の `5` を返すとは限りません。

存在しない場合は、

```text
-1
```

を返します。

### 計算量

期待 **O(log N)**

---

# 16. MinBinary

<a id="minbinary"></a>

## `MinBinary(T value)`

`value` より小さい要素の個数を利用して位置を求めます。

実装では、

```csharp
if (cmp <= 0)
    n = n.Left;
else
{
    rank += Size(n.Left) + n.Count;
    n = n.Right;
}
```

として探索します。

ただし、このメソッドは一般的な `LowerBound` / `UpperBound` と比べると返り値の意味が分かりにくいため、利用時には注意が必要です。

### 計算量

期待 **O(log N)**

---

# 17. LowerBound

<a id="lowerbound"></a>

## `LowerBound(T value)`

`value` 以上となる最小の要素を返します。

つまり、

```text
最初の x >= value
```

です。

例えば、

```text
1 3 5 5 8
```

に対して、

```csharp
treap.LowerBound(4);
```

なら、

```text
5
```

が返ります。

```csharp
treap.LowerBound(5);
```

なら、

```text
5
```

です。

---

## 存在しない場合

```text
1 3 5
```

に対して、

```csharp
treap.LowerBound(10);
```

を実行すると、該当要素がないため `InvalidOperationException` が発生します。

### 計算量

期待 **O(log N)**

---

# 18. UpperBound

<a id="upperbound"></a>

## `UpperBound(T value)`

`value` より大きい最小の要素を返します。

つまり、

```text
最初の x > value
```

です。

例えば、

```text
1 3 5 5 8
```

に対して、

```csharp
treap.UpperBound(5);
```

なら、

```text
8
```

が返ります。

### LowerBound との違い

```text
value = 5

LowerBound → 5
UpperBound → 8
```

です。

### 計算量

期待 **O(log N)**

---

# 19. TryLowerBound

<a id="trylowerbound"></a>

## `TryLowerBound(T value, out T result)`

`LowerBound` の例外を発生させない版です。

```csharp
if (treap.TryLowerBound(5, out int x))
{
    Console.WriteLine(x);
}
else
{
    Console.WriteLine("存在しません");
}
```

該当する値があれば `true` を返し、`result` に値を格納します。

存在しなければ、

```csharp
false
```

を返します。

### 計算量

期待 **O(log N)**

---

# 20. TryUpperBound

<a id="tryupperbound"></a>

## `TryUpperBound(T value, out T result)`

`UpperBound` の例外を発生させない版です。

```csharp
if (treap.TryUpperBound(5, out int x))
{
    Console.WriteLine(x);
}
```

`value` より大きい値が存在すれば、その最小値を取得します。

### 計算量

期待 **O(log N)**

---

# 21. TryGetAt

<a id="trygetat"></a>

## `TryGetAt(int index, out T result)`

指定位置の要素を取得します。

通常のインデクサと違い、範囲外でも例外を発生させません。

```csharp
if (treap.TryGetAt(3, out int x))
{
    Console.WriteLine(x);
}
```

範囲外なら、

```csharp
false
```

になります。

---

## 負のインデックス

`RemoveAt` やインデクサと同じく負のインデックスにも対応しています。

```csharp
treap.TryGetAt(-1, out int x);
```

なら最後の要素を取得します。

### 計算量

期待 **O(log N)**

---

# 22. Clear

<a id="clear"></a>

## `Clear()`

Treap を空にします。

```csharp
treap.Clear();
```

実装では、

```csharp
root = null;
```

としているだけです。

以前の Node は参照されなくなるため、GC によって回収可能になります。

### 計算量

**O(1)**

---

# 23. foreach / GetEnumerator

<a id="getenumerator"></a>

## `GetEnumerator()`

Treap の要素を昇順に列挙します。

```csharp
foreach (var x in treap)
{
    Console.WriteLine(x);
}
```

例えば、

```text
5
2
8
2
```

を追加した場合、

```text
2
2
5
8
```

の順番で列挙されます。

---

## 内部処理

スタックを使用して非再帰的な中順巡回を行っています。

```text
        5
       / \
      2   8
     / \
    1   3
```

に対して、

```text
左 → 自分 → 右
```

の順に訪問します。

そのため、

```text
1 2 3 5 8
```

と昇順になります。

Node に重複数 `Count` が保存されているため、

```csharp
for (int i = 0; i < cur.Count; i++)
    yield return cur.Value;
```

によって重複も正しく列挙されます。

### 計算量

全要素を列挙すると **O(N)**

追加メモリは木の高さに比例し、

**期待 O(log N)**

です。

---

# 24. 内部データ構造

この Treap の Node は以下の情報を持っています。

```csharp
private class Node
{
    public T Value;
    public int Priority;
    public int Count;
    public int Size;
    public Node Left;
    public Node Right;
}
```

それぞれの意味は次の通りです。

| フィールド      | 意味           |
| ---------- | ------------ |
| `Value`    | 保存している値      |
| `Priority` | Treap の優先度   |
| `Count`    | 同じ値の個数       |
| `Size`     | 部分木に含まれる総要素数 |
| `Left`     | 左の子          |
| `Right`    | 右の子          |

---

# 25. Count と Size の違い

ここは非常に重要です。

例えば、

```text
        5 (Count=3)
       /         \
   2 (Count=1)  8 (Count=2)
```

なら、

```text
5 の Count = 3
```

ですが、

```text
5 の Size
= 1 + 3 + 2
= 6
```

です。

つまり、

```text
Count → その値が何個あるか

Size → その部分木全体に何個の要素があるか
```

です。

`RemoveAt` や `Kth` などはこの `Size` を利用しています。

---

# 26. Update

<a id="update"></a>

## `Update(Node n)`

Node の `Size` を再計算します。

```csharp
n.Size =
    n.Count
    + Size(n.Left)
    + Size(n.Right);
```

### 計算量

**O(1)**

---

# 27. 回転

Treap では、Priority の大小関係を維持するために回転を行います。

## RotateRight

```text
      y                 x
     / \               / \
    x   C     →       A   y
   / \                   / \
  A   B                 B   C
```

## RotateLeft

```text
    x                     y
   / \                   / \
  A   y       →         x   C
     / \               / \
    B   C             A   B
```

これによって BST と Heap の両方の性質を維持します。

### 計算量

**O(1)**

---

# 28. Insert

<a id="internal-insert"></a>

## `Insert(ref Node n, T value)`

`Add` の内部で使用されます。

まず BST として、

```text
value < n.Value → 左
value > n.Value → 右
value == n.Value → Count++
```

と進みます。

新しい Node が追加された場合、Priority が親より大きければ回転します。

```csharp
if (n.Left.Priority > n.Priority)
    RotateRight(ref n);
```

または、

```csharp
if (n.Right.Priority > n.Priority)
    RotateLeft(ref n);
```

### 計算量

期待 **O(log N)**

最悪の場合は木が偏って、

**O(N)**

になる可能性があります。

---

# 29. Erase

<a id="internal-erase"></a>

## `Erase(ref Node n, T value)`

`Remove` の内部処理です。

値を BST として探索します。

値が見つかった場合、

```text
Count > 1
```

なら、

```csharp
Count--;
```

するだけです。

`Count == 1` なら Node 自体を削除します。

Node を削除するときは、

```csharp
Merge(ref n, n.Left, n.Right);
```

として左右の部分木を結合します。

### 計算量

期待 **O(log N)**

---

# 30. EraseAt

<a id="internal-eraseat"></a>

## `EraseAt(ref Node n, int index)`

インデックスを指定して削除する内部メソッドです。

`Size` を利用して、

```text
左部分木のサイズ
```

と比較しながら目的位置を探します。

例えば、

```text
leftSize = Size(n.Left)
```

として、

```text
index < leftSize
```

なら左へ進みます。

```text
index >= leftSize + Count
```

なら右へ進みます。

それ以外なら現在の Node の値を削除します。

### 計算量

期待 **O(log N)**

---

# 31. Kth

<a id="internal-kth"></a>

## `Kth(Node n, int k)`

`k` 番目の要素を取得する内部メソッドです。

`Size` と `Count` を利用して、

```text
左部分木
現在の Node
右部分木
```

のどこに `k` 番目が存在するかを判断します。

例えば、

```text
leftSize = 4
Count = 3
```

なら、

```text
0 ～ 3     → 左部分木
4 ～ 6     → 現在の値
7 以降     → 右部分木
```

となります。

### 計算量

期待 **O(log N)**

---

# 32. Merge

<a id="internal-merge"></a>

## `Merge(ref Node n, Node a, Node b)`

2つの Treap を結合します。

前提として、

```text
a の全要素 < b の全要素
```

である必要があります。

Priority の大きい方を新しい根にします。

```csharp
if (a.Priority > b.Priority)
```

なら `a` 側を根にし、

```csharp
Merge(ref a.Right, a.Right, b);
```

と右部分木へ再帰します。

### 計算量

期待 **O(log N)**

---

# 33. なぜ Treap は高速なのか

通常の BST は、挿入順によっては、

```text
1
 \
  2
   \
    3
     \
      4
       \
        5
```

のように完全に偏る可能性があります。

この場合、

```text
探索 O(N)
```

になってしまいます。

Treap では各 Node にランダムな Priority を割り当てます。

```text
Value     Priority

5         123
2         932
8         456
3         781
```

Priority に従って回転することで、平均的には木の高さが

```text
O(log N)
```

になります。

そのため、

```text
Add
Remove
Contains
Kth
BinarySearch
LowerBound
UpperBound
```

などが期待 O(log N) になります。

---

# 34. 計算量まとめ

`N` を要素数とします。

| 操作              |      期待計算量 | 最悪計算量 |
| --------------- | ---------: | ----: |
| `Treap()`       |       O(1) |  O(1) |
| `Treap(value)`  |       O(1) |  O(1) |
| `Treap(values)` | O(N log N) | O(N²) |
| `Count`         |       O(1) |  O(1) |
| `IsEmpty`       |       O(1) |  O(1) |
| `Add`           |   O(log N) |  O(N) |
| `Remove`        |   O(log N) |  O(N) |
| `RemoveAt`      |   O(log N) |  O(N) |
| `Contains`      |   O(log N) |  O(N) |
| `Min`           |   O(log N) |  O(N) |
| `Max`           |   O(log N) |  O(N) |
| `this[int]`     |   O(log N) |  O(N) |
| `this[Index]`   |   O(log N) |  O(N) |
| `BinarySearch`  |   O(log N) |  O(N) |
| `MinBinary`     |   O(log N) |  O(N) |
| `LowerBound`    |   O(log N) |  O(N) |
| `UpperBound`    |   O(log N) |  O(N) |
| `TryLowerBound` |   O(log N) |  O(N) |
| `TryUpperBound` |   O(log N) |  O(N) |
| `TryGetAt`      |   O(log N) |  O(N) |
| `Clear`         |       O(1) |  O(1) |
| `foreach`       |       O(N) |  O(N) |

---

# 35. 典型的な使用例

## ソート済み集合として使用

```csharp
var treap = new Treap<int>();

treap.Add(10);
treap.Add(3);
treap.Add(7);
treap.Add(3);
treap.Add(20);

foreach (var x in treap)
{
    Console.Write($"{x} ");
}
```

出力：

```text
3 3 7 10 20
```

---

## 最小値・最大値

```csharp
Console.WriteLine(treap.Min);
Console.WriteLine(treap.Max);
```

---

## 値の存在判定

```csharp
if (treap.Contains(10))
{
    Console.WriteLine("10 exists");
}
```

---

## k 番目の要素

```csharp
int x = treap[2];
```

---

## 最後の要素

```csharp
int x = treap[^1];
```

---

## 値を削除

```csharp
treap.Remove(10);
```

---

## k 番目を削除

```csharp
int x = treap.RemoveAt(2);
```

---

## LowerBound

```csharp
int x = treap.LowerBound(8);
```

これは、

```text
8以上の最小値
```

を返します。

---

## UpperBound

```csharp
int x = treap.UpperBound(8);
```

これは、

```text
8より大きい最小値
```

を返します。

---

# 36. `SortedSet<T>` / `SortedDictionary<TKey,TValue>` との違い

この `Treap<T>` は、単純な集合としてだけでなく、

**「重複を含むソート済み列」**

として使えるのが大きな特徴です。

例えば、

```text
1 2 2 2 5 8
```

のような状態をそのまま管理できます。

また、

```csharp
treap[3]
```

によって k 番目の要素を取得できます。

これは通常の `SortedSet<T>` では直接できません。

---

# 37. この Treap が向いている問題

特に以下のような競技プログラミングの問題で便利です。

### 1. 動的な集合

```text
追加
削除
存在判定
```

を大量に行う問題。

### 2. 重複を含む集合

```text
1 1 1 2 2 5
```

のように同じ値を複数管理したい場合。

### 3. k 番目の値

```text
現在の集合の k 番目に小さい値
```

を高速に求めたい場合。

### 4. LowerBound / UpperBound

```text
x以上で最小
xより大きくて最小
```

を高速に求めたい場合。

### 5. 動的な中央値

例えば要素数が常に奇数なら、

```csharp
treap[treap.Count / 2]
```

で中央値を取得できます。

---

# 38. 注意点

## `T` は比較可能である必要がある

クラス宣言が、

```csharp
public class Treap<T> : IEnumerable<T>
    where T : IComparable<T>
```

となっているため、`T` は `IComparable<T>` を実装している必要があります。

例えば、

```csharp
Treap<int>
Treap<long>
Treap<string>
```

などは使用できます。

---

## 値の比較で順序が決まる

この Treap は、

```csharp
value.CompareTo(n.Value)
```

によって順序を決めています。

そのため、`CompareTo` が正しく順序を定義している必要があります。

---

## 最悪 O(N) の可能性

Treap は**確率的な平衡木**です。

平均的には非常に高速ですが、Priority のランダム性によっては木が偏る可能性があります。

したがって厳密には、

```text
期待 O(log N)
```

であり、

```text
最悪 O(log N)
```

ではありません。

競技プログラミングでは通常、Treap の標準的な計算量として期待 O(log N) を利用します。

---

# 39. この実装の重要ポイントまとめ

この `Treap<T>` は、

```text
BST
+
Random Priority
+
Size
+
Count
```

という構成になっています。

特に、

```text
Value
```

でソート順を維持し、

```text
Priority
```

で木のバランスを維持し、

```text
Count
```

で重複をまとめ、

```text
Size
```

で k 番目の要素へのアクセスを可能にしています。

その結果、

```text
Add             期待 O(log N)
Remove          期待 O(log N)
Contains        期待 O(log N)
RemoveAt        期待 O(log N)
k番目取得       期待 O(log N)
BinarySearch    期待 O(log N)
LowerBound      期待 O(log N)
UpperBound      期待 O(log N)
```

を1つのデータ構造で実現できます。

競技プログラミングでは、

> **「重複ありの Ordered Set + k-th 操作 + LowerBound / UpperBound」**

として使うのが特に分かりやすいです。
