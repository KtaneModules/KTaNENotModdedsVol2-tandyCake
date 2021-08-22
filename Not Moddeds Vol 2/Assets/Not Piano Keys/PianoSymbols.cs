namespace NotPianoKeys
{
    public enum SymbolName
    {
        Flat,
        Natural,
        Sharp,
        Double_Sharp,
        Common_Time,
        Cut_Time,
        Marcato,
        Up_Bow,
        Accent,
        Down_Bow,
        Dal_Segno,
        Caesura,
        Pedal_Up,
        Fermata,
        Turn,
        Mordent,
        Whole_Note,
        Breve,
        C_Clef,
        Sixteenth_Note,
        Sixteenth_Rest,
        Quarter_Rest
    }

    public class PianoSymbol
    {
        public SymbolName name { get; private set; }
        public char symbol { get; private set; }
        public int family { get; private set; }
        public PianoSymbol(SymbolName name, char symbol, int family)
        {
            this.name = name;
            this.symbol = symbol;
            this.family = family;
        }

        public static readonly PianoSymbol[][] allSymbols = new PianoSymbol[][]
        {
            new PianoSymbol[]
            {
                new PianoSymbol(SymbolName.Flat, 'b', 0) ,
                new PianoSymbol(SymbolName.Natural, 'n', 0) ,
                new PianoSymbol(SymbolName.Sharp, '#', 0) ,
                new PianoSymbol(SymbolName.Double_Sharp, '', 0)
            },
            new PianoSymbol[]
            {
                new PianoSymbol(SymbolName.Common_Time, 'c', 1) ,
                new PianoSymbol(SymbolName.Cut_Time, 'C', 1) ,
                new PianoSymbol(SymbolName.Marcato, '^', 1) ,
                new PianoSymbol(SymbolName.Up_Bow, 'v', 1) ,
                new PianoSymbol(SymbolName.Accent, '>', 1) ,
                new PianoSymbol(SymbolName.Down_Bow, '', 1 ) ,
                new PianoSymbol(SymbolName.Dal_Segno, '%', 1 ) ,
                new PianoSymbol(SymbolName.Caesura, '"', 1) ,
                new PianoSymbol(SymbolName.Pedal_Up, '*', 1) ,
            },
            new PianoSymbol[]
            {
                new PianoSymbol(SymbolName.Fermata, 'U', 2) ,
                new PianoSymbol(SymbolName.Turn, 'T', 2) ,
                new PianoSymbol(SymbolName.Mordent, 'm', 2) ,
                new PianoSymbol(SymbolName.Whole_Note, 'w', 2) ,
                new PianoSymbol(SymbolName.Breve, '', 2) ,
                new PianoSymbol(SymbolName.C_Clef, 'B', 2) ,
                new PianoSymbol(SymbolName.Sixteenth_Note, 'w', 2) ,
                new PianoSymbol(SymbolName.Sixteenth_Rest, '', 2) ,
                new PianoSymbol(SymbolName.Quarter_Rest, '', 2)
            }
        };
        public int GetValue()
        {
            switch (family)
            {
                case 0: return (int)name;
                case 1: return (int)name - 4;
                case 2: return (int)name - 13;
            }
            throw new System.ArgumentOutOfRangeException();
        }
    }

}
