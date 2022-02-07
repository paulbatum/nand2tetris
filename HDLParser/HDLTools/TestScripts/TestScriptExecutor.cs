using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools.TestScripts
{
    public class TestScriptExecutor
    {
        private ChipLibrary chipLibrary;
        private List<TestScriptCommand> commands;
        private int current;
        private int cycle;
        private Chip? chip;
        private StreamWriter? outputFile;
        private StreamReader? compareFile;
        private OutputListCommand? outputList;
        private bool wroteHeader;

        public TestScriptExecutor(ChipLibrary chipLibrary, List<TestScriptCommand> commands)
        {
            this.chipLibrary = chipLibrary;
            this.commands = commands;
            this.current = -1;
            this.cycle = 0;
        }

        public bool HasMoreLines => current < commands.Count - 1;
        public Chip Chip => chip;

        public void Step()
        {
            if (!HasMoreLines)
                return;

            current++;            
            var currentCommand = commands[current];

            switch (currentCommand)
            {
                case LoadCommand loadCommand:
                    var chipDescription = HDLParser.ParseString(File.ReadAllText(Path.Combine("hdl", loadCommand.Filename))).Single();
                    chip = chipLibrary.GetChip(chipDescription.Name, "");
                    break;
                case OutputFileCommand outputFileCommand:
                    outputFile = new StreamWriter(outputFileCommand.Filename);
                    break;
                case CompareToCommand compareToCommand:
                    compareFile = new StreamReader(Path.Combine("cmp",compareToCommand.Filename));
                    break;
                case OutputListCommand outputListCommand:
                    outputList = outputListCommand;
                    break;
                case SetVariableCommand setVariableCommand:
                    if (chip == null)
                        throw new Exception("No chip loaded");

                    var targetPin = chip.Pins.Single(x => x.Name == setVariableCommand.VariableName);
                    targetPin.SetValue(cycle, Conversions.ConvertDecimalIntToIntArray(setVariableCommand.VariableValue.Value, targetPin.Width));
                    break;
                case EvalCommand evalCommand:
                    if (chip == null)
                        throw new Exception("No chip loaded");
                    
                    chip.Simulate(cycle);
                    break;
                case OutputCommand outputCommand:
                    if (outputList == null)
                        throw new Exception("no output list");
                    if (outputFile == null)
                        throw new Exception("no output file");
                    if (chip == null)
                        throw new Exception("No chip loaded");

                    if (!wroteHeader)
                    {

                        foreach (var o in outputList.OutputSpecs)
                        {
                            var headerText = o.VariableName.PadLeft(o.Length + o.PadLeft);
                            headerText = headerText.PadRight(o.Length + o.PadLeft + o.PadRight);
                            outputFile.Write('|');
                            outputFile.Write(headerText);
                        }

                        outputFile.WriteLine('|');
                        wroteHeader = true;
                    }

                    foreach (var o in outputList.OutputSpecs)
                    {
                        var outputPin = chip.Pins.Single(x => x.Name == o.VariableName);
                        var outputPinValue = outputPin.GetValue(cycle);

                        var convertedOutputValue = o.Format switch
                        {                            
                            ValueFormat.Binary => Conversions.ConvertIntArrayToBinaryString(outputPinValue),
                            _ => throw new Exception($"Unrecognized output format: {o.Format}"),
                        };

                        convertedOutputValue = convertedOutputValue.Substring(convertedOutputValue.Length - o.Length);
                        convertedOutputValue = convertedOutputValue.PadLeft(convertedOutputValue.Length + o.PadLeft);
                        convertedOutputValue = convertedOutputValue.PadRight(convertedOutputValue.Length + o.PadRight);
                        outputFile.Write('|');
                        outputFile.Write(convertedOutputValue);
                    }
                    outputFile.WriteLine('|');

                    outputFile.Flush();
                    chip.Invalidate(cycle);
                    // do the comparison logic later
                    break;
                default:
                    throw new Exception($"Unrecognized command: {currentCommand}");
            }


        }
    }
}
