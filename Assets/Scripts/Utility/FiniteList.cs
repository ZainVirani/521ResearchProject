using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiniteList<T> {
    private T[] storage;
    private int curIndex;

    public FiniteList(int size)
    {
        storage = new T[size];
        curIndex = 0;
    }

    // Append an item to the front of the list
    public T AddFirst(T item)
    {
        storage[curIndex] = item;
        curIndex = (curIndex + 1) % storage.Length;
        return item;
    }

    // Get most recently added item
    public T GetFirst()
    {
        return storage[mapToIndex(curIndex - 1)];
    }

    // Get item at a given position. 0 being most recently added.
    public T GetAtPosition(int position)
    {
        if (position >= storage.Length)
            throw new System.IndexOutOfRangeException();
        return storage[mapToIndex(curIndex - (position + 1))]; // +1 cause cur pos points to next empty position, not most recent
    }

    int mapToIndex(int num)
    {
        return (num % storage.Length + storage.Length) % storage.Length;
    }
}
