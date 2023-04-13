using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackAnalyzer
{
    internal class SymbolTable
    {
        private Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();
        private Dictionary<SymbolKind, int> indexes = new Dictionary<SymbolKind, int>();

        public SymbolTable()
        {
            foreach (var k in Enum.GetValues<SymbolKind>())
                indexes[k] = 0;
        }

        public void Define(string name, string type, SymbolKind kind)
        {
            var index = indexes[kind];
            indexes[kind] = index + 1;

            var s = new Symbol { Name = name, Type = type, Index = index, Kind = kind };
            symbols[name] = s;
        }

        public Symbol? Lookup(string name) => symbols.GetValueOrDefault(name);

        public int VarCount(SymbolKind kind) => indexes[kind];

    }

    public class Symbol
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public SymbolKind Kind { get; set; }
        public int Index { get; set; }

        public Segment Segment => this.Kind switch
        {
            SymbolKind.Static => Segment.Static,
            SymbolKind.Field => Segment.This,
            SymbolKind.Arg => Segment.Argument,
            SymbolKind.Var => Segment.Local,
        };
    }



    public enum SymbolKind
    {
        Static = 0,
        Field = 1,
        Arg = 2,
        Var = 3
    }
}
