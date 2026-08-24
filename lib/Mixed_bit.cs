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

public static class Mixedbit
{
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
foreach (var p in EnumerateEx.Product(new[] { 5, 2, 3 }))
{
    Console.WriteLine(string.Join(" ", p));
}
*/