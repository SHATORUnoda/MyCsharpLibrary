using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using static System.Console;
using static System.Math;
using static System.Array;
using static Radix;
using System.Numerics;

public static class Radix
{
    const string Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    // 10進数 → n進数
    public static string ToBase(long value, int radix)
    {
        if (radix < 2 || radix > 36)
            throw new ArgumentException();

        if (value == 0) return "0";

        var sb = new StringBuilder();

        while (value > 0)
        {
            sb.Append(Chars[(int)(value % radix)]);
            value /= radix;
        }

        var a = sb.ToString().ToCharArray();
        Array.Reverse(a);
        return new string(a);
    }

    // n進数 → 10進数
    public static long ToDecimal(string s, int radix)
    {
        if (radix < 2 || radix > 36)
            throw new ArgumentException();

        long result = 0;

        foreach (char c in s.ToUpper())
        {
            int digit =
                c <= '9'
                ? c - '0'
                : c - 'A' + 10;

            result = result * radix + digit;
        }

        return result;
    }
}