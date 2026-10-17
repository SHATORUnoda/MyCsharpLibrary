# DeepCopy クラス 解説書

`DeepCopy` クラスは、**配列やリストの入れ子構造を安全に複製する**ためのユーティリティクラスです。

特に競技プログラミングでは、

```csharp
int[][] a = ...
```

のようなジャグ配列や、

```csharp
List<List<int>> b = ...
```

のような二次元リストを、そのまま代入すると**参照が共有されてしまう**問題があります。

このクラスを使うと、各階層ごとに新しい配列やリストを作り、元データに影響を与えずに複製できます。

---

# 目次

| メソッド | 概要 | 計算量 |
| --- | --- | --- |
| [`Copy`](#copy) | 型に応じた簡単なコピーを行う | 依存 |
| [`Jagged`](#jagged) | ジャグ配列向けの明示的なコピーを行う | 依存 |
| [`Array1`](#array1) | 1次元配列を複製する | `O(N)` |
| [`Array2`](#array2) | 2次元ジャグ配列を複製する | `O(N × M)` |
| [`Array3`](#array3) | 3次元ジャグ配列を複製する | `O(N × M × L)` |
| [`Array4`](#array4) | 4次元ジャグ配列を複製する | `O(N × M × L × K)` |
| [`Copy2D`](#copy2d) | 2次元多次元配列を複製する | `O(N × M)` |
| [`Copy3D`](#copy3d) | 3次元多次元配列を複製する | `O(N × M × L)` |
| [`Copy4D`](#copy4d) | 4次元多次元配列を複製する | `O(N × M × L × K)` |
| [`List1`](#list1) | 1次元リストを複製する | `O(N)` |
| [`List2`](#list2) | 2次元リストを複製する | `O(N × M)` |
| [`List3`](#list3) | 3次元リストを複製する | `O(N × M × L)` |
| [`List4`](#list4) | 4次元リストを複製する | `O(N × M × L × K)` |
| `Copy` | `Dictionary`、`HashSet`、`Queue`、`Stack`、`LinkedList`、`SortedSet`、`SortedDictionary` を複製する | 要素数に比例 |
| `Copy` | `Treap<T>` と `ImplicitTreap<T>` を複製する | 要素数に比例 |

---

# 前提

このクラスの役割は、**配列やリストの各階層を新しいオブジェクトとして複製すること**です。

たとえば、次のようなコードでは:

```csharp
int[][] a = { new[] { 1, 2 }, new[] { 3, 4 } };
int[][] b = a;
```

`b` は `a` の参照そのものなので、どちらかを更新するともう片方も変わります。

そのため、コピーを作りたいときは、`DeepCopy.Array2(a)` のように呼び出します。

また、より簡単に書くために、以下のようなショートカットも用意しています。

```csharp
var b = DeepCopy.Copy(a);
var c = DeepCopy.Jagged(a);
```

`Copy` は型に応じたコピーを自動的に選ぶため、`var` と組み合わせて使いやすいです。

`Jagged` は、ジャグ配列のコピーであることを明示したいときに使うメソッドです。

---

# Copy

```csharp
public static T[] Copy<T>(T[] a)
public static T[][] Copy<T>(T[][] a)
public static T[][][] Copy<T>(T[][][] a)
public static T[][][][] Copy<T>(T[][][][] a)
public static T[,] Copy<T>(T[,] a)
public static T[,,] Copy<T>(T[,,] a)
public static T[,,,] Copy<T>(T[,,,] a)
```

## 意味

コピー対象の型に応じて適切なメソッドを選び、配列やリストを複製します。

型が `int[][]` なら `Copy(int[][])` が呼ばれ、`bool[,]` なら `Copy(bool[,])` が呼ばれます。

## 例

```csharp
int[][] a =
{
    new[] { 1, 2 },
    new[] { 3, 4 }
};

var b = DeepCopy.Copy(a);
b[0][0] = 99;

Console.WriteLine(a[0][0]);
Console.WriteLine(b[0][0]);
```

出力：

```text
1
99
```

また、2次元多次元配列も同じように使えます。

```csharp
bool[,] grid =
{
    { true, false },
    { false, true }
};

var copy = DeepCopy.Copy(grid);
copy[0, 0] = false;
```

このように、`Copy` は「型に応じたコピー」を簡潔に書くためのショートカットです。

---

# Jagged

```csharp
public static T[][] Jagged<T>(T[][] a)
public static T[][][] Jagged<T>(T[][][] a)
public static T[][][][] Jagged<T>(T[][][][] a)
```

## 意味

ジャグ配列を明示的にコピーしたいときに使うメソッドです。

`Array2` / `Array3` / `Array4` の別名として扱えます。

## 例

```csharp
int[][] a =
{
    new[] { 1, 2, 3 },
    new[] { 4 },
    new[] { 5, 6 }
};

var b = DeepCopy.Jagged(a);
b[0][0] = 100;

Console.WriteLine(a[0][0]);
Console.WriteLine(b[0][0]);
```

出力：

```text
1
100
```

各行の長さが異なるジャグ配列でも、内部では各行ごとに独立した配列として複製されます。

---

# Copy2D

```csharp
public static T[,] Copy<T>(T[,] a)
```

## 意味

`bool[,]` や `int[,]` のような通常の2次元配列を複製します。

この場合はジャグ配列ではなく、C# の通常の多次元配列としてメモリ上に連続して配置されます。

## 例

```csharp
int[,] a =
{
    { 1, 2 },
    { 3, 4 }
};

var b = DeepCopy.Copy(a);
b[0, 0] = 99;

Console.WriteLine(a[0, 0]);
Console.WriteLine(b[0, 0]);
```

出力：

```text
1
99
```

## 計算量

行数を `N`、列数を `M` とすると、

* 時間計算量: `O(N × M)`
* 空間計算量: `O(N × M)`

---

# Copy3D

```csharp
public static T[,,] Copy<T>(T[,,] a)
```

## 意味

3次元の通常の多次元配列を複製します。

## 計算量

要素数を `N × M × L` とすると、

* 時間計算量: `O(N × M × L)`
* 空間計算量: `O(N × M × L)`

---

# Copy4D

```csharp
public static T[,,,] Copy<T>(T[,,,] a)
```

## 意味

4次元の通常の多次元配列を複製します。

## 計算量

要素数を `N × M × L × K` とすると、

* 時間計算量: `O(N × M × L × K)`
* 空間計算量: `O(N × M × L × K)`

---

# 注意点

このクラスは「配列の各層」「リストの各層」を複製するものです。

ただし、要素の中身が参照型である場合、**各要素そのものの中身までは再帰的に複製しません**。

つまり、

```csharp
string[][] a = ...
```

の場合は、各 `string` 自体は値そのものなので問題ありません。

一方、

```csharp
Person[][] a = ...
```

のようなオブジェクト配列では、`Person` の中身の深いコピーはされません。

そのため、`DeepCopy` は「配列・リストの構造をコピーする」用途に特化しています。

標準コレクションと `Treap<T>` / `ImplicitTreap<T>` にも対応しています。これらはコンテナとノードを新しく作成するため、コピー後の追加・削除・並べ替えは元のデータ構造に影響しません。

```csharp
var treapCopy = DeepCopy.Copy(treap);
var queueCopy = DeepCopy.Copy(queue);
var mapCopy = DeepCopy.Copy(map);
```

値が参照型の場合、その値オブジェクト自体はコピーされません。必要なら、値をコピーしてからコンテナへ追加してください。

---

# Array1

```csharp
public static T[] Array1<T>(T[] a)
```

## 意味

1次元配列を複製します。

```csharp
T[] copied = DeepCopy.Array1(a);
```

このとき、`copied` は `a` とは別の配列になります。

## 例

```csharp
int[] a = { 1, 2, 3 };
int[] b = DeepCopy.Array1(a);

b[0] = 99;

Console.WriteLine(string.Join(" ", a));
Console.WriteLine(string.Join(" ", b));
```

出力：

```text
1 2 3
99 2 3
```

`a` は変わらず、`b` だけが変更されています。

## 計算量

* 時間計算量: `O(N)`
* 空間計算量: `O(N)`

---

# Array2

```csharp
public static T[][] Array2<T>(T[][] a)
```

## 意味

2次元配列を複製します。

外側の配列と各行の配列の両方を新しく作ります。

## 例

```csharp
int[][] a = { new[] { 1, 2 }, new[] { 3, 4 } };
int[][] b = DeepCopy.Array2(a);

b[0][0] = 100;

Console.WriteLine(a[0][0]);
Console.WriteLine(b[0][0]);
```

出力：

```text
1
100
```

## 計算量

行数を `N`、1行あたりの長さを `M` とすると、

* 時間計算量: `O(N × M)`
* 空間計算量: `O(N × M)`

---

# Array3

```csharp
public static T[][][] Array3<T>(T[][][] a)
```

## 意味

3次元配列を複製します。

各レイヤーごとに新しい配列を確保して、内容をコピーします。

## 例

```csharp
int[][][] a =
{
    new[]
    {
        new[] { 1, 2 },
        new[] { 3, 4 }
    },
    new[]
    {
        new[] { 5, 6 },
        new[] { 7, 8 }
    }
};

int[][][] b = DeepCopy.Array3(a);
b[0][0][0] = 99;

Console.WriteLine(a[0][0][0]);
Console.WriteLine(b[0][0][0]);
```

出力：

```text
1
99
```

## 計算量

要素数を大まかに `N × M × L` とすると、

* 時間計算量: `O(N × M × L)`
* 空間計算量: `O(N × M × L)`

---

# Array4

```csharp
public static T[][][][] Array4<T>(T[][][][] a)
```

## 意味

4次元配列を複製します。

内部の3次元配列・2次元配列・1次元配列まで順に新しい参照を作成します。

## 例

```csharp
int[][][][] a = new int[2][][][];

a[0] = new int[2][][];
a[0][0] = new int[2][];
a[0][0][0] = new[] { 1, 2 };
a[0][0][1] = new[] { 3, 4 };

int[][][][] b = DeepCopy.Array4(a);
b[0][0][0][0] = 100;

Console.WriteLine(a[0][0][0][0]);
Console.WriteLine(b[0][0][0][0]);
```

出力：

```text
1
100
```

## 計算量

要素数を大まかに `N × M × L × K` とすると、

* 時間計算量: `O(N × M × L × K)`
* 空間計算量: `O(N × M × L × K)`

---

# List1

```csharp
public static List<T> List1<T>(List<T> a)
```

## 意味

1次元リストを複製します。

```csharp
List<int> copied = DeepCopy.List1(list);
```

このとき、`copied` は元のリストと独立しています。

## 例

```csharp
var a = new List<int> { 1, 2, 3 };
var b = DeepCopy.List1(a);

b[0] = 99;

Console.WriteLine(string.Join(" ", a));
Console.WriteLine(string.Join(" ", b));
```

出力：

```text
1 2 3
99 2 3
```

## 計算量

* 時間計算量: `O(N)`
* 空間計算量: `O(N)`

---

# List2

```csharp
public static List<List<T>> List2<T>(List<List<T>> a)
```

## 意味

2次元リストを複製します。

外側のリストと内側のリストを両方新しく作ります。

## 例

```csharp
var a = new List<List<int>>
{
    new List<int> { 1, 2 },
    new List<int> { 3, 4 }
};

var b = DeepCopy.List2(a);
b[0][0] = 100;

Console.WriteLine(a[0][0]);
Console.WriteLine(b[0][0]);
```

出力：

```text
1
100
```

## 計算量

行数を `N`、1行あたりの長さを `M` とすると、

* 時間計算量: `O(N × M)`
* 空間計算量: `O(N × M)`

---

# List3

```csharp
public static List<List<List<T>>> List3<T>(List<List<List<T>>> a)
```

## 意味

3次元リストを複製します。

深い階層まで順に新しいリストを作ります。

## 例

```csharp
var a = new List<List<List<int>>>
{
    new List<List<int>>
    {
        new List<int> { 1, 2 },
        new List<int> { 3, 4 }
    }
};

var b = DeepCopy.List3(a);
b[0][0][0] = 99;

Console.WriteLine(a[0][0][0]);
Console.WriteLine(b[0][0][0]);
```

出力：

```text
1
99
```

## 計算量

要素数を `N × M × L` とすると、

* 時間計算量: `O(N × M × L)`
* 空間計算量: `O(N × M × L)`

---

# List4

```csharp
public static List<List<List<List<T>>>> List4<T>(
    List<List<List<List<T>>>> a)
```

## 意味

4次元リストを複製します。

4重にネストしたリストを、各階層ごとに別オブジェクトとして複製します。

## 例

```csharp
var a = new List<List<List<List<int>>>>();
var inner = new List<List<List<int>>>();
var inner2 = new List<List<int>>();
var inner3 = new List<int> { 1, 2 };

inner2.Add(inner3);
inner.Add(inner2);
a.Add(inner);

var b = DeepCopy.List4(a);
b[0][0][0][0] = 100;

Console.WriteLine(a[0][0][0][0]);
Console.WriteLine(b[0][0][0][0]);
```

出力：

```text
1
100
```

## 計算量

要素数を `N × M × L × K` とすると、

* 時間計算量: `O(N × M × L × K)`
* 空間計算量: `O(N × M × L × K)`

---

# まとめ

`DeepCopy` は、以下のような場面で特に役立ちます。

* 配列やリストを別オブジェクトとして保持したい
* 破壊的変更を避けたい
* 2次元・3次元・4次元の入れ子構造を安全に後処理したい
* 競技プログラミングで探索や更新を複数回行う前に状態を退避したい

実際には、

```csharp
var copied = DeepCopy.Array2(a);
```

のように使うだけで、元データを壊さずに安全にコピーできます。

ただし、`DeepCopy` は「配列・リストの構造」コピーに特化しているため、要素そのものが参照型のときは**さらに深く複製されない**ことを理解しておくとよいです。

このクラスは、競技プログラミングや状態管理でとても便利なユーティリティです。
