using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spectrum.Utilities
{
    public static class ColorRandomizer
    {
        public static Dictionary<int, Color> colorDictionary = new Dictionary<int, Color>()
        {
            {0, Color.gray },
            {1, Color.white },
            {2, Color.blue },
            {3, Color.red },
            {4, Color.green },
            {5, Color.yellow },
            {6, Color.cyan },
            {7, Color.magenta },
        };

        public static int[] GetNewRandomColors(int playerCount)
        {
            // Adding 3 extra colors to keep it interesting.

            ArrayList tempColorIndexList = new ArrayList(colorDictionary.Keys);
            int[] finalIndexList = new int[playerCount + 3];

            for (int i = 0; i < playerCount + 3; ++i)
            {
                int index = Random.Range(0, tempColorIndexList.Count);
                finalIndexList[i] = (int)tempColorIndexList[index];
                tempColorIndexList.RemoveAt(index);
            }

            return finalIndexList;
        }
    }
}