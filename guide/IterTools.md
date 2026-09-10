# Itertools クラス 解説書

`Itertools` は、競技プログラミングなどで頻繁に使用する**パターン列挙・全探索**を簡単に行うためのユーティリティクラスです。

Python の `itertools` に近い機能を C# で利用できるようにしています。

主な機能は以下の7つです。

* `Nbit` — n桁・m進数の全パターン
* `Permutations` — 順列
* `Combinations` — 組合せ
* `CombinationsWithReplacement` — 重複組合せ
* `Range` — 連続した整数の生成
* `Product` — 直積
* `Mix` — 各桁の選択肢数が異なる全パターン

---

# 目次

| メソッド                                                          | 簡単な説明             | 主な用途         |               時間計算量 |
| ------------------------------------------------------------- | ----------------- | ------------ | ------------------: |
| [`Nbit`](#nbit)                                               | `n`桁の`m`進数を全列挙    | bit全探索・状態全探索 |        `O(n × m^n)` |
| [`Permutations`](#permutations)                               | 順番を区別して`r`個選ぶ     | 順列・並び替え      |     `O(P(n,r) × r)` |
| [`Combinations`](#combinations)                               | 順番を区別せず`r`個選ぶ     | 部分集合・選択      |     `O(C(n,r) × r)` |
| [`CombinationsWithReplacement`](#combinationswithreplacement) | 重複を許して`r`個選ぶ      | 重複ありの選択      | `O(C(n+r-1,r) × r)` |
| [`Range`](#range)                                             | 連続した整数を生成         | `for` の代用    |          `O(count)` |
| [`Product`](#product)                                         | 同じ集合から複数回選ぶ       | 全状態列挙        |        `O(n^r × r)` |
| [`Mix`](#mix)                                                 | 各桁の選択肢数が異なる状態を全列挙 | 制約の異なる全探索    | `O(n × ∏limits[i])` |

### 記号

以下では、

* `n` = 要素数・桁数
* `r` = 選択する個数
* `m` = 選択肢の数
* `P(n,r)` = 順列の個数
* `C(n,r)` = 組合せの個数

とします。

---

# Nbit

```csharp
public static IEnumerable<int[]> Nbit(int n, int m)
```

## 意味

`n`桁の`m`進数を**すべて列挙**します。

各桁は、

```text
0 ～ m - 1
```

の値を取ります。

例えば、

```csharp
var result = Itertools.Nbit(2, 3);
```

とすると、

```text
[0, 0]
[0, 1]
[0, 2]
[1, 0]
[1, 1]
[1, 2]
[2, 0]
[2, 1]
[2, 2]
```

が順番に生成されます。

---

## 使用例

### 0/1をすべて列挙する

```csharp
foreach (var bits in Itertools.Nbit(3, 2))
{
    Console.WriteLine(string.Join(" ", bits));
}
```

出力：

```text
0 0 0
0 0 1
0 1 0
0 1 1
1 0 0
1 0 1
1 1 0
1 1 1
```

これは3個の要素について、

```text
選ばない = 0
選ぶ     = 1
```

という状態をすべて列挙していると考えることができます。

したがって、競技プログラミングの**bit全探索**にも利用できます。

---

## 3進数を列挙する例

```csharp
foreach (var p in Itertools.Nbit(2, 3))
{
    Console.WriteLine(string.Join("", p));
}
```

出力：

```text
00
01
02
10
11
12
20
21
22
```

---

## 仕組み

まず、

```csharp
long lim = 1;

for (int i = 0; i < n; i++)
    lim *= m;
```

によって全パターン数を求めます。

これは、

```text
m^n
```

です。

例えば `n = 3, m = 2` なら、

```text
2^3 = 8
```

パターンあります。

その後、

```csharp
for (long mask = 0; mask < lim; mask++)
```

によって `0 ～ m^n - 1` を順番に処理します。

各整数を`m`進数として分解することで、各パターンを生成しています。

---

## 注意点

`lim` が `long` の範囲を超える場合、オーバーフローする可能性があります。

また、パターン数が `m^n` なので、`n` が少し大きくなるだけでも急激に増加します。

例えば、

```text
n = 20
m = 2
```

なら、

```text
2^20 = 1,048,576
```

パターンあります。

---

## 計算量

1パターンにつき `n` 桁を処理するため、

* 1パターン: `O(n)`
* 全列挙: `O(n × m^n)`
* 追加メモリ: `O(n)`

です。

---

# Permutations

```csharp
public static IEnumerable<T[]> Permutations<T>(
    IEnumerable<T> iterable,
    int? r = null)
```

## 意味

与えられた要素から、**順番を区別して`r`個選ぶ**すべてのパターンを生成します。

同じ要素は1回のパターンの中では使用できません。

例えば、

```csharp
var result = Itertools.Permutations(
    new[] { 1, 2, 3 },
    2
);
```

とすると、

```text
[1, 2]
[1, 3]
[2, 1]
[2, 3]
[3, 1]
[3, 2]
```

が生成されます。

---

## 使用例

```csharp
int[] a = { 1, 2, 3 };

foreach (var p in Itertools.Permutations(a, 2))
{
    Console.WriteLine(string.Join(" ", p));
}
```

出力：

```text
1 2
1 3
2 1
2 3
3 1
3 2
```

---

## `r`を省略した場合

```csharp
foreach (var p in Itertools.Permutations(new[] { 1, 2, 3 }))
{
    Console.WriteLine(string.Join(" ", p));
}
```

`r`を指定しない場合は、すべての要素を使用します。

結果は、

```text
1 2 3
1 3 2
2 1 3
2 3 1
3 1 2
3 2 1
```

です。

つまり通常の順列になります。

---

## `r`個選ぶ場合

要素数を `n` とすると、生成されるパターン数は、

```text
P(n,r) = n! / (n-r)!
```

です。

例えば `n = 4, r = 2` なら、

```text
P(4,2) = 4 × 3 = 12
```

通りです。

---

## 仕組み

内部では、

```csharp
bool[] used = new bool[n];
```

を使用しています。

`used[i]` は、

> `items[i]` が現在のパターンですでに使われているか

を表します。

DFSによって、

```text
1
├─ 2
│   └─ [1,2]
└─ 3
    └─ [1,3]

2
├─ 1
│   └─ [2,1]
└─ 3
    └─ [2,3]
```

のように探索します。

---

## `Clone()`について

完成したパターンを返すとき、

```csharp
yield return (T[])perm.Clone();
```

としています。

これは、`perm` がDFSの途中で何度も書き換えられるためです。

コピーを返さないと、過去に生成した結果まで変更されてしまいます。

---

## 不正な`r`

```csharp
if (m < 0 || m > n)
    yield break;
```

となっているため、

```csharp
Permutations(a, -1)
Permutations(a, 10)
```

のような場合は何も生成しません。

---

## 計算量

* 1パターン: `O(r)`
* 全列挙: `O(P(n,r) × r)`
* 追加メモリ: `O(n + r)`

です。

---

# Combinations

```csharp
public static IEnumerable<T[]> Combinations<T>(
    IEnumerable<T> iterable,
    int r)
```

## 意味

与えられた要素から、**順番を区別せず`r`個選ぶ**すべてのパターンを生成します。

例えば、

```csharp
Itertools.Combinations(
    new[] { 1, 2, 3 },
    2
);
```

なら、

```text
[1, 2]
[1, 3]
[2, 3]
```

です。

`[1,2]` と `[2,1]` は同じ組合せとして扱われるため、両方は生成されません。

---

## 使用例

```csharp
int[] a = { 1, 2, 3, 4 };

foreach (var c in Itertools.Combinations(a, 2))
{
    Console.WriteLine(string.Join(" ", c));
}
```

出力：

```text
1 2
1 3
1 4
2 3
2 4
3 4
```

---

## `Permutations`との違い

例えば、

```text
{1, 2, 3}
```

から2個選ぶ場合、

### `Permutations`

```text
[1,2]
[1,3]
[2,1]
[2,3]
[3,1]
[3,2]
```

### `Combinations`

```text
[1,2]
[1,3]
[2,3]
```

となります。

つまり、

> **順番を区別する → `Permutations`**

> **順番を区別しない → `Combinations`**

です。

---

## 仕組み

DFSの引数に、

```csharp
Dfs(depth, start)
```

という`start`を持っています。

そして、

```csharp
Dfs(depth + 1, i + 1)
```

としています。

例えば `1` を選んだ後は、`1`より後の要素だけを選択します。

```text
[1, 2, 3, 4]
 ↑
1を選んだ
 ↓
2,3,4から選ぶ
```

これによって、

```text
[1,2]
```

を生成した後に、

```text
[2,1]
```

を生成することを防いでいます。

---

## 組合せ数

要素数が `n`、選択数が `r` の場合、

```text
C(n,r) = n! / (r!(n-r)!)
```

通りです。

例えば、

```text
n = 5
r = 2
```

なら、

```text
C(5,2) = 10
```

通りです。

---

## 計算量

* 1パターン: `O(r)`
* 全列挙: `O(C(n,r) × r)`
* 追加メモリ: `O(n + r)`

です。

---

# CombinationsWithReplacement

```csharp
public static IEnumerable<T[]> CombinationsWithReplacement<T>(
    IEnumerable<T> iterable,
    int r)
```

## 意味

`Combinations`と同じように**順番を区別しません**が、同じ要素を何度でも選択できます。

例えば、

```csharp
Itertools.CombinationsWithReplacement(
    new[] { 1, 2, 3 },
    2
);
```

では、

```text
[1,1]
[1,2]
[1,3]
[2,2]
[2,3]
[3,3]
```

が生成されます。

---

## 使用例

```csharp
int[] a = { 1, 2, 3 };

foreach (var c in Itertools.CombinationsWithReplacement(a, 2))
{
    Console.WriteLine(string.Join(" ", c));
}
```

出力：

```text
1 1
1 2
1 3
2 2
2 3
3 3
```

---

## `Combinations`との違い

通常の組合せ：

```text
[1,2,3] から2個

[1,2]
[1,3]
[2,3]
```

重複組合せ：

```text
[1,2,3] から2個

[1,1]
[1,2]
[1,3]
[2,2]
[2,3]
[3,3]
```

---

## 最も重要な部分

`Combinations`では、

```csharp
Dfs(depth + 1, i + 1)
```

としていました。

このメソッドでは、

```csharp
Dfs(depth + 1, i)
```

となっています。

この違いによって、

```text
Combinations
    i + 1
    ↓
    今選んだ要素は再利用しない

CombinationsWithReplacement
    i
    ↓
    今選んだ要素を再利用できる
```

となっています。

---

## 例えば

`[1,2,3]`から2個選び、

```text
1
```

を選んだ場合、

通常の`Combinations`では、

```text
2,3
```

から選びます。

一方、`CombinationsWithReplacement`では、

```text
1,2,3
```

から選べます。

そのため、

```text
[1,1]
```

が生成できます。

---

## `r = 0`の場合

```csharp
if (r == 0)
{
    yield return Array.Empty<T>();
    yield break;
}
```

となっています。

したがって、

```csharp
var result =
    Itertools.CombinationsWithReplacement(
        new[] { 1, 2, 3 },
        0
    );
```

では、

```text
[]
```

という1つのパターンが生成されます。

---

## 計算量

生成されるパターン数は、

```text
C(n+r-1,r)
```

です。

したがって、

* 1パターン: `O(r)`
* 全列挙: `O(C(n+r-1,r) × r)`
* 追加メモリ: `O(n + r)`

です。

---

# Range

```csharp
public static IEnumerable<int> Range(int start, int count)
    => Enumerable.Range(start, count);
```

## 意味

`start`から始まる整数を、`count`個生成します。

例えば、

```csharp
Itertools.Range(3, 5)
```

なら、

```text
3
4
5
6
7
```

です。

---

## 使用例

```csharp
foreach (int i in Itertools.Range(3, 5))
{
    Console.WriteLine(i);
}
```

出力：

```text
3
4
5
6
7
```

---

## `for`との比較

例えば、

```csharp
for (int i = 3; i < 8; i++)
{
    Console.WriteLine(i);
}
```

は、

```csharp
foreach (int i in Itertools.Range(3, 5))
{
    Console.WriteLine(i);
}
```

とほぼ同じ範囲を生成します。

---

## 注意点

第2引数は**終了値ではなく個数**です。

```csharp
Range(3, 5)
```

は、

```text
3 ～ 5
```

ではなく、

```text
3 ～ 7
```

です。

---

## 計算量

生成する個数を `count` とすると、

* 全列挙: `O(count)`
* 追加メモリ: `O(1)`

です。

---

# Product

```csharp
public static IEnumerable<T[]> Product<T>(
    IEnumerable<T> iterable,
    int repeat = 1)
```

## 意味

同じ集合から`repeat`回選択する**直積（Cartesian Product）**を生成します。

同じ要素を何度選んでもよく、順番も区別されます。

例えば、

```csharp
Itertools.Product(
    new[] { 0, 1, 2 },
    2
);
```

なら、

```text
[0,0]
[0,1]
[0,2]
[1,0]
[1,1]
[1,2]
[2,0]
[2,1]
[2,2]
```

となります。

---

## 使用例

```csharp
int[] a = { 0, 1 };

foreach (var p in Itertools.Product(a, 3))
{
    Console.WriteLine(string.Join(" ", p));
}
```

出力：

```text
0 0 0
0 0 1
0 1 0
0 1 1
1 0 0
1 0 1
1 1 0
1 1 1
```

これは、

> 長さ3の0/1列をすべて列挙する

処理になっています。

---

## パターン数

要素数を `n`、`repeat`を`r`とすると、

```text
n^r
```

通りです。

例えば、

```text
n = 3
r = 4
```

なら、

```text
3^4 = 81
```

通りです。

---

## `Nbit`との違い

例えば、

```csharp
Nbit(2, 2)
```

では、

```text
[0,0]
[0,1]
[1,0]
[1,1]
```

となります。

一方、

```csharp
Product(new[] { 0, 1 }, 2)
```

でも同じ結果になります。

ただし、

```text
Nbit
```

は整数の`m`進数を列挙することに特化しています。

一方、

```text
Product
```

は任意の要素を使用できます。

例えば、

```csharp
Itertools.Product(
    new[] { "A", "B", "C" },
    2
);
```

なら、

```text
[A,A]
[A,B]
[A,C]
[B,A]
...
```

となります。

---

## `CombinationsWithReplacement`との違い

`Product`では順番を区別します。

```text
[1,2]
[2,1]
```

は別物です。

一方、

```text
CombinationsWithReplacement
```

では、

```text
[1,2]
```

だけが生成されます。

したがって、

```text
順番を区別する
    → Product

順番を区別しない
    → CombinationsWithReplacement
```

です。

---

## 計算量

* 1パターン: `O(repeat)`
* 全列挙: `O(n^repeat × repeat)`
* 追加メモリ: `O(n + repeat)`

です。

---

# Mix

```csharp
public static IEnumerable<int[]> Mix(int[] limits)
```

## 意味

**各桁で選択できる値の数が異なる場合**に、すべてのパターンを生成します。

`limits[i]` は、

> `i`番目の位置で選択できる値の個数

を表します。

値そのものは、

```text
0 ～ limits[i] - 1
```

です。

---

## 使用例

```csharp
int[] limits = { 2, 3 };

foreach (var p in Itertools.Mix(limits))
{
    Console.WriteLine(string.Join(" ", p));
}
```

出力：

```text
0 0
0 1
0 2
1 0
1 1
1 2
```

---

## 意味を具体的にすると

```text
limits = [2, 3]
```

なら、

```text
0番目
    0, 1

1番目
    0, 1, 2
```

です。

したがって、

```text
(2通り) × (3通り)
= 6通り
```

となります。

---

## 3桁の場合

例えば、

```csharp
int[] limits = { 2, 3, 4 };
```

なら、

```text
0番目 → 0,1
1番目 → 0,1,2
2番目 → 0,1,2,3
```

なので、

```text
2 × 3 × 4 = 24
```

通り生成されます。

---

## 使用例：問題の選択肢数が違う場合

例えば3つの変数があり、

```text
A → 0,1
B → 0,1,2
C → 0,1,2,3
```

をすべて試したいとします。

この場合、

```csharp
foreach (var state in Itertools.Mix(new[] { 2, 3, 4 }))
{
    int a = state[0];
    int b = state[1];
    int c = state[2];

    // この状態について処理
}
```

と書けます。

---

## 仕組み

現在の状態を、

```csharp
int[] p = new int[n];
```

で管理しています。

最初は、

```text
[0,0,...,0]
```

です。

その後、右端から値を増やします。

例えば、

```text
limits = [2,3]
```

なら、

```text
[0,0]
[0,1]
[0,2]
```

の次に右端を増やそうとすると、

```text
[0,3]
```

になります。

しかし右端は `0～2` しか使えません。

そこで、

```text
[0,3]
 ↓
[0,0]
 ↓ 繰り上がり
[1,0]
```

となります。

これは通常の数値の繰り上がりと同じ仕組みです。

---

## `Clone()`について

```csharp
yield return (int[])p.Clone();
```

としているのは、`p` が次のパターンで書き換えられるからです。

コピーを返すことで、

```text
[0,0]
[0,1]
[0,2]
```

などの生成済みパターンをそれぞれ独立した配列として扱えます。

---

## パターン数

パターン数は、

```text
limits[0]
× limits[1]
× ...
× limits[n-1]
```

です。

例えば、

```text
limits = [2,3,4]
```

なら、

```text
2 × 3 × 4 = 24
```

通りです。

---

## 計算量

パターン数を、

```text
L = ∏ limits[i]
```

とすると、

* 1パターンあたりの出力サイズ: `O(n)`
* 全列挙: `O(n × L)` 程度
* 追加メモリ: `O(n)`

です。

---

# 各メソッドの使い分け

全探索を書くときは、次のように考えると分かりやすいです。

## ① 各位置が `0 ～ m-1`

```text
Nbit
```

例：

```text
[0,0,0]
[0,0,1]
...
[1,1,1]
```

---

## ② 任意の要素から順番を区別して選ぶ

```text
Permutations
```

例：

```text
[1,2]
[1,3]
[2,1]
...
```

---

## ③ 任意の要素から順番を区別せず選ぶ

```text
Combinations
```

例：

```text
[1,2]
[1,3]
[2,3]
```

---

## ④ 同じ要素を何度でも選べる

```text
CombinationsWithReplacement
```

例：

```text
[1,1]
[1,2]
[1,3]
[2,2]
...
```

---

## ⑤ 同じ集合から何度も選び、順番も区別する

```text
Product
```

例：

```text
[1,1]
[1,2]
[2,1]
[2,2]
```

---

## ⑥ 各位置の選択肢数が違う

```text
Mix
```

例：

```text
limits = [2,3]

[0,0]
[0,1]
[0,2]
[1,0]
[1,1]
[1,2]
```

---

# 全メソッド比較

| メソッド                          | 順番    | 重複 | 選択肢     |
| ----------------------------- | ----- | -- | ------- |
| `Nbit`                        | 区別する  | 可能 | 各桁同じ    |
| `Permutations`                | 区別する  | 不可 | 元の要素    |
| `Combinations`                | 区別しない | 不可 | 元の要素    |
| `CombinationsWithReplacement` | 区別しない | 可能 | 元の要素    |
| `Product`                     | 区別する  | 可能 | 各位置同じ   |
| `Mix`                         | 区別する  | 可能 | 各位置で異なる |
| `Range`                       | ―     | ―  | 連続した整数  |

---

# `yield return`について

このクラスでは、

```csharp
yield return
```

が多く使われています。

これは、すべてのパターンを一度に作るのではなく、**1つずつ生成する**ための仕組みです。

例えば、

```csharp
foreach (var p in Itertools.Permutations(a))
{
    // pを処理
}
```

とした場合、すべての順列を同時にメモリへ保存する必要はありません。

これは全探索のパターン数が非常に多い場合に重要です。

ただし、

```csharp
var all = Itertools.Permutations(a).ToList();
```

とすると、`ToList()`によってすべての結果をメモリに保存するため、パターン数が多い場合は大量のメモリを使用します。

---

# 注意：要素数が増えると急激に重くなる

これらのメソッドは全探索を行うため、パターン数が非常に大きくなる場合があります。

例えば、

```text
Nbit
    m^n

Permutations
    n!

Combinations
    C(n,r)

CombinationsWithReplacement
    C(n+r-1,r)

Product
    n^r

Mix
    ∏limits[i]
```

となります。

特に、

```text
n!
```

や、

```text
n^r
```

は非常に速く増加します。

そのため、競技プログラミングでは、

> **生成されるパターン数が制約内に収まるか**

を最初に確認することが重要です。

---

# まとめ

`Itertools` は、競技プログラミングで頻繁に登場する全探索を簡潔に書くためのライブラリです。

```text
Nbit
    ↓
n桁・m進数の全列挙

Permutations
    ↓
順列

Combinations
    ↓
組合せ

CombinationsWithReplacement
    ↓
重複組合せ

Range
    ↓
連続整数

Product
    ↓
直積

Mix
    ↓
各位置の選択肢数が異なる全列挙
```

特に、

```text
「順番を区別するか」
「同じ要素を再利用できるか」
「位置ごとに選択肢が違うか」
```

の3点を考えると、適切なメソッドを選びやすくなります。
