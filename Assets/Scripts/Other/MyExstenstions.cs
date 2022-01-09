using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyExstenstions
{
    public static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (null == obj)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public static PlayerStats[] SortArray(PlayerStats[] array)
    {
        int length = array.Length;
        if (length == 0)
            return array;
        PlayerStats temp = array[0];

        for (int i = 0; i < length; i++)
        {
            for (int j = i + 1; j < length; j++)
            {
                if (array[i].deaths > array[j].deaths)
                {
                    temp = array[i];

                    array[i] = array[j];

                    array[j] = temp;
                }
            }
        }
        return array;
    }
}
