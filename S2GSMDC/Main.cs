using System;
using System.IO;
using System.Text.RegularExpressions;

namespace S2GSMDC
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Either drag and drop a file onto this exe, or use it from the command line with the files to convert as arguments.\nPress any key to exit...");
                Console.Read();
            }
            else
                ParseArgs(args);

            Console.WriteLine("All files converted.\nPress any key to exit.");
            Console.Read();
        }

        //Class to hold which methods an .smd file went though. 
        static class SuccessReport
        {
            public static bool UnderSpace { get; set; }
            public static bool BlenSuccess { get; set; }
            public static bool MesaSuccess { get; set; }
            public static bool BmpSuccess { get; set; }
            public static bool SciNotaSuccess { get; set; }
            public static bool DecimalPointSuccess { get; set; }
            public static bool UnderSpaceSuccess { get; set; }
        }

        //Class to hold all the Regular Expressions (Regex)
        static class ProgramRules
        {
            public static String BlenderRegex = @"\d+(?<vertex> +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+) +\d+ +(?<bone>\d+) +-?\d+\.\d+";
            public static String MesaRegex = @"\d+(?<vertex> +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+) +\d+ +(?<bone>\d+) +-?\d+";
            public static String AllZeros = @"^00+$";
            public static String isSciNota = @"^(?<posORneg>(-|\+)?)(?<num>\d+)(\.|)[e|E]-[0-9]+$";
            public static String isTGA = @"(?<texture>[^\s]+\.)(tga|TGA)+$"; 
            public static String UnderScoreDetect = @"^+-?\d+ \042\w+_\w+\042 +-?\d+$"; //This should detect nodes formatting, but make sure there is an underscore between words. 
            public static bool? ParseUnderscore { get; set; }
        }

        //Seperate function to show some output if multiple files are used.
        static void SuccessMessage()
        {
            Console.WriteLine("Methods tried for conversion.");    
            Console.WriteLine("Blender: {0}", SuccessReport.BlenSuccess);
            Console.WriteLine("TGA to BMP: {0}", SuccessReport.BmpSuccess);
            Console.WriteLine("Maya Mesa: {0}", SuccessReport.MesaSuccess);
            Console.WriteLine("Decimal Coversion: {0}", SuccessReport.DecimalPointSuccess);
            Console.WriteLine("Node underscore to space: {0}", SuccessReport.UnderSpaceSuccess);
            Console.WriteLine("Sci notation Conversion: {0}\n", SuccessReport.SciNotaSuccess);
        }

        //Seperate function to parse lines that contain "0000000" (All Zeros) or "1.e-00500" (Sci Notation)
        static string Refinement(string selection)
        {
            //Some strings from MESA are quirky, below will split 'selection' and address them individually.
            Match match;
            string[] tempArray = selection.Split(' ');

            for (int i = 0; i < tempArray.Length; i++)
            {
                //Some lines have "All Zeros", below will place a decimal after the first 0. 
                //Ex: 0 -7.43246 -47.64437 280.3624 -0.99999 0.00000 0000000 0.94172 0.82541 1 7
                //Outcome Ex: -------------------------------------> 0.000000
                match = Regex.Match(tempArray[i], ProgramRules.AllZeros);
                {
                    if (match.Success)
                    {
                        tempArray[i] = tempArray[i].Insert(1, ".");
                        SuccessReport.DecimalPointSuccess = true;
                    }
                }

                //Some lines have a wierd Scientific Notation format, but they do mean something. Read Ex.
                //Ex:  0 -53.39532 1.e-00500 13.03102 -0.62235 0.000000 0.78273 0.22154 0.76860 1 0 1
                //Outcome Ex: 1e-00500 = 0.000010, -4e-00500 = -0.000040, -2e-00500 = -0.000020, (OR 1.e-00500 = 0.000010, etc)
                //TODO: This does not find Sci Nota values that are double digit, safely AFAIK. ("-11e-00500").
                match = Regex.Match(tempArray[i], ProgramRules.isSciNota);
                {
                    if (match.Success)
                    {
                        tempArray[i] = match.Groups["posORneg"].Value + "0.0000" + match.Groups["num"].Value + "0";
                        SuccessReport.SciNotaSuccess = true;
                    }
                }
            }

            //Combine 'tempArray' and return it back home.
            string refined = string.Join(" ", tempArray);
            return refined;
        }

        //Seperate function for user to select whether they want underscores (_) within node names to be replaced with spaces ( ).
        static void UnderscoreSpaceConmfirm()
        {
            Console.Write("Underscore character found in node names." +
                "\nReplace with spaces?" +
                "\nNOTE: Node mismatch upon model compile may occur if set improperly!" +
                "\nIf you dont wish to continue for now, type quit" +
                "\nContinue with Underscore conversion? Y/N: ");

            string answer = Console.ReadLine().ToLower();                   //Lowercase answer to keep things simple

            if (answer.Equals("y") || answer.Equals("yes"))                 //ID10T proofing and what not.
            {
                ProgramRules.ParseUnderscore = true;
            }
            else if (answer.Equals("n") || answer.Equals("no"))
            {
                ProgramRules.ParseUnderscore = false;
            }
            else if (answer.Equals("q") || answer.Equals("quit"))           //Quit, show them a silly quirk.
            {
                Console.WriteLine("Quitting!" + 
                    "\nDue to how this program is written, there will be a _converted.smd file created with 0kb." +
                    "\nThat can be thrown in the recycle bin.\nPress any key to exit.");
                Console.Read();
                Environment.Exit(0);
            }
            else                                                            //Invalid answer, recursion so we know what to do. 
            {
                Console.WriteLine("Invalid answer. Please select an appropriate answer.\n\n\n");
                UnderscoreSpaceConmfirm();
            }
                
        }

        static void ParseArgs(string[] args)
        { 
            foreach (string file in args)
            {
                //Set ParseUnderscore to false with each file being processed.
                ProgramRules.ParseUnderscore = null;

                Console.WriteLine("Opening \"{0}\" for conversion...\n", file);

                //Reset variables with each file in args.
                bool triangle = false;
                SuccessReport.UnderSpace = false;
                SuccessReport.BlenSuccess = false;
                SuccessReport.MesaSuccess = false;
                SuccessReport.SciNotaSuccess = false;
                SuccessReport.DecimalPointSuccess = false;
                SuccessReport.UnderSpaceSuccess = false;
                Match match;

                using (StreamReader r = new StreamReader(file))
                {
                    String output = file;
                    output = output.Insert(output.IndexOf("."), "_converted");
                    using (StreamWriter w = new StreamWriter(output))
                    {
                        string line; //String hold the current line within "file"
                        
                        while ((line = r.ReadLine()) != null)
                        {
                            try
                            {
                                //Node's may have an underscore instead of spaces, see if user wants to fix.
                                match = Regex.Match(line, ProgramRules.UnderScoreDetect);
                                {
                                    if (match.Success)
                                    {
                                        if (ProgramRules.ParseUnderscore == null)
                                            UnderscoreSpaceConmfirm();

                                        if (ProgramRules.ParseUnderscore == true)
                                        {
                                            line = line.Replace("_", " ");
                                            SuccessReport.UnderSpaceSuccess = true;
                                        }
                                    }
                                }

                                //Dont toucn any lines before "triangles".
                                match = Regex.Match(line, @"(triangles)");
                                {
                                    if (match.Success)
                                        triangle = true;
                                }

                                if (triangle)
                                {
                                    //TGA to BMP
                                    match = Regex.Match(line, ProgramRules.isTGA);
                                    {
                                        if (match.Success)
                                        {
                                            line = match.Groups["texture"] + "bmp";
                                            SuccessReport.BmpSuccess = true;
                                        }
                                    }

                                    //Blender Source Tools 2.4.0 attempt
                                    //Ex: 0 1.887930 -53.549610 328.655060 -0.043270 -0.994390 -0.096420 0.820470 0.854180 1 7 1.000000
                                    match = Regex.Match(line, ProgramRules.BlenderRegex);
                                    {
                                        if (match.Success)
                                        {
                                            line = match.Groups["bone"].Value + match.Groups["vertex"].Value;
                                            SuccessReport.BlenSuccess = true;
                                        }
                                    }

                                    //Maya MESA v2.1 attempt
                                    //Ex: 0 -7.43246 -47.64437 280.3624 -0.99999 0.00000 0000000 0.94172 0.82541 1 7 1
                                    match = Regex.Match(line, ProgramRules.MesaRegex);
                                    {
                                        if (match.Success)
                                        {
                                            line = match.Groups["bone"].Value + match.Groups["vertex"].Value;
                                            SuccessReport.MesaSuccess = true;
                                        }
                                        else
                                        {
                                            line = Refinement(line);
                                            match = Regex.Match(line, ProgramRules.MesaRegex);
                                                if(match.Success)
                                                    line = match.Groups["bone"].Value + match.Groups["vertex"].Value;
                                        }
                                    }
                                }
                                w.WriteLine(line);  //Write the new line. 
                            }
                            //There shouldn't be issues, but if there is catch it so the program doesn't crash.
                            catch (Exception e)
                            {
                                Console.WriteLine(e + "\nUh-oh, something went wrong!\nAt: {0}\nPress any key to exit.", line);
                                Console.Read();
                                Environment.Exit(0);
                            }
                        }
                        w.Close();  //Close StreamWriter
                    }
                    r.Close();      //Close StreamReader.
                }
                SuccessMessage();   //Show some output.
            }
        }
    }
}