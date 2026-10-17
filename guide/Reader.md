# Reader クラス 解説書

`Reader` は、競技プログラミング用に入力を簡単に読むためのユーティリティクラスです。

標準入力から値を読み込むとき、毎回 `Console.ReadLine()` や `Split()` を書くのは面倒です。

このクラスを使うと、次のように書けます。

```csharp
int n = Reader.ReadInt();
var a = Reader.ReadIntArray();
```

---

# 目次

| メソッド | 概要 | 計算量 |
| --- | --- | --- |
| [`ReadInt`](#readint) | 1つの int を読む | `O(1)` |
| [`ReadLong`](#readlong) | 1つの long を読む | `O(1)` |
| [`ReadIntArray`](#readintarray) | int 配列を読む | `O(N)` |
| [`ReadLongArray`](#readlongarray) | long 配列を読む | `O(N)` |
| [`ReadCharArray`](#readchararray) | 文字列を char 配列にする | `O(N)` |
| [`ReadStringArray`](#readstringarray) | 文字列配列を読む | `O(N)` |
| [`ReadInt2`](#readint2) | 2個の int を読む | `O(1)` |
| [`ReadInt3`](#readint3) | 3個の int を読む | `O(1)` |
| [`ReadInt4`](#readint4) | 4個の int を読む | `O(1)` |
| [`ReadInt5`](#readint5) | 5個の int を読む | `O(1)` |
| [`ReadInt6`](#readint6) | 6個の int を読む | `O(1)` |
| [`ReadLong2`](#readlong2) | 2個の long を読む | `O(1)` |
| [`ReadLong3`](#readlong3) | 3個の long を読む | `O(1)` |
| [`ReadLong4`](#readlong4) | 4個の long を読む | `O(1)` |
| [`ReadLong5`](#readlong5) | 5個の long を読む | `O(1)` |
| [`ReadLong6`](#readlong6) | 6個の long を読む | `O(1)` |
| [`WriteArray`](#writearray) | 配列を出力する | `O(N)` |

---

# ReadInt

```csharp
public static int ReadInt()
```

## 意味

1行の入力を `int` として読み込みます。

```csharp
int n = Reader.ReadInt();
```

## 例

入力:

```text
10
```

コード:

```csharp
int n = Reader.ReadInt();
Console.WriteLine(n);
```

出力：

```text
10
```

---

# ReadLong

```csharp
public static long ReadLong()
```

## 意味

1行の入力を `long` として読み込みます。

```csharp
long x = Reader.ReadLong();
```

---

# ReadIntArray

```csharp
public static int[] ReadIntArray()
```

## 意味

1行の空白区切り整数列を `int[]` に変換します。

## 例

入力:

```text
1 2 3 4 5
```

コード:

```csharp
var a = Reader.ReadIntArray();
Console.WriteLine(string.Join(" ", a));
```

出力：

```text
1 2 3 4 5
```

## 注意点

入力行が空文字列の場合、空配列を返します。

---

# ReadLongArray

```csharp
public static long[] ReadLongArray()
```

## 意味

1行の空白区切り long 列を読み込むためのメソッドです。

---

# ReadCharArray

```csharp
public static char[] ReadCharArray()
```

## 意味

1行の文字列を `char[]` にして返します。

## 例

入力:

```text
abcd
```

コード：

```csharp
var c = Reader.ReadCharArray();
Console.WriteLine(c[0]);
```

出力：

```text
a
```

---

# ReadStringArray

```csharp
public static string[] ReadStringArray()
```

## 意味

空白区切りの文字列を配列として読み込みます。

---

# ReadInt2 / ReadInt3 / ReadInt4 / ReadInt5 / ReadInt6

```csharp
public static (int, int) ReadInt2()
public static (int, int, int) ReadInt3()
public static (int, int, int, int) ReadInt4()
public static (int, int, int, int, int) ReadInt5()
public static (int, int, int, int, int, int) ReadInt6()
```

## 意味

複数の整数をまとめて読み込み、タプルで返します。

## 例

```csharp
var (x, y) = Reader.ReadInt2();
```

または

```csharp
var (a, b, c) = Reader.ReadInt3();
```

と書けます。

---

# ReadLong2 / ReadLong3 / ReadLong4 / ReadLong5 / ReadLong6

```csharp
public static (long, long) ReadLong2()
public static (long, long, long) ReadLong3()
public static (long, long, long, long) ReadLong4()
public static (long, long, long, long, long) ReadLong5()
public static (long, long, long, long, long, long) ReadLong6()
```

## 意味

複数の `long` をまとめて読み込むためのメソッドです。

---

# WriteArray

```csharp
public static void WriteArray<T>(IEnumerable<T> array, string separator = " ")
```

## 意味

配列や列挙可能な要素を、区切り文字付きで一行に出力します。

## 例

```csharp
Reader.WriteArray(new[] { 1, 2, 3, 4 });
```

出力：

```text
1 2 3 4
```

---

# 使いどころ

競技プログラミングでは、入力が次の形式で来ることが多いです。

```text
n
1 2 3 4 5
```

このとき、

```csharp
int n = Reader.ReadInt();
var a = Reader.ReadIntArray();
```

のように書けるので、非常に簡単です。

---

# 計算量

入力の総長が `L` のとき、

* 各メソッドの読み取り: `O(L)`
* 配列出力: `O(N)`

です。

---

# まとめ

`Reader` は、競技プログラミングで必須になりやすい入力処理を簡略化するクラスです。

たとえば、

```csharp
var (n, m) = Reader.ReadInt2();
var a = Reader.ReadIntArray();
```

のように書けるので、コードの見通しが良くなります。

このクラスを使うと、入力の定型部分をほとんど書かずに済むため、問題解決に集中しやすくなります。
