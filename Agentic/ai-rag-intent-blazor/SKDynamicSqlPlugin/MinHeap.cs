// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SKDynamicSqlPlugin;

/// <summary>
/// Implements the classic 'heap' data structure. By default, the item with the lowest value is at the top of the heap.
/// </summary>
/// <typeparam name="T">Data type.</typeparam>
internal sealed class MinHeap<T> : IEnumerable<T> where T : IComparable<T>
{
    private const int DefaultCapacity = 7;
    private const int MinCapacity = 0;

    private static readonly T[] s_emptyBuffer = new T[0];

    private T[] _items;
    private int _count;

    /// <summary>
    /// Initializes a new instance of the <see cref="MinHeap{T}"/> class.
    /// </summary>
    /// <param name="minValue">Heap minimum value, which will be used as first item in collection.</param>
    /// <param name="capacity">Number of elements that collection can hold.</param>
    public MinHeap(T minValue, int capacity = DefaultCapacity)
    {
        if (capacity < MinCapacity)
        {
            Verify.ThrowArgumentOutOfRangeException(nameof(capacity), capacity, $"MinHeap capacity must be greater than {MinCapacity}.");
        }

        _items = new T[capacity + 1];
        //
        // The 0'th item is a sentinel entry that simplifies the code
        //
        _items[0] = minValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinHeap{T}"/> class.
    /// </summary>
    /// <param name="minValue">Heap minimum value, which will be used as first item in collection.</param>
    /// <param name="items">List of items to add.</param>
    public MinHeap(T minValue, IList<T> items)
        : this(minValue, items.Count)
    {
        Add(items);
    }

    /// <summary>
    /// Gets the current number of items in the collection.
    /// </summary>
    public int Count
    {
        get => _count;
        internal set
        {
            Debug.Assert(value <= Capacity);
            _count = value;
        }
    }

    /// <summary>
    /// Gets the number of elements that collection can hold.
    /// </summary>
    public int Capacity => _items.Length - 1; // 0'th item is always a sentinel to simplify code

    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    public T this[int index]
    {
        get => _items[index + 1];
        internal set { _items[index + 1] = value; }
    }

    /// <summary>
    /// Gets first item in collection.
    /// </summary>
    public T Top => _items[1];

    /// <summary>
    /// Gets the boolean flag which indicates if collection is empty.
    /// </summary>
    public bool IsEmpty => _count == 0;

    /// <summary>
    /// Sets collection item count to zero.
    /// </summary>
    public void Clear()
    {
        _count = 0;
    }

    /// <summary>
    /// Sets collection item count to zero and removes all items in collection.
    /// </summary>
    public void Erase()
    {
        Array.Clear(_items, 1, _count);
        _count = 0;
    }

    /// <summary>
    /// Removes all items in collection and returns them.
    /// </summary>
    public T[] DetachBuffer()
    {
        T[] buf = _items;
        _items = s_emptyBuffer;
        _count = 0;
        return buf;
    }

    /// <summary>
    /// Adds new item to collection.
    /// </summary>
    /// <param name="item">Item to add.</param>
    public void Add(T item)
    {
        //
        // the 0'th item is always a sentinel and not included in this._count.
        // The length of the buffer is always this._count + 1
        //
        _count++;
        EnsureCapacity();
        _items[_count] = item;
        UpHeap(_count);
    }

    /// <summary>
    /// Adds new items to collection.
    /// </summary>
    /// <param name="items">Items to add.</param>
    public void Add(IEnumerable<T> items)
    {
        foreach (T item in items)
        {
            Add(item);
        }
    }

    /// <summary>
    /// Adds new items starting from specified index.
    /// </summary>
    /// <param name="items">Items to add.</param>
    /// <param name="startAt">Starting point of items to add.</param>
    public void Add(IList<T> items, int startAt = 0)
    {
        Verify.NotNull(items);

        int newItemCount = items.Count;
        if (startAt >= newItemCount)
        {
            Verify.ThrowArgumentOutOfRangeException(nameof(startAt), startAt, $"{nameof(startAt)} value must be less than {nameof(items)}.{nameof(items.Count)}.");
        }

        EnsureCapacity(_count + (newItemCount - startAt));
        for (int i = startAt; i < newItemCount; ++i)
        {
            //
            // the 0'th item is always a sentinel and not included in this._count.
            // The length of the buffer is always this._count + 1
            //
            _count++;
            _items[_count] = items[i];
            UpHeap(_count);
        }
    }

    /// <summary>
    /// Removes first item in collection and returns it.
    /// </summary>
    public T RemoveTop()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("MinHeap is empty.");
        }

        T item = _items[1];
        _items[1] = _items[_count--];
        DownHeap(1);
        return item;
    }

    /// <summary>
    /// Removes all items in collection and returns them.
    /// </summary>
    public IEnumerable<T> RemoveAll()
    {
        while (_count > 0)
        {
            yield return RemoveTop();
        }
    }

    /// <summary>
    /// Resizes collection to specified capacity.
    /// </summary>
    /// <param name="capacity">Number of elements that collection can hold.</param>
    public void EnsureCapacity(int capacity)
    {
        if (capacity < MinCapacity)
        {
            Verify.ThrowArgumentOutOfRangeException(nameof(capacity), capacity, $"MinHeap capacity must be greater than {MinCapacity}.");
        }

        // 0th item is always a sentinel
        capacity++;
        if (capacity > _items.Length)
        {
            Array.Resize(ref _items, capacity);
        }
    }

    /// <summary>
    /// Doubles collection capacity.
    /// </summary>
    public void EnsureCapacity()
    {
        if (_count == _items.Length)
        {
            Array.Resize(ref _items, _count * 2 + 1);
        }
    }

    private void UpHeap(int startAt)
    {
        int i = startAt;
        T[] items = _items;
        T item = items[i];
        int parent = i >> 1; //i / 2;

        while (parent > 0 && items[parent].CompareTo(item) > 0)
        {
            // Child > parent. Exchange with parent, thus moving the child up the queue
            items[i] = items[parent];
            i = parent;
            parent = i >> 1; //i / 2;
        }

        items[i] = item;
    }

    private void DownHeap(int startAt)
    {
        int i = startAt;
        int count = _count;
        int maxParent = count >> 1;
        T[] items = _items;
        T item = items[i];

        while (i <= maxParent)
        {
            int child = i + i;
            //
            // Exchange the item with the smaller of its two children - if one is smaller, i.e.
            //
            // First, find the smaller child
            //
            if (child < count && items[child].CompareTo(items[child + 1]) > 0)
            {
                child++;
            }

            if (item.CompareTo(items[child]) <= 0)
            {
                // Heap condition is satisfied. Parent <= both its children
                break;
            }

            // Else, swap parent with the smallest child
            items[i] = items[child];
            i = child;
        }

        items[i] = item;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        // The 0'th item in the queue is a sentinel. i is 1 based.
        for (int i = 1; i <= _count; ++i)
        {
            yield return _items[i];
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Heap Sort in-place.
    /// This is destructive. Once you do this, the heap order is lost.
    /// The advantage on in-place is that we don't need to do another allocation
    /// </summary>
    public void SortDescending()
    {
        int count = _count;
        int i = count; // remember that the 0'th item in the queue is always a sentinel. So i is 1 based

        while (_count > 0)
        {
            //
            // this dequeues the item with the current LOWEST relevancy
            // We take that and place it at the 'back' of the array - thus inverting it
            //
            T item = RemoveTop();
            _items[i--] = item;
        }

        _count = count;
    }

    /// <summary>
    /// Restores heap order
    /// </summary>
    internal void Restore()
    {
        Clear();
        Add(_items, 1);
    }

    internal void Sort(IComparer<T> comparer)
    {
        Array.Sort(_items, 1, _count, comparer);
    }
}
