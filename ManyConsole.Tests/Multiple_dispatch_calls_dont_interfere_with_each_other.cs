﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Options;
using NJasmine;
using NJasmine.Extras;

namespace ManyConsole.Tests
{
    public class Multiple_dispatch_calls_dont_interfere_with_each_other : GivenWhenThenFixture
    {
        public override void Specify()
        {
            when("repeatedly dispatching a command", delegate
            {
                var trace = new StringWriter();

                arrange(delegate
                {
                    ConsoleCommandDispatcher.DispatchCommand(SomeProgram.GetCommands(trace), new[] { "move", "-x", "1", "-y", "2" }, new StringWriter());
                    ConsoleCommandDispatcher.DispatchCommand(SomeProgram.GetCommands(trace), new[] { "move", "-x", "3" }, new StringWriter());
                    ConsoleCommandDispatcher.DispatchCommand(SomeProgram.GetCommands(trace), new[] { "move", "-y", "4" }, new StringWriter());
                    ConsoleCommandDispatcher.DispatchCommand(SomeProgram.GetCommands(trace), new[] { "move" }, new StringWriter());
                });

                then("all parameters are evaluated independently", delegate
                {
                    Expect.That(trace.ToString()).ContainsInOrder(
                            "You walk to 1, 2 and find a maze of twisty little passages, all alike.",
                            "You walk to 3, 0 and find a maze of twisty little passages, all alike.",
                            "You walk to 0, 4 and find a maze of twisty little passages, all alike.",
                            "You walk to 0, 0 and find a maze of twisty little passages, all alike."
                        );
                });
            });
        }

        public class SomeProgram
        {
            public static CoordinateCommand[] GetCommands(StringWriter trace)
            {
                return new[]
            {
                new CoordinateCommand(trace)    
            };
            }
        }

        public class CoordinateCommand : ConsoleCommand
        {
            readonly TextWriter _recorder;

            public CoordinateCommand(TextWriter recorder)
            {
                _recorder = recorder;

                this.IsCommand("move");
                Options = new OptionSet()
                {
                    {"x=", "Coordinate along the x axis.", v => X = int.Parse(v)},
                    {"y=", "Coordinate along the y axis.", v => Y = int.Parse(v)},
                };
            }

            public int X;
            public int Y;

            public override int Run(string[] remainingArguments)
            {
                _recorder.WriteLine("You walk to {0}, {1} and find a maze of twisty little passages, all alike.", X, Y);
                return 0;
            }
        }
    }
}
