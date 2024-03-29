﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools.TestScripts
{
    public class TestScriptExecutor
    {
        private IFileSystem fs;
        private ChipLibrary chipLibrary;
        private List<TestScriptCommand> commands;
        private int currentCommandIndex;
        private int cycle;
        private Chip? chip;
        private StreamWriter? outputFile;
        private string[]? compareFileLines;
        private int currentCompareLine;
        private OutputListCommand? outputList;
        private bool wroteHeader;

        public List<ComparisonFailure> ComparisonFailures { get; }
        public bool HasMoreLines => currentCommandIndex < commands.Count - 1;
        public Chip? Chip => chip;

        public TestScriptExecutor(IFileSystem fs, ChipLibrary chipLibrary, List<TestScriptCommand> commands)
        {
            this.fs = fs;
            this.chipLibrary = chipLibrary;
            this.commands = commands;
            this.currentCommandIndex = -1;
            this.cycle = 0;
            this.ComparisonFailures = new List<ComparisonFailure>();
            this.currentCompareLine = 1; // skip the header
        }

        public void Step()
        {
            if (!HasMoreLines)
                return;

            this.currentCommandIndex++;            
            var currentCommand = commands[this.currentCommandIndex];

            switch (currentCommand)
            {
                case LoadCommand loadCommand:
                    var chipDescription = HDLParser.ParseString(File.ReadAllText(Path.Combine("hdl", loadCommand.Filename))).Single();
                    chip = chipLibrary.GetChip(chipDescription.Name, "");
                    break;
                case OutputFileCommand outputFileCommand:
                    outputFile = new StreamWriter(fs.FileStream.Create(outputFileCommand.Filename, FileMode.Create));
                    break;
                case CompareToCommand compareToCommand:
                    compareFileLines = fs.File.ReadAllLines(Path.Combine("cmp",compareToCommand.Filename));
                    break;
                case OutputListCommand outputListCommand:
                    outputList = outputListCommand;
                    break;
                case SetVariableCommand setVariableCommand:
                    if (chip == null)
                        throw new Exception("No chip loaded");

                    var targetPin = chip.Pins.Single(x => x.Name == setVariableCommand.VariableName);
                    targetPin.Init(Conversions.ConvertDecimalIntToIntArray(setVariableCommand.VariableValue.Value, targetPin.Width));
                    break;
                case EvalCommand evalCommand:
                    if (chip == null)
                        throw new Exception("No chip loaded");

                    //chip.InvalidateOutputs(cycle);
                    chip.Evaluate(); 
                    Debug.WriteLine(chip.DumpTree(cycle));
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
                            var headerText = OutputPadding.PadHeader(o);
                            outputFile.Write('|');
                            outputFile.Write(headerText);
                        }

                        outputFile.WriteLine('|');
                        wroteHeader = true;
                    }

                    var builder = new StringBuilder();

                    foreach (var o in outputList.OutputSpecs)
                    {
                        string convertedOutputValue;
                        if (o.VariableName == "time")
                        {
                            var outputSuffix = cycle % 2 == 1 ? "+" : "";
                            convertedOutputValue = $"{cycle / 2}{outputSuffix}";
                            convertedOutputValue = convertedOutputValue.PadRight(o.Length);
                        }
                        else
                        {
                            var outputPin = chip.Pins.Single(x => x.Name == o.VariableName);
                            var outputPinValue = outputPin.GetValue();

                            convertedOutputValue = o.Format switch
                            {
                                ValueFormat.Binary => Conversions.ConvertIntArrayToBinaryString(outputPinValue),
                                ValueFormat.Decimal => Conversions.ConvertIntArrayToDecimalString(outputPinValue),
                                _ => throw new Exception($"Unrecognized output format: {o.Format}"),
                            };

                            if(o.Length < convertedOutputValue.Length)
                                convertedOutputValue = convertedOutputValue.Substring(convertedOutputValue.Length - o.Length);
                        }
                        convertedOutputValue = OutputPadding.PadValue(convertedOutputValue, o);
                        builder.Append('|');
                        builder.Append(convertedOutputValue);
                    }
                    builder.Append('|');
                    var outputLine = builder.ToString();
                    outputFile.WriteLine(outputLine);
                    outputFile.Flush();

                    if (compareFileLines != null)
                    {
                        if (compareFileLines[currentCompareLine] != outputLine)
                        {
                            ComparisonFailures.Add(new ComparisonFailure(compareFileLines[currentCompareLine], outputLine));
                        }

                        currentCompareLine++;
                    }
                    break;
                case TickCommand tickCommand:
                    if (chip == null)
                        throw new Exception("No chip loaded");
                    if (cycle % 2 != 0)
                        throw new Exception($"Current cycle is {cycle}, expected a tock.");

                    //Debug.WriteLine(chip.DumpTree(cycle));
                    chip.Tick();
                    cycle++;
                    //Debug.WriteLine(chip.DumpTree(cycle));
                    break;
                case TockCommand tockCommand:
                    if (chip == null)
                        throw new Exception("No chip loaded");
                    if (cycle % 2 != 1)
                        throw new Exception($"Current cycle is {cycle}, expected a tick.");
                    chip.Tock();
                    cycle++;
                    break;
                default:
                    throw new Exception($"Unrecognized command: {currentCommand}");
            }


        }
    }

    public record ComparisonFailure(string CompareLine, string OutputLine)
    {               
    }

    public class OutputPadding
    {
        public static string PadHeader(OutputSpec o)
        {
            var paddingCount = o.Length + o.PadLeft + o.PadRight - o.VariableName.Length;
            var padRight = Math.Max(o.PadRight, paddingCount - (paddingCount / 2));
            var headerText = o.VariableName.PadRight(o.VariableName.Length + padRight);
            return headerText.PadLeft(o.VariableName.Length + paddingCount);
        }

        public static string PadValue(string value, OutputSpec o)
        {
            value = value.PadLeft(value.Length + o.PadLeft);
            value = value.PadRight(value.Length + o.PadRight);
            return value;
        }
    }
}
