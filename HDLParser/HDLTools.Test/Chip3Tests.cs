using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HDLTools.Test
{
    public class Chip3Tests
    {
        private readonly ITestOutputHelper testOutput;
        public Chip3Tests(ITestOutputHelper output)
        {
            this.testOutput = output;
        }

        [Theory]
        [InlineData(0, 1)]
        //[InlineData(1, 0)]
        public void Not(int inValue, int outValue)
        {
            var library = new ChipLibrary();
            var notHDL = File.ReadAllText(@"hdl\Not.hdl");
            ChipDescription notDescription = HDLParser.ParseString(notHDL).Single();
            library.Register(notDescription);

            var chip = Chip3.Build(notDescription, library);

            var sim = chip.BuildSimulator();
            var inputPin = sim.Pins.Single(x => x.Name == "in");
            var outputPin = sim.Pins.Single(x => x.Name =="out");

            inputPin.Value = (ushort) inValue;
            sim.Simulate();
            Assert.Equal(outValue, outputPin.Value);


        }

        [Fact]
        public void Mux()
        {
            var library = new ChipLibrary();
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\And.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Or.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Not.hdl")).Single());

            var hdl = File.ReadAllText(@"hdl\Mux.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();

            var chip = Chip3.Build(desc, library);

            var sim = chip.BuildSimulator();
            var aPin = sim.Pins.Single(x => x.Name == "a");
            var bPin = sim.Pins.Single(x => x.Name == "b");
            var selPin = sim.Pins.Single(x => x.Name == "sel");
            var outPin = sim.Pins.Single(x => x.Name == "out");

            aPin.Value = 1;
            bPin.Value = 0;
            selPin.Value = 0;

            sim.Simulate();
            Assert.Equal(1, outPin.Value);

            aPin.Value = 1;
            bPin.Value = 0;
            selPin.Value = 1;

            sim.Simulate();
            Assert.Equal(0, outPin.Value);
        }

        
    }
}
