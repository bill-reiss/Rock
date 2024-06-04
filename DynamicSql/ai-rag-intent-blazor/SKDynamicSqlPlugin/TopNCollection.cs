// Copyright (c) Microsoft. All rights reserved.

using System.Collections;
using System.Collections.Generic;

namespace SKDynamicSqlPlugin;

/// <summary>
/// A collector for Top N matches. Keeps only the best N matches by Score.
/// Automatically flushes out any not in the top N.
/// By default, items are not sorted by score until you call <see cref="TopNCollection{T}.SortByScore"/>.
/// </summary>
internal sealed class TopNCollection<T> : IEnumerable<ScoredValue<T>>
{
    private int maxItems;
    private readonly MinHeap<ScoredValue<T>> _heap;
    private bool _sorted = false;

    /// <summary>
    /// Gets the maximum number of items allowed in the collection.
    /// </summary>
    public int MaxItems 
    { 
        get
        {
            return maxItems;
        }
    }

    /// <summary>
    /// Gets the current number of items in the collection.
    /// </summary>
    public int Count => _heap.Count;

    internal ScoredValue<T> this[int i] => _heap[i];
    internal ScoredValue<T> Top => _heap.Top;


    public TopNCollection(int maxItems)
    {
        this.maxItems = maxItems;
        this._heap = new(ScoredValue<T>.Min(), maxItems);
    }

    /// <summary>
    /// Resets the collection, allowing it to be reused.
    /// </summary>
    public void Reset()
    {
        _heap.Clear();
    }

    /// <summary>
    /// Adds a single scored value to the collection.
    /// </summary>
    /// <param name="value">The scored value to add.</param>
    public void Add(ScoredValue<T> value)
    {
        if (_sorted)
        {
            _heap.Restore();
            _sorted = false;
        }

        if (_heap.Count == MaxItems)
        {
            // Queue is full. We will need to dequeue the item with lowest weight
            if (value.Score <= Top.Score)
            {
                // This score is lower than the lowest score on the queue right now. Ignore it
                return;
            }

            _heap.RemoveTop();
        }

        _heap.Add(value);
    }

    /// <summary>
    /// Adds a value with a specified score to the collection.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <param name="score">The score associated with the value.</param>
    public void Add(T value, double score)
    {
        Add(new ScoredValue<T>(value, score));
    }

    /// <summary>
    /// Sorts the collection in descending order by score.
    /// </summary>
    public void SortByScore()
    {
        if (!_sorted && _heap.Count > 0)
        {
            _heap.SortDescending();
            _sorted = true;
        }
    }

    /// <summary>
    /// Returns a list containing the scored values in the collection.
    /// </summary>
    /// <returns>A list of scored values.</returns>
    public IList<ScoredValue<T>> ToList()
    {
        var list = new List<ScoredValue<T>>(Count);
        for (int i = 0, count = Count; i < count; ++i)
        {
            list.Add(this[i]);
        }

        return list;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator for the collection.</returns>
    public IEnumerator<ScoredValue<T>> GetEnumerator()
    {
        return _heap.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _heap.GetEnumerator();
    }
}