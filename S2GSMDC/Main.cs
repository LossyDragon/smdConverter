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

            Console.WriteLine("All files converted.\nPress any key to exit.");
            Console.Read();
        }

        static class Success
        {
            public static bool BlenSuccess { get; set; }
            public static bool MesaSuccess { get; set; }
            public static bool DecimalPointSuccess { get; set; }
        }

        static void ParseArgs(string[] argFile)
        {

            String BlenderRegex = @"\d+(?<vertex> +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+) +\d+ +(?<bone>\d+) +-?\d+\.\d+";
            String MesaRegex = @"\d+(?<vertex> +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+) +\d+ +(?<bone>\d+) +-?\d+";
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
                                //Decimal point modification
                                //TODO: Could this be Regex'ed??
                                /**
                                 *                                                       (Not Decimal)
                                 * Example: 0 -7.43246 -47.64437 280.3624 -0.99999 0.00000 0000000 0.94172 0.82541 1 7 1
                                 * */
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
                                        //If array [i] is greater than 6 characters AND NOT containing a decimal, proceed. 
                                        if (tempArray[i].Length > 6 && !(tempArray[i].Contains(hasDecimal)))
                                        {
                                            //Regex bool to make sure we're dealing with ONLY NUMBERS.
                                            bool containsNum = Regex.IsMatch(tempArray[i], @"\d");

                                            //If that line is only numbers, lets put a decimal after the first digit. 
                                            if (containsNum)
                                            {
                                                tempArray[i] = tempArray[i].Insert(1, hasDecimal);

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
                                            Success.DecimalPointSuccess = true;
                                        }
                                    }
                                }

                                //SciNotation bypass.

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