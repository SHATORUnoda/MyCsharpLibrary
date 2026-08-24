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

public class Treap<T> : IEnumerable<T>
where T : IComparable<T>
{
    private class Node
    {
        public T Value;
        public int Priority;
        public int Count;
        public int Size;
        public Node Left;
        public Node Right;

        public Node(T value, int priority)
        {
            Value = value;
            Priority = priority;
            Count = 1;
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
            n.Size = n.Count + Size(n.Left) + Size(n.Right);
    }

    private static void RotateRight(ref Node n)
    {
        Node l = n.Left;

        n.Left = l.Right;
        l.Right = n;

        Update(n);
        Update(l);

        n = l;
    }

    private static void RotateLeft(ref Node n)
    {
        Node r = n.Right;

        n.Right = r.Left;
        r.Left = n;

        Update(n);
        Update(r);

        n = r;
    }

    private static void Merge(ref Node n, Node a, Node b)
    {
        if (a == null || b == null)
        {
            n = a ?? b;
            return;
        }

        if (a.Priority > b.Priority)
        {
            Merge(ref a.Right, a.Right, b);
            n = a;
        }
        else
        {
            Merge(ref b.Left, a, b.Left);
            n = b;
        }

        Update(n);
    }

    private void Insert(ref Node n, T value)
    {
        if (n == null)
        {
            n = new Node(value, rnd.Next());
            return;
        }

        int cmp = value.CompareTo(n.Value);

        if (cmp == 0)
        {
            n.Count++;
        }
        else if (cmp < 0)
        {
            Insert(ref n.Left, value);

            if (n.Left.Priority > n.Priority)
                RotateRight(ref n);
        }
        else
        {
            Insert(ref n.Right, value);

            if (n.Right.Priority > n.Priority)
                RotateLeft(ref n);
        }

        Update(n);
    }

    private void Erase(ref Node n, T value)
    {
        if (n == null)
            return;

        int cmp = value.CompareTo(n.Value);

        if (cmp == 0)
        {
            if (n.Count > 1)
            {
                n.Count--;
            }
            else
            {
                Merge(ref n, n.Left, n.Right);
                return;
            }
        }
        else if (cmp < 0)
        {
            Erase(ref n.Left, value);
        }
        else
        {
            Erase(ref n.Right, value);
        }

        Update(n);
    }

    private static T EraseAt(ref Node n, int index)
    {
        int leftSize = Size(n.Left);

        if (index < leftSize)
        {
            T value = EraseAt(ref n.Left, index);
            Update(n);
            return value;
        }

        if (index >= leftSize + n.Count)
        {
            T value = EraseAt(ref n.Right, index - leftSize - n.Count);
            Update(n);
            return value;
        }

        T result = n.Value;

        if (n.Count > 1)
        {
            n.Count--;
            Update(n);
        }
        else
        {
            Merge(ref n, n.Left, n.Right);
        }

        return result;
    }

    private static T Kth(Node n, int k)
    {
        int leftSize = Size(n.Left);

        if (k < leftSize)
            return Kth(n.Left, k);

        if (k < leftSize + n.Count)
            return n.Value;

        return Kth(n.Right, k - leftSize - n.Count);
    }

    public int Count => Size(root);

    public bool IsEmpty => root == null;

    public void Add(T value)
    {
        Insert(ref root, value);
    }

    public bool Remove(T value)
    {
        int before = Count;

        Erase(ref root, value);

        return Count != before;
    }

    public T RemoveAt(int index)
    {
        if (index < 0)
            index += Count;

        if ((uint)index >= (uint)Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return EraseAt(ref root, index);
    }

    public bool Contains(T value)
    {
        Node n = root;

        while (n != null)
        {
            int cmp = value.CompareTo(n.Value);

            if (cmp == 0)
                return true;

            n = cmp < 0 ? n.Left : n.Right;
        }

        return false;
    }

    public T Min
    {
        get
        {
            if (root == null)
                throw new InvalidOperationException();

            Node n = root;

            while (n.Left != null)
                n = n.Left;

            return n.Value;
        }
    }

    public T Max
    {
        get
        {
            if (root == null)
                throw new InvalidOperationException();

            Node n = root;

            while (n.Right != null)
                n = n.Right;

            return n.Value;
        }
    }

    public T this[int index]
    {
        get
        {
            if (index < 0)
                index += Count;

            if ((uint)index >= (uint)Count)
                throw new IndexOutOfRangeException();

            return Kth(root, index);
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
    }
    public int BinarySearch(T value)
    {
        int rank = 0;
        Node cur = root;

        while (cur != null)
        {
            int cmp = value.CompareTo(cur.Value);

            if (cmp == 0)
                return rank + Size(cur.Left);

            if (cmp < 0)
            {
                cur = cur.Left;
            }
            else
            {
                rank += Size(cur.Left) + cur.Count;
                cur = cur.Right;
            }
        }

        return -1;
    }
    public int MinBinary(T value)
    {
        int rank = 0;

        Node n = root;

        while (n != null)
        {
            int cmp = value.CompareTo(n.Value);

            if (cmp <= 0)
            {
                n = n.Left;
            }
            else
            {
                rank += Size(n.Left) + n.Count;
                n = n.Right;
            }
        }

        return rank-1;
    }

    public T LowerBound(T value)
    {
        Node cur = root;
        Node ans = null;

        while (cur != null)
        {
            if (cur.Value.CompareTo(value) >= 0)
            {
                ans = cur;
                cur = cur.Left;
            }
            else
            {
                cur = cur.Right;
            }
        }

        if (ans == null)
            throw new InvalidOperationException();

        return ans.Value;
    }

    public T UpperBound(T value)
    {
        Node cur = root;
        Node ans = null;

        while (cur != null)
        {
            if (cur.Value.CompareTo(value) > 0)
            {
                ans = cur;
                cur = cur.Left;
            }
            else
            {
                cur = cur.Right;
            }
        }

        if (ans == null)
            throw new InvalidOperationException();

        return ans.Value;
    }

    public bool TryLowerBound(T value, out T result)
    {
        Node cur = root;
        Node ans = null;

        while (cur != null)
        {
            if (cur.Value.CompareTo(value) >= 0)
            {
                ans = cur;
                cur = cur.Left;
            }
            else
            {
                cur = cur.Right;
            }
        }

        if (ans == null)
        {
            result = default;
            return false;
        }

        result = ans.Value;
        return true;
    }

    public bool TryUpperBound(T value, out T result)
    {
        Node cur = root;
        Node ans = null;

        while (cur != null)
        {
            if (cur.Value.CompareTo(value) > 0)
            {
                ans = cur;
                cur = cur.Left;
            }
            else
            {
                cur = cur.Right;
            }
        }

        if (ans == null)
        {
            result = default;
            return false;
        }

        result = ans.Value;
        return true;
    }

    public bool TryGetAt(int index, out T result)
    {
        if (index < 0)
            index += Count;

        if ((uint)index >= (uint)Count)
        {
            result = default;
            return false;
        }

        result = Kth(root, index);
        return true;
    }

    public void Clear()
    {
        root = null;
    }

    public IEnumerator<T> GetEnumerator()
    {
        var stack = new Stack<Node>();
        Node cur = root;

        while (stack.Count > 0 || cur != null)
        {
            while (cur != null)
            {
                stack.Push(cur);
                cur = cur.Left;
            }

            cur = stack.Pop();

            for (int i = 0; i < cur.Count; i++)
                yield return cur.Value;

            cur = cur.Right;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
