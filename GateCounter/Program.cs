using Spectre.Console;
using System.Diagnostics;

string inputPath = Directory.GetCurrentDirectory();

if (args.Length > 0)
    inputPath = args[0];

var hdlFiles = Directory.GetFiles(inputPath, "*.hdl");

var chips = new List<Chip>();
var nand = new Chip { Name = Chip.NAND };
chips.Add(nand);

foreach (var hdlFile in hdlFiles)
{
    var name = Path.GetFileNameWithoutExtension(hdlFile);
    var chip = new Chip { Name = name, FilePath = hdlFile };
    chips.Add(chip);
}

foreach(var chip in chips)
{
    if (chip.FilePath == null)
        continue;

    var input = await File.ReadAllLinesAsync(chip.FilePath);
    var foundParts = false;
    

    foreach (var line in input)
    {
        var trimmed = line.Trim();

        if(foundParts && trimmed.EndsWith(';'))
        {
            var partName = trimmed.Split('(')[0].Trim();
            var part = chips.Single(x => x.Name.Equals(partName, StringComparison.CurrentCultureIgnoreCase));
            chip.Parts.Add(part);
        }

        if (trimmed == "PARTS:")
        {
            foundParts = true;
            continue;
        }                    

    }
}


var table = new Table()
    .AddColumn("Gate")
    .AddColumn("Nand Count");

foreach (var chip in chips)
{
    table.AddRow(chip.Name, chip.NandCount.ToString());
}



AnsiConsole.Write(table);

class Chip
{
    public const string NAND = "Nand";

    public string FilePath { get; set; }
    public string Name { get; set; }
    public List<Chip> Parts { get; }

    private int nandCount = 0;

    public Chip()
    {
        Parts = new List<Chip>();
    }

    public int NandCount
    {
        get
        {
            if (nandCount == 0)
                CalculateNandCount();
            return nandCount;
        }
    }

    private void CalculateNandCount()
    {
        if (Name == NAND)
        {
            nandCount = 1;
            return;
        }

        var total = 0;
        foreach(var chip in Parts)
        {
            total += chip.NandCount;
        }

        Debug.Assert(total > 0);
        nandCount = total;
    }
}
