using System;
using System.Collections.Generic;

namespace NotPianoKeys
{
    public static class NPKTables
    {
        public static readonly int[,] table1 = new int[4, 9]
        {
        { 2, 0, 4, 3, 1, 4, 3, 0, 4 } ,
        { 4, 0, 3, 1, 1, 0, 3, 1, 4 } ,
        { 3, 2, 2, 1, 2, 0, 2, 3, 0 } ,
        { 4, 0, 1, 0, 0, 2, 0, 4, 4 }
        };
        public static readonly int[,] table2 = new int[4, 9]
        {
        { 1, 1, 0, 2, 4, 3, 2, 2, 2 } ,
        { 1, 2, 4, 0, 3, 3, 3, 1, 3 } ,
        { 1, 3, 3, 3, 2, 4, 2, 4, 4 } ,
        { 3, 0, 1, 2, 2, 3, 2, 2, 0 }
        };
        public static readonly int[,] table3 = new int[9, 9]
        {
        { 1, 4, 4, 1, 2, 0, 0, 0, 2 } ,
        { 4, 3, 3, 1, 3, 4, 4, 1, 2 } ,
        { 2, 2, 2, 0, 4, 1, 3, 1, 1 } ,
        { 3, 3, 4, 2, 0, 0, 2, 2, 4 } ,
        { 3, 0, 4, 3, 1, 1, 4, 3, 1 } ,
        { 1, 3, 2, 3, 0, 3, 0, 3, 2 } ,
        { 2, 2, 2, 2, 0, 1, 0, 1, 4 } ,
        { 3, 3, 4, 3, 1, 4, 2, 0, 1 } ,
        { 2, 4, 1, 1, 4, 3, 4, 3, 3 }
        };

        public static readonly int[][] orders = new int[][]
        {
            new int[] { 0, 7, 1, 8, 2, 9, 3, 10, 4, 11, 5, 6 },
            new int[] { 8, 1, 2, 9, 0, 5, 11, 3, 7, 4, 6, 10 },
            new int[] { 4, 3, 0, 8, 2, 7, 6, 1, 9, 5, 10, 11 },
            new int[] { 1, 8, 5, 7, 3, 6, 4, 10, 0, 11, 2, 9 },
            new int[] { 5, 11, 0, 4, 9, 6, 8, 2, 7, 3, 1, 10 },
            new int[] { 5, 7, 0, 8, 11, 10, 1, 4, 6, 9, 2, 3 },
            new int[] { 2, 4, 1, 11, 8, 0, 7, 9, 5, 3, 10, 6 },
            new int[] { 10, 6, 0, 2, 8, 9, 3, 11, 4, 1, 7, 5 },
            new int[] { 6, 7, 2, 11, 0, 3, 10, 4, 1, 5, 8, 9 },
            new int[] { 5, 2, 1, 3, 7, 9, 10, 0, 6, 8, 11, 4 }
        };

        public static int Index(PianoSymbol s1, PianoSymbol s2)
        {
            int f1 = s1.family;
            int f2 = s2.family;
            if (f2 <= f1) //The symbols must always be in order 0, 1, 2
                throw new ArgumentException("The two symbols entered are not in the correct order.");
            if (f1 == 0 && f2 == 1)
                return table1[(int)s1.name, (int)s2.name - 4];
            if (f1 == 0 && f2 == 2)
                return table2[(int)s1.name, (int)s2.name - 13];
            if (f1 == 1 && f2 == 2)
                return table3[(int)s2.name - 13, (int)s1.name - 4];
            throw new ArgumentOutOfRangeException("The pair of symbols supplied cannot be indexed into the table.");
        }

        public static readonly Dictionary<char, SwapPair> Swaps = new Dictionary<char, SwapPair>()
        {
            {'A', new SwapPair(new Swap(6, false), new Swap(3, true)) },
            {'B', new SwapPair(new Swap(2, true ), new Swap(5, false)) },
            {'C', new SwapPair(new Swap(0, true ), new Swap(5, true)) },
            {'D', new SwapPair(new Swap(3, false), new Swap(1, false)) },
            {'E', new SwapPair(new Swap(3, false), new Swap(2, false)) },
            {'F', new SwapPair(new Swap(4, true ), new Swap(1, false)) },
            {'G', new SwapPair(new Swap(1, true ), new Swap(5, false)) },
            {'H', new SwapPair(new Swap(6, false), new Swap(5, false)) },
            {'I', new SwapPair(new Swap(2, true ), new Swap(6, false)) },
            {'J', new SwapPair(new Swap(4, false), new Swap(2, true)) },
            {'K', new SwapPair(new Swap(6, false), new Swap(5, true)) },
            {'L', new SwapPair(new Swap(3, true ), new Swap(5, false)) },
            {'M', new SwapPair(new Swap(5, true ), new Swap(4, false)) },
            {'N', new SwapPair(new Swap(5, true ), new Swap(3, false)) },
            {'O', new SwapPair(new Swap(3, true ), new Swap(6, false)) },
            {'P', new SwapPair(new Swap(1, true ), new Swap(2, true)) },
            {'Q', new SwapPair(new Swap(4, true ), new Swap(2, false)) },
            {'R', new SwapPair(new Swap(5, true ), new Swap(2, false)) },
            {'S', new SwapPair(new Swap(6, false), new Swap(0, true)) },
            {'T', new SwapPair(new Swap(6, true ), new Swap(3, false)) },
            {'U', new SwapPair(new Swap(0, true ), new Swap(4, false)) },
            {'V', new SwapPair(new Swap(5, true ), new Swap(3, true)) },
            {'W', new SwapPair(new Swap(5, true ), new Swap(0, true)) },
            {'X', new SwapPair(new Swap(5, false), new Swap(2, false)) },
            {'Y', new SwapPair(new Swap(3, true ), new Swap(1, true)) },
            {'Z', new SwapPair(new Swap(6, false), new Swap(2, true)) },
            {'0', new SwapPair(new Swap(2, true ), new Swap(4, false)) },
            {'1', new SwapPair(new Swap(2, false), new Swap(5, true)) },
            {'2', new SwapPair(new Swap(4, true ), new Swap(0, true)) },
            {'3', new SwapPair(new Swap(5, false), new Swap(1, true)) },
            {'4', new SwapPair(new Swap(3, true ), new Swap(4, false)) },
            {'5', new SwapPair(new Swap(4, true ), new Swap(1, true)) },
            {'6', new SwapPair(new Swap(0, false), new Swap(6, false)) },
            {'7', new SwapPair(new Swap(4, false), new Swap(5, true)) },
            {'8', new SwapPair(new Swap(2, false), new Swap(4, false)) },
            {'9', new SwapPair(new Swap(0, true ), new Swap(5, true)) }
        };
    }
}
