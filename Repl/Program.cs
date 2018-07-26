using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using QDP;
using QDP.AST;
using QDP.Grammar;
using JLisp;
using JLisp.Values;
using System.IO;
using System.Reflection;

namespace Repl
{
    class Program
    {
        static void Main(string[] args)
        {
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var libraryFolder = Path.Combine(folder, "Library");
            var libraryFile = Path.Combine(libraryFolder, "Library.jl");

            var a = Directory.GetCurrentDirectory();

            Env environment = Env.New();

            if(File.Exists(libraryFile))
            {
                Console.WriteLine("Loading Library.jl");
                Console.WriteLine(environment.EvaluateFile(libraryFile));
            }

            if (args.Length == 0)
            {
                // infinite REPL loop
                while (true)
                {
                    Console.Write("JLISP> ");
                    string input = Console.ReadLine();
                    foreach (var v in environment.Evaluate(input))
                    {
                        Console.WriteLine(v);
                    }
                }
            }
            else
            {
                foreach (var arg in args)
                {
                    Console.WriteLine(environment.EvaluateFile(arg));
                }
            }
        }
    }
}
