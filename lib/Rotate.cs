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

public static class Rotate
{
    // ===== List<T[]> =====

    public static List<T[]> RotateR90<T>(List<T[]> a)
    {
        int h = a.Count;
        int w = a[0].Length;

        var res = new List<T[]>();
        for (int i = 0; i < w; i++) res.Add(new T[h]);

        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                res[j][h - 1 - i] = a[i][j];

        return res;
    }

    public static List<T[]> RotateR180<T>(List<T[]> a)
    {
        int h = a.Count;
        int w = a[0].Length;

        var res = new List<T[]>();
        for (int i = 0; i < h; i++) res.Add(new T[w]);

        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                res[h - 1 - i][w - 1 - j] = a[i][j];

        return res;
    }

    public static List<T[]> RotateR270<T>(List<T[]> a)
    {
        int h = a.Count;
        int w = a[0].Length;

        var res = new List<T[]>();
        for (int i = 0; i < w; i++) res.Add(new T[h]);

        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                res[w - 1 - j][i] = a[i][j];

        return res;
    }

    // ===== List<List<T>> =====

    public static List<List<T>> RotateR90<T>(List<List<T>> a)
    {
        int h = a.Count;
        int w = a[0].Count;

        var res = new List<List<T>>();
        for (int i = 0; i < w; i++)
            res.Add(new List<T>(new T[h]));

        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                res[j][h - 1 - i] = a[i][j];

        return res;
    }

    public static List<List<T>> RotateR180<T>(List<List<T>> a)
    {
        int h = a.Count;
        int w = a[0].Count;

        var res = new List<List<T>>();
        for (int i = 0; i < h; i++)
            res.Add(new List<T>(new T[w]));

        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                res[h - 1 - i][w - 1 - j] = a[i][j];

        return res;
    }

    public static List<List<T>> RotateR270<T>(List<List<T>> a)
    {
        int h = a.Count;
        int w = a[0].Count;

        var res = new List<List<T>>();
        for (int i = 0; i < w; i++)
            res.Add(new List<T>(new T[h]));

        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                res[w - 1 - j][i] = a[i][j];

        return res;
    }
// ===== List<T[]> =====

public static List<T[]> RotateL90<T>(List<T[]> a)
=> RotateR270(a);

public static List<T[]> RotateL180<T>(List<T[]> a)
=> RotateR180(a);

public static List<T[]> RotateL270<T>(List<T[]> a)
=> RotateR90(a);

// ===== List<List<T>> =====

public static List<List<T>> RotateL90<T>(List<List<T>> a)
=> RotateR270(a);

public static List<List<T>> RotateL180<T>(List<List<T>> a)
=> RotateR180(a);

public static List<List<T>> RotateL270<T>(List<List<T>> a)
=> RotateR90(a);
    //左右反転
    public static List<T[]> lrswap<T>(List<T[]> a)
    {
        /*
        abc      cba
        def  →   fed
        ghi      ihg
        */
        int h = a.Count;
        int w = a[0].Length;

        var res = new List<T[]>();
        for (int i = 0; i < h; i++) res.Add(new T[w]);

        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                res[i][w - 1 - j] = a[i][j];

        return res;
    }
    public static List<T[]> udswap<T>(List<T[]> a)
    {
        /*
        abc      ghi
        def  →   def
        ghi      abc
        */
        int h = a.Count;
        int w = a[0].Length;

        var res = new List<T[]>();
        for (int i = 0; i < h; i++) res.Add(new T[w]);

        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                res[h - 1 - i][j] = a[i][j];

        return res;
    }
    public static List<T[]> naname<T>(List<T[]> a)
    {
        /*
        abc      adg
        def  →  beh
        ghi      cfi
        */
        int h = a.Count;
        int w = a[0].Length;

        var res = new List<T[]>();
        for (int i = 0; i < w; i++) res.Add(new T[h]);

        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                res[j][i] = a[i][j];

        return res;
    }
    public static bool GridEqual<T>(List<T[]> a, List<T[]> b)
    {
        if (a.Count != b.Count) return false;
        if (a[0].Length != b[0].Length) return false;

        for (int i = 0; i < a.Count; i++)
            if (!a[i].SequenceEqual(b[i]))
                return false;

        return true;
    }
}