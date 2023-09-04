using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Heap<T> where T : IHeapItem<T>
{
    T[] items;
    int curCount;

    public Heap(int maxHeapSize)
    {
        items = new T[maxHeapSize];
    }

    public void Add(T item)
    {
        item.HeapIndex = curCount;
        items[curCount] = item;
        SortUp(item);
        curCount++;
    }
    public T RemoveFirst()
    {
        //set first item and count - 1
        T firstItem = items[0];
        curCount--;

        //set the last item as the new first, then sort down
        items[0] = items[curCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return firstItem;
    }
    public int Count => curCount;
    public bool Contains(T _item) => Equals(items[_item.HeapIndex], _item);
    public void UpdateItem(T _item)
    {
        SortUp(_item);
    }
    void SortDown(T _item)
    {
        while(true)
        {
            int childIndexLeft = _item.HeapIndex * 2 + 1;
            int childIndexRight = _item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            if(childIndexLeft < curCount) // if left is less than cur count
            {
                swapIndex = childIndexLeft;

                if(childIndexRight < curCount) //if right is also less than cur count
                {
                    //compare fcosts and hcost(if fcost equal) -1 = left less priority. 1 = left high priority
                    if(items[childIndexLeft].CompareTo(items[childIndexRight]) < 0) 
                    {
                        swapIndex = childIndexRight;
                    }
                }

                if (_item.CompareTo(items[swapIndex]) < 0)
                {
                    Swap(_item, items[swapIndex]);
                }
                else return;
            }
            else return;
        }
    }
    void SortUp(T _item)
    {
        int parentIndex = (_item.HeapIndex - 1) / 2;

        while(true)
        {
            T parentItem = items[parentIndex];
            if(_item.CompareTo(parentItem) > 0)
            {
                Swap(_item, parentItem); //this swaps the heapindex
            }
            else // if item's priority is the same or less than parent, we break out of the loop and skips swapping
            {
                break; 
            }

            parentIndex = (_item.HeapIndex - 1) / 2; //after swapping heapIndex, the item's parent changes, updates again
        }
    }
    void Swap(T _itemA, T _itemB)
    {
        items[_itemA.HeapIndex] = _itemB;
        items[_itemB.HeapIndex] = _itemA;
        int itemAIndex = _itemA.HeapIndex;
        _itemA.HeapIndex = _itemB.HeapIndex;
        _itemB.HeapIndex = itemAIndex;
    }
}
public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex{get; set;}
}
