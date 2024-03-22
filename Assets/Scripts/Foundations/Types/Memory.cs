using System;
using System.Collections.Generic;
using System.Linq;

public class WeakRef<T> : IEquatable<WeakRef<T>> where T : class
{
    private readonly WeakReference<T> reference;
    private readonly int hashCode;

    public WeakRef(T target)
    {
        reference = new WeakReference<T>(target);
        hashCode = target.GetHashCode();
    }

    public override int GetHashCode()
    {
        return hashCode;
    }

    public bool Equals(WeakRef<T> other)
    {
        return other != null && hashCode == other.hashCode;
    }

    public T Target
    {
        get
        {
            if (reference.TryGetTarget(out var target))
            {
                return target;
            }

            return null;
        }
    }

    public bool IsAlive => reference.TryGetTarget(out var _);
}

public class WeakSet<T> where T : class
{
    private HashSet<WeakRef<T>> items = new HashSet<WeakRef<T>>();

    public void Add(T item)
    {
        CleanUp();

        if (!Contains(item))
        {
            items.Add(new WeakRef<T>(item));
        }
    }

    public bool Contains(T item)
    {
        CleanUp();
        return items.Any(x => ReferenceEquals(x.Target, item));
    }

    public bool Remove(T item)
    {
        CleanUp();

        var WeakRef = items.FirstOrDefault(x => ReferenceEquals(x.Target, item));
        if (WeakRef != null)
        {
            return items.Remove(WeakRef);
        }

        return false;
    }

    public IEnumerable<T> GetAliveItems()
    {
        CleanUp();
        return items.Select(x => x.Target).Where(x => x != null).ToList();
    }

    private void CleanUp()
    {
        items.RemoveWhere(x => !x.IsAlive);
    }
}
