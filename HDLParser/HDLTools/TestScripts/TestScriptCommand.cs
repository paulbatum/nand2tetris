using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools.TestScripts
{
    public abstract record TestScriptCommand
    {

    }

    public record LoadCommand(string Filename) : TestScriptCommand;
    public record OutputFileCommand(string Filename) : TestScriptCommand;
    public record CompareToCommand(string Filename) : TestScriptCommand;
    public record OutputListCommand(List<OutputSpec> OutputSpecs) : TestScriptCommand;
    public record SetVariableCommand(string VariableName, VariableValue VariableValue) : TestScriptCommand;
    public record EvalCommand() : TestScriptCommand;
    public record OutputCommand() : TestScriptCommand;

    public enum ValueFormat
    {
        Binary,
        Decimal,
        String,
        Hex
    }
    public record OutputSpec(string VariableName, ValueFormat Format, int PadLeft, int Length, int PadRight);
    public record VariableValue(int Value);
}
