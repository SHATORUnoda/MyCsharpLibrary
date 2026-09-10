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
        public Node Parent;

        public Node(T value, int priority)
        {
            Value = value;
            Priority = priority;
            Size = 1;
        }
    }

    private Node root;

    private readonly Dictionary<T, HashSet<Node>> nodesByValue = new();
    private readonly HashSet<Node> nullValueNodes = new();

    private static readonly Random rnd = new();

    private static int Size(Node n)
        => n?.Size ?? 0;

    private static void Update(Node n)
    {
        if (n != null)
            n.Size = 1 + Size(n.Left) + Size(n.Right);
    }

    private static void SetRoot(ref Node root, Node node)
    {
        root = node;

        if (node != null)
            node.Parent = null;
    }

    private static void SetLeft(Node parent, Node child)
    {
        parent.Left = child;

        if (child != null)
            child.Parent = parent;
    }

    private static void SetRight(Node parent, Node child)
    {
        parent.Right = child;

        if (child != null)
            child.Parent = parent;
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
        {
            if (b != null)
                b.Parent = null;

            return b;
        }

        if (b == null)
        {
            a.Parent = null;

            return a;
        }

        if (a.Priority > b.Priority)
        {
            Push(a);
            SetRight(a, Merge(a.Right, b));
            Update(a);
            a.Parent = null;
            return a;
        }
        else
        {
            Push(b);
            SetLeft(b, Merge(a, b.Left));
            Update(b);
            b.Parent = null;
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
            Split(n.Left, k, out a, out Node left);
            SetLeft(n, left);
            b = n;
            b.Parent = null;

            if (a != null)
                a.Parent = null;

            Update(b);
        }
        else
        {
            Split(n.Right, k - Size(n.Left) - 1, out Node right, out b);
            SetRight(n, right);
            a = n;
            a.Parent = null;

            if (b != null)
                b.Parent = null;

            Update(a);
        }
    }

    private static T Kth(Node n, int k)
    {
        return KthNode(n, k).Value;
    }

    private static Node KthNode(Node n, int k)
    {
        Push(n);

        int leftSize = Size(n.Left);

        if (k < leftSize)
            return KthNode(n.Left, k);

        if (k == leftSize)
            return n;

        return KthNode(n.Right, k - leftSize - 1);
    }

    private static int GetIndex(Node node)
    {
        var path = new Stack<Node>();

        for (Node current = node; current != null; current = current.Parent)
            path.Push(current);

        Node currentNode = path.Pop();
        Push(currentNode);
        int index = 0;

        while (path.Count > 0)
        {
            Node child = path.Pop();

            if (currentNode.Right == child)
                index += Size(currentNode.Left) + 1;

            currentNode = child;
            Push(currentNode);
        }

        return index + Size(node.Left);
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

    private void AddToIndex(Node node)
    {
        if (node.Value is null)
        {
            nullValueNodes.Add(node);
            return;
        }

        if (!nodesByValue.TryGetValue(node.Value, out HashSet<Node> nodes))
        {
            nodes = new HashSet<Node>();
            nodesByValue.Add(node.Value, nodes);
        }

        nodes.Add(node);
    }

    private void RemoveFromIndex(Node node)
    {
        if (node.Value is null)
        {
            nullValueNodes.Remove(node);
            return;
        }

        HashSet<Node> nodes = nodesByValue[node.Value];
        nodes.Remove(node);

        if (nodes.Count == 0)
            nodesByValue.Remove(node.Value);
    }

    private bool TryGetNodes(T value, out HashSet<Node> nodes)
    {
        if (value is null)
        {
            nodes = nullValueNodes;
            return nodes.Count > 0;
        }

        return nodesByValue.TryGetValue(value, out nodes);
    }

    private void MoveTo(ImplicitTreap<T> destination, Node node)
    {
        RemoveFromIndex(node);
        destination.AddToIndex(node);
    }

    private static void CollectNodes(Node node, List<Node> nodes)
    {
        if (node == null)
            return;

        Push(node);
        CollectNodes(node.Left, nodes);
        nodes.Add(node);
        CollectNodes(node.Right, nodes);
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

    public bool Contains(T value)
    {
        return TryGetNodes(value, out _);
    }

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

            Node node = KthNode(root, index);

            if (!EqualityComparer<T>.Default.Equals(node.Value, value))
            {
                RemoveFromIndex(node);
                node.Value = value;
                AddToIndex(node);
            }
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

        var node = new Node(value, rnd.Next());
        Split(root, index, out Node a, out Node b);
        SetRoot(ref root, Merge(Merge(a, node), b));
        AddToIndex(node);
    }

    public void AddFirst(T value)
    {
        Insert(0, value);
    }

    public void AddLast(T value)
    {
        Insert(Count, value);
    }

    public bool Remove(T value)
    {
        if (!TryGetNodes(value, out HashSet<Node> nodes))
            return false;

        foreach (Node node in nodes)
        {
            RemoveAt(GetIndex(node));
            return true;
        }

        return false;
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
        RemoveFromIndex(c);
        SetRoot(ref root, Merge(a, b));

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
        SetRoot(ref root, Merge(Merge(a, c), b));
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

        SetRoot(ref root, Merge(Merge(a, c), b));

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

        if (c != null)
            c.Parent = null;

        var nodes = new List<Node>(length);
        CollectNodes(c, nodes);

        foreach (Node node in nodes)
            MoveTo(ret, node);

        SetRoot(ref root, Merge(a, b));

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
        {
            var node = new Node(item, rnd.Next());
            mid = Merge(mid, node);
            AddToIndex(node);
        }

        Split(root, index, out Node a, out Node b);
        SetRoot(ref root, Merge(Merge(a, mid), b));
    }

    public void Clear()
    {
        root = null;
        nodesByValue.Clear();
        nullValueNodes.Clear();
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
