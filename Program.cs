using Microsoft.Diagnostics.Runtime.Utilities;
using System;
using System.IO;
using System.Linq;

namespace SymbolChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: <default-symbol-path> <assembly-to-check.dll> ...");
                Environment.Exit(1);
            }
            if (args.Length == 0)
            {
                Console.WriteLine("Please pass individual .dlls.");
                Environment.Exit(1);
            }

            DefaultSymbolLocator sym = new DefaultSymbolLocator();
            sym.SymbolPath = args[0];

            try
            {
                foreach (string file in args.Skip(1))
                {
                    if (!File.Exists(file))
                    {
                        Console.WriteLine($"File not found: {file}");
                        continue;
                    }

                    using (FileStream fs = File.OpenRead(file))
                    {
                        PEImage img = new PEImage(fs);

                        string localImg = sym.FindBinary(Path.GetFileName(file), img.IndexTimeStamp, img.IndexFileSize, checkProperties: false);
                        if (string.IsNullOrWhiteSpace(localImg))
                            Console.WriteLine($"Failed to find PEImage: {file}");
                        else
                            Console.WriteLine($"Found PEImage: {file}");

                        string localPdb = sym.FindPdb(img.Pdbs.Single());
                        if (string.IsNullOrWhiteSpace(localPdb))
                            Console.WriteLine($"Failed to find PDB: {file} guid:{img.DefaultPdb.Guid} rev:{img.DefaultPdb.Revision}");
                        else
                            Console.WriteLine($"Found PDB: {file} guid:{img.DefaultPdb.Guid} rev:{img.DefaultPdb.Revision}");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:");
                Console.WriteLine(ex);
            }
        }
    }
}
