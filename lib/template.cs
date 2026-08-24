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

public class DebugOutput : TextWriter
{
    public readonly TextWriter console;
    public readonly TextWriter file;

    public DebugOutput(TextWriter console, TextWriter file)
    {
        this.console = console;
        this.file = file;
    }

    public override Encoding Encoding => console.Encoding;

    public override void Write(char value)
    {
        console.Write(value);
        file.Write(value);
    }

    public override void Write(string value)
    {
        console.Write(value);
        file.Write(value);
    }

    public override void Flush()
    {
        console.Flush();
        file.Flush();
    }

    public static void Init()
    {
        var file = new StreamWriter("../out.txt", false)
        {
            AutoFlush = true
        };

        Console.SetOut(
            new DebugOutput(Console.Out, file)
        );
    }
}

public static class Reader
{
    public static int ReadInt()
        => int.Parse(ReadLine());

    public static long ReadLong()
        => long.Parse(ReadLine());

    public static int[] ReadIntArray()
    {
        string str = ReadLine();
        return str != ""
            ? str.Split().Select(int.Parse).ToArray()
            : new int[0];
    }

    public static long[] ReadLongArray()
    {
        string str = ReadLine();
        return str != ""
            ? str.Split().Select(long.Parse).ToArray()
            : new long[0];
    }

    public static List<int> ReadIntList()
    {
        string str = ReadLine();
        return str != ""
            ? str.Split().Select(int.Parse).ToList()
            : new List<int>();
    }

    public static List<long> ReadLongList()
    {
        string str = ReadLine();
        return str != ""
            ? str.Split().Select(long.Parse).ToList()
            : new List<long>();
    }

    public static char[] ReadCharArray()
        => ReadLine().ToCharArray();

    public static string[] ReadStringArray()
    {
        string str = ReadLine();
        return str != ""
            ? str.Split()
            : Empty<string>();
    }

    public static (int, int) ReadInt2()
    {
        int[] v = ReadIntArray();
        return (v[0], v[1]);
    }

    public static (int, int, int) ReadInt3()
    {
        int[] v = ReadIntArray();
        return (v[0], v[1], v[2]);
    }

    public static (int, int, int, int) ReadInt4()
    {
        int[] v = ReadIntArray();
        return (v[0], v[1], v[2], v[3]);
    }

    public static (int, int, int, int, int) ReadInt5()
    {
        int[] v = ReadIntArray();
        return (v[0], v[1], v[2], v[3], v[4]);
    }
    public static (int, int, int, int, int,int) ReadInt6()
    {
        int[] v = ReadIntArray();
        return (v[0], v[1], v[2], v[3], v[4],v[5]);
    }

    public static (long, long) ReadLong2()
    {
        long[] v = ReadLongArray();
        return (v[0], v[1]);
    }

    public static (long, long, long) ReadLong3()
    {
        long[] v = ReadLongArray();
        return (v[0], v[1], v[2]);
    }

    public static (long, long, long, long) ReadLong4()
    {
        long[] v = ReadLongArray();
        return (v[0], v[1], v[2], v[3]);
    }

    public static (long, long, long, long, long) ReadLong5()
    {
        long[] v = ReadLongArray();
        return (v[0], v[1], v[2], v[3], v[4]);
    }
    public static (long, long, long, long, long,long) ReadLong6()
    {
        long[] v = ReadLongArray();
        return (v[0], v[1], v[2], v[3], v[4],v[5]);
    }
    public static void WriteArray<T>(IEnumerable<T> array, string separator = " ")
    {
        WriteLine(string.Join(separator, array));
    }
    public static long Pow(long a, long n)
    {
        long res = 1;
        while (n > 0)
        {
            if ((n & 1) != 0) res *= a;
            a *= a;
            n >>= 1;
        }
        return res;
    }
}