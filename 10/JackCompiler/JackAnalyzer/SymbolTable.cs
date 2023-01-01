using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackAnalyzer
{
    internal class SymbolTable
    {
        private Dictionary<string, Symbol> _symbols = new Dictionary<string, Symbol>();
        private Dictionary<SymbolKind, int> _indexes = new Dictionary<SymbolKind, int>();

        public SymbolTable()
        {
            foreach (var k in Enum.GetValues<SymbolKind>())
                _indexes[k] = 0;
        }

        public void Define(string name, string type, SymbolKind kind)
        {
            var index = _indexes[kind];
            _indexes[kind] = index + 1;

            var s = new Symbol { Name = name, Type = type, Index = index, Kind = kind };
            _symbols[name] = s;
        }

        public Symbol Lookup(string name) => _symbols[name];

        public int VarCount(SymbolKind kind) => _indexes[kind] - 1;

        public class Symbol
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public SymbolKind Kind { get; set; }
            public int Index { get; set; }
        }
    }

    public enum SymbolKind
    {
        Static = 0,
        Field = 1,
        Arg = 2,
        Var = 3
    }
}
