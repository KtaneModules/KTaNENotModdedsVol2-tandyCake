using System;

public class WireInfo
{
    public string word { get; set; }
    public int swap1 { get; private set; }
    public int swap2 { get; private set; }

    public int AssociatedWire
    {
        get { return wireTable[swap1, swap2]; }
        private set { }
    }
    
    
    public WireInfo(string word, int swap1, int swap2)
    {
        if (swap1 >= 6 || swap1 < 0)
            throw new ArgumentOutOfRangeException("The first swap number is outside the given range");
        if (swap2 >= 6 || swap2 < 0)
            throw new ArgumentOutOfRangeException("The second swap number is outside the given range");
        if (swap1 == swap2)
            throw new ArgumentException("The swap numbers cannot be equal");
        this.word = word;
        this.swap1 = swap1;
        this.swap2 = swap2;
    }
    public WireInfo(string word, int[] swaps) : this(word, swaps[0], swaps[1])
    { } 

    private static readonly int[,] wireTable = new int[,]
    {
        { -1, 2, 4, 0, 1, 5 },
        { 2, -1, 1, 2, 3, 0 },
        { 4, 1, -1, 0, 5, 3 },
        { 0, 2, 0, -1, 4, 2 },
        { 1, 3, 5, 4, -1, 4 },
        { 5, 0, 3, 2, 4, -1 }
    };
}
