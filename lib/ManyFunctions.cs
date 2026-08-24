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

public static class IndexList
{
    public static Dictionary<T, List<int>> Counter<T>(IEnumerable<T> items)
    {
        var d = new Dictionary<T, List<int>>();

        int i = 0;
        foreach (var item in items)
        {
            if (!d.TryGetValue(item, out var list))
            {
                list = new List<int>();
                d[item] = list;
            }

            list.Add(i);
            i++;
        }

        return d;
    }

    public static Dictionary<T, List<long>> CounterLong<T>(IEnumerable<T> items)
    {
        var d = new Dictionary<T, List<long>>();

        long i = 0;
        foreach (var item in items)
        {
            if (!d.TryGetValue(item, out var list))
            {
                list = new List<long>();
                d[item] = list;
            }

            list.Add(i);
            i++;
        }

        return d;
    }

    public static Dictionary<T, int> CounterInt<T>(IEnumerable<T> items)
    {
        var d = new Dictionary<T, int>();

        foreach (var item in items)
        {
            if (!d.ContainsKey(item))
                d[item] = 0;

            d[item]++;
        }

        return d;
    }

    public static Dictionary<T, long> CounterLongInt<T>(IEnumerable<T> items)
    {
        var d = new Dictionary<T, long>();

        foreach (var item in items)
        {
            if (!d.ContainsKey(item))
                d[item] = 0;

            d[item]++;
        }

        return d;
    }

    public static Dictionary<T, SortedSet<int>> CounterSet<T>(IEnumerable<T> items)
    {
        var d = new Dictionary<T, SortedSet<int>>();

        int i = 0;
        foreach (var item in items)
        {
            if (!d.TryGetValue(item, out var list))
            {
                list = new SortedSet<int>();
                d[item] = list;
            }

            list.Add(i);
            i++;
        }

        return d;
    }

    public static Dictionary<T, SortedSet<long>> CounterSetLong<T>(IEnumerable<T> items)
    {
        var d = new Dictionary<T, SortedSet<long>>();

        long i = 0;
        foreach (var item in items)
        {
            if (!d.TryGetValue(item, out var list))
            {
                list = new SortedSet<long>();
                d[item] = list;
            }

            list.Add(i);
            i++;
        }

        return d;
    }
}
    public static class PrefixSum
    {
        // int[] / List<int> -> int[]
        public static int[] PSIntArray(IReadOnlyList<int> a)
        {
            var sum = new int[a.Count + 1];
            for (int i = 0; i < a.Count; i++)
                sum[i + 1] = sum[i] + a[i];
            return sum;
        }

        // int[] / List<int> -> long[]
        public static long[] PSLongArray(IReadOnlyList<int> a)
        {
            var sum = new long[a.Count + 1];
            for (int i = 0; i < a.Count; i++)
                sum[i + 1] = sum[i] + a[i];
            return sum;
        }

        // long[] / List<long> -> long[]
        public static long[] PSLongArray(IReadOnlyList<long> a)
        {
            var sum = new long[a.Count + 1];
            for (int i = 0; i < a.Count; i++)
                sum[i + 1] = sum[i] + a[i];
            return sum;
        }

        // int[] / List<int> -> List<int>
        public static List<int> PSIntList(IReadOnlyList<int> a)
        {
            var sum = new List<int>(a.Count + 1) { 0 };
            int s = 0;
            for (int i = 0; i < a.Count; i++)
            {
                s += a[i];
                sum.Add(s);
            }
            return sum;
        }

        // int[] / List<int> -> List<long>
        public static List<long> PSLongList(IReadOnlyList<int> a)
        {
            var sum = new List<long>(a.Count + 1) { 0 };
            long s = 0;
            for (int i = 0; i < a.Count; i++)
            {
                s += a[i];
                sum.Add(s);
            }
            return sum;
        }

        // long[] / List<long> -> List<long>
        public static List<long> PSLongList(IReadOnlyList<long> a)
        {
            var sum = new List<long>(a.Count + 1) { 0 };
            long s = 0;
            for (int i = 0; i < a.Count; i++)
            {
                s += a[i];
                sum.Add(s);
            }
            return sum;
        }
    }
