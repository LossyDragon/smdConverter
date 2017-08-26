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

            Console.WriteLine("Methods tried for conversion.\n");
            Console.WriteLine("Blender: {0}", Success.BlenSuccess);
            Console.WriteLine("Maya Mesa: {0}", Success.MesaSuccess);
            Console.WriteLine("Decimal Coversion: {0}", Success.DecimalPointSuccess);
            Console.WriteLine("Sci notation Conversion: {0}", Success.SciNoteSuccess);

            Console.WriteLine("All files converted.\nPress any key to exit.");
            Console.Read();
        }

        static class Success
        {
            public static bool BlenSuccess { get; set; }
            public static bool MesaSuccess { get; set; }
            public static bool SciNoteSuccess { get; set; }
            public static bool DecimalPointSuccess { get; set; }
        }

        static void ParseArgs(string[] argFile)
        {

            String BlenderRegex = @"\d+(?<vertex> +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+) +\d+ +(?<bone>\d+) +-?\d+\.\d+";
            String MesaRegex    = @"\d+(?<vertex> +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+) +\d+ +(?<bone>\d+) +-?\d+";
            String OnlyNumbers  = @"^\d+$";
            String isSciNote    = @"^[+-?\d][e|E][-][0-9]+$";

            bool triangle = false;
            Match match;

            foreach (string file in argFile)
            {
                Console.WriteLine("Opening {0} for conversion...", file);

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
                                match = Regex.Match(line, BlenderRegex);
                                {
                                    if(match.Success)
                                    {
                                        line = match.Groups["bone"].Value + match.Groups["vertex"].Value;
                                        Success.BlenSuccess = true;
                                    }

                                }
                                //Maya MESA v2.1 attempt
                                match = Regex.Match(line, MesaRegex);
                                {
                                    if(match.Success)
                                    {
                                        line = match.Groups["bone"].Value + match.Groups["vertex"].Value;
                                        Success.MesaSuccess = true;
                                    }

                                }
                                //Decimal point & Sci Notation modification
                                //TODO: Could this be Regex'ed??
                                /**
                                 *                                                       (Not Decimal)
                                 * Example: 0 -7.43246 -47.64437 280.3624 -0.99999 0.00000 0000000 0.94172 0.82541 1 7 1
                                 * */
                                //Dont touch anything above a line that states "triangles"
                                match = Regex.Match(line, @"(triangles)");
                                {
                                    if (match.Success)
                                        triangle = true;
                                }

                                if(triangle)
                                {
                                    string[] tempArray = line.Split(' ');   //Split that line into a string array.
                                    string hasDecimal = ".";                //Placeholder

                                    //Parse through that string array we made. 
                                    for (int i = 0; i < tempArray.Length; i++)
                                    {
                                        if (tempArray[i].Length > 6 && !(tempArray[i].Contains(hasDecimal)))
                                        {
                                            match = Regex.Match(tempArray[i], OnlyNumbers);
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
                                    //Note:     Based on the control files I'm testing, 1e-00500 is close enough to 0.000000...
                                    //TODO:     This needs SEVERE validation. 
                                    for (int i = 0; i < tempArray.Length; i++)
                                    {
                                        match = Regex.Match(tempArray[i], isSciNote);
                                        {
                                            if (match.Success)
                                            {
                                                tempArray[i] = tempArray[i].Replace(tempArray[i], "0.000000");
                                                Success.SciNoteSuccess = true;
                                            }
                                        }
                                    }

                                    //Combine that string array into the string streamwriter is working with.
                                    line = string.Join(" ", tempArray);

                                    match = Regex.Match(line, MesaRegex);
                                    {
                                        if (match.Success)
                                        {
                                            line = match.Groups["bone"].Value + match.Groups["vertex"].Value;
                                        }
                                    }
                                }

                                //A line couldnt be parsed, was it something uneeded?

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                Console.WriteLine("At: " + line.ToString());
                                Console.WriteLine("Application will now exit after a Key Press");
                                Environment.Exit(0);

                            }
                            w.WriteLine(line);
                        }
                        w.Close();  //Close StreamWriter
                    }
                    r.Close();      //Close StreamReader.
                }
            }
        }
    }
}

//                                                    //Some lines may have Sci Notation on it..... >.<''
//                                                    else
//                                                    {
//                                                        //TODO: Some lines appear to have scientific notation in them... Try and REGEX it if possible.
//                                                        //Example: 0 -53.39532 1.e-00500 13.03102 -0.62235 0.000000 0.78273 0.22154 0.76860 1 0 1
//                                                        //         0  1        2         3         4       5        6       7       8       9 10 11
//                                                        //Coloums C-H should check for regular numbers or Sci Notation after decimal pount.
//
//                                                        if (tempArray.Length == 11)
//                                                        {
//                                                            for (int i = 0; i < tempArray.Length; i++)
//                                                            {
//                                                                Console.WriteLine(tempArray[i]);
//                                                            }
//                                                        }