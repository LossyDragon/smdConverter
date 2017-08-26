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
            {
                ParseArgs(args);
            }

            Console.WriteLine("All files converted.\nPress any key to exit.");
            Console.Read();
        }

        //Class to hold which methods an .smd file went though. 
        static class Success
        {
            public static bool BlenSuccess { get; set; }
            public static bool MesaSuccess { get; set; }
            public static bool SciNotaSuccess { get; set; }
            public static bool DecimalPointSuccess { get; set; }
        }
        
        //Class to hold all the Regular Expressions (Regex)
        static class Boolean
        {
            public static String BlenderRegex = @"\d+(?<vertex> +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+) +\d+ +(?<bone>\d+) +-?\d+\.\d+";
            public static String MesaRegex = @"\d+(?<vertex> +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+) +\d+ +(?<bone>\d+) +-?\d+";
            public static String OnlyNumbers = @"^\d+$";
            public static String isSciNota = @"^[+-?\d][e|E][-][0-9]+$";
        }
        
        //Seperate function to show some output if multiple files are used.
        static void SuccessMessage()
        {
            Console.WriteLine("Methods tried for conversion.");
            Console.WriteLine("Blender: {0}", Success.BlenSuccess);
            Console.WriteLine("Maya Mesa: {0}", Success.MesaSuccess);
            Console.WriteLine("Decimal Coversion: {0}", Success.DecimalPointSuccess);
            Console.WriteLine("Sci notation Conversion: {0}\n", Success.SciNotaSuccess);
        }

        static void ParseArgs(string[] argFile)
        {
            foreach (string file in argFile)
            {
                Console.WriteLine("Opening \"{0}\" for conversion...\n", file);

                //Reset variables with each file.
                bool triangle = false;
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
                                //Blender Source Tools 2.4.0 attempt
                                match = Regex.Match(line, Boolean.BlenderRegex);
                                {
                                    if(match.Success)
                                    {
                                        line = match.Groups["bone"].Value + match.Groups["vertex"].Value;
                                        Success.BlenSuccess = true;
                                    }
                                }
                                //Maya MESA v2.1 attempt
                                match = Regex.Match(line, Boolean.MesaRegex);
                                {
                                    if (match.Success)
                                    {
                                        line = match.Groups["bone"].Value + match.Groups["vertex"].Value;
                                        Success.MesaSuccess = true;
                                    }
                                }

                                //Decimal point & Sci Notation modification
                                //Dont toucn any lines before "triangles".
                                match = Regex.Match(line, @"(triangles)");
                                {
                                    if (match.Success)
                                        triangle = true;
                                }

                                /**
                                 * Example: 0 -7.43246 -47.64437 280.3624 -0.99999 0.00000 0000000 0.94172 0.82541 1 7 1
                                 **/
                                if (triangle)                               //TODO: Could this be Regex'ed??
                                {
                                    string[] tempArray = line.Split(' ');   //Split that line into a string array.
                                    string hasDecimal = ".";                //Placeholder

                                    //Parse through that string array we made. 
                                    for (int i = 0; i < tempArray.Length; i++)
                                    {
                                        if (tempArray[i].Length > 6 && !(tempArray[i].Contains(hasDecimal)))
                                        {
                                            match = Regex.Match(tempArray[i], Boolean.OnlyNumbers);
                                            {
                                                if (match.Success)
                                                {
                                                    tempArray[i] = tempArray[i].Insert(1, hasDecimal);
                                                    Success.DecimalPointSuccess = true;
                                                }
                                            }
                                        }
                                    }

                                    //Before we join the string array, lets check for Sci-Notation
                                    //Example:  0 -53.39532 1.e-00500 13.03102 -0.62235 0.000000 0.78273 0.22154 0.76860 1 0 1
                                    //Note:     Based on the control during testing, "1e-00500" is close enough to "0.000000".
                                    //TODO:     Needs more testing with more files.
                                    for (int i = 0; i < tempArray.Length; i++)
                                    {
                                        match = Regex.Match(tempArray[i], Boolean.isSciNota);
                                        {
                                            if (match.Success)
                                            {
                                                tempArray[i] = tempArray[i].Replace(tempArray[i], "0.000000");//Hack
                                                Success.SciNotaSuccess = true;
                                            }
                                        }
                                    }

                                    //Combine that string array into the string streamwriter is working with.
                                    line = string.Join(" ", tempArray);

                                    //Double check the modification to see if it meets spec. 
                                    match = Regex.Match(line, Boolean.MesaRegex);
                                    {
                                        if (match.Success)
                                        {
                                            line = match.Groups["bone"].Value + match.Groups["vertex"].Value;
                                        }
                                    }
                                }
                            }
                            catch (Exception e) //There shouldn't be issues, but if there is, catch so we don't crash
                            {
                                Console.WriteLine(e);
                                Console.WriteLine("At: " + line.ToString());
                                Console.WriteLine("Press any key to exit.");
                                Environment.Exit(0);

                            }
                            w.WriteLine(line);  //Write the new line. 
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