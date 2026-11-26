using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Array;
using static System.Console;
using static System.Math;

public static class Itertools
{
    // n桁・m進数の全パターン
    // 例: Nbit(2, 3)
    // => [0,0], [0,1], [0,2], [1,0], ..., [2,2]
    public static IEnumerable<int[]> Nbit(int n, int m)
    {
        long lim = 1;

        for (int i = 0; i < n; i++)
            lim *= m;

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

    // 順列
    public static IEnumerable<T[]> Permutations<T>(
        IEnumerable<T> iterable,
        int? r = null)
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
                if (used[i])
                    continue;

                used[i] = true;
                perm[depth] = items[i];

                foreach (var p in Dfs(depth + 1))
                    yield return p;

                used[i] = false;
            }
        }
    }

    // 組合せ
    public static IEnumerable<T[]> Combinations<T>(
        IEnumerable<T> iterable,
        int r)
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

    // 重複組合せ
    public static IEnumerable<T[]> CombinationsWithReplacement<T>(
        IEnumerable<T> iterable,
        int r)
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

                // i + 1 ではなく i にすることで
                // 同じ要素を何回でも選べる
                foreach (var c in Dfs(depth + 1, i))
                    yield return c;
            }
        }
    }

    // Python の range(start, count) 相当
    public static IEnumerable<int> Range(int start, int count)
        => Enumerable.Range(start, count);

    // 直積
    // 例: Product([0,1,2], 2)
    // => [0,0], [0,1], [0,2], [1,0], ...
    public static IEnumerable<T[]> Product<T>(
        IEnumerable<T> iterable,
        int repeat = 1)
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

    // 各桁の上限が異なる全パターン
    //
    // limits[i] が「その桁で取り得る値の個数」
    //
    // 例:
    // Mix(new[] { 2, 3 })
    // => [0,0], [0,1], [0,2],
    //    [1,0], [1,1], [1,2]
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

            if (pos < 0)
                yield break;
        }
    }
}