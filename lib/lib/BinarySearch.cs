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

public static class Binary
{
    // target より小さい最大の要素のインデックス
    public static int MinBinary<T>(T[] data, T target)
        where T : IComparable<T>
    {
        int low = 0;
        int high = data.Length - 1;
        int result = -1;

        while (low <= high)
        {
            int mid = low + (high - low) / 2;

            if (data[mid].CompareTo(target) < 0)
            {
                result = mid;
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        return result;
    }

    // value 以下の個数
    public static int UpperBound<T>(T[] data, T value)
        where T : IComparable<T>
    {
        int l = 0;
        int r = data.Length;

        while (l < r)
        {
            int mid = l + (r - l) / 2;

            if (data[mid].CompareTo(value) <= 0)
                l = mid + 1;
            else
                r = mid;
        }

        return l;
    }

    // value と一致する要素のインデックス
    public static int BinarySearch<T>(T[] data, T value)
        where T : IComparable<T>
    {
        int left = 0;
        int right = data.Length - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;

            int cmp = data[mid].CompareTo(value);

            if (cmp == 0)
                return mid;
            else if (cmp < 0)
                left = mid + 1;
            else
                right = mid - 1;
        }

        return -1;
    }
}