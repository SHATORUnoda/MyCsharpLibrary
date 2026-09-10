# `LibMath` クラス 解説書

## 目次

### 基本

* [LibMath クラス](#libmath-class)
* [計算量について](#complexity)

### メソッド一覧

| メソッド                                            | 概要                 |               計算量 |
| ----------------------------------------------- | ------------------ | ----------------: |
| [`Gcd`](#gcd)                                   | 最大公約数を求める          | `O(log min(a,b))` |
| [`Lcm`](#lcm)                                   | 最小公倍数を求める          | `O(log min(a,b))` |
| [`Pow`](#pow)                                   | `a^n` を求める         |        `O(log n)` |
| [`ExtGcd`](#extgcd)                             | 拡張ユークリッドの互除法       | `O(log min(a,b))` |
| [`PowMod`](#powmod)                             | `a^e mod mod` を求める |        `O(log e)` |
| [`MulMod`](#mulmod)                             | オーバーフローを避けて乗算剰余    |            `O(1)` |
| [`ModInverse`](#modinverse)                     | 法における逆元を求める        |      `O(log mod)` |
| [`IsPrime`](#isprime)                           | 素数判定               |     `O(log n)` 程度 |
| [`Eratosthenes`](#eratosthenes)                 | エラトステネスの篩          |  `O(N log log N)` |
| [`SmallestPrimeFactor`](#smallest-prime-factor) | 各整数の最小素因数を求める      |  `O(N log log N)` |
| [`PrimeFactorize`](#prime-factorize)            | 素因数分解              |           `O(√N)` |
| [`Divisors`](#divisors)                         | 正の約数を昇順で列挙         | `O(√N + D log D)` |
| [`Factorial`](#factorial)                       | `n!` を求める          |            `O(N)` |
| [`NPr`](#npr)                                   | `nPr` を求める         |            `O(r)` |
| [`NCr`](#ncr)                                   | `nCr` を求める         |   `O(min(r,n-r))` |
| [`NCrMod`](#ncrmod)                             | `nCr mod p` を求める   |    `O(r + log p)` |
| [`Totient`](#totient)                           | Euler の φ 関数       |           `O(√N)` |
| [`Mobius`](#mobius)                             | Möbius 関数 μ(n)     |           `O(√N)` |
| [`ChineseRemainder`](#chinese-remainder)        | 中国剰余定理を解く          |   `O(N log M)` 程度 |

---

<a id="libmath-class"></a>

# `LibMath` クラス

`LibMath` は、競技プログラミングで頻繁に使用する**整数数学・数論関連のアルゴリズム**をまとめたクラスです。

主に以下の処理を扱います。

```text
最大公約数・最小公倍数
        ↓
べき乗・拡張ユークリッド
        ↓
mod 演算・逆元
        ↓
素数判定・素因数分解
        ↓
約数
        ↓
階乗・順列・組合せ
        ↓
Euler φ 関数
        ↓
Möbius 関数
        ↓
中国剰余定理
```

すべて `static` メソッドなので、インスタンスを作成する必要はありません。

```csharp
long g = LibMath.Gcd(12, 18);
```

---

<a id="complexity"></a>

# 計算量について

このクラスでは、主に以下の計算量が登場します。

| 計算量              | 代表的な処理                                            |
| ---------------- | ------------------------------------------------- |
| `O(1)`           | `MulMod`                                          |
| `O(log N)`       | `Gcd`, `Pow`, `PowMod`, `ModInverse`              |
| `O(√N)`          | `PrimeFactorize`, `Totient`, `Mobius`, `Divisors` |
| `O(N log log N)` | `Eratosthenes`, `SmallestPrimeFactor`             |
| `O(N)`           | `Factorial`                                       |
| 組合せ数に比例          | `NCr`, `NPr`                                      |

特に `PrimeFactorize` や `Totient` は、単純に `2～N` を調べるのではなく、最大でも `√N` 程度まで調べればよいという性質を利用しています。

---

<a id="gcd"></a>

# `Gcd`

```csharp
public static long Gcd(long a, long b)
```

## 概要

2つの整数 `a`, `b` の**最大公約数**を求めます。

例えば、

```csharp
LibMath.Gcd(12, 18)
```

なら、

```text
6
```

が返ります。

負数にも対応しており、内部で絶対値を取ります。

```csharp
LibMath.Gcd(-12, 18)
```

も、

```text
6
```

になります。

## 計算量

ユークリッドの互除法を使用しているため、

```text
O(log(min(|a|, |b|)))
```

です。

## 仕組み

基本的な性質、

```text
gcd(a,b) = gcd(b,a mod b)
```

を利用します。

例えば、

```text
gcd(48,18)

48 mod 18 = 12
18 mod 12 = 6
12 mod 6  = 0
```

なので、

```text
gcd(48,18) = 6
```

となります。

## 使い方

```csharp
long g = LibMath.Gcd(a, b);
```

---

<a id="lcm"></a>

# `Lcm`

```csharp
public static long Lcm(long a, long b)
```

## 概要

2つの整数の**最小公倍数**を求めます。

```csharp
LibMath.Lcm(12, 18)
```

なら、

```text
36
```

が返ります。

## 計算量

`Gcd` を利用しているため、

```text
O(log(min(|a|, |b|)))
```

です。

## 計算式

以下の公式を使用しています。

```text
lcm(a,b) = |a / gcd(a,b) × b|
```

先に割り算を行うことで、単純な

```text
a × b / gcd(a,b)
```

よりオーバーフローしにくくしています。

## 使い方

```csharp
long l = LibMath.Lcm(a, b);
```

`a` または `b` が `0` の場合は、

```text
0
```

を返します。

## 注意

最終的な結果が `long` の範囲を超える場合は、当然オーバーフローする可能性があります。

---

<a id="pow"></a>

# `Pow`

```csharp
public static long Pow(long a, long n)
```

## 概要

```text
a^n
```

を求めます。

例えば、

```csharp
LibMath.Pow(2, 10)
```

なら、

```text
1024
```

です。

## 計算量

**繰り返し二乗法**を使用しているため、

```text
O(log n)
```

です。

## 仕組み

例えば、

```text
2^13
```

を考えます。

`13` を二進数で表すと、

```text
13 = 1101₂
```

なので、

```text
2^13 = 2^8 × 2^4 × 2^1
```

と分解できます。

これによって、13回掛ける必要がなくなります。

```csharp
while (n > 0)
{
    if ((n & 1) != 0)
        res *= a;

    a *= a;
    n >>= 1;
}
```

## 使い方

```csharp
long x = LibMath.Pow(3, 20);
```

## 注意

このメソッドは**剰余を取らない通常のべき乗**です。

結果が `long` の範囲を超える場合はオーバーフローします。

また、`n < 0` の場合はループに入らないため、実質的に `1` が返ります。

競技プログラミングでは、通常 `n >= 0` を前提として使用してください。

---

<a id="extgcd"></a>

# `ExtGcd`

```csharp
public static (long Gcd, long X, long Y) ExtGcd(
    long a,
    long b)
```

## 概要

**拡張ユークリッドの互除法**を使用して、

```text
a × x + b × y = gcd(a,b)
```

を満たす

```text
gcd(a,b)
x
y
```

を求めます。

戻り値は、

```csharp
var (g, x, y) = LibMath.ExtGcd(a, b);
```

のように受け取れます。

## 例

```csharp
var (g, x, y) = LibMath.ExtGcd(30, 18);
```

とした場合、

```text
g = 6
```

で、

```text
30x + 18y = 6
```

を満たす `x`, `y` が返されます。

## 計算量

```text
O(log(min(|a|,|b|)))
```

です。

## 主な用途

特に、

* モジュラ逆元
* 一次不定方程式
* 中国剰余定理

などで使用します。

実際にこのクラスの `ModInverse` と `ChineseRemainder` でも利用されています。

---

<a id="powmod"></a>

# `PowMod`

```csharp
public static long PowMod(
    long a,
    long e,
    long mod)
```

## 概要

```text
a^e mod mod
```

を求めます。

例えば、

```csharp
LibMath.PowMod(2, 10, 1000)
```

なら、

```text
24
```

です。

## 計算量

繰り返し二乗法を使用するため、

```text
O(log e)
```

です。

## 使い方

```csharp
long x = LibMath.PowMod(a, e, MOD);
```

競技プログラミングで非常によく使用する形です。

## 特徴

通常の `Pow` と違い、途中の掛け算でも `MulMod` を使用しています。

そのため、

```text
a^e
```

自体が非常に大きくなる場合でも、剰余を維持しながら計算できます。

## 条件

```text
mod > 0
e >= 0
```

が必要です。

それ以外の場合は `ArgumentOutOfRangeException` が発生します。

---

<a id="mulmod"></a>

# `MulMod`

```csharp
public static long MulMod(
    long a,
    long b,
    long mod)
```

## 概要

```text
(a × b) mod mod
```

を求めます。

通常の、

```csharp
(a * b) % mod
```

では `a * b` の時点で `long` の範囲を超える可能性があります。

このメソッドでは `.NET 7+` の `UInt128` を利用して、その問題を回避します。

## 計算量

```text
O(1)
```

です。

## 仕組み

```csharp
(UInt128)(ulong)a * (ulong)b
```

によって、64bit × 64bit の乗算結果を128bitで保持します。

その後、

```text
mod
```

で剰余を取ります。

## 使い方

```csharp
long x = LibMath.MulMod(a, b, MOD);
```

特に、

```csharp
PowMod
IsPrime
NCrMod
ChineseRemainder
```

などの内部計算で使用されています。

## 注意

この実装は `long` の値を `ulong` に変換して計算しているため、**負数を一般的な数学上の mod として扱う用途には注意が必要です**。

主に、

```text
0 <= a,b < mod
```

のように正規化された値で使用することを想定しています。

---

<a id="modinverse"></a>

# `ModInverse`

```csharp
public static long ModInverse(
    long a,
    long mod)
```

## 概要

```text
a × x ≡ 1 (mod mod)
```

を満たす `x`、つまり **a の mod における逆元**を求めます。

逆元が存在する条件は、

```text
gcd(a, mod) = 1
```

です。

## 例

```csharp
LibMath.ModInverse(3, 11)
```

では、

```text
3 × 4 = 12 ≡ 1 (mod 11)
```

なので、

```text
4
```

が返ります。

## 計算量

拡張ユークリッドの互除法を使用するため、

```text
O(log mod)
```

です。

## 仕組み

`ExtGcd` によって、

```text
a × x + mod × y = gcd(a,mod)
```

を求めます。

`gcd(a,mod)=1` なら、

```text
a × x + mod × y = 1
```

なので、

```text
a × x ≡ 1 (mod mod)
```

となります。

したがって `x` が逆元です。

## 使い方

```csharp
long inv = LibMath.ModInverse(a, MOD);
```

## 逆元が存在しない場合

例えば、

```csharp
LibMath.ModInverse(6, 15)
```

では、

```text
gcd(6,15) = 3
```

なので逆元は存在しません。

この場合、

```text
ArgumentException
```

が発生します。

---

<a id="isprime"></a>

# `IsPrime`

```csharp
public static bool IsPrime(long n)
```

## 概要

`n` が素数かどうかを判定します。

```csharp
LibMath.IsPrime(97)
```

なら、

```text
true
```

です。

```csharp
LibMath.IsPrime(100)
```

なら、

```text
false
```

です。

## アルゴリズム

**Miller-Rabin 素数判定法**を使用しています。

さらに、

```text
2
325
9375
28178
450775
9780504
1795265022
```

の7つの底を固定して使用しています。

この組み合わせにより、`long` の範囲で決定的に判定できます。

## 計算量

固定された7個の底について Miller-Rabin を実行するため、

```text
O(log n)
```

程度です。

通常の `√n` まで試し割りする方法より高速です。

## 使い方

```csharp
if (LibMath.IsPrime(n))
{
    // n は素数
}
```

## 特徴

`long` 程度の大きな整数に対しても高速に素数判定できます。

内部では `PowMod` と `MulMod` を利用しています。

---

<a id="eratosthenes"></a>

# `Eratosthenes`

```csharp
public static bool[] Eratosthenes(int n)
```

## 概要

`0～n` の各整数について、

```text
prime[i] == true
```

なら `i` が素数、

```text
prime[i] == false
```

なら素数ではない、という配列を返します。

例えば、

```csharp
var prime = LibMath.Eratosthenes(10);
```

なら、

```text
prime[0] = false
prime[1] = false
prime[2] = true
prime[3] = true
prime[4] = false
prime[5] = true
...
```

となります。

## 計算量

```text
O(N log log N)
```

です。

空間計算量は、

```text
O(N)
```

です。

## 使い方

```csharp
var prime = LibMath.Eratosthenes(N);

for (int i = 2; i <= N; i++)
{
    if (prime[i])
    {
        // i は素数
    }
}
```

## 特徴

大量の整数について素数判定を行いたい場合に適しています。

例えば、

```text
IsPrime を N 回呼ぶ
```

よりも、

```text
Eratosthenes(N)
```

で一度に求めたほうが効率的な場合があります。

---

<a id="smallest-prime-factor"></a>

# `SmallestPrimeFactor`

```csharp
public static int[] SmallestPrimeFactor(int n)
```

## 概要

各整数について、**最小の素因数**を求めます。

返される配列は、

```text
spf[i] = i の最小素因数
```

です。

例えば、

```text
i       2  3  4  5  6  8  9  10
spf[i]  2  3  2  5  2  2  3  2
```

となります。

素数の場合は、

```text
spf[p] = p
```

です。

## 計算量

```text
O(N log log N)
```

空間計算量：

```text
O(N)
```

です。

## 使い方

```csharp
var spf = LibMath.SmallestPrimeFactor(N);
```

例えば `60` を素因数分解するなら、

```csharp
int x = 60;

while (x > 1)
{
    int p = spf[x];

    Console.WriteLine(p);

    x /= p;
}
```

のようにできます。

結果は、

```text
2
2
3
5
```

となります。

## メリット

同じ範囲の整数を何度も素因数分解する場合に非常に便利です。

---

<a id="prime-factorize"></a>

# `PrimeFactorize`

```csharp
public static List<(long Prime, int Count)>
    PrimeFactorize(long n)
```

## 概要

整数 `n` を素因数分解し、

```text
(素因数, 指数)
```

のリストとして返します。

例えば、

```csharp
var f = LibMath.PrimeFactorize(360);
```

なら、

```text
(2,3)
(3,2)
(5,1)
```

です。

つまり、

```text
360 = 2^3 × 3^2 × 5
```

です。

## 計算量

```text
O(√N)
```

です。

ただし、素因数が早い段階で見つかる場合は実際の処理量はもっと少なくなります。

## 使い方

```csharp
foreach (var (p, e) in LibMath.PrimeFactorize(n))
{
    Console.WriteLine($"{p}^{e}");
}
```

## 例

```text
n = 840
```

なら、

```text
840 = 2^3 × 3 × 5 × 7
```

なので、

```text
(2,3)
(3,1)
(5,1)
(7,1)
```

となります。

## 対応範囲

`0` は素因数分解できないため、

```text
ArgumentException
```

が発生します。

負数については絶対値を取ってから分解します。

`1` と `-1` の場合は空のリストが返ります。

---

<a id="divisors"></a>

# `Divisors`

```csharp
public static long[] Divisors(long n)
```

## 概要

`n` の**正の約数を昇順**で列挙します。

例えば、

```csharp
LibMath.Divisors(12)
```

なら、

```text
[1, 2, 3, 4, 6, 12]
```

です。

## 計算量

約数の個数を `D` とすると、

```text
O(√N + D log D)
```

です。

ただし、この実装ではソートを行っているのではなく、

```text
small
large
```

の2つのリストを利用して昇順にしています。

したがって、実際の処理は約数探索 `O(√N)` と、`Reverse` / `AddRange` による `O(D)` が中心です。

## 仕組み

`i` が `n` の約数なら、

```text
n / i
```

も約数です。

例えば `12` なら、

```text
1 × 12
2 × 6
3 × 4
```

です。

そのため `√n` まで調べれば、両方の約数を取得できます。

## 使い方

```csharp
foreach (long d in LibMath.Divisors(n))
{
    // n の約数 d
}
```

## 注意

このメソッドは、

```text
n > 0
```

を要求します。

`n <= 0` の場合は `ArgumentOutOfRangeException` です。

---

<a id="factorial"></a>

# `Factorial`

```csharp
public static BigInteger Factorial(int n)
```

## 概要

```text
n!
```

を求めます。

例えば、

```csharp
LibMath.Factorial(5)
```

なら、

```text
120
```

です。

## 計算量

```text
O(N)
```

です。

## `BigInteger` を使用

通常の `long` では、

```text
20!
```

程度で限界になります。

このメソッドでは `BigInteger` を使用しているため、`long` より遥かに大きな値を扱えます。

## 使い方

```csharp
BigInteger fact = LibMath.Factorial(100);
```

---

<a id="npr"></a>

# `NPr`

```csharp
public static BigInteger NPr(int n, int r)
```

## 概要

順列

```text
nPr
```

を求めます。

公式は、

```text
nPr = n × (n-1) × ... × (n-r+1)
```

です。

例えば、

```csharp
LibMath.NPr(5, 2)
```

なら、

```text
5 × 4 = 20
```

です。

## 計算量

`r` 回の乗算を行うため、

```text
O(r)
```

です。

## 使い方

```csharp
BigInteger x = LibMath.NPr(n, r);
```

`r < 0` または `r > n` の場合は、

```text
0
```

を返します。

---

<a id="ncr"></a>

# `NCr`

```csharp
public static BigInteger NCr(int n, int r)
```

## 概要

組合せ

```text
nCr
```

を求めます。

公式は、

```text
nCr = n! / (r!(n-r)!)
```

です。

ただし、この実装では階乗を直接計算せず、

```text
n(n-1)... / r!
```

の形で計算しています。

## 計算量

対称性、

```text
C(n,r) = C(n,n-r)
```

を利用して、

```text
r = min(r, n-r)
```

としています。

そのため、

```text
O(min(r,n-r))
```

です。

## 使い方

```csharp
BigInteger x = LibMath.NCr(n, r);
```

例えば、

```csharp
LibMath.NCr(5, 2)
```

は、

```text
10
```

です。

## 大きな値にも対応

戻り値が `BigInteger` なので、

```csharp
NCr(1000, 500)
```

のような非常に大きな組合せ数も扱えます。

---

<a id="ncrmod"></a>

# `NCrMod`

```csharp
public static long NCrMod(
    int n,
    int r,
    long p)
```

## 概要

```text
nCr mod p
```

を求めます。

ここで `p` は素数で、

```text
n < p
```

を仮定しています。

## 計算量

実際には、

* `r` 回の乗算
* 逆元計算 `O(log p)`
* 素数判定

を行います。

したがって、概ね

```text
O(r + log p)
```

に加えて `IsPrime(p)` の計算量が必要です。

## 仕組み

組合せを、

```text
C(n,r)
=
(n-r+1)(n-r+2)...n
--------------------------------
1 × 2 × ... × r
```

として計算します。

分子を `numerator`、

分母を `denominator`

として、

```text
numerator × denominator^(-1) mod p
```

を求めます。

## 使い方

```csharp
long ans = LibMath.NCrMod(n, r, MOD);
```

## 条件

このメソッドでは、

```text
p は素数
n < p
```

が必要です。

そのため、

```text
n >= p
```

の場合は例外になります。

また、`p` が素数でなければ `ArgumentException` です。

---

<a id="totient"></a>

# `Totient`

```csharp
public static long Totient(long n)
```

## 概要

Euler の φ 関数、

```text
φ(n)
```

を求めます。

これは、

```text
1 <= k <= n
```

のうち、

```text
gcd(k,n) = 1
```

となる `k` の個数です。

例えば、

```csharp
LibMath.Totient(9)
```

では、

```text
1,2,4,5,7,8
```

が互いに素なので、

```text
φ(9) = 6
```

です。

## 計算量

素因数を `√n` 程度まで探索するため、

```text
O(√N)
```

です。

## 公式

素因数分解が、

```text
n = p1^a1 × p2^a2 × ...
```

なら、

```text
φ(n)
= n
  × (1 - 1/p1)
  × (1 - 1/p2)
  × ...
```

です。

この実装ではこの公式を直接利用しています。

## 使い方

```csharp
long phi = LibMath.Totient(n);
```

---

<a id="mobius"></a>

# `Mobius`

```csharp
public static int Mobius(long n)
```

## 概要

Möbius 関数

```text
μ(n)
```

を求めます。

定義は、

### `n = 1`

```text
μ(1) = 1
```

### 平方因子を持つ場合

```text
μ(n) = 0
```

### 異なる素因数が偶数個

```text
μ(n) = 1
```

### 異なる素因数が奇数個

```text
μ(n) = -1
```

です。

## 例

```text
n = 6
6 = 2 × 3
```

異なる素因数は2個なので、

```text
μ(6) = 1
```

です。

一方、

```text
n = 12
12 = 2^2 × 3
```

は平方因子 `2²` を持つので、

```text
μ(12) = 0
```

です。

## 計算量

```text
O(√N)
```

です。

## 使い方

```csharp
int mu = LibMath.Mobius(n);
```

## 注意

`n <= 0` の場合は `ArgumentOutOfRangeException` です。

---

<a id="chinese-remainder"></a>

# `ChineseRemainder`

```csharp
public static (long Remainder, long Modulus)
    ChineseRemainder(
        long[] rem,
        long[] mod)
```

## 概要

**中国剰余定理（CRT）**を利用して、複数の合同式を同時に満たす解を求めます。

入力は、

```text
x ≡ rem[0] (mod mod[0])
x ≡ rem[1] (mod mod[1])
...
```

です。

戻り値は、

```text
(Remainder, Modulus)
```

です。

つまり、

```text
x ≡ Remainder (mod Modulus)
```

が解になります。

すべての解は、

```text
Remainder + k × Modulus
```

と表せます。

---

## 例

次の連立合同式を考えます。

```text
x ≡ 2 (mod 3)
x ≡ 3 (mod 5)
```

```csharp
var (r, m) = LibMath.ChineseRemainder(
    new long[] { 2, 3 },
    new long[] { 3, 5 }
);
```

とすると、

```text
r = 8
m = 15
```

となります。

つまり、

```text
x ≡ 8 (mod 15)
```

です。

実際、

```text
8 mod 3 = 2
8 mod 5 = 3
```

となっています。

## 計算量

入力される合同式の個数を `N`、法の大きさを `M` とすると、

```text
O(N log M)
```

程度です。

各ステップで `Gcd`、`ModInverse` などを使用します。

## 仕組み

現在の解を、

```text
x ≡ r (mod m)
```

とします。

次の条件、

```text
x ≡ ri (mod mi)
```

を追加する場合、

```text
x = r + m × k
```

と置きます。

すると、

```text
r + m×k ≡ ri (mod mi)
```

なので、

```text
m×k ≡ ri-r (mod mi)
```

となります。

これを `ExtGcd` / `ModInverse` を利用して解きます。

---

## 互いに素でない法にも対応

一般的な中国剰余定理では、

```text
mod[i] 同士が互いに素
```

という条件を要求することがあります。

この実装では、より一般的に、

```text
gcd(m, mi)
```

を考慮しています。

したがって、法が互いに素でなくても、

```text
(ri-r) % gcd(m,mi) == 0
```

なら解を統合できます。

例えば、

```text
x ≡ 1 (mod 4)
x ≡ 5 (mod 6)
```

では、

```text
gcd(4,6) = 2
```

で、

```text
5 - 1 = 4
```

は2で割り切れるので解が存在します。

---

## 解が存在しない場合

例えば、

```text
x ≡ 0 (mod 2)
x ≡ 1 (mod 2)
```

では同時に満たすことができません。

この場合、

```text
ArgumentException
```

が発生します。

---

## オーバーフロー対策

CRTでは途中計算で、

```text
m × k
```

や、

```text
m × mi
```

が `long` の範囲を超える可能性があります。

この実装では、

```csharp
BigInteger
```

を途中計算に使用しています。

最終的に、

```text
Remainder
Modulus
```

が `long` の範囲に収まるか確認し、超える場合は

```text
OverflowException
```

を発生させます。

## 使い方

```csharp
long[] rem = { 2, 3, 2 };
long[] mod = { 3, 5, 7 };

var (r, m) = LibMath.ChineseRemainder(rem, mod);

Console.WriteLine(r);
Console.WriteLine(m);
```

この場合、

```text
x ≡ 2 (mod 3)
x ≡ 3 (mod 5)
x ≡ 2 (mod 7)
```

を同時に満たす `x` が求まります。

---

# メソッドの関係

このクラスのメソッドは独立しているわけではなく、いくつかのメソッドが別のメソッドを利用しています。

```text
Gcd
 ├─ Lcm
 ├─ ExtGcd
 │    └─ ModInverse
 │          └─ NCrMod
 │
 └─ ChineseRemainder
```

また、

```text
MulMod
 └─ PowMod
      └─ IsPrime
```

という関係があります。

素数関連では、

```text
Eratosthenes
    └─ 大量の素数判定

SmallestPrimeFactor
    └─ 大量の素因数分解

PrimeFactorize
    ├─ Totient
    └─ Mobius と考え方が共通
```

という使い分けになります。

---

# 典型的な使用例

## 最大公約数

```csharp
long g = LibMath.Gcd(a, b);
```

---

## 最小公倍数

```csharp
long l = LibMath.Lcm(a, b);
```

---

## mod 付きべき乗

```csharp
long ans = LibMath.PowMod(a, n, MOD);
```

---

## 逆元

```csharp
long inv = LibMath.ModInverse(a, MOD);
```

---

## 素数判定

```csharp
if (LibMath.IsPrime(N))
{
    // 素数
}
```

---

## 素因数分解

```csharp
foreach (var (p, e) in LibMath.PrimeFactorize(N))
{
    // p^e
}
```

---

## 約数列挙

```csharp
foreach (long d in LibMath.Divisors(N))
{
    // 約数 d
}
```

---

## 組合せ

```csharp
BigInteger ans = LibMath.NCr(N, R);
```

---

## mod 上の組合せ

```csharp
long ans = LibMath.NCrMod(N, R, MOD);
```

---

## Euler φ 関数

```csharp
long phi = LibMath.Totient(N);
```

---

## Möbius 関数

```csharp
int mu = LibMath.Mobius(N);
```

---

## CRT

```csharp
var (r, m) = LibMath.ChineseRemainder(
    rem,
    mod
);
```

---

# まとめ

`LibMath` は競技プログラミングで使用頻度の高い数論アルゴリズムをまとめたライブラリです。

特に重要なものを分類すると、

```text
【基本的な整数演算】

Gcd
Lcm
Pow


【ユークリッドの互除法】

ExtGcd
ModInverse


【mod 演算】

MulMod
PowMod
NCrMod


【素数】

IsPrime
Eratosthenes
SmallestPrimeFactor
PrimeFactorize


【約数・数論関数】

Divisors
Totient
Mobius


【組合せ】

Factorial
NPr
NCr
NCrMod


【合同式】

ChineseRemainder
```

となります。

競技プログラミングでは特に、

```csharp
LibMath.Gcd(...)
LibMath.PowMod(...)
LibMath.ModInverse(...)
LibMath.IsPrime(...)
LibMath.PrimeFactorize(...)
LibMath.Divisors(...)
LibMath.NCr(...)
LibMath.NCrMod(...)
LibMath.Totient(...)
LibMath.ChineseRemainder(...)
```

あたりを覚えておくと、多くの数論問題でそのまま利用できます。

なお、このライブラリは**`long` の範囲を超える中間計算**に対して、`MulMod` では `UInt128`、`ChineseRemainder` や組合せ計算では `BigInteger` を使い分けている点が特徴です。
