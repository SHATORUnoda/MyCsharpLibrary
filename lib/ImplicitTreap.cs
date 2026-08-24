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

public class ImplicitTreap<T> : IEnumerable<T>
{
    private class Node
    {
        public T Value;
        public int Priority;
        public int Size;
        public bool Rev;
        public Node Left;
        public Node Right;

        public Node(T value, int priority)
        {
            Value = value;
            Priority = priority;
            Size = 1;
        }
    }

    private Node root;

    private static readonly Random rnd = new();

    private static int Size(Node n)
        => n?.Size ?? 0;

    private static void Update(Node n)
    {
        if (n != null)
            n.Size = 1 + Size(n.Left) + Size(n.Right);
    }

    private static void Toggle(Node n)
    {
        if (n != null)
            n.Rev ^= true;
    }

    private static void Push(Node n)
    {
        if (n == null || !n.Rev)
            return;

        (n.Left, n.Right) = (n.Right, n.Left);
        Toggle(n.Left);
        Toggle(n.Right);
        n.Rev = false;
    }

    private static Node Merge(Node a, Node b)
    {
        if (a == null)
            return b;

        if (b == null)
            return a;

        if (a.Priority > b.Priority)
        {
            Push(a);
            a.Right = Merge(a.Right, b);
            Update(a);
            return a;
        }
        else
        {
            Push(b);
            b.Left = Merge(a, b.Left);
            Update(b);
            return b;
        }
    }

    private static void Split(Node n, int k, out Node a, out Node b)
    {
        if (n == null)
        {
            a = null;
            b = null;
            return;
        }

        Push(n);

        if (Size(n.Left) >= k)
        {
            Split(n.Left, k, out a, out n.Left);
            b = n;
            Update(b);
        }
        else
        {
            Split(n.Right, k - Size(n.Left) - 1, out n.Right, out b);
            a = n;
            Update(a);
        }
    }

    private static T Kth(Node n, int k)
    {
        Push(n);

        int leftSize = Size(n.Left);

        if (k < leftSize)
            return Kth(n.Left, k);

        if (k == leftSize)
            return n.Value;

        return Kth(n.Right, k - leftSize - 1);
    }

    private static void SetValue(Node n, int k, T value)
    {
        Push(n);

        int leftSize = Size(n.Left);

        if (k < leftSize)
        {
            SetValue(n.Left, k, value);
        }
        else if (k == leftSize)
        {
            n.Value = value;
        }
        else
        {
            SetValue(n.Right, k - leftSize - 1, value);
        }
    }

    private static void Enumerate(Node n, List<T> list)
    {
        if (n == null)
            return;

        Push(n);
        Enumerate(n.Left, list);
        list.Add(n.Value);
        Enumerate(n.Right, list);
    }

    public ImplicitTreap()
    {
    }

    public ImplicitTreap(IEnumerable<T> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        foreach (T item in items)
            AddLast(item);
    }

    public int Count => Size(root);

    public bool IsEmpty => root == null;

    public T this[int index]
    {
        get
        {
            if (index < 0)
                index += Count;

            if ((uint)index >= (uint)Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            return Kth(root, index);
        }
        set
        {
            if (index < 0)
                index += Count;

            if ((uint)index >= (uint)Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            SetValue(root, index, value);
        }
    }

    public T this[Index index]
    {
        get
        {
            int i = index.IsFromEnd
                ? Count - index.Value
                : index.Value;

            return this[i];
        }
        set
        {
            int i = index.IsFromEnd
                ? Count - index.Value
                : index.Value;

            this[i] = value;
        }
    }

    public void Insert(int index, T value)
    {
        if (index < 0)
            index += Count + 1;

        if ((uint)index > (uint)Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        Split(root, index, out Node a, out Node b);
        root = Merge(Merge(a, new Node(value, rnd.Next())), b);
    }

    public void AddFirst(T value)
    {
        Insert(0, value);
    }

    public void AddLast(T value)
    {
        Insert(Count, value);
    }

    public T RemoveAt(int index)
    {
        if (index < 0)
            index += Count;

        if ((uint)index >= (uint)Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        Split(root, index, out Node a, out Node b);
        Split(b, 1, out Node c, out b);

        Push(c);
        T ret = c.Value;
        root = Merge(a, b);

        return ret;
    }

    public T PopFront()
    {
        return RemoveAt(0);
    }

    public T PopBack()
    {
        return RemoveAt(Count - 1);
    }

    public void Reverse()
    {
        Toggle(root);
    }

    public void Reverse(int left, int length)
    {
        if (left < 0)
            left += Count;

        if (length < 0 || left < 0 || left + length > Count)
            throw new ArgumentOutOfRangeException();

        Split(root, left, out Node a, out Node b);
        Split(b, length, out Node c, out b);

        Toggle(c);
        root = Merge(Merge(a, c), b);
    }

    public ImplicitTreap<T> Slice(int left, int length)
    {
        if (left < 0)
            left += Count;

        if (length < 0 || left < 0 || left + length > Count)
            throw new ArgumentOutOfRangeException();

        Split(root, left, out Node a, out Node b);
        Split(b, length, out Node c, out b);

        var list = new List<T>(length);
        Enumerate(c, list);

        root = Merge(Merge(a, c), b);

        return new ImplicitTreap<T>(list);
    }

    public ImplicitTreap<T> Cut(int left, int length)
    {
        if (left < 0)
            left += Count;

        if (length < 0 || left < 0 || left + length > Count)
            throw new ArgumentOutOfRangeException();

        Split(root, left, out Node a, out Node b);
        Split(b, length, out Node c, out b);

        var ret = new ImplicitTreap<T>
        {
            root = c
        };

        root = Merge(a, b);

        return ret;
    }

    public void AddRange(IEnumerable<T> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        foreach (T item in items)
            AddLast(item);
    }

    public void InsertRange(int index, IEnumerable<T> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        if (index < 0)
            index += Count + 1;

        if ((uint)index > (uint)Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        Node mid = null;

        foreach (T item in items)
            mid = Merge(mid, new Node(item, rnd.Next()));

        Split(root, index, out Node a, out Node b);
        root = Merge(Merge(a, mid), b);
    }

    public void Clear()
    {
        root = null;
    }

    public T[] ToArray()
    {
        var list = new List<T>(Count);

        Enumerate(root, list);

        return list.ToArray();
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (T item in ToArray())
            yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
