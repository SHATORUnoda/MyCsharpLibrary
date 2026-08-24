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

public static class Itertools
{
    public static IEnumerable<int[]> Nbit(int n, int m) //nが桁数,mが進数
    {
        long lim = 1;
        for (int i = 0; i < n; i++) lim *= m;

        for (long mask = 0; mask < lim; mask++)
        {
            long x = mask;
            int[] p = new int[n];

            for (int i = n - 1; i >= 0; i--)
            {
                p[i] = (int)(x % m);
                x /= m;
            }

            yield return p;
        }
    }
    public static IEnumerable<T[]> Permutations<T>(IEnumerable<T> iterable, int? r = null)
    {
        T[] items = iterable.ToArray();
        int n = items.Length;
        int m = r ?? n;

        if (m < 0 || m > n)
            yield break;

        bool[] used = new bool[n];
        T[] perm = new T[m];

        foreach (var p in Dfs(0))
            yield return p;

        IEnumerable<T[]> Dfs(int depth)
        {
            if (depth == m)
            {
                yield return (T[])perm.Clone();
                yield break;
            }

            for (int i = 0; i < n; i++)
            {
                if (used[i]) continue;

                used[i] = true;
                perm[depth] = items[i];

                foreach (var p in Dfs(depth + 1))
                    yield return p;

                used[i] = false;
            }
        }
    }

    public static IEnumerable<T[]> Combinations<T>(IEnumerable<T> iterable, int r)
    {
        T[] items = iterable.ToArray();
        int n = items.Length;

        if (r < 0 || r > n)
            yield break;

        T[] comb = new T[r];

        foreach (var c in Dfs(0, 0))
            yield return c;

        IEnumerable<T[]> Dfs(int depth, int start)
        {
            if (depth == r)
            {
                yield return (T[])comb.Clone();
                yield break;
            }

            for (int i = start; i <= n - (r - depth); i++)
            {
                comb[depth] = items[i];

                foreach (var c in Dfs(depth + 1, i + 1))
                    yield return c;
            }
        }
    }
    public static IEnumerable<T[]> CombinationsWithReplacement<T>(IEnumerable<T> iterable, int r)
    {
        T[] items = iterable.ToArray();
        int n = items.Length;

        if (r < 0)
            yield break;

        if (r == 0)
        {
            yield return Array.Empty<T>();
            yield break;
        }

        if (n == 0)
            yield break;

        T[] comb = new T[r];

        foreach (var c in Dfs(0, 0))
            yield return c;

        IEnumerable<T[]> Dfs(int depth, int start)
        {
            if (depth == r)
            {
                yield return (T[])comb.Clone();
                yield break;
            }

            for (int i = start; i < n; i++)
            {
                comb[depth] = items[i];

                // i + 1 ではなく i にすることで同じ要素を何回でも選べる
                foreach (var c in Dfs(depth + 1, i))
                    yield return c;
            }
        }
    }
    public static IEnumerable<int> Range(int start, int count) => Enumerable.Range(start, count);
    public static IEnumerable<T[]> Product<T>(IEnumerable<T> iterable, int repeat = 1)
    {
        T[] items = iterable.ToArray();

        if (repeat < 0)
            yield break;

        if (repeat == 0)
        {
            yield return Array.Empty<T>();
            yield break;
        }

        if (items.Length == 0)
            yield break;

        T[] product = new T[repeat];

        foreach (var p in Dfs(0))
            yield return p;

        IEnumerable<T[]> Dfs(int depth)
        {
            if (depth == repeat)
            {
                yield return (T[])product.Clone();
                yield break;
            }

            foreach (var item in items)
            {
                product[depth] = item;

                foreach (var p in Dfs(depth + 1))
                    yield return p;
            }
        }
    
}
    public static IEnumerable<int[]> Mix(int[] limits)
    {
        int n = limits.Length;
        int[] p = new int[n];

        while (true)
        {
            yield return (int[])p.Clone();

            int pos = n - 1;

            while (pos >= 0)
            {
                p[pos]++;

                if (p[pos] < limits[pos])
                    break;

                p[pos] = 0;
                pos--;
            }

            if (pos < 0) yield break;
        }
    }
}
/*
foreach (var p in EnumerateEx.Product(3, 2))
{
    Console.WriteLine(string.Join("", p));
}
*/