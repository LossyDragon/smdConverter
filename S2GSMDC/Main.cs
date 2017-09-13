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
        static class Success
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
        static class Boolean
        {
            //^[a-zA-Z0-9_]+$
            public static String BlenderRegex = @"\d+(?<vertex> +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+) +\d+ +(?<bone>\d+) +-?\d+\.\d+";
            public static String MesaRegex = @"\d+(?<vertex> +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+) +\d+ +(?<bone>\d+) +-?\d+";
            public static String AllZeros = @"^00+$";
            public static String isSciNota = @"^(?<posORneg>(-|\+)?)(?<num>\d+)(\.|)[e|E]-[0-9]+$"; //Old 2:^+-?\d[e|E]-[0-9]+$ Old:^[+-?\d][e|E][-][0-9]+$
            public static String isTGA = @"(?<texture>[^\s]+\.)(tga|TGA)+$";       
        }

        //Seperate function to show some output if multiple files are used.
        static void SuccessMessage()
        {
            Console.WriteLine("Methods tried for conversion.");
            
            Console.WriteLine("Blender: {0}", Success.BlenSuccess);
            Console.WriteLine("TGA to BMP: {0}", Success.BmpSuccess);
            Console.WriteLine("Maya Mesa: {0}", Success.MesaSuccess);
            Console.WriteLine("Decimal Coversion: {0}", Success.DecimalPointSuccess);
            Console.WriteLine("Node underscore to space: {0}", Success.UnderSpaceSuccess);
            Console.WriteLine("Sci notation Conversion: {0}\n", Success.SciNotaSuccess);
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
                //Outcome Ex:                                        0.000000
                match = Regex.Match(tempArray[i], Boolean.AllZeros);
                {
                    if (match.Success)
                    {
                        tempArray[i] = tempArray[i].Insert(1, ".");
                        Success.DecimalPointSuccess = true;
                    }
                }

                //Some lines have a wierd Scientific Notation format, but they do mean something. Read Ex.
                //Ex:  0 -53.39532 1.e-00500 13.03102 -0.62235 0.000000 0.78273 0.22154 0.76860 1 0 1
                //Outcome Ex: 1e-00500 = 0.000010, -4e-00500 = -0.000040, -2e-00500 = -0.000020, (OR 1.e-00500 = 0.000010, etc)
                //TODO: This does not find Sci Nota values that are double digit ("-11e-00500").
                match = Regex.Match(tempArray[i], Boolean.isSciNota);
                {
                    if (match.Success)
                    {
                        tempArray[i] = match.Groups["posORneg"].Value + "0.0000" + match.Groups["num"].Value + "0"; //h...HACK? :o
                        Success.SciNotaSuccess = true;
                    }
                }
            }

            //Combine 'tempArray' and return it back home.
            string refined = string.Join(" ", tempArray);
            return refined;
        }

        static void ParseArgs(string[] args)
        {
            foreach (string file in args)
            {
                Console.WriteLine("Opening \"{0}\" for conversion...\n", file);

                //Reset variables with each file in args.
                bool triangle = false;
                Success.UnderSpace = false;
                Success.BlenSuccess = false;
                Success.MesaSuccess = false;
                Success.SciNotaSuccess = false;
                Success.DecimalPointSuccess = false;
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
                                match = Regex.Match(line, "\"(?<under>[^\"]*)\""); //Quotes break everything.
                                {
                                    if  (match.Success)
                                    {
                                        line = line.Replace("_", " ");
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
                                    match = Regex.Match(line, Boolean.isTGA);
                                    {
                                        if (match.Success)
                                        {
                                            line = match.Groups["texture"] + "bmp";
                                            Success.BmpSuccess = true;
                                        }
                                    }


                                    //Blender Source Tools 2.4.0 attempt
                                    //Ex: 0 1.887930 -53.549610 328.655060 -0.043270 -0.994390 -0.096420 0.820470 0.854180 1 7 1.000000
                                    match = Regex.Match(line, Boolean.BlenderRegex);
                                    {
                                        if (match.Success)
                                        {
                                            line = match.Groups["bone"].Value + match.Groups["vertex"].Value;
                                            Success.BlenSuccess = true;
                                        }
                                    }

                                    //Maya MESA v2.1 attempt
                                    //Ex: 0 -7.43246 -47.64437 280.3624 -0.99999 0.00000 0000000 0.94172 0.82541 1 7 1
                                    match = Regex.Match(line, Boolean.MesaRegex);
                                    {
                                        if (match.Success)
                                        {
                                            line = match.Groups["bone"].Value + match.Groups["vertex"].Value;
                                            Success.MesaSuccess = true;
                                        }
                                        else
                                        {
                                            line = Refinement(line);

                                            match = Regex.Match(line, Boolean.MesaRegex);
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