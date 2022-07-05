using HDLTools;
using Parlot;
using Parlot.Fluent;
using System.Text;
using static Parlot.Fluent.Parsers;

var library = new ChipLibrary();
library.LoadAll("hdl");

var chip = library.GetChip("Mux");
Console.WriteLine(chip.DumpTree());
