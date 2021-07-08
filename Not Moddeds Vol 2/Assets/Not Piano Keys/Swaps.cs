using System;

namespace NotPianoKeys
{
    public class Swap
    {
        public int value { get; private set; }
        public bool labelBased { get; private set; }
        public Swap(int val, bool isLabel)
        {
            value = val;
            labelBased = isLabel;
        }
        public static int[] PerformSwap(int[] input, SwapPair swaps)
        {
            int position1 = swaps.swap1.labelBased ? Array.IndexOf(input, swaps.swap1.value) : swaps.swap1.value;
            int position2 = swaps.swap2.labelBased ? Array.IndexOf(input, swaps.swap2.value) : swaps.swap2.value;
            int temp = input[position1];
            input[position1] = input[position2];
            input[position2] = temp;
            return input;
        }
    }
    public class SwapPair
    {
        public Swap swap1;
        public Swap swap2;
        public SwapPair(Swap s1, Swap s2)
        {
            swap1 = s1;
            swap2 = s2;
        }
    }
}
