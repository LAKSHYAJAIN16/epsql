using System.Net;
using Epsql.Packages.Default;

#pragma warning disable SYSLIB0014
namespace Epsql.Compiler
{
    public class Compiler
    {
        public static void Compile(string pathToFile, string outputPath)
        {
            //Get the Text
            string[] text = FetchPackages(ReadFile(pathToFile));

            //Declare Output
            string[] output = text;

            //Define Our Variable Dictionary
            Dictionary<string, string> variables = new Dictionary<string, string>();

            //Main Loop
            for (int i = 0; i < text.Length; i++)
            {
                //Split using String.split
                string[] expressions = text[i].Split(' ');

                //Loop through each expression
                for (int j = 0; j < expressions.Length; j++)
                {
                    //If the expression is ^, declare it as a variable
                    if(expressions[j] == "^" && expressions[j + 2] == "=")
                    {
                        //Get the Name of the Variable
                        string name = expressions[j + 1];

                        //Get the Value of the Variable
                        string value = expressions[j + 3];

                        //Store in our Dictionary
                        variables.Add(name, value);

                        Console.WriteLine($"Declared {name} as {value}");

                        //Remove everything from our output, as it is not anything logical
                        output[i] = output[i]
                                            .Replace(name, "")
                                            .Replace(value, "")
                                            .Replace("=", "")
                                            .Replace("^", "");

                        //Continue, as we've already found our target
                        continue;
                    }

                    //If the expression is /*, declare it as a comment and look for the ending Bracket
                    if(expressions[j] == "/*")
                    {
                        //Loop through all remaining expressions and check which one is the ending bracket
                        List<string> commentSections = new List<string>();
                        for (int l = j; l < expressions.Length; l++)
                        {
                            if(expressions[l] == "*/")
                            {
                                commentSections.Add(expressions[l]);
                                break;
                            }
                            else
                            {
                                commentSections.Add(expressions[l]);
                            }
                        }

                        //Delete the Comment
                        foreach (string comment in commentSections)
                        {
                            output[i] = output[i].Replace(comment, "");
                        }

                        //Continue, since we've found our target
                        continue;
                    }

                    //If the Expression is //, just don't do anything and leave it blank
                    if(expressions[j] == "//")
                    {
                        //Empty String
                        output[i] = "";

                        //Continue, since we've found our target
                        continue;
                    }

                    //If the Expression is function, create Function
                    if(expressions[j] == "function" && expressions[j + 2] == "{")
                    {
                       //Get Function name
                       string name = expressions[j + 1];

                       //Loop through all remaining expressions and check which one is the ending bracket
                       List<string> functionStatements = new List<string>();
                       for (int m = i + 1; m < output.Length; m++)
                       {
                           for (int l = j; l < expressions.Length; l++)
                           {
                               if (expressions[l] == "}")
                               {
                                    functionStatements.Add(expressions[l]);

                                    //Remove from output stream
                                    output[m] = output[m].Replace(expressions[l], "");

                                    break;
                               }
                               else
                               {
                                    functionStatements.Add(expressions[l]);

                                    //Remove from output stream
                                    output[m] = output[m].Replace(expressions[l], "");
                                }
                           }
                        }
                    }

                    //Check if we have defined it as a variable
                    if (variables.ContainsKey(expressions[j]))
                    {
                        //Remove from output
                        output[i] = output[i].Replace(expressions[j], variables[expressions[j]]);

                        //Continue, as we've already found our target
                        continue;
                    }
                }
            }

            //Final Check to make sure that no line is blank
            List<string> final_output = new List<string>();
            for (int k = 0; k < output.Length; k++)
            {
                if(string.IsNullOrWhiteSpace(output[k]) || string.IsNullOrEmpty(output[k]))
                {
                   // Woho! Empty line! noice!
                }
                else
                {
                    final_output.Add(output[k]);
                }
            }

            //Now Write that Text in Standard Output
            File.WriteAllLines(outputPath, final_output.ToArray());
        }

        internal static string RetrievePackage(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        internal static string[] FetchPackages(string[] text)
        {
            //Define Output
            List<string> output = new List<string>();

            //Define String Output
            string[] sOutput = text;

            //Main Loop
            for (int i = 0; i < text.Length; i++)
            {
                //Split using String.split
                string[] expressions = text[i].Split(' ');

                for (int j = 0; j < expressions.Length; j++)
                {
                    //If the expression is Import, send HTTP request
                    if(expressions[j] == "import")
                    {
                        //Get the name of the package we're importing.
                        string packageName = expressions[j + 1];

                        //Remove from output
                        sOutput[i] = sOutput[i].Replace(expressions[j], "").Replace(packageName, "");

                        //Retrieve Package
                        string lPackage = RetrievePackage(packageName);

                        //Add it to the output List
                        output.Add(lPackage);
                        continue;
                    }

                    //If the expression is DImport, import local Package
                    if (expressions[j] == "dimport")
                    {
                        //Get the name of the package we're importing.
                        string packageName = expressions[j + 1];

                        //Remove from output
                        sOutput[i] = sOutput[i].Replace(expressions[j], "").Replace(packageName, "");

                        //Retrieve Package
                        string[] lPackage = DefaultPackages.Find(packageName);

                        //Add it to the output List
                        output.AddRange(lPackage);
                        continue;
                    }
                }
            }

            //Convert list to array
            string[] newOutput = output.ToArray();
            return newOutput.Concat(sOutput).ToArray();
        }

        internal static string[] ReadFile(string pathToFile)
        {
            string[] text = File.ReadAllLines(pathToFile);
            return text;
        }
    }

    public class CompilerOptions
    {
        public bool useVariables { get; set; }
        public bool useDoubleSlashComments { get; set; }
        public bool useSlashAsteriksComments { get; set; }
        public bool useImports { get; set; }
        public bool useFunctions { get; set; }

        /// <summary>
        /// Removes basically all Compiler Functionality. Lol, just write SQL then.
        /// </summary>
        public static CompilerOptions None
        {
            get
            {
                return new CompilerOptions()
                {
                    useVariables = false,
                    useDoubleSlashComments = false,
                    useImports = false,
                    useFunctions = false,
                    useSlashAsteriksComments = false,
                };
            }
        }

        /// <summary>
        /// All Compiler Functionality. Everything is EPSQL
        /// </summary>
        public static CompilerOptions Standard
        {
            get
            {
                return new CompilerOptions()
                {
                    useVariables=true,
                    useSlashAsteriksComments=true,
                    useFunctions=true,
                    useImports=true,
                    useDoubleSlashComments=true,
                };
            }
        }
    }
}
