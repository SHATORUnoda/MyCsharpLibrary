# Radix クラス 解説書

`Radix` は、**10進数と n 進数の相互変換**を簡単に行うためのユーティリティクラスです。

主に次の2つの機能があります。

* `ToBase` — 10進数を n 進数に変換する
* `ToDecimal` — n 進数文字列を 10進数に変換する

---

# 目次

| メソッド | 概要 | 計算量 |
| --- | --- | --- |
| [`ToBase`](#tobase) | 10進数から任意の基数へ変換する | `O(log_r value)` |
| [`ToDecimal`](#todecimal) | 任意の基数から10進数へ変換する | `O(len(s))` |

---

# 前提

このクラスでは、基数は `2 ～ 36` の範囲を扱います。

文字として使えるのは次の順番です。

```text
0 1 2 3 4 5 6 7 8 9 A B C D E F ... Z
```

つまり、

```text
10 -> A
11 -> B
15 -> F
```

のように変換されます。

---

# ToBase

```csharp
public static string ToBase(long value, int radix)
```

## 意味

10進数の `value` を、`radix` 進数の表記に変換します。

## 例

```csharp
string s = Radix.ToBase(42, 2);
Console.WriteLine(s);
```

出力：

```text
101010
```

## もう一つの例

```csharp
Console.WriteLine(Radix.ToBase(255, 16));
```

出力：

```text
FF
```

## 計算量

`value` を `radix` で割り続けるため、

* 時間計算量: `O(log_r value)`
* 空間計算量: `O(log_r value)`

---

# ToDecimal

```csharp
public static long ToDecimal(string s, int radix)
```

## 意味

`radix` 進数の文字列 `s` を 10進数に変換します。

## 例

```csharp
long value = Radix.ToDecimal("FF", 16);
Console.WriteLine(value);
```

出力：

```text
255
```

## もう一つの例

```csharp
long value = Radix.ToDecimal("1010", 2);
Console.WriteLine(value);
```

出力：

```text
10
```

## 計算量

文字列の長さを `L` とすると、

* 時間計算量: `O(L)`
* 空間計算量: `O(1)`

---

# 例としてまとめる

```csharp
string a = Radix.ToBase(12345, 16);
Console.WriteLine(a); // 3039

long b = Radix.ToDecimal("3039", 16);
Console.WriteLine(b); // 12345
```

---

# 注意点

### 1. 基数の範囲

`radix` が `2 ～ 36` の範囲外なら例外が発生します。

```csharp
Radix.ToBase(100, 1);
```

のような呼び出しは不正です。

### 2. 大きな数

`long` で表せる範囲に収まる値に対して使用するのが前提です。

非常に大きい数については、別の扱いが必要になります。

---

# まとめ

`Radix` は、

* 二進数・八進数・十六進数などを扱う
* 数値の基数変換が必要なとき
* 文字列として表現された数を整数として読取りたいとき

に非常に便利なクラスです。

特に競技プログラミングでは、

```csharp
var x = Radix.ToBase(value, 2);
```

のようにビット表現や進数変換を簡単に行えるため、役立ちます。
