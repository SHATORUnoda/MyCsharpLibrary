using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using static System.Console;
using static System.Math;
using static System.Array;
using System.Numerics;
using System.Runtime.CompilerServices;

public static class LibMath
{
    public static int Gcd(int a, int b)
    {
        while (b != 0)
        {
            int t = a % b;
            a = b;
            b = t;
        }
        return a;
    }
    public static bool IsPrime(long n)
    {
        if (n < 2) return false;
        if (n == 2 || n == 3) return true;
        if ((n & 1) == 0) return false;

        long d = n - 1;
        int s = 0;
        while ((d & 1) == 0)
        {
            d >>= 1;
            s++;
        }

        long[] bases = { 2, 325, 9375, 28178, 450775, 9780504, 1795265022 };

        foreach (long a in bases)
        {
            if (a % n == 0) continue;

            long x = PowMod(a, d, n);

            if (x == 1 || x == n - 1) continue;

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

            if (!ok) return false;
        }

        return true;

        static long PowMod(long a, long e, long mod)
        {
            long res = 1;
            while (e > 0)
            {
                if ((e & 1) != 0)
                    res = MulMod(res, a, mod);

                a = MulMod(a, a, mod);
                e >>= 1;
            }
            return res;
        }

        static long MulMod(long a, long b, long mod)
        {
            return (long)((UInt128)(ulong)a * (ulong)b % (ulong)mod);
        }
    }
    public static bool[] Eratosthenes(int n)
    {
        var prime = new bool[n + 1];

        if (n < 2) return prime;

        Array.Fill(prime, true);
        prime[0] = prime[1] = false;

        for (int i = 2; (long)i * i <= n; i++)
        {
            if (!prime[i]) continue;

            for (int j = i * i; j <= n; j += i)
                prime[j] = false;
        }
        return prime;
    }
}