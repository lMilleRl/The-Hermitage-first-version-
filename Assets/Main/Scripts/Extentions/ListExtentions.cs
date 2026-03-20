using System.Collections.Generic;

public static class ListExtentions
{
    public static void UnSortRemoveAt<T>(this List<T> list, int index)
    {
        T lastElement = list[list.Count - 1];
        list[index] = lastElement;
        list.RemoveAt(index);        
    }
}
