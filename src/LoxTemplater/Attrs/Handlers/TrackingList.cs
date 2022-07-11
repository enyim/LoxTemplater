using System.Collections;

namespace Enyim.LoxTempl;

public sealed class TrackingList<T> : IList<T>
{
    private readonly List<T> inner = new();
    private readonly HashSet<T> removed = new();

    public int Count => inner.Count;
    public bool IsReadOnly => ((ICollection<T>)inner).IsReadOnly;

    public T this[int index]
    {
        get => inner[index];
        set
        {
            var old = inner[index];
            inner[index] = value;

            if (!inner.Contains(old)) removed.Add(old);
        }
    }

    internal IReadOnlySet<T> Removed => removed;

    public void Reset() => removed.Clear();

    public void Add(T item)
    {
        removed.Remove(item);
        inner.Add(item);
    }

    public void Clear()
    {
        inner.ForEach((item) => removed.Add(item));
        inner.Clear();
    }

    public void Insert(int index, T item)
    {
        inner.Insert(index, item);
        removed.Remove(item);
    }

    public bool Remove(T item)
    {
        if (inner.Remove(item))
        {
            if (!inner.Contains(item)) removed.Add(item);
            return true;
        }

        return false;
    }

    public void RemoveAt(int index)
    {
        var previous = this[index];
        inner.RemoveAt(index);

        if (!inner.Contains(previous)) removed.Add(previous);
    }

    public bool Contains(T item) => inner.Contains(item);
    public int IndexOf(T item) => inner.IndexOf(item);
    public void CopyTo(T[] array, int arrayIndex) => inner.CopyTo(array, arrayIndex);
    public IEnumerator<T> GetEnumerator() => inner.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => inner.GetEnumerator();
}
