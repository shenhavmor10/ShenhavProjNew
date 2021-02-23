using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using ClassesSolution;
using System;
using System.Data;
using System.Linq;

namespace testServer2
{

    class GeneralRestApiServerMethods
    {
        //Patterns Declaration.
        const int NOT_FOUND_STRING = -1;
        static Regex functionPatternInH = new Regex(@"^[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]\;$");
        static Regex OpenBlockPattern = new Regex(@".*{.*");
        static Regex CloseBlockPattern = new Regex(@".*}.*");
        static Regex FunctionPatternInC = new Regex(@"^([^ ]+\s)?[^ ]+\s(.*\s)?[^ ]+\([^()]*\)$");
        const string ReturnPattern = @"(\s)+?return(\s)?[^\s]+;";
        const string patternFilePath = @"..\..\..\Patterns.txt";

        /// Function - FunctionCode
        /// <summary>
        /// function gets a buffer and a refference to the code line and returns all the function code.
        /// in the scope.
        /// </summary>
        /// <param name="sr"> buffer type MyStream.</param>
        /// <param name="codeLine"> code line type string.</param>
        /// <returns> returns the whole function code. </returns>
        public static string FunctionCode(MyStream sr, ref string codeLine)
        {
            uint curPos = sr.Pos;
            int functionLength = 0;
            string finalCode = GeneralConsts.EMPTY_STRING;
            Stack myStack = new Stack();
            codeLine = sr.ReadLine();
            myStack.Push(codeLine);
            while ((codeLine != null && myStack.Count > 0))
            {
                codeLine = sr.ReadLine();
                finalCode += codeLine + GeneralConsts.NEW_LINE;
                functionLength++;
                if (OpenBlockPattern.IsMatch(codeLine))
                {
                    myStack.Push(codeLine);
                }

                if (CloseBlockPattern.IsMatch(codeLine))
                {
                    myStack.Pop();
                }
            }
            myStack.Clear();
            return finalCode;
        }
        /// Function - FunctionLength
        /// <summary>
        /// gets the function line and buffer returns the function length.
        /// </summary>
        /// <param name="sr"> Buffer type MyStream.</param>
        /// <param name="codeLine"> Code line type string</param>
        /// <returns> returns the function length type int.</returns>
        public static int FunctionLength(MyStream sr, string codeLine)
        {
            int count = 0;
            uint curPos = sr.Pos;
            Stack myStack = new Stack();
            codeLine = sr.ReadLine();
            myStack.Push(codeLine);
            bool found = false;
            while ((codeLine != null && myStack.Count > 0))
            {
                count++;
                codeLine = sr.ReadLine();
                if (codeLine.IndexOf("{") != NOT_FOUND_STRING)
                {
                    myStack.Push(codeLine);
                }
                if (codeLine.IndexOf("}") != NOT_FOUND_STRING)
                {
                    myStack.Pop();
                }

            }
            if (myStack.Count == 0)
            {
                found = true;
            }
            count = count - 1;
            myStack.Clear();
            //returns the buffer to the start of the function.
            sr.Seek(curPos);
            return count;

        }
        /// Function - findFunction
        /// <summary>
        /// find the next function in the code and returns the function line.
        /// </summary>
        /// <param name="sr"> Buffer type MyStream.</param>
        /// <param name="pattern"> Regex Pattern for the function.</param>
        /// <returns></returns>
        public static string findFunction(MyStream sr, Regex pattern)
        {
            string codeLine = sr.ReadLine();
            while ((!pattern.IsMatch(codeLine)) && ((codeLine = sr.ReadLine()) != null)) ;
            return codeLine;
        }
        /// Function - findAllFunctionNames
        /// <summary>
        /// find all the function names in the code.
        /// </summary>
        /// <param name="path"> Path for the code.</param>
        /// <param name="pattern"> Pattern for the function.</param>
        public static void findAllFunctionNames(string path, Regex pattern)
        {
            string codeLine = GeneralConsts.EMPTY_STRING;
            MyStream sr = new MyStream(path, System.Text.Encoding.UTF8);
            while (codeLine != null)
            {
                codeLine = findFunction(sr, pattern);
                //enter function to where i store it.
                //add it to where i store the function code.
            }
            sr.Close();
        }
        /// Function - takeSecondNotNullString
        /// <summary>
        /// take the second not null string in the string array.
        /// </summary>
        /// <param name="str"> array after split type string array.</param>
        /// <returns> returns the string that is in the second place that isnt null in the array . type string.</returns>
        static string takeSecondNotNullString(string[] str)
        {
            int i;
            string result = GeneralConsts.EMPTY_STRING;
            bool endLoop = false;
            int count = 0;
            for (i = 0; i < str.Length&&!endLoop; i++)
            {
                if (str[i] != GeneralConsts.EMPTY_STRING && str[i] != GeneralConsts.SPACEBAR)
                {
                    count++;
                }
                if (count == 2)
                {
                    result = str[i];
                    endLoop = true; ;
                }
            }
            return result;
        }
        /// Function - FindParameters
        /// <summary>
        /// get a string of a function and returns all the parameters of the function in a parameter type array.
        /// </summary>
        /// <param name="codeLine"> function line type string.</param>
        /// <returns> returns all of the parameters in an array type ParemetrsType.</returns>
        public static ParametersType[] FindParameters(string codeLine)
        {
            string[] tempSplit;
            string[] finalSplit;
            string tempSplit2;
            string finalType;
            int i;
            tempSplit = Regex.Split(codeLine, @"\(");
            tempSplit2 = tempSplit[1];
            tempSplit = Regex.Split(tempSplit2, @"\,|\)");
            ParametersType[] finalParameters = new ParametersType[tempSplit.Length - 1];
            char[] charsToTrim = { '*', '&' };
            if (tempSplit2.Length > 2)
            {
                for (i = 0; i < tempSplit.Length - 1; i++)
                {
                    tempSplit2 = tempSplit[i];
                    if (tempSplit2.IndexOf("*") != NOT_FOUND_STRING)
                    {
                        finalSplit = Regex.Split(tempSplit2, @"\*");
                    }
                    else
                    {
                        finalSplit = Regex.Split(tempSplit2, @"\s");

                    }

                    if (finalSplit.Length == 1)
                    {
                        tempSplit2 = finalSplit[0];
                    }
                    else
                    {
                        tempSplit2 = takeSecondNotNullString(finalSplit);
                    }
                    if (tempSplit2.IndexOf("&") != NOT_FOUND_STRING || tempSplit2.IndexOf("*") != NOT_FOUND_STRING)
                    {
                        tempSplit2 = tempSplit2.Trim(charsToTrim);
                    }
                    //trimEnd
                    tempSplit[i] = tempSplit[i].Substring(0, tempSplit[i].Length - (tempSplit2.Length));
                    finalType = tempSplit[i].Replace(GeneralConsts.SPACEBAR, GeneralConsts.EMPTY_STRING);
                    tempSplit2 = tempSplit2.Replace(GeneralConsts.SPACEBAR, GeneralConsts.EMPTY_STRING);
                    finalParameters[i] = new ParametersType(tempSplit2, finalType);

                }
            }
            else
            {
                finalParameters = new ParametersType[0];
            }
            return finalParameters;
        }
        /// Function - FindDocumentation
        /// <summary>
        /// Finds the documentation of a function.
        /// </summary>
        /// <param name="sr"> Buffer type MyStream.</param>
        /// <param name="documentation"> Position of the first documentation line type uint.</param>
        /// <param name="firstLineDocumentation"> First documentation line type string.</param>
        /// <param name="functionPos"> Position of the function type uint.</param>
        /// <returns> returns the documentation of the function included.</returns>
        public static string FindDocumentation(MyStream sr, uint documentation, string firstLineDocumentation, uint functionPos)
        {
            string documetationString = firstLineDocumentation + GeneralConsts.NEW_LINE;
            sr.Seek(documentation);
            string codeLine = sr.ReadLine();
            documetationString += codeLine + GeneralConsts.NEW_LINE;
            if (!(firstLineDocumentation.IndexOf("//") != NOT_FOUND_STRING) && !(firstLineDocumentation.IndexOf("/*") != NOT_FOUND_STRING))
            {
                documetationString = GeneralConsts.EMPTY_STRING;
            }
            if ((firstLineDocumentation.IndexOf("/*") != NOT_FOUND_STRING))
            {
                while (!(codeLine.IndexOf("*/") != NOT_FOUND_STRING))
                {
                    codeLine = sr.ReadLine();
                    documetationString += codeLine + GeneralConsts.NEW_LINE;
                }

            }
            sr.Seek(functionPos);
            return documetationString;

        }
        /// Function - CreateFunctionsJsonFile
        /// <summary>
        /// create a json file for functions.
        /// </summary>
        /// <param name="path"> path of the code.</param>
        /// <param name="pattern"> function pattern type string</param>
        /// <returns> return a json for the functions get in "SyncServer".</returns>
        public static void CreateFinalJson(string filePath,Hashtable includes,ArrayList globalVariables,Dictionary<string,ArrayList>variables,Dictionary<string,string>defines, Dictionary<string, Dictionary<string, Dictionary<string, Object>>> final_json,string eVars,string typeEnding,Hashtable memoryHandleFuncs,Dictionary<string,ArrayList>calledFromFunc, Dictionary<string, Dictionary<string,string []>> callsFromThisFunction, Dictionary<string,string>functionContent,string codeContent)
        {
            //if its h type file.
            if(typeEnding=="h")
            {
                CreateFunctionsJsonFile(filePath, functionPatternInH, typeEnding, final_json,eVars);
            }
            //if its a c type file for now.
            else 
            {
                CreateFunctionsJsonFile(filePath, FunctionPatternInC, typeEnding, final_json,eVars,variables,memoryHandleFuncs,calledFromFunc,callsFromThisFunction,functionContent);
            }
            //for both files
            CreateCodeJsonFile(filePath,includes,globalVariables,defines,final_json,eVars,codeContent);
        }
        /// Function - FindVariables
        /// <summary>
        /// Gets an arrayList and creates "ParametersType" array and attach all values of the arrayList to it (converts it from ArrayList to
        /// ParametersType Array).
        /// </summary>
        /// <param name="variables"> arrayList type "ParametersType".</param>
        /// <returns> array type "ParametersType".</returns>
        public static ParametersType[] FindVariables(ArrayList variables)
        {
            ParametersType[] parameterTypeVariables = new ParametersType[variables.Count];
            for(int i=0;i<variables.Count;i++)
            {
                parameterTypeVariables[i] = (ParametersType)variables[i];
            }
            return parameterTypeVariables;
        }
        /// Function - FindNameFromCodeLine
        /// <summary>
        /// Gets a code line of a function decleration and returns the name of the function.
        /// </summary>
        /// <param name="codeLine"> The code line of the function declaration</param>
        /// <returns></returns>
        public static string FindNameFromCodeLine(string codeLine)
        {
            codeLine = codeLine.Split('(')[0];
            codeLine = codeLine.Substring(codeLine.LastIndexOf(" "));
            codeLine = codeLine.Trim();
            return codeLine;
        }
        static string CreatePatternForFunction(ParametersType [] parameters,string functionName,string returnType)
        {
            string pattern = @"([^\s]+)?" +returnType+@"[\s*]+"+functionName + @"[\s(]+";
            for(int i=0;i<parameters.Length;i++)
            {
                if(parameters.Length>i+1)
                {
                    pattern += parameters[i].parameterType + @"[\s*]+" + parameters[i].parameterName + @"[,\s]+";
                }
                else
                {
                    pattern += parameters[i].parameterType + @"[\s*]+" + parameters[i].parameterName;
                }
                
            }
            pattern += @"[)\s]+$";
            pattern = pattern.Replace("*", @"\*");
            return pattern;
        }
        /// Function - TakeExitPoints
        /// <summary>
        /// Searches for all exit points in the code and returns them in an array string.
        /// </summary>
        /// <param name="content"> The whole function content.</param>
        /// <returns> An array type string that contains the whole exit points code lines.</returns>
        static string [] FindPatternInCode(string content,string regexPattern)
        {
            MatchCollection m = Regex.Matches(content, regexPattern);
            string[] result = new string[m.Count];
            for(int i=0;i<result.Length;i++)
            {
                result[i] = m[i].ToString();
                result[i] = result[i].Trim();
            }
            return result;
        }
        /// Fubnction - CreateFunctionsJsonFile
        /// <summary>
        /// function is creating the json file for the "Function" GET request.
        /// </summary>
        /// <param name="path"> path of the file that is being checked.</param>
        /// <param name="pattern"></param>
        /// <param name="variables"> Dictionary that every key on him is the function LINE and every value is an arrayList
        /// type "ParameterType" of all of his variables.
        /// </param>
        /// <param name="final_json"> the final big json.</param>
        static void CreateFunctionsJsonFile(string path, Regex pattern, string typeEnding, Dictionary<string, Dictionary<string, Dictionary<string, Object>>> final_json, string eVars, Dictionary<string,ArrayList> variables=null,Hashtable memoryHandleFuncs=null,Dictionary<string,ArrayList>calledFromFunc=null, Dictionary<string, Dictionary<string, string[]>> callsFromThisFunction = null, Dictionary<string,string>functionsContent=null)
        {
            string codeLine = GeneralConsts.EMPTY_STRING;
            string fName;
            string[] temp;
            string returnType = GeneralConsts.EMPTY_STRING;
            bool exitFlag = false;
            string firstLineDocumentation = GeneralConsts.EMPTY_STRING;
            uint curPos;
            Object tempDict = new Dictionary<string, FunctionInfoJson>();
            string[] allExitPoints;
            MyStream sr = new MyStream(path, System.Text.Encoding.UTF8);
            uint documentPos = sr.Pos;
            while (codeLine != null)
            {
                codeLine = sr.ReadLine();
                if (codeLine == null)
                    exitFlag = true;
                //saves the last documentation.
                while (!exitFlag && !pattern.IsMatch(codeLine))
                {

                    firstLineDocumentation = GeneralConsts.EMPTY_STRING;
                    if(codeLine!=null)
                    {
                        if (codeLine.IndexOf("//") != NOT_FOUND_STRING)
                        {
                            documentPos = sr.Pos;
                            firstLineDocumentation = codeLine;
                        }
                        while ((codeLine.IndexOf("//") != NOT_FOUND_STRING))
                        {
                            if (codeLine != null)
                                codeLine = sr.ReadLine();
                        }
                        if ((codeLine.IndexOf("/*") != NOT_FOUND_STRING))
                        {
                            documentPos = sr.Pos;
                            firstLineDocumentation = codeLine;
                            while (!(codeLine.IndexOf("*/") != NOT_FOUND_STRING))
                            {
                                if (codeLine != null)
                                    codeLine = sr.ReadLine();
                            }
                            if ((codeLine.IndexOf("*/") != NOT_FOUND_STRING))
                            {
                                if (codeLine != null)
                                    codeLine = sr.ReadLine();
                            }
                        }
                    }
                    if (codeLine != null&&!pattern.IsMatch(codeLine))
                    {
                        codeLine = sr.ReadLine();
                    }
                    if(codeLine==null)
                    {
                        exitFlag = true;
                    }
                }
                if (!exitFlag)
                {
                    fName = codeLine;
                    if (fName != null)
                    {
                        temp = Regex.Split(fName, @"\*|\s");
                        if (fName.IndexOf("static") != NOT_FOUND_STRING)
                        {
                            returnType = takeSecondNotNullString(temp);
                        }
                        else
                        {
                            returnType = temp[0];
                        }

                        returnType = returnType.Trim();
                        //enter function to where i store it. 
                        Object tempStorage = new FunctionInfoJson();
                        //if its a c code than it has those extra information that only c has.
                        if(typeEnding=="c")
                        {
                            //goes to the next scope.
                            GeneralCompilerFunctions.NextScopeLength(sr, ref codeLine, ref ((FunctionInfoJson)tempStorage).codeLength, true);
                            //gets the function code.
                            if(functionsContent.ContainsKey(fName))
                            {
                                ((FunctionInfoJson)tempStorage).content = functionsContent[fName];
                            }
                            //gets the variables of the function
                            ((FunctionInfoJson)tempStorage).variables = FindVariables(variables[fName]);
                            //gets the exit points.
                            ((FunctionInfoJson)tempStorage).allExitPoints = FindPatternInCode(((FunctionInfoJson)tempStorage).content,ReturnPattern);
                            //gets the amount of the exit points.
                            ((FunctionInfoJson)tempStorage).exitPointsAmount = ((FunctionInfoJson)tempStorage).allExitPoints.Length;
                            if(memoryHandleFuncs.ContainsKey(GeneralCompilerFunctions.CreateMD5(fName)))
                            {
                                string tempIfCheck = (string)memoryHandleFuncs[GeneralCompilerFunctions.CreateMD5(fName)];
                                if (tempIfCheck==GeneralConsts.MEMORY_MANAGEMENT)
                                {
                                    ((FunctionInfoJson)tempStorage).memoryAllocation = true;
                                    ((FunctionInfoJson)tempStorage).memoryRelease = true;
                                }
                                else if(tempIfCheck== GeneralConsts.MEMORY_ALLOCATION)
                                {
                                    ((FunctionInfoJson)tempStorage).memoryAllocation = true;
                                }
                                else
                                {
                                    ((FunctionInfoJson)tempStorage).memoryRelease = true;
                                }
                            }
                        }
                        //this one is for all files.
                        ((FunctionInfoJson)tempStorage).parameters = FindParameters(fName);
                        if (typeEnding == "c")
                        {
                            string funcKeyInDict = FindNameFromCodeLine(fName);
                            funcKeyInDict += "(";
                            for (int i = 0; i < ((FunctionInfoJson)tempStorage).parameters.Length - 1; i++)
                            {
                                funcKeyInDict += (((FunctionInfoJson)tempStorage).parameters[i]).parameterType + ",";
                            }
                            funcKeyInDict += (((FunctionInfoJson)tempStorage).parameters[((FunctionInfoJson)tempStorage).parameters.Length - 1]).parameterType + ")";
                            ((FunctionInfoJson)tempStorage).calledFromFunc = (string[])calledFromFunc[funcKeyInDict].ToArray(typeof(string));
                            ((FunctionInfoJson)tempStorage).callsFromThisFunction=callsFromThisFunction[fName];
                        }
                        //those are for all files.
                        ((FunctionInfoJson)tempStorage).returnType = returnType;
                        curPos = sr.Pos;
                        ((FunctionInfoJson)tempStorage).documentation = FindDocumentation(sr, documentPos, firstLineDocumentation, curPos);
                        ((FunctionInfoJson)tempStorage).fName = FindNameFromCodeLine(fName);
                        ((FunctionInfoJson)tempStorage).pattern = CreatePatternForFunction(((FunctionInfoJson)tempStorage).parameters, ((FunctionInfoJson)tempStorage).fName, ((FunctionInfoJson)tempStorage).returnType);
                        if (!((Dictionary<string, FunctionInfoJson>)tempDict).ContainsKey(fName))
                        {
                            ((Dictionary<string, FunctionInfoJson>)tempDict).Add(fName, (FunctionInfoJson)tempStorage);
                        }
                       
                    }
                    else
                    {
                        exitFlag = true;
                    }

                }
                //add it to where i store the function code.
            }
            //Serialize.
            final_json[path][eVars].Add("function", tempDict);
            sr.Close();
        }
        /// Function - CreateCodeJsonFile
        /// <summary>
        /// Creates a Json file for the Code get.
        /// </summary>
        /// <param name="includes"> Hashtable includes for all of the includes in the code.</param>
        /// <param name="defines"> Dictionary of defines that has all defines in the code. 
        ///                        (Including all imports defines.)</param>
        /// <returns> returns a json file type string.</returns>
        static void CreateCodeJsonFile(string path,Hashtable includes,ArrayList globalVariables,Dictionary<string,string>defines, Dictionary<string, Dictionary<string, Dictionary<string, Object>>> final_json, string eVars,string codeContent)
        {
            CodeInfoJson code=new CodeInfoJson();
            code.includes = new string[includes.Values.Count];
            includes.Values.CopyTo(code.includes, 0);
            code.includesAmount = includes.Values.Count;
            code.defines = defines;
            code.definesAmount = defines.Count;
            code.Globalvariables = FindVariables(globalVariables);
            code.codeContent = codeContent;
            MyStream sr = null;
            try
            {
                sr = new MyStream(path, System.Text.Encoding.UTF8);
            }
            catch (Exception e)
            {
                MainProgram.AddToLogString(path, "Error open file  " + path);
            }
            code.rawCodeContent=sr.ReadToEnd();
            //Serialize.
            final_json[path][eVars].Add("codeInfo",code);
        }
        /// Function - ReadAllScope
        /// <summary>
        /// reads all of the scope that the code line is in.
        /// </summary>
        /// <param name="sr"> buffer type MyStream.</param>
        /// <param name="pos"> position of the line type uint.</param>
        /// <param name="line"> code line type string.</param>
        /// <returns> the string of all the scope.</returns>
        public static string ReadAllScope(MyStream sr,uint pos,string line)
        {
            string result = "";
            if (pos==0)
            {
                result = line;
            }
            else
            {
                sr.Seek(pos);
                result = line+GeneralConsts.NEW_LINE+FunctionCode(sr, ref line);
            }
            return result;
        }
        /// Function - findOpenScopeIndex
        /// <summary>
        /// finding the open scope of the index given.
        /// </summary>
        /// <param name="content"> the content that it is searching on.</param>
        /// <param name="matchIndex"> the index of the code you want to check what is it scope</param>
        /// <returns> returns the index of the open block of the scope.</returns>
        static int findOpenScopeIndex(string content, int matchIndex)
        {
            //each open block decrease the counter by one and each close block increase the counter by one.
            int counter = 1;
            int tempOpenBlock = 0, tempCloseBlock = 0;
            int newOpenIndex = matchIndex, newCloseIndex = matchIndex;
            while (counter != 0)
            {
                tempOpenBlock = content.LastIndexOf("{", newOpenIndex);
                tempCloseBlock = content.LastIndexOf("}", newCloseIndex);
                if (tempCloseBlock > tempOpenBlock)
                {
                    counter++;
                    newCloseIndex = tempCloseBlock - 1;
                }
                else
                {
                    counter--;
                    newOpenIndex = tempOpenBlock - 1;
                }
            }
            return (tempOpenBlock > tempCloseBlock) ? tempOpenBlock : tempCloseBlock;

        }
        /// Function - findCloseScopeIndex
        /// <summary>
        /// finding the close scope of the index given.
        /// </summary>
        /// <param name="content"> the content that it is searching on.</param>
        /// <param name="matchIndex"> the index of the code you want to check what is it scope</param>
        /// <returns> returns the index of the close block of the scope.</returns>
        static int findCloseScopeIndex(string content, int matchIndex)
        {
            //each open block decrease the counter by one and each close block increase the counter by one.
            int counter = 1;
            int tempOpenBlock = 0, tempCloseBlock = 0;
            int newOpenIndex = matchIndex, newCloseIndex = matchIndex;
            while (counter != 0)
            {
                tempOpenBlock = content.IndexOf("{", newOpenIndex);
                tempCloseBlock = content.IndexOf("}", newCloseIndex);
                if (tempCloseBlock < tempOpenBlock || tempOpenBlock == -1)
                {
                    counter--;
                    newCloseIndex = tempCloseBlock + 1;
                }
                else
                {
                    counter++;
                    newOpenIndex = tempOpenBlock + 1;
                }
            }
            return (tempOpenBlock < tempCloseBlock) ? tempCloseBlock : tempOpenBlock;
        }
        /// Function - makeScopeMatch
        /// <summary>
        /// making the actuall scope (string) and returning it.
        /// </summary>
        /// <param name="content"> the code where you search the code on.</param>
        /// <param name="match"> the actually match it found when searching for a pattern.</param>
        /// <param name="matchIndex"> the index of the match.</param>
        /// <returns></returns>
        static string makeScopeMatch(string content, string match, int matchIndex)
        {
            //finding open scope.
            int openScope = findOpenScopeIndex(content, matchIndex);
            //finding close scope.
            int closeScope = findCloseScopeIndex(content, matchIndex + match.Length);
            //make the scope (string).
            string resultString = content.Substring(openScope + 1, closeScope -openScope);
            return resultString;

        }
        /// Function - SearchPattern
        /// <summary>
        /// this function gets a pattern and a content and returns in the wanted return size - (scope,line,model).
        /// </summary>
        /// <param name="pattern"> the pattern that is being checked.</param>
        /// <param name="returnSize"> the return size wanted as explained before.</param>
        /// <param name="content"> the content you are checking on.</param>
        /// <returns></returns>
        public static string [] SearchPattern(string pattern,string returnSize,string content)
        {
            ArrayList results = new ArrayList();
            var m1 = Regex.Matches(content, pattern);
            string tempNewMatch;
            if(m1.Count>0)
            {
                if (returnSize == "model")
                {
                    results.Add(content);
                }
                else if(returnSize=="line")
                {
                    foreach(Match match in m1)
                    {
                        results.Add(match.Value);
                    }
                }
                else if(returnSize=="scope")
                {
                    foreach (Match match in m1)
                    {

                        tempNewMatch = makeScopeMatch(content, match.Value, match.Index);
                        results.Add(tempNewMatch);
                    }
                }

            }
            string[] finalResult = (string[])results.ToArray(typeof(string));
            return finalResult;
            
        }
        /// Function - TakePatternFromFile
        /// <summary>
        /// Search for the pattern in the patterns file and returns it.
        /// </summary>
        /// <param name="pattern"> name of the pattern the tool requested.</param>
        /// <returns> pattern type string.</returns>
        public static string TakePatternFromFile(string pattern)
        {
            bool found = false;
            MyStream sr = new MyStream(patternFilePath, System.Text.Encoding.UTF8);
            string line;
            string final_pattern=GeneralConsts.EMPTY_STRING;
            string[] split = { ",,," };
            while((line=sr.ReadLine())!=null&&!found)
            {
                if(line.Split(split, System.StringSplitOptions.None)[0]==pattern)
                {
                    final_pattern = line.Split(split, System.StringSplitOptions.None)[1];
                    found = true;
                }

            }
            return final_pattern;
        }
        /// Function - CutOnlyFirstFromLine
        /// <summary>
        /// cut the first line from an equal section. the section before the '='.
        /// </summary>
        /// <param name="codeLine"> the code line.</param>
        /// <returns> returns the first section.</returns>
        public static string CutOnlyFirstFromLine(string codeLine)
        {
            string result = codeLine.Split('=')[0];
            result = result.Trim();
            try
            {
                result = result.Substring(result.LastIndexOf(' '));
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            char[] trimChars = { ' ', '(', ')', ';', '\t', '*' };
            result = result.Trim(trimChars);
            return result;
        }
        /// Function - CutOnlyLastFromLine
        /// <summary>
        /// cut the last line from an equal section. the section after the '='
        /// </summary>
        /// <param name="codeLine"></param>
        /// <returns> returns the last part of the section.</returns>
        public static string CutOnlyLastFromLine(string codeLine)
        {
            string result = codeLine.Split('=')[1];
            char[] trimChars = { ' ', '(', ')',';','\t','*' };
            result = result.Trim(trimChars);
            return result;
        }
        /// Function - GetEqualVariables
        /// <summary>
        /// gets a json and the name of the parameter and returns the whole variables who are equal to the
        /// paramater given. (memory equal).
        /// </summary>
        /// <param name="json"> the json of the function.</param>
        /// <param name="parameterName">the name of the parameter.</param>
        /// <returns></returns>
        public static string [] GetEqualVariables(FunctionInfoJson json,string parameterName)
        {
            ArrayList result = new ArrayList();
            result.Add(parameterName);
            string pattern = @"([^\s()]+\s)?((\*)*(\s))?[^\s()]+(\s)?=(\s)?[A-Za-z][^\s()]*;";
            string [] resultsEqualParams=SearchPattern(pattern, "line", json.content);
            for(int i=0;i<resultsEqualParams.Length;i++)
            {
                if(result.Contains(CutOnlyLastFromLine(resultsEqualParams[i])))
                {
                    result.Add(CutOnlyFirstFromLine(resultsEqualParams[i].Trim()));
                }
            }
            return (string[])result.ToArray(typeof(string));
        }

    }
}
