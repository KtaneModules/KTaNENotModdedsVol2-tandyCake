using System;

namespace NotPianoKeys
{
    public static class NPKData
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

        public static int AstroIndex(PianoSymbol s1, PianoSymbol s2)
        {
            int f1 = s1.family;
            int f2 = s2.family;
            if (f2 <= f1) //The symbols must always be in order 0, 1, 2
                throw new FormatException("The two symbols entered are not in the correct order.");
            if (f1 == 0 && f2 == 1)
                return table1[(int)s1.name, (int)s2.name - 4];
            if (f1 == 0 && f2 == 2)
                return table2[(int)s1.name, (int)s2.name - 13];
            if (f1 == 1 && f2 == 2)
                return table3[(int)s2.name - 13, (int)s1.name - 4];
            throw new ArgumentOutOfRangeException("The pair of symbols supplied cannot be indexed into the table.");
        }

        public static readonly int[][] quadrants = new int[][]
        {
            new[]{4, 3, 0, 1}, new[] { 4, 1, 2, 5}, new[] { 4, 5, 8, 7 }, new[] { 4, 7, 6, 3}
        };
        public static string[][] bigGrid = new string[][] //This is the individual 3x3s, not the rows or columns
        {
            new string[]{"67","72","15","21","46","31","14","21","53",},
            new string[]{"51","36","23","17","26","64","75","41","12",},
            new string[]{"42","15","24","71","52","13","26","37","62",},
            new string[]{"52","17","74","31","65","12","76","51","37",},
            new string[]{"43","25","31","21","71","56","42","21","41",},
            new string[]{"61","24","16","12","45","21","21","26","63",},
            new string[]{"76","32","42","25","13","61","12","54","27",},
            new string[]{"24","63","27","16","52","71","32","15","45",},
            new string[]{"15","13","51","34","21","47","23","63","72",},
        };
        public static int[] DoSwap(int[] sequence, string swap)
        {
            int[] positions = { swap[0] - '1', swap[1] - '1' };
            int temp = sequence[positions[0]];
            sequence[positions[0]] = sequence[positions[1]];
            sequence[positions[1]] = temp;
            return sequence;
        }
    };
}
