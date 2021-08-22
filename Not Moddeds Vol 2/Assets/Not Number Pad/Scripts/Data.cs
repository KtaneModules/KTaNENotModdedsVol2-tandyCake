using UnityEngine;

namespace NotNumberPad
{
    public static class Data
    {
        public static readonly Color[] unlitButtonColors = new Color[]
        {
            new Color(1, 0.3f, 0.3f),
            new Color(1, 1, 0.3f),
            new Color(0.3f, 1, 0.3f),
            new Color(0.3f, 0.3f, 1),
        };
        public static readonly Color[] litButtonColors = new Color[]
        {
            Color.red,
            Color.yellow,
            Color.green,
            Color.blue
        };
        public static readonly int[][] priorities = new int[][]
        {
            new int[]{ 2,1,8,0,3,9,7,4,6,5},
            new int[]{ 9,2,5,1,3,7,8,4,6,0},
            new int[]{ 4,8,0,5,7,3,6,2,1,9},
            new int[]{ 8,5,0,2,3,4,1,6,9,7},
        };

        public static readonly int[,] multiplierTable = new int[,]
        {
            { 41, 15, 6, 43, 7, 30, 47, 14, 32, },
            { 20, 25, 23, 31, 21, 22, 19, 34, 12, },
            { 35, 6, 11, 18, 9, 42, 36, 8, 46, },
            { 10, 13, 17, 4, 45, 2, 27, 26, 3, },
        };
    }
}