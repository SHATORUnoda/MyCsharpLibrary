using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class DeepCopy
{
    // =========================
    // 便利なショートカット
    // =========================

    // 1次元/2次元/3次元/4次元の配列をまとめてコピー
    public static T[] Copy<T>(T[] a) => Array1(a);
    public static T[][] Copy<T>(T[][] a) => Array2(a);
    public static T[][][] Copy<T>(T[][][] a) => Array3(a);
    public static T[][][][] Copy<T>(T[][][][] a) => Array4(a);

    // 多次元配列（bool[,] など）向け
    public static T[,] Copy<T>(T[,] a)
    {
        var res = new T[a.GetLength(0), a.GetLength(1)];
        Array.Copy(a, res, a.Length);
        return res;
    }

    public static T[,,] Copy<T>(T[,,] a)
    {
        var res = new T[a.GetLength(0), a.GetLength(1), a.GetLength(2)];
        Array.Copy(a, res, a.Length);
        return res;
    }

    public static T[,,,] Copy<T>(T[,,,] a)
    {
        var res = new T[a.GetLength(0), a.GetLength(1), a.GetLength(2), a.GetLength(3)];
        Array.Copy(a, res, a.Length);
        return res;
    }

    // 1次元/2次元/3次元/4次元のリストをまとめてコピー
    public static List<T> Copy<T>(List<T> a) => List1(a);
    public static List<List<T>> Copy<T>(List<List<T>> a) => List2(a);
    public static List<List<List<T>>> Copy<T>(List<List<List<T>>> a) => List3(a);
    public static List<List<List<List<T>>>> Copy<T>(List<List<List<List<T>>>> a) => List4(a);

    // 標準コレクション
    public static Dictionary<TKey, TValue> Copy<TKey, TValue>(
        Dictionary<TKey, TValue> a)
        where TKey : notnull
    {
        return new Dictionary<TKey, TValue>(a);
    }

    public static HashSet<T> Copy<T>(HashSet<T> a)
    {
        return new HashSet<T>(a);
    }

    public static SortedSet<T> Copy<T>(SortedSet<T> a)
    {
        return new SortedSet<T>(a, a.Comparer);
    }

    public static SortedDictionary<TKey, TValue> Copy<TKey, TValue>(
        SortedDictionary<TKey, TValue> a)
        where TKey : notnull
    {
        return new SortedDictionary<TKey, TValue>(a, a.Comparer);
    }

    public static Queue<T> Copy<T>(Queue<T> a)
    {
        return new Queue<T>(a);
    }

    public static Stack<T> Copy<T>(Stack<T> a)
    {
        return new Stack<T>(a);
    }

    public static LinkedList<T> Copy<T>(LinkedList<T> a)
    {
        return new LinkedList<T>(a);
    }

    // lib のデータ構造
    public static Treap<T> Copy<T>(Treap<T> a)
        where T : IComparable<T>
    {
        return new Treap<T>(a);
    }

    public static ImplicitTreap<T> Copy<T>(ImplicitTreap<T> a)
    {
        return new ImplicitTreap<T>(a);
    }


    // =========================
    // Array (ジャグ配列)
    // =========================

    // 1次元
    public static T[] Array1<T>(T[] a)
    {
        return (T[])a.Clone();
    }

    // 2次元
    public static T[][] Array2<T>(T[][] a)
    {
        var res = new T[a.Length][];

        for (int i = 0; i < a.Length; i++)
        {
            res[i] = (T[])a[i].Clone();
        }

        return res;
    }

    // 3次元
    public static T[][][] Array3<T>(T[][][] a)
    {
        var res = new T[a.Length][][];

        for (int i = 0; i < a.Length; i++)
        {
            res[i] = Array2(a[i]);
        }

        return res;
    }

    // 4次元
    public static T[][][][] Array4<T>(T[][][][] a)
    {
        var res = new T[a.Length][][][];

        for (int i = 0; i < a.Length; i++)
        {
            res[i] = Array3(a[i]);
        }

        return res;
    }


    // =========================
    // List
    // =========================

    // 1次元
    public static List<T> List1<T>(List<T> a)
    {
        return new List<T>(a);
    }

    // 2次元
    public static List<List<T>> List2<T>(List<List<T>> a)
    {
        var res = new List<List<T>>(a.Count);

        foreach (var x in a)
        {
            res.Add(new List<T>(x));
        }

        return res;
    }

    // 3次元
    public static List<List<List<T>>> List3<T>(List<List<List<T>>> a)
    {
        var res = new List<List<List<T>>>(a.Count);

        foreach (var x in a)
        {
            res.Add(List2(x));
        }

        return res;
    }

    // 4次元
    public static List<List<List<List<T>>>> List4<T>(
        List<List<List<List<T>>>> a)
    {
        var res = new List<List<List<List<T>>>>(a.Count);

        foreach (var x in a)
        {
            res.Add(List3(x));
        }

        return res;
    }
}