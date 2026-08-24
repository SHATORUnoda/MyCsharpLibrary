using System;
using System.Collections.Generic;
using System.Linq;

public static class DeepCopy
{
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