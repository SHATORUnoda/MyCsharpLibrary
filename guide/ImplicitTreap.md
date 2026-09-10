# ImplicitTreap<T> クラス 解説書

`ImplicitTreap<T>` は、**配列のように要素を順番に管理しながら、途中への挿入・削除・取得・区間反転などを高速に行えるデータ構造**です。

内部では **Implicit Treap（暗黙Treap）** を使用しています。

通常の `List<T>` では途中への挿入・削除に `O(N)` かかりますが、このクラスでは Treap の `Split` / `Merge` を利用することで、基本的な操作を**期待 `O(log N)`**で処理できます。

また、以下の機能にも対応しています。

* インデックスによる要素取得・変更
* 負のインデックス
* `^1` などの `Index`
* 値の存在判定
* 値による削除
* 先頭・末尾への追加
* 先頭・末尾からの削除
* 全体反転
* 区間反転
* 区間のコピー
* 区間の切り離し
* 複数要素の追加・挿入
* 配列への変換
* `foreach` による列挙

---

# 目次

## 基本情報

| 項目                      | 説明            |
| ----------------------- | ------------- |
| [`Count`](#count)       | 現在の要素数を取得     |
| [`IsEmpty`](#isempty)   | Treap が空か判定   |
| [`Contains`](#contains) | 指定した値が存在するか判定 |

## 要素へのアクセス

| メソッド / プロパティ                            | 説明               | 計算量           |
| --------------------------------------- | ---------------- | ------------- |
| [`this[int index]`](#thisint-index)     | インデックスで取得・変更     | 期待 `O(log N)` |
| [`this[Index index]`](#thisindex-index) | `^1` などを使って取得・変更 | 期待 `O(log N)` |

## 追加

| メソッド                          | 説明           | 計算量                     |
| ----------------------------- | ------------ | ----------------------- |
| [`Insert`](#insert)           | 指定位置に1要素を挿入  | 期待 `O(log N)`           |
| [`AddFirst`](#addfirst)       | 先頭に追加        | 期待 `O(log N)`           |
| [`AddLast`](#addlast)         | 末尾に追加        | 期待 `O(log N)`           |
| [`AddRange`](#addrange)       | 複数要素を末尾に追加   | 期待 `O(M log N)`         |
| [`InsertRange`](#insertrange) | 複数要素を指定位置に挿入 | 期待 `O(M log M + log N)` |

## 削除

| メソッド                    | 説明         | 計算量           |
| ----------------------- | ---------- | ------------- |
| [`Remove`](#remove)     | 指定した値を1つ削除 | 期待 `O(log N)` |
| [`RemoveAt`](#removeat) | 指定位置を削除    | 期待 `O(log N)` |
| [`PopFront`](#popfront) | 先頭を削除して返す  | 期待 `O(log N)` |
| [`PopBack`](#popback)   | 末尾を削除して返す  | 期待 `O(log N)` |
| [`Clear`](#clear)       | すべての要素を削除  | `O(N)`        |

## 区間操作

| メソッド                                           | 説明                  | 計算量                 |
| ---------------------------------------------- | ------------------- | ------------------- |
| [`Reverse()`](#reverse)                        | 全体を反転               | `O(1)`              |
| [`Reverse(left, length)`](#reverseleft-length) | 指定区間を反転             | 期待 `O(log N)`       |
| [`Slice`](#slice)                              | 区間をコピーして新しいTreapを作る | `O(length + log N)` |
| [`Cut`](#cut)                                  | 区間を切り離して新しいTreapにする | `O(length + log N)` |

## 変換・列挙

| メソッド                              | 説明                 | 計算量    |
| --------------------------------- | ------------------ | ------ |
| [`ToArray`](#toarray)             | Treapを配列に変換        | `O(N)` |
| [`GetEnumerator`](#getenumerator) | `foreach` で列挙可能にする | `O(N)` |

## 補足

* [Implicit Treapとは](#implicit-treapとは)
* [内部構造](#内部構造)
* [SplitとMerge](#splitとmerge)
* [Lazy PropagationとReverse](#lazy-propagationとreverse)
* [Parentとインデックス取得](#parentとインデックス取得)
* [値検索用インデックス](#値検索用インデックス)
* [List<T>との比較](#listtとの比較)
* [計算量まとめ](#計算量まとめ)
* [使用例](#使用例)
* [注意点](#注意点)
* [まとめ](#まとめ)

---

# Implicit Treapとは

Implicit Treap は、**配列のインデックスを明示的に保存せず、部分木のサイズから要素の位置を判断するTreap**です。

例えば、

```text
[10, 20, 30, 40, 50]
```

を管理しているとします。

通常の配列なら、

```text
index:  0   1   2   3   4
value: 10  20  30  40  50
```

となります。

Implicit Treapでは、各ノードに

```text
「私はインデックス2です」
```

のような情報を保存しません。

代わりに、

```text
左部分木に何個の要素があるか
```

を利用して現在の位置を求めます。

そのため、ノードの追加・削除によって後ろのすべての要素のインデックスを書き換える必要がありません。

これが、途中への挿入・削除を高速にできる理由です。

---

# 内部構造

内部のノードは以下のようになっています。

```csharp
private class Node
{
    public T Value;
    public int Priority;
    public int Size;
    public bool Rev;
    public Node Left;
    public Node Right;
    public Node Parent;
}
```

それぞれの意味は以下です。

| フィールド      | 意味             |
| ---------- | -------------- |
| `Value`    | ノードが保持している値    |
| `Priority` | Treapの優先度      |
| `Size`     | その部分木に存在するノード数 |
| `Rev`      | 区間反転の遅延フラグ     |
| `Left`     | 左の子            |
| `Right`    | 右の子            |
| `Parent`   | 親ノード           |

例えば、

```text
        30
       /  \
     10    50
       \
       20
```

なら、

```text
Size(20) = 1
Size(10) = 2
Size(50) = 1
Size(30) = 4
```

となります。

この `Size` が Implicit Treap において非常に重要です。

---

# SplitとMerge

Implicit Treapの中心となる操作が、

```csharp
Split
Merge
```

です。

## Split

`Split(n, k)` は、

> 先頭から `k` 個の要素と、それ以降の要素に分割する

操作です。

例えば、

```text
[10, 20, 30, 40, 50]
```

を `k = 2` で分割すると、

```text
A = [10, 20]
B = [30, 40, 50]
```

になります。

---

## Merge

`Merge(a, b)` は、

> `a` の後ろに `b` を連結する

操作です。

```text
A = [10, 20]
B = [30, 40, 50]
```

なら、

```text
Merge(A, B)
```

によって、

```text
[10, 20, 30, 40, 50]
```

になります。

---

## Insertへの利用

例えば、

```csharp
treap.Insert(2, 30);
```

なら、

```text
元

[10, 20, 40, 50]
```

を、

```text
A = [10, 20]
B = [40, 50]
```

に分割します。

そして、

```text
A + [30] + B
```

と結合します。

結果：

```text
[10, 20, 30, 40, 50]
```

この処理を `O(log N)` 程度で行えるのが特徴です。

---

# Lazy PropagationとReverse

このクラスでは、区間反転を高速に行うために、

```csharp
public bool Rev;
```

というフラグを使用しています。

例えば、

```text
[1, 2, 3, 4, 5]
```

を反転すると、

```text
[5, 4, 3, 2, 1]
```

になります。

普通に要素を反転すると `O(N)` 必要ですが、このクラスでは、

```csharp
Toggle(root);
```

として反転フラグを設定するだけです。

実際の反転処理は必要になったときに、

```csharp
Push(node);
```

で行います。

そのため、

```csharp
Reverse();
```

は `O(1)` で実行できます。

---

# Count

<a id="count"></a>

```csharp
public int Count
```

## 意味

現在Treapに入っている要素数を返します。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>();

treap.AddLast(10);
treap.AddLast(20);
treap.AddLast(30);

Console.WriteLine(treap.Count);
```

出力：

```text
3
```

---

## 使用例

```csharp
for (int i = 0; i < treap.Count; i++)
{
    Console.WriteLine(treap[i]);
}
```

配列のように要素数を取得できます。

---

## 計算量

```text
時間計算量: O(1)
空間計算量: O(1)
```

---

# IsEmpty

<a id="isempty"></a>

```csharp
public bool IsEmpty
```

## 意味

Treapが空かどうかを判定します。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>();

Console.WriteLine(treap.IsEmpty);

treap.AddLast(100);

Console.WriteLine(treap.IsEmpty);
```

出力：

```text
True
False
```

---

## 使用例

`PopFront()` や `PopBack()` を行う前の確認にも使えます。

```csharp
if (!treap.IsEmpty)
{
    int x = treap.PopFront();
}
```

---

## 計算量

```text
時間計算量: O(1)
空間計算量: O(1)
```

---

# Contains

<a id="contains"></a>

```csharp
public bool Contains(T value)
```

## 意味

指定した値がTreap内に存在するか判定します。

存在すれば `true`、存在しなければ `false` を返します。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 10, 20, 30, 40 }
);

Console.WriteLine(treap.Contains(30));
Console.WriteLine(treap.Contains(50));
```

出力：

```text
True
False
```

---

## 重複した値

重複も可能です。

```text
[10, 20, 20, 30, 20]
```

この場合、

```csharp
treap.Contains(20)
```

は、

```text
true
```

になります。

---

## 計算量

内部では、

```csharp
Dictionary<T, HashSet<Node>>
```

を利用しています。

そのため平均的には、

```text
時間計算量: 期待 O(1)
空間計算量: O(N)
```

です。

---

# `this[int index]`

<a id="thisint-index"></a>

```csharp
public T this[int index]
```

## 意味

インデックスを指定して要素を取得・変更します。

通常の配列や `List<T>` と同じ感覚で使用できます。

---

## 取得

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 10, 20, 30, 40, 50 }
);

Console.WriteLine(treap[2]);
```

出力：

```text
30
```

---

## 変更

```csharp
treap[2] = 100;

Console.WriteLine(treap[2]);
```

出力：

```text
100
```

---

## 負のインデックス

このクラスでは負のインデックスも使用できます。

```csharp
Console.WriteLine(treap[-1]);
```

これは、

```csharp
treap[treap.Count - 1]
```

と同じです。

例えば、

```text
[10, 20, 30, 40, 50]
```

なら、

```text
treap[-1] → 50
treap[-2] → 40
treap[-3] → 30
```

です。

---

## 使用例

```csharp
for (int i = 0; i < treap.Count; i++)
{
    Console.Write($"{treap[i]} ");
}
```

---

## 計算量

```text
時間計算量: 期待 O(log N)
空間計算量: O(log N)
```

---

# `this[Index index]`

<a id="thisindex-index"></a>

```csharp
public T this[Index index]
```

## 意味

C#の `Index` を利用して要素を取得・変更できます。

そのため、

```csharp
treap[^1]
```

のような書き方が可能です。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 10, 20, 30, 40, 50 }
);

Console.WriteLine(treap[^1]);
Console.WriteLine(treap[^2]);
```

出力：

```text
50
40
```

---

## `int` との比較

```csharp
treap[0]   // 先頭
treap[1]   // 2番目
treap[2]   // 3番目

treap[^1]  // 最後
treap[^2]  // 最後から2番目
treap[^3]  // 最後から3番目
```

---

## 変更も可能

```csharp
treap[^1] = 100;
```

とすれば最後の要素を変更できます。

---

## 計算量

```text
時間計算量: 期待 O(log N)
空間計算量: O(log N)
```

---

# Insert

<a id="insert"></a>

```csharp
public void Insert(int index, T value)
```

## 意味

指定した位置に要素を1つ挿入します。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 10, 20, 40, 50 }
);

treap.Insert(2, 30);

Console.WriteLine(string.Join(" ", treap));
```

出力：

```text
10 20 30 40 50
```

---

## インデックスの考え方

```text
[10, 20, 40, 50]
       ↑
     index 2
```

に `30` を挿入するため、

```text
[10, 20, 30, 40, 50]
```

になります。

---

## 負のインデックス

```csharp
treap.Insert(-1, 100);
```

のように指定できます。

`Insert` の負のインデックスは、

```text
末尾からの位置
```

として利用できます。

例えば、

```text
[10, 20, 30, 40]
```

に、

```csharp
Insert(-1, 100)
```

を行うと、

```text
[10, 20, 30, 100, 40]
```

となります。

末尾のさらに後ろへ追加したい場合は、

```csharp
AddLast(100);
```

を使用してください。

---

## 計算量

```text
時間計算量: 期待 O(log N)
空間計算量: O(log N)
```

---

# AddFirst

<a id="addfirst"></a>

```csharp
public void AddFirst(T value)
```

## 意味

先頭に要素を追加します。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 20, 30, 40 }
);

treap.AddFirst(10);

Console.WriteLine(string.Join(" ", treap));
```

出力：

```text
10 20 30 40
```

---

## 内部動作

実質的には、

```csharp
Insert(0, value);
```

と同じです。

---

## 計算量

```text
時間計算量: 期待 O(log N)
空間計算量: O(log N)
```

---

# AddLast

<a id="addlast"></a>

```csharp
public void AddLast(T value)
```

## 意味

末尾に要素を追加します。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 10, 20, 30 }
);

treap.AddLast(40);

Console.WriteLine(string.Join(" ", treap));
```

出力：

```text
10 20 30 40
```

---

## 内部動作

実質的には、

```csharp
Insert(Count, value);
```

と同じです。

---

## 計算量

```text
時間計算量: 期待 O(log N)
空間計算量: O(log N)
```

---

# AddRange

<a id="addrange"></a>

```csharp
public void AddRange(IEnumerable<T> items)
```

## 意味

複数の要素を末尾に追加します。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 1, 2, 3 }
);

treap.AddRange(new[] { 4, 5, 6 });

Console.WriteLine(string.Join(" ", treap));
```

出力：

```text
1 2 3 4 5 6
```

---

## `List<T>` との似ている点

```csharp
list.AddRange(items);
```

に近い感覚で使用できます。

---

## 計算量

この実装では要素を1つずつ `AddLast` しています。

追加する要素数を `M` とすると、

```text
時間計算量: 期待 O(M log N)
空間計算量: O(M)
```

程度です。

---

# InsertRange

<a id="insertrange"></a>

```csharp
public void InsertRange(int index, IEnumerable<T> items)
```

## 意味

複数の要素を指定した位置にまとめて挿入します。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 1, 2, 6, 7 }
);

treap.InsertRange(2, new[] { 3, 4, 5 });

Console.WriteLine(string.Join(" ", treap));
```

出力：

```text
1 2 3 4 5 6 7
```

---

## 動作

元の配列を、

```text
[1, 2] [6, 7]
```

に分割し、

```text
[3, 4, 5]
```

を間に入れます。

```text
[1, 2] + [3, 4, 5] + [6, 7]
```

---

## 計算量

追加する要素数を `M` とすると、

```text
時間計算量: 期待 O(M log M + log N)
空間計算量: O(M + log N)
```

程度です。

---

# Remove

<a id="remove"></a>

```csharp
public bool Remove(T value)
```

## 意味

指定した値を持つ要素を1つ削除します。

削除できた場合は `true`、存在しない場合は `false` を返します。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 10, 20, 30, 40 }
);

bool result = treap.Remove(30);

Console.WriteLine(result);
Console.WriteLine(string.Join(" ", treap));
```

出力：

```text
True
10 20 40
```

---

## 存在しない場合

```csharp
bool result = treap.Remove(100);
```

なら、

```text
result == false
```

になります。

---

## 重複値

```text
[10, 20, 20, 30]
```

に対して、

```csharp
Remove(20);
```

を実行すると、`20` が1つだけ削除されます。

どの `20` が削除されるかは保証されません。

「インデックス2の `20` を削除したい」という場合は、

```csharp
RemoveAt(2);
```

を使用してください。

---

## 計算量

値からノードをハッシュテーブルで検索し、そのノードの位置を求めて削除します。

平均的には、

```text
時間計算量: 期待 O(log N)
空間計算量: O(log N)
```

です。

---

# RemoveAt

<a id="removeat"></a>

```csharp
public T RemoveAt(int index)
```

## 意味

指定したインデックスの要素を削除し、削除した値を返します。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 10, 20, 30, 40, 50 }
);

int x = treap.RemoveAt(2);

Console.WriteLine(x);
Console.WriteLine(string.Join(" ", treap));
```

出力：

```text
30
10 20 40 50
```

---

## 負のインデックス

```csharp
int x = treap.RemoveAt(-1);
```

とすると、最後の要素を削除できます。

---

## 動作

例えば、

```text
[10, 20, 30, 40, 50]
```

から `index = 2` を削除する場合、

```text
[10, 20] [30] [40, 50]
```

のように分割します。

中央の `[30]` を取り除き、

```text
[10, 20] + [40, 50]
```

を結合します。

---

## 計算量

```text
時間計算量: 期待 O(log N)
空間計算量: O(log N)
```

---

# PopFront

<a id="popfront"></a>

```csharp
public T PopFront()
```

## 意味

先頭の要素を削除して返します。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 10, 20, 30 }
);

int x = treap.PopFront();

Console.WriteLine(x);
Console.WriteLine(string.Join(" ", treap));
```

出力：

```text
10
20 30
```

---

## 内部動作

実質的には、

```csharp
RemoveAt(0);
```

です。

---

## 計算量

```text
時間計算量: 期待 O(log N)
空間計算量: O(log N)
```

---

# PopBack

<a id="popback"></a>

```csharp
public T PopBack()
```

## 意味

末尾の要素を削除して返します。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 10, 20, 30 }
);

int x = treap.PopBack();

Console.WriteLine(x);
Console.WriteLine(string.Join(" ", treap));
```

出力：

```text
30
10 20
```

---

## 内部動作

実質的には、

```csharp
RemoveAt(Count - 1);
```

です。

---

## 計算量

```text
時間計算量: 期待 O(log N)
空間計算量: O(log N)
```

---

# Reverse()

<a id="reverse"></a>

```csharp
public void Reverse()
```

## 意味

Treap全体の順番を反転します。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 1, 2, 3, 4, 5 }
);

treap.Reverse();

Console.WriteLine(string.Join(" ", treap));
```

出力：

```text
5 4 3 2 1
```

---

## 最大の特徴

普通に配列を反転すると、

```text
O(N)
```

かかります。

しかし、このクラスでは `Rev` フラグを利用するため、

```csharp
treap.Reverse();
```

だけなら、

```text
O(1)
```

で実行できます。

---

## 計算量

```text
時間計算量: O(1)
空間計算量: O(1)
```

---

# Reverse(left, length)

<a id="reverseleft-length"></a>

```csharp
public void Reverse(int left, int length)
```

## 意味

指定した区間だけを反転します。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 1, 2, 3, 4, 5, 6 }
);

treap.Reverse(1, 4);

Console.WriteLine(string.Join(" ", treap));
```

出力：

```text
1 5 4 3 2 6
```

反転対象は、

```text
1 [2 3 4 5] 6
```

です。

---

## `left` と `length`

ここで、

```text
left   = 開始位置
length = 反転する要素数
```

です。

例えば、

```csharp
Reverse(2, 3);
```

なら、

```text
index:
 0  1 [2  3  4] 5
```

の部分を反転します。

---

## 負の `left`

```csharp
treap.Reverse(-3, 2);
```

のような指定もできます。

例えば、

```text
[1, 2, 3, 4, 5]
```

なら、

```text
left = -3
```

は、

```text
left = 2
```

として扱われます。

そのため、

```text
[1, 2, [3, 4], 5]
```

が、

```text
[1, 2, 4, 3, 5]
```

になります。

---

## 計算量

```text
時間計算量: 期待 O(log N)
空間計算量: O(log N)
```

反転する区間の長さには基本的に依存しません。

---

# Slice

<a id="slice"></a>

```csharp
public ImplicitTreap<T> Slice(int left, int length)
```

## 意味

指定した区間を**コピーして、新しい `ImplicitTreap<T>` として返します**。

元のTreapは変更されません。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 10, 20, 30, 40, 50 }
);

var sliced = treap.Slice(1, 3);

Console.WriteLine(string.Join(" ", sliced));
Console.WriteLine(string.Join(" ", treap));
```

出力：

```text
20 30 40
10 20 30 40 50
```

---

## イメージ

```text
元

[10, 20, 30, 40, 50]
      └────────┘
        Slice

新しいTreap

[20, 30, 40]
```

元のTreapはそのままです。

---

## `Cut` との違い

`Slice` は**コピー**です。

```text
元:
[1, 2, 3, 4, 5]

Slice(1, 3)

元:
[1, 2, 3, 4, 5]

新:
[2, 3, 4]
```

一方、`Cut` は元から切り取ります。

```text
元:
[1, 5]

新:
[2, 3, 4]
```

---

## 計算量

区間の要素を新しいノードへコピーするため、

```text
時間計算量: O(length + log N)
空間計算量: O(length + log N)
```

です。

---

# Cut

<a id="cut"></a>

```csharp
public ImplicitTreap<T> Cut(int left, int length)
```

## 意味

指定した区間を元のTreapから切り離し、新しい `ImplicitTreap<T>` として返します。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 10, 20, 30, 40, 50 }
);

var cut = treap.Cut(1, 3);

Console.WriteLine(string.Join(" ", cut));
Console.WriteLine(string.Join(" ", treap));
```

出力：

```text
20 30 40
10 50
```

---

## イメージ

```text
元:

[10, 20, 30, 40, 50]

        ↓ Cut(1, 3)

元:
[10, 50]

新:
[20, 30, 40]
```

---

## `Slice` との違い

|          | `Slice` | `Cut`    |
| -------- | ------- | -------- |
| 新しいTreap | 作る      | 作る       |
| 元の区間     | 残る      | 消える      |
| ノード      | コピー     | 元のノードを利用 |
| 主な用途     | コピー     | 切り離し     |

---

## 使用例

例えば、列を前半と後半に分けたい場合、

```csharp
var right = treap.Cut(5, treap.Count - 5);
```

のようにできます。

結果、

```text
treap → 前半
right → 後半
```

となります。

---

## 計算量

```text
時間計算量: O(length + log N)
空間計算量: O(length + log N)
```

---

# Clear

<a id="clear"></a>

```csharp
public void Clear()
```

## 意味

Treapのすべての要素を削除します。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 1, 2, 3, 4, 5 }
);

treap.Clear();

Console.WriteLine(treap.Count);
```

出力：

```text
0
```

---

## 使用例

同じTreapオブジェクトを再利用したい場合に便利です。

```csharp
treap.Clear();

treap.AddLast(100);
treap.AddLast(200);
```

---

## 計算量

```text
時間計算量: O(N)
空間計算量: O(1)
```

---

# ToArray

<a id="toarray"></a>

```csharp
public T[] ToArray()
```

## 意味

Treapの要素を現在の順番で `T[]` に変換します。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 10, 20, 30 }
);

int[] array = treap.ToArray();

Console.WriteLine(string.Join(" ", array));
```

出力：

```text
10 20 30
```

---

## 使用例

AtCoderなどで、最後に配列として出力したい場合にも利用できます。

```csharp
Console.WriteLine(
    string.Join(" ", treap.ToArray())
);
```

---

## 計算量

すべての要素を見る必要があるため、

```text
時間計算量: O(N)
空間計算量: O(N)
```

です。

---

# GetEnumerator

<a id="getenumerator"></a>

```csharp
public IEnumerator<T> GetEnumerator()
```

## 意味

`IEnumerable<T>` を実装することで、`foreach` でTreapを直接列挙できます。

---

## 使い方

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 10, 20, 30, 40 }
);

foreach (int x in treap)
{
    Console.WriteLine(x);
}
```

出力：

```text
10
20
30
40
```

---

## `string.Join` でも使える

```csharp
Console.WriteLine(
    string.Join(" ", treap)
);
```

のように使用できます。

---

## 注意

現在の実装では、

```csharp
foreach (T item in ToArray())
    yield return item;
```

となっています。

そのため、列挙時には一度配列を作成します。

---

## 計算量

```text
時間計算量: O(N)
空間計算量: O(N)
```

---

# Parentとインデックス取得

このクラスでは各ノードが、

```csharp
public Node Parent;
```

を持っています。

これは特に、

```csharp
GetIndex(Node node)
```

で重要です。

例えば、

```text
[10, 20, 30, 40, 50]
```

の `40` のノードを直接持っていた場合、そのノードから親をたどって根まで戻ります。

```text
       root
        ↑
      Parent
        ↑
      Parent
        ↑
       40
```

そして、

```text
左部分木のサイズ
+
右へ進んだときに追加される要素数
```

を計算することで、

```text
40 → index 3
```

を求めます。

これは `Remove(value)` で特に利用されています。

---

# 値検索用インデックス

このクラスでは、

```csharp
private readonly Dictionary<T, HashSet<Node>> nodesByValue = new();
```

を持っています。

これは、

```text
値
 ↓
その値を持つNode
```

という対応を高速に管理するためのものです。

例えば、

```text
[10, 20, 20, 30, 20]
```

なら、

```text
10 → Node
20 → Node
     Node
     Node
30 → Node
```

のようになります。

そのため、

```csharp
Contains(20)
```

では、Treap全体を探索する必要がありません。

---

# `null` の管理

`T` が参照型の場合、

```csharp
null
```

も格納できます。

そのため、

```csharp
private readonly HashSet<Node> nullValueNodes = new();
```

が用意されています。

例えば、

```csharp
var treap = new ImplicitTreap<string>();

treap.AddLast(null);
treap.AddLast("ABC");

Console.WriteLine(treap.Contains(null));
```

出力：

```text
True
```

となります。

---

# List<T>との比較

Implicit Treapを理解するうえで、`List<T>` と比較すると分かりやすいです。

## 末尾への追加

```csharp
list.Add(x);
treap.AddLast(x);
```

どちらも高速です。

---

## インデックスによる取得

```csharp
list[100];
treap[100];
```

`List<T>` は、

```text
O(1)
```

です。

一方、Implicit Treapは、

```text
期待 O(log N)
```

です。

---

## 途中への挿入

```csharp
list.Insert(i, x);
```

は、

```text
O(N)
```

です。

一方、

```csharp
treap.Insert(i, x);
```

は、

```text
期待 O(log N)
```

です。

---

## 途中への削除

```csharp
list.RemoveAt(i);
```

は、

```text
O(N)
```

です。

一方、

```csharp
treap.RemoveAt(i);
```

は、

```text
期待 O(log N)
```

です。

---

## 区間反転

`List<T>` には標準の区間反転機能はありません。

一方、

```csharp
treap.Reverse(l, length);
```

によって、

```text
期待 O(log N)
```

で区間を反転できます。

---

# どんなときに使うべきか

Implicit Treapは特に、

```text
要素の順番が変化する
+
途中への挿入・削除が多い
+
区間操作がある
```

という問題で強力です。

例えば、

```text
N = 200000
Q = 200000
```

で、

```text
・位置 i に追加
・位置 i を削除
・区間 [l,r] を反転
・位置 i の値を取得
```

などを大量に行う問題です。

普通の配列では厳しい場合でも、Implicit Treapなら期待 `O(log N)` で処理できます。

---

# 使用例

以下は基本的な操作をまとめた例です。

```csharp
var treap = new ImplicitTreap<int>(
    new[] { 1, 2, 3, 4, 5 }
);

// 先頭に追加
treap.AddFirst(0);

// 末尾に追加
treap.AddLast(6);

// 位置3に追加
treap.Insert(3, 100);

Console.WriteLine(string.Join(" ", treap));
```

出力：

```text
0 1 2 100 3 4 5 6
```

次に区間を反転します。

```csharp
treap.Reverse(2, 4);

Console.WriteLine(string.Join(" ", treap));
```

結果：

```text
0 1 4 3 100 2 5 6
```

さらに、

```csharp
var part = treap.Cut(2, 3);
```

とすると、

```text
treap:
[0, 1, 2, 5, 6]

part:
[4, 3, 100]
```

のように区間を別のTreapへ切り離せます。

---

# 計算量まとめ

`N` を現在の要素数、`M` を追加する要素数、`length` を区間長とします。

| 操作                     |                平均・期待計算量 |
| ---------------------- | ----------------------: |
| `Count`                |                  `O(1)` |
| `IsEmpty`              |                  `O(1)` |
| `Contains`             |               期待 `O(1)` |
| `this[int]`            |           期待 `O(log N)` |
| `this[Index]`          |           期待 `O(log N)` |
| `Insert`               |           期待 `O(log N)` |
| `AddFirst`             |           期待 `O(log N)` |
| `AddLast`              |           期待 `O(log N)` |
| `Remove`               |           期待 `O(log N)` |
| `RemoveAt`             |           期待 `O(log N)` |
| `PopFront`             |           期待 `O(log N)` |
| `PopBack`              |           期待 `O(log N)` |
| `Reverse()`            |                  `O(1)` |
| `Reverse(left,length)` |           期待 `O(log N)` |
| `Slice`                |     `O(length + log N)` |
| `Cut`                  |     `O(length + log N)` |
| `AddRange`             |         期待 `O(M log N)` |
| `InsertRange`          | 期待 `O(M log M + log N)` |
| `Clear`                |                  `O(N)` |
| `ToArray`              |                  `O(N)` |
| `GetEnumerator`        |                  `O(N)` |

---

# 空間計算量

Treapそのものは `N` 個のノードを持つため、

```text
O(N)
```

のメモリを使用します。

さらに、

```csharp
Dictionary<T, HashSet<Node>>
```

による値検索用のインデックスも保持しています。

したがって、全体としては、

```text
空間計算量: O(N)
```

です。

---

# Treapの最悪計算量について

この実装では、

```csharp
private static readonly Random rnd = new();
```

を利用して各ノードの優先度をランダムに決定しています。

そのため、Treapの高さは**期待 `O(log N)`**になります。

したがって、

```text
Insert
RemoveAt
this[index]
Split
Merge
Reverse(left,length)
```

などは基本的に、

```text
期待 O(log N)
```

です。

ただし、ランダムな優先度によっては木が偏る可能性があります。

理論上の最悪の場合は、

```text
高さ O(N)
```

になるため、厳密な最悪計算量は `O(N)` です。

競技プログラミングでは通常、

```text
期待 O(log N)
```

として扱います。

---

# 注意点

## 空のTreapでの削除

以下のようなコードは、

```csharp
var treap = new ImplicitTreap<int>();

treap.PopFront();
```

例外になります。

安全に使用する場合は、

```csharp
if (!treap.IsEmpty)
{
    treap.PopFront();
}
```

とします。

---

## インデックス範囲

取得・変更では、

```text
0 ～ Count - 1
```

が通常の範囲です。

負のインデックスも使用できます。

```text
-1 → 最後
-2 → 最後から2番目
```

---

## Insertの負のインデックス

`Insert` の負のインデックスは取得時とは少し意味が異なります。

例えば、

```csharp
treap.Insert(-1, x);
```

は「最後の要素を置き換える」という意味ではなく、**末尾の直前に挿入する位置**になります。

末尾に追加したい場合は、

```csharp
treap.AddLast(x);
```

を使うのが分かりやすいです。

---

## SliceとCut

```csharp
Slice(...)
```

はコピーです。

```csharp
Cut(...)
```

は切り取りです。

この違いを間違えないようにしてください。

---

## `foreach` のメモリ使用

現在の `GetEnumerator()` は、

```csharp
ToArray()
```

を経由しているため、

```csharp
foreach (var x in treap)
```

を行うと一時的に `O(N)` の配列が作られます。

大量のデータを扱う場合は、この点に注意が必要です。

---

# 使い分け

競技プログラミングでは、以下のように考えると使いやすいです。

```text
単純な配列
    ↓
T[]
```

```text
末尾への追加・ランダムアクセスが中心
    ↓
List<T>
```

```text
途中への挿入・削除が多い
    ↓
ImplicitTreap<T>
```

```text
区間反転がある
    ↓
ImplicitTreap<T>
```

```text
区間を別の列としてコピー
    ↓
Slice
```

```text
区間を別の列として切り離す
    ↓
Cut
```

```text
値の存在を高速に確認
    ↓
Contains
```

```text
指定位置の値を取得
    ↓
treap[index]
```

---

# まとめ

`ImplicitTreap<T>` は、

> **動的に変化する配列を高速に操作するためのデータ構造**

です。

特に重要なメソッドは以下です。

## `Insert`

```text
指定位置に要素を挿入
→ 期待 O(log N)
```

## `RemoveAt`

```text
指定位置を削除
→ 期待 O(log N)
```

## `this[index]`

```text
指定位置の値を取得・変更
→ 期待 O(log N)
```

## `Reverse()`

```text
全体を反転
→ O(1)
```

## `Reverse(left, length)`

```text
区間を反転
→ 期待 O(log N)
```

## `Slice`

```text
区間をコピー
→ O(length + log N)
```

## `Cut`

```text
区間を切り離す
→ O(length + log N)
```

## `Contains`

```text
値の存在を確認
→ 期待 O(1)
```

---

# 最重要ポイント

このクラスを使うときは、まず次の関係を覚えておくと便利です。

```text
┌──────────────────────────────┐
│        Implicit Treap        │
├──────────────────────────────┤
│ index取得       → 期待 O(log N) │
│ 挿入             → 期待 O(log N) │
│ 削除             → 期待 O(log N) │
│ 区間反転         → 期待 O(log N) │
│ 全体反転         → O(1)         │
│ 値検索           → 期待 O(1)    │
└──────────────────────────────┘
```

特に、

```text
「順番が変化する配列」
+
「途中への挿入・削除」
+
「区間反転」
```

という条件が揃った場合、Implicit Treapは非常に有力な選択肢です。
