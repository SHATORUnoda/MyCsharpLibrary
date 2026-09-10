using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using static System.Console;
using static System.Math;
using static System.Array;

public static class LibMath
{
    // ============================================================
    // GCD / LCM
    // ============================================================

    /// <summary>
    /// 最大公約数
    /// </summary>
    /// <remarks>
    /// 計算量: O(log(min(a, b)))
    /// </remarks>
    public static long Gcd(long a, long b)
    {
        a = Abs(a);
        b = Abs(b);

        while (b != 0)
        {
            long t = a % b;
            a = b;
            b = t;
        }

        return a;
    }

    /// <summary>
    /// 最小公倍数
    /// </summary>
    /// <remarks>
    /// 計算量: O(log(min(a, b)))
    /// </remarks>
    public static long Lcm(long a, long b)
    {
        if (a == 0 || b == 0)
            return 0;

        return Abs(a / Gcd(a, b) * b);
    }


// ============================================================
// Power
// ============================================================

    /// <summary>
    /// a^n を求める。
    /// 繰り返し二乗法を使用する。
    /// </summary>
    /// <remarks>
    /// 計算量: O(log n)
    /// </remarks>
    public static long Pow(long a, long n)
    {
        long res = 1;

        while (n > 0)
        {
            if ((n & 1) != 0)
                res *= a;

            a *= a;
            n >>= 1;
        }

        return res;
    }


    // ============================================================
    // Extended GCD
    // ============================================================
    /// <summary>
    /// 拡張ユークリッドの互除法。
    /// gcd(a,b), x, y を返し、
    /// a*x + b*y = gcd(a,b)
    /// を満たす。
    /// </summary>
    /// <remarks>
    /// 計算量: O(log(min(a, b)))
    /// </remarks>
    public static (long Gcd, long X, long Y) ExtGcd(long a, long b)
    {
        long oldR = a;
        long r = b;

        long oldS = 1;
        long s = 0;

        long oldT = 0;
        long t = 1;

        while (r != 0)
        {
            long q = oldR / r;

            (oldR, r) = (r, oldR - q * r);
            (oldS, s) = (s, oldS - q * s);
            (oldT, t) = (t, oldT - q * t);
        }

        if (oldR < 0)
        {
            oldR = -oldR;
            oldS = -oldS;
            oldT = -oldT;
        }

        return (oldR, oldS, oldT);
    }


    // ============================================================
    // Modular Arithmetic
    // ============================================================

    /// <summary>
    /// a^e mod mod
    /// </summary>
    /// <remarks>
    /// 計算量: O(log e)
    /// </remarks>
    public static long PowMod(long a, long e, long mod)
    {
        if (mod <= 0)
            throw new ArgumentOutOfRangeException(nameof(mod));

        if (e < 0)
            throw new ArgumentOutOfRangeException(nameof(e));

        a %= mod;

        long res = 1 % mod;

        while (e > 0)
        {
            if ((e & 1) != 0)
                res = MulMod(res, a, mod);

            a = MulMod(a, a, mod);
            e >>= 1;
        }

        return res;
    }

    /// <summary>
    /// オーバーフローを避けた (a*b) mod mod
    /// </summary>
    /// <remarks>
    /// .NET 7+ の UInt128 を使用。
    /// 計算量: O(1)
    /// </remarks>
    public static long MulMod(long a, long b, long mod)
    {
        return (long)((UInt128)(ulong)a * (ulong)b % (ulong)mod);
    }

    /// <summary>
    /// a の mod における逆元。
    /// gcd(a, mod) = 1 の場合のみ存在する。
    /// </summary>
    /// <remarks>
    /// 計算量: O(log mod)
    /// </remarks>
    public static long ModInverse(long a, long mod)
    {
        var (g, x, _) = ExtGcd(a, mod);

        if (g != 1)
            throw new ArgumentException(
                "Modular inverse does not exist.");

        x %= mod;

        if (x < 0)
            x += mod;

        return x;
    }


    // ============================================================
    // Prime
    // ============================================================

    /// <summary>
    /// 素数判定。
    /// long の範囲で決定的に判定できる Miller-Rabin。
    /// </summary>
    /// <remarks>
    /// 計算量: O(log n) 程度。
    /// 固定された7個の底を使用するため long 全域で決定的。
    /// </remarks>
    public static bool IsPrime(long n)
    {
        if (n < 2)
            return false;

        if (n == 2 || n == 3)
            return true;

        if ((n & 1) == 0)
            return false;

        long d = n - 1;
        int s = 0;

        while ((d & 1) == 0)
        {
            d >>= 1;
            s++;
        }

        // long 全域で決定的な Miller-Rabin の底
        long[] bases =
        {
            2,
            325,
            9375,
            28178,
            450775,
            9780504,
            1795265022
        };

        foreach (long a in bases)
        {
            if (a % n == 0)
                continue;

            long x = PowMod(a, d, n);

            if (x == 1 || x == n - 1)
                continue;

            bool ok = false;

            for (int r = 1; r < s; r++)
            {
                x = MulMod(x, x, n);

                if (x == n - 1)
                {
                    ok = true;
                    break;
                }
            }

            if (!ok)
                return false;
        }

        return true;
    }


    /// <summary>
    /// エラトステネスの篩。
    /// prime[i] == true ⇔ i が素数。
    /// </summary>
    /// <remarks>
    /// 計算量: O(N log log N)
    /// 空間計算量: O(N)
    /// </remarks>
    public static bool[] Eratosthenes(int n)
    {
        var prime = new bool[n + 1];

        if (n < 2)
            return prime;

        Array.Fill(prime, true);

        prime[0] = false;
        prime[1] = false;

        for (int i = 2; (long)i * i <= n; i++)
        {
            if (!prime[i])
                continue;

            for (int j = i * i; j <= n; j += i)
                prime[j] = false;
        }

        return prime;
    }


    // ============================================================
    // Smallest Prime Factor
    // ============================================================

    /// <summary>
    /// 各整数の最小素因数を求める。
    /// spf[i] = i の最小の素因数。
    /// </summary>
    /// <remarks>
    /// 計算量: O(N log log N)
    /// 空間計算量: O(N)
    /// </remarks>
    public static int[] SmallestPrimeFactor(int n)
    {
        var spf = new int[n + 1];

        if (n < 1)
            return spf;

        for (int i = 0; i <= n; i++)
            spf[i] = i;

        if (n >= 1)
            spf[1] = 1;

        for (int i = 2; (long)i * i <= n; i++)
        {
            if (spf[i] != i)
                continue;

            for (int j = i * i; j <= n; j += i)
            {
                if (spf[j] == j)
                    spf[j] = i;
            }
        }

        return spf;
    }


    // ============================================================
    // Prime Factorization
    // ============================================================

    /// <summary>
    /// 素因数分解。
    /// (素数, 指数) のリストを返す。
    /// </summary>
    /// <example>
    /// 360 -> [(2,3), (3,2), (5,1)]
    /// </example>
    /// <remarks>
    /// 計算量: O(sqrt(N))
    /// </remarks>
    public static List<(long Prime, int Count)> PrimeFactorize(long n)
    {
        var result = new List<(long Prime, int Count)>();

        if (n == 0)
            throw new ArgumentException(
                "0 cannot be factorized.");

        n = Abs(n);

        if (n == 1)
            return result;

        int count = 0;

        while ((n & 1) == 0)
        {
            n >>= 1;
            count++;
        }

        if (count > 0)
            result.Add((2, count));

        for (long p = 3; p <= n / p; p += 2)
        {
            if (n % p != 0)
                continue;

            count = 0;

            while (n % p == 0)
            {
                n /= p;
                count++;
            }

            result.Add((p, count));
        }

        if (n > 1)
            result.Add((n, 1));

        return result;
    }


    // ============================================================
    // Divisors
    // ============================================================

    /// <summary>
    /// 正の約数を列挙する。
    /// 昇順で返す。
    /// </summary>
    /// <remarks>
    /// 計算量: O(sqrt(N) + D log D)
    /// D = 約数の個数
    /// </remarks>
    public static long[] Divisors(long n)
    {
        if (n <= 0)
            throw new ArgumentOutOfRangeException(nameof(n));

        var small = new List<long>();
        var large = new List<long>();

        for (long i = 1; i <= n / i; i++)
        {
            if (n % i != 0)
                continue;

            small.Add(i);

            if (i != n / i)
                large.Add(n / i);
        }

        large.Reverse();

        small.AddRange(large);

        return small.ToArray();
    }


    // ============================================================
    // Factorial / Permutation / Combination
    // ============================================================

    /// <summary>
    /// n!
    /// </summary>
    /// <remarks>
    /// BigInteger を使用するため long より大きな値も扱える。
    /// 計算量: O(N)
    /// </remarks>
    public static BigInteger Factorial(int n)
    {
        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n));

        BigInteger result = 1;

        for (int i = 2; i <= n; i++)
            result *= i;

        return result;
    }


    /// <summary>
    /// nPr
    /// </summary>
    /// <remarks>
    /// 計算量: O(r)
    /// </remarks>
    public static BigInteger NPr(int n, int r)
    {
        if (r < 0 || r > n)
            return 0;

        BigInteger result = 1;

        for (int i = 0; i < r; i++)
            result *= n - i;

        return result;
    }


    /// <summary>
    /// nCr
    /// </summary>
    /// <remarks>
    /// 計算量: O(min(r, n-r))
    /// </remarks>
    public static BigInteger NCr(int n, int r)
    {
        if (r < 0 || r > n)
            return 0;

        r = Min(r, n - r);

        BigInteger result = 1;

        for (int i = 1; i <= r; i++)
        {
            result *= n - r + i;
            result /= i;
        }

        return result;
    }


    /// <summary>
    /// nCr mod p。
    /// p は素数、n < p を想定。
    /// </summary>
    /// <remarks>
    /// 計算量: O(r log p)
    /// </remarks>
    public static long NCrMod(int n, int r, long p)
    {
        if (r < 0 || r > n)
            return 0;

        if (!IsPrime(p))
            throw new ArgumentException(
                "Modulus must be prime.");

        if (n >= p)
            throw new ArgumentException(
                "This method requires n < p.");

        r = Min(r, n - r);

        long numerator = 1;
        long denominator = 1;

        for (int i = 1; i <= r; i++)
        {
            numerator = MulMod(
                numerator,
                n - r + i,
                p);

            denominator = MulMod(
                denominator,
                i,
                p);
        }

        return MulMod(
            numerator,
            ModInverse(denominator, p),
            p);
    }


    // ============================================================
    // Euler's Totient
    // ============================================================

    /// <summary>
    /// Euler の φ 関数。
    /// 1 <= k <= n かつ gcd(k,n)=1 となる k の個数。
    /// </summary>
    /// <remarks>
    /// 計算量: O(sqrt(N))
    /// </remarks>
    public static long Totient(long n)
    {
        if (n <= 0)
            throw new ArgumentOutOfRangeException(nameof(n));

        long result = n;

        for (long p = 2; p <= n / p; p++)
        {
            if (n % p != 0)
                continue;

            while (n % p == 0)
                n /= p;

            result -= result / p;
        }

        if (n > 1)
            result -= result / n;

        return result;
    }


    // ============================================================
    // Möbius Function
    // ============================================================

    /// <summary>
    /// Möbius 関数 μ(n)。
    ///
    /// n = 1       ->  1
    /// 平方因子あり ->  0
    /// 素因数が奇数個 -> -1
    /// 素因数が偶数個 ->  1
    /// </summary>
    /// <remarks>
    /// 計算量: O(sqrt(N))
    /// </remarks>
    public static int Mobius(long n)
    {
        if (n <= 0)
            throw new ArgumentOutOfRangeException(nameof(n));

        if (n == 1)
            return 1;

        int count = 0;

        for (long p = 2; p <= n / p; p++)
        {
            if (n % p != 0)
                continue;

            n /= p;
            count++;

            // p^2 が n に含まれている
            if (n % p == 0)
                return 0;

            while (n % p == 0)
                n /= p;
        }

        if (n > 1)
            count++;

        return (count & 1) == 0 ? 1 : -1;
    }


    // ============================================================
    // Chinese Remainder Theorem
    // ============================================================

    /// <summary>
    /// 中国剰余定理。
    ///
    /// x ≡ rem[i] (mod mod[i])
    ///
    /// を満たす x を求める。
    ///
    /// mod は互いに素であることを想定。
    /// 戻り値は (x, lcm)。
    ///
    /// すべての解は
    /// x + k*lcm
    /// </summary>
    /// <remarks>
    /// 計算量: O(N log M)
    /// </remarks>
    public static (long Remainder, long Modulus)
        ChineseRemainder(
            long[] rem,
            long[] mod)
    {
        if (rem.Length != mod.Length)
            throw new ArgumentException(
                "rem and mod must have the same length.");

        long r = 0;
        long m = 1;

        for (int i = 0; i < rem.Length; i++)
        {
            long ri = rem[i];
            long mi = mod[i];

            if (mi <= 0)
                throw new ArgumentException(
                    "Modulus must be positive.");

            ri %= mi;

            if (ri < 0)
                ri += mi;

            // r + m*k ≡ ri (mod mi)
            //
            // m*k ≡ ri-r (mod mi)

            long g = Gcd(m, mi);

            if ((ri - r) % g != 0)
                throw new ArgumentException(
                    "No solution exists.");

            long mDivG = m / g;
            long miDivG = mi / g;

            long diff = ri - r;
            long diffDivG = diff / g;

            long inv = ModInverse(
                ((mDivG % miDivG) + miDivG) % miDivG,
                miDivG);

            long k;

            if (miDivG == 1)
            {
                k = 0;
            }
            else
            {
                k = MulMod(
                    ((diffDivG % miDivG) + miDivG) % miDivG,
                    inv,
                    miDivG);
            }

            // r + m*k
            BigInteger newR =
                (BigInteger)r + (BigInteger)m * k;

            BigInteger newM =
                (BigInteger)m * miDivG;

            newR %= newM;

            if (newR < 0)
                newR += newM;

            if (newR > long.MaxValue ||
                newM > long.MaxValue)
            {
                throw new OverflowException(
                    "CRT result does not fit in Int64.");
            }

            r = (long)newR;
            m = (long)newM;
        }

        return (r, m);
    }
}