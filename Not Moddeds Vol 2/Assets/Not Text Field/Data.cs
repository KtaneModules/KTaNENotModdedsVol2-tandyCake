using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NotTextField
{
    public static class Data
    {
        public static Dictionary<char, Dictionary<char, string>> diagrams = new Dictionary<char, Dictionary<char, string>>()
        {
            { 'A', new Dictionary<char, string>()
                {
                    {'A', "DBF" },
                    {'B', "EFC" },
                    {'C', "DEA" },
                    {'D', "FAC" },
                    {'E', "DFA" },
                    {'F', "ACB" },
                }},
            { 'B', new Dictionary<char, string>()
                {
                    {'A', "BCD" },
                    {'B', "FDA" },
                    {'C', "EDF" },
                    {'D', "EFC" },
                    {'E', "BAD" },
                    {'F', "EAA" },
                }},
            { 'C', new Dictionary<char, string>()
                {
                    {'A', "CEF" },
                    {'B', "DEA" },
                    {'C', "FAB" },
                    {'D', "FAE" },
                    {'E', "ADF" },
                    {'F', "ABD" },
                }},
            { 'D', new Dictionary<char, string>()
                {
                    {'A', "EBD" },
                    {'B', "DCE" },
                    {'C', "ADE" },
                    {'D', "AFE" },
                    {'E', "BDA" },
                    {'F', "DCB" },
                }},
            { 'E', new Dictionary<char, string>()
                {
                    {'A', "FCD" },
                    {'B', "FCD" },
                    {'C', "FEA" },
                    {'D', "BAF" },
                    {'E', "CBA" },
                    {'F', "EAB" },
                }},
            { 'F', new Dictionary<char, string>()
                {
                    {'A', "EBD" },
                    {'B', "EFC" },
                    {'C', "DBF" },
                    {'D', "BFE" },
                    {'E', "DCF" },
                    {'F', "ACD" },
                }},
        };
    }

}
