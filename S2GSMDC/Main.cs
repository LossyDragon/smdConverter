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
                Console.WriteLine("Either drag and drop a file onto this exe, or use it from the command line with the files to convert as arguments.\nPress any key to exit.\n");
                Console.Read();
            }
            else
            {

                int successes = 0;
                int failures = 0;
                foreach (string file in args)
                {
                    Console.WriteLine("Opening {0} for conversion...\n", file);

                    using (StreamReader r = new StreamReader(file))
                    {
                        String output = file;
                        output = output.Insert(output.IndexOf("."), "_fixed");
                        using (StreamWriter w = new StreamWriter(output))
                        {
                            string line;
                            while ((line = r.ReadLine()) != null)
                            {
                                Match match = 
                                    Regex.Match(line, @"\d+(?<vertex> +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+) +\d+ +(?<bone>\d+) +-?\d+\.\d+");
                                {
                                    //First check to see if the file format is using Blender Source Tools 2.4.0
                                    if (match.Success)
                                    {
                                        line = match.Groups["bone"].Value + match.Groups["vertex"].Value;
                                        successes++;

                                        //Console.WriteLine("Blender attempt!");

                                    }

                                    /**This could be written better**/
                                    //ELSE we'll try Maya MESA format.
                                    else
                                    {
                                        //Remove the last bit of the regex, as Maya MESA, doesnt put a floating number at the end.
                                        match = 
                                            Regex.Match(line, @"\d+(?<vertex> +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+) +\d+ +(?<bone>\d+) +-?\d+");
                                        {
                                            //If that's correct, convert it. 
                                            if (match.Success)
                                            {
                                                line = match.Groups["bone"].Value + match.Groups["vertex"].Value;
                                                successes++;

                                                //Console.WriteLine("MESA attempt!");

                                            }
                                            //if THAT doesnt work on a line, there's probably a set of numbers without a decimal, shown below. 
                                            /**
                                             * The following else case should fix some lines having no decimal place
                                             * Example: 0 -7.43246 -47.64437 280.3624 -0.99999 0.00000 0000000 0.94172 0.82541 1 7 1
                                             * */
                                            //We'll attempt to find that non decimal number, and fix it to have one. 
                                            else
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
                                                            tempArray[i] = tempArray[i].Insert(1, ".");

                                                        }
                                                    }
                                                }

                                                //Combine that string array into the string streamwriter is working with.
                                                line = string.Join(" ", tempArray);

                                                //Lets double check the coversion now works before writing it.
                                                match = Regex.Match(line, @"\d+(?<vertex> +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+ +-?\d+\.\d+) +\d+ +(?<bone>\d+) +-?\d+");
                                                {
                                                    if (match.Success)
                                                    {
                                                        line = match.Groups["bone"].Value + match.Groups["vertex"].Value;
                                                        successes++;
                                                    }
                                                    //Some lines may have Sci Notation on it..... >.<''
                                                    else
                                                    {
                                                        //TODO: Some lines appear to have scientific notation in them. 
                                                        //Example: 0 -53.39532 1.e-00500 13.03102 -0.62235 0.000000 0.78273 0.22154 0.76860 1 0 1
                                                        //Attempt: A  B        C         D         E       F        G       H       H
                                                        //Coloums C-H should check for regular numbers or Sci Notation after decimal pount.
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                w.WriteLine(line);  //Write the converted line to new file.
                            }
                            w.Close(); //Close streamwriter
                        }
                        Console.WriteLine("The converted file is located at {0}\n", output);
                    }
                }

                Console.WriteLine("All files converted.\nPress any key to exit.");
                Console.WriteLine(successes + " successes.");
                Console.WriteLine(failures + " failures.");
                Console.Read();
            }
        }
    }
}