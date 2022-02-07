using HDLTools.TestScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HDLTools.Test
{
    public class TestScriptExecutorTests    
    {
        private readonly ITestOutputHelper testOutput;
        public TestScriptExecutorTests(ITestOutputHelper output)
        {
            this.testOutput = output;
        }

        [Fact]
        public void ExecutesSetCommand()
        {
            var library = new ChipLibrary();
            library.LoadAll("hdl");

            var loadCommand = new LoadCommand("Mux16.hdl");
            var setCommand = new SetVariableCommand("a", new VariableValue(16));

            var executor = new TestScriptExecutor(library, new List<TestScriptCommand> { loadCommand, setCommand });

            while (executor.HasMoreLines)
                executor.Step();

            var pin = executor.Chip.Pins.Single(x => x.Name == "a");
            Assert.Equal(Conversions.ConvertDecimalIntToIntArray(setCommand.VariableValue.Value, pin.Width), pin.GetValue(0));
        }
    }
}
