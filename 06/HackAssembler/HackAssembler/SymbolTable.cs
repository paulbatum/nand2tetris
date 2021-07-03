using System;
using System.Collections.Generic;
using System.Text;

namespace HackAssembler
{
    public class SymbolTable
    {
        private Dictionary<string, short> table;
        private short nextVariable = 16;

        public SymbolTable()
        {
            table = new Dictionary<string, short>();

            for(short i = 0; i < 16; i++)
                table.Add($"R{i}", i);

            table.Add("SP", 0);
            table.Add("LCL", 1);
            table.Add("ARG", 2);
            table.Add("THIS", 3);
            table.Add("THAT", 4);
            table.Add("SCREEN", 16384);
            table.Add("KBD", 24576);
        }

        public void AddEntry(string symbol, short address)
        {
            table[symbol] = address;
        }

        public int AddVariable(string symbol)
        {
            table[symbol] = nextVariable;
            return nextVariable++;
        }

        public bool Contains(string symbol)
        {
            return table.ContainsKey(symbol);
        }

        public int GetAddress(string symbol)
        {
            return table[symbol];
        }
    }
}
