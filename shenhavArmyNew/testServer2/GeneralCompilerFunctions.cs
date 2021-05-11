using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using ClassesSolution;
using Server;
using System.Globalization;

namespace testServer2
{
    public static class GeneralCompilerFunctions
    {
        const string FILE_NOT_FOUND = "No such file found.";
        const int IFDEF = 0;
        const int IFNDEF = 1;
        //All Patterns That is being searched in the code.
        static Regex OpenBlockPattern = new Regex(@".*{.*");
        static Regex CloseBlockPattern = new Regex(@".*}.*");
        static Regex functionPatternInH = new Regex(@"^[^\s().+\/\,=]+((\*)*(\s))[^\s().+\/\,=]+.*\(.*\)\;$");
        static Regex staticFunctionPatternInC = new Regex(@"^.*static.*\s.*[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]$");
        static Regex FunctionPatternInC = new Regex(@"^([^ ]+\s)?[^# ]+\s(.*\s)?[^ ]+\([^()]*\)$");
        static Regex CallFunction = new Regex(@"[^ ]+\([^()]*\);$");
        static Regex StructPattern = new Regex(@"^([^\s\/\*()]+)?(\s)?struct(([^;]*$)|(\s(.+{$|.*{$|[^\s;]+$)))");
        static Regex EnumPattern = new Regex(@"^([^\s\/\*()]+)?(\s)?(enum\s(.+{$|.*{$;?|[^\s;]+;?$))");
        static Regex TypedefOneLine = new Regex(@"^.*typedef\s(struct|enum)\s[^\s]+\s[^\s]+;$");
        static Regex TypdedefNoStruct = new Regex(@"^.*typedef\s.+\s[^\s]+;$");
        static Regex VariableDecleration = new Regex(@"^(?!.*return)(?=(\s)?([^\s.(),]+\s)?[^\s().+*\/\-,=]+(\s|\s?(\*)+\s?)[^\s\-*+\/()=]+((\s?((\+|\-))?(\*)?=.+;)|([^()=]+;)))");
        static Regex VariableEquation = new Regex(@"^(?!.*return)(?=(\s)?([^\s()]+\s)?((\*)*(\s))?[^\s.()]+(\s)?=(\s)?[A-Za-z][^\s()]*;$)");
        //static Regex DefineDecleration = new Regex(@"^(\s)?#define ([^ ]+) [^\d][^ ()]*( [^ ()]+)?$");
        static Regex DefineDecleration = new Regex(@"^(\s)?#define ([^ ]+) [^ ()]*( [^ ()]+)?$");
        //include <NAME>
        static Regex IncludeTrianglesPattern = new Regex(@"^(\s)?#include.{0,2}<.+>$");
        static Regex IncludeRegularPattern = new Regex(@"^(\s)?#include\s{0,2}"".+\""$");
        static Regex IncludePathPattern = new Regex(@"^(\s)?#include\s{0,2}"".+\""$");
        static Regex IfdefPattern = new Regex(@"(?i)^(\s)?#(\s+)?ifdef\s([^\s]*)(\s)?$");
        static Regex IfndefPattern = new Regex(@"(?i)^(\s)?#(\s+)?ifndef\s([^\s]*)(\s)?$");
        static Regex IfPattern = new Regex(@"(?i)^(\s)?#(\s+)?if\s.*(\s)?$");
        static Regex IfdefMatchPattern = new Regex(@"#ifdef\s{[^\s]*}");
        static Regex IfndefMatchPattern = new Regex(@"#ifndef\s{[^\s]*}");
        static Regex EndifPattern = new Regex(@"(?i)^(\s)?#(\s+)?endif(\s)?$");
        const string Documentation = @"(\/\*.*\*\/)|((?!.*\/\*).*\*\/)|(\/\/.*)";
        //chars to trim.
        static char[] CharsToTrim = { '&', '*', '\t', ' ', ';', '{', '}' };
        static string[] ignoreWhenCheckingEndRow = { @"if(\s)*?\(", @"while(\s)*?\(", @"for(\s)*?\(" };
        [ThreadStatic] static bool CompileError = false;
        static ArrayList ignoreVarialbesType = new ArrayList();
        /// Function - CreateMd5
        /// <summary>
        /// Function gets a string as an input and turns it to an MD5.
        /// </summary>
        /// <param name="input"> this paramter is the string that is being changed to an MD5 format.</param>
        /// <returns>MD5 format type string.</returns>
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        /// Function - NextScopeLength
        /// <summary>
        ///  in order to find function length or struct length or "next scope" this function can be used.
        /// </summary>
        /// <param name="sr"> type MyStream buffer for the file. </param>
        /// <param name="codeLine"> refference of the current code line type string. </param>
        /// <param name="count"> refference of the length of the scope type int. </param>
        /// <param name="Seek"> bool type parameter for returning the buffer to where it started from
        ///                     or to keep the buffer where it is after the scope ends. </param>
        /// <returns></returns>
        public static bool NextScopeLength(MyStream sr, ref string codeLine, ref int count, bool Seek)
        {
            //stack to count the blocks.
            Stack myStack = new Stack();
            //saving the current position of the buffer.
            uint curPos = sr.Pos;
            string ScopeName = new string(codeLine.ToCharArray());
            codeLine = sr.ReadLine();
            myStack.Push(codeLine);
            bool found = false;
            while ((codeLine != null && myStack.Count > 0))
            {
                codeLine = sr.ReadLine();
                count++;
                if (codeLine.IndexOf("{") != GeneralConsts.NOT_FOUND_STRING)
                {
                    myStack.Push(codeLine);
                }
                if (codeLine.IndexOf("}") != GeneralConsts.NOT_FOUND_STRING)
                {
                    myStack.Pop();
                }
                

            }
            if (myStack.Count == 0)
            {
                found = true;
            }
            count = count - 1;
            //checking the bool for seeking.
            if (Seek)
            {
                sr.Seek(curPos);
                codeLine = ScopeName;
                Console.WriteLine(codeLine);
            }
            myStack.Clear();
            return found;
        }
        /// Function - CheckIfStringInHash
        /// <summary>
        /// function checks if in the given hashtable it can find a key 
        /// that is equal to the s parameter.
        /// </summary>
        /// <param name="a"> Hashtable </param>
        /// <param name="codeLine"> string </param>
        /// <returns> returns if the string is in the hash type bool</returns>
        static bool CheckIfStringInHash(Hashtable a, string codeLine)
        {
            bool foundString = false;
            string result = codeLine.Trim(CharsToTrim);
            if (a.ContainsKey(CreateMD5(result)))
            {
                foundString = true;
            }
            return foundString;
        }
        /// Function - skipDocumentation
        /// <summary>
        /// skips the documentation in c file in a buffer. 
        /// </summary>
        /// <param name="sr"> buffer type MyStream</param>
        /// <param name="codeLine"> string </param>
        /// <returns> returns the amount of rows the documentation was.</returns>
        public static int skipDocumentation(MyStream sr, string codeLine)
        {
            int count = 0;
            uint pos = sr.Pos;
            if (codeLine.IndexOf("//") != GeneralConsts.NOT_FOUND_STRING)
            {
                while ((codeLine.IndexOf("//") != GeneralConsts.NOT_FOUND_STRING))
                {
                    pos = sr.Pos;
                    count++;
                    codeLine = sr.ReadLine();
                }
                sr.Seek(pos);
                count--;
            }
            if (codeLine.IndexOf("/*") != GeneralConsts.NOT_FOUND_STRING)
            {
                while (!(codeLine.IndexOf("*/") != GeneralConsts.NOT_FOUND_STRING))
                {
                    count++;
                    codeLine = sr.ReadLine();
                }
            }
            return count;
        }
        /// Function - KeywordsAmountOnVariableDeclaration
        /// <summary>
        /// Checks how many words are in the variable declaration before the name of the variable.
        /// </summary>
        /// <param name="codeLine"> variable declaration line type string.</param>
        /// <returns> returns type int word count.</returns>
        static int KeywordsAmountOnVariableDeclaration(string codeLine)
        {
            int count = 0;
            int pos = codeLine.IndexOf(GeneralConsts.SPACEBAR);
            bool endLoop = false;
            while (pos > 0 && !endLoop)
            {
                if (codeLine.IndexOf(GeneralConsts.EQUAL_SIGN) != GeneralConsts.NOT_FOUND_STRING)
                {
                    endLoop = true;
                }
                count++;
                if (codeLine.IndexOf(GeneralConsts.ASTERIX) != GeneralConsts.NOT_FOUND_STRING)
                    count = count - 1;

                pos = codeLine.IndexOf(GeneralConsts.SPACEBAR, pos + 1);
            }
            return count;
        }
        //the checks that are being mad in syntax Check are being written here.
        /// Function - IsExistInArrayList
        /// <summary>
        /// Checks if the string can be found in the ArrayList.
        /// </summary>
        /// <param name="a"> ArrayList </param>
        /// <param name="name"> the string that is being checked.</param>
        /// <returns> returns the ArrayList Node.</returns>
        public static ParametersType IsExistInArrayList(ArrayList a, string name)
        {
            ParametersType result = null;
            bool found = false;
            for (int i = 0; i < a.Count && !found; i++)
            {
                if (((ParametersType)a[i]).parameterName == name)
                {
                    result = (ParametersType)a[i];
                    found = true;
                }
            }
            return result;
        }
        /// Function - VariableDeclarationHandler
        /// <summary>
        /// Handling the variable declaration part in the function "ChecksInSyntaxCheck" by
        /// checking the whole variable declaration syntax.. (Checks the keywords and if the variable is being equal so checks
        /// the types of both of variables are equal and adding the new variable to "blocksAndNames" ArrayList).
        /// </summary>
        /// <param name="codeLine"> refference of the string s (code line).</param>
        /// <param name="pos"></param>
        /// <param name="keywords"> Hashtable type that stores all keywords in the code.</param>
        /// <param name="blocksAndNames"> ArrayList Type that stores all variables in the scopes.</param>
        /// <param name="IsScope"> Variable that checks if the function is being called inside a scope or 
        ///                        outside a scope.</param>
        /// <param name="sr"> buffer type MyStream.</param>
        /// <returns></returns>
        static bool VariableDeclarationHandler(string path,ref string codeLine, ref int pos,Hashtable anciCWords, Hashtable keywords, int threadNumber, bool IsScope, MyStream sr,string typeEnding, ArrayList variables=null, ArrayList globalVariables=null, ArrayList blocksAndNames=null)
        {
            bool DifferentTypes = true;
            int loopCount;
            string parameterType = GeneralConsts.EMPTY_STRING;
            int j;
            bool found = true;
            char[] trimChars = { '\t', ' ', ';','*' };
            
            loopCount = KeywordsAmountOnVariableDeclaration(codeLine);
            for (j = 0; j < loopCount; j++)
            {
                //checks if the keywords in the declaration is exist.
                found = found && CheckIfStringInHash(keywords, codeLine.Substring(pos, codeLine.Substring(pos, codeLine.Length - pos).IndexOf(GeneralConsts.SPACEBAR)).Trim(CharsToTrim));
                pos = codeLine.IndexOf(GeneralConsts.SPACEBAR, pos + 1) + 1;
            }
            if (loopCount == 0)
            {
                //gets in the if section only if there is only 1 keyword.
                found = found && CheckIfStringInHash(keywords, codeLine.Substring(pos, codeLine.Substring(pos, codeLine.Length - pos).IndexOf(GeneralConsts.SPACEBAR)).Trim(CharsToTrim));
            }
            if (codeLine.IndexOf("struct") != GeneralConsts.NOT_FOUND_STRING)
            {
                //gets in if the variable type includes a struct without a typedef.
                pos = codeLine.IndexOf("struct");
                parameterType = codeLine.Substring(pos, codeLine.IndexOf(GeneralConsts.SPACEBAR, pos + 7) - pos);
                found = CheckIfStringInHash(keywords, parameterType.Trim(CharsToTrim));
            }
            string name;
            ParametersType result;
            string tempCut;
            string parameterName;
            //if the line has equation in the declaration.
            if (codeLine.IndexOf(GeneralConsts.EQUAL_SIGN) != -1)
            {
                parameterType = parameterType.Trim();
                parameterType = Regex.Split(codeLine, GeneralConsts.EQUAL_SIGN)[0];
                parameterType = parameterType.Trim();
                parameterName = parameterType;
                if (parameterType.Split(' ').Length>2&&!(parameterType.IndexOf("struct")!=GeneralConsts.NOT_FOUND_STRING))
                {
                    tempCut= parameterType.Substring(parameterType.IndexOf(' ') + 1, parameterType.Length - (parameterType.IndexOf(' ') + 1));
                    if(tempCut.IndexOf(' ')!=GeneralConsts.NOT_FOUND_STRING)
                    {
                        parameterType = tempCut.Substring(0, tempCut.Length - (tempCut.IndexOf(' ') + 1));
                    }
                    else
                    {
                        parameterType = tempCut;
                    }
                }
                else if (parameterType.Split(' ').Length > 2 && (parameterType.IndexOf("struct") != GeneralConsts.NOT_FOUND_STRING))
                {
                    parameterType = parameterType.Substring(0, parameterType.LastIndexOf(" "));
                }
                else
                {
                    tempCut = parameterType;
                    parameterType = parameterType.Substring(0, parameterType.Length - (parameterType.Length-parameterType.IndexOf(' ')));
                }
                parameterType = parameterType.Trim(trimChars);
                parameterName = parameterName.Trim();
                parameterName = parameterName.Substring(parameterName.LastIndexOf(' ')).Trim();
                result = new ParametersType(parameterName, parameterType);
            }
            //only declaration.
            else
            {
                parameterType = codeLine;
                parameterType = parameterType.Trim(trimChars);
                parameterName = parameterType.Substring(parameterType.IndexOf(' ') + 1).Trim();
                parameterType = parameterType.Substring(0, parameterType.IndexOf(' '));
                result = new ParametersType(parameterName, parameterType);
            }
            name = result.parameterName;
            parameterType = result.parameterType;
            if(!(parameterType.IndexOf("struct")!=GeneralConsts.NOT_FOUND_STRING))
            {
                parameterType = parameterType.Replace(GeneralConsts.SPACEBAR, GeneralConsts.EMPTY_STRING);
            }
            if(keywords.ContainsKey(CreateMD5(parameterType))||anciCWords.ContainsKey(CreateMD5(parameterType)))
            {
                found = true;
            }
            // checks if there is already the same name in the same scope.
            if (IsExistInArrayList(((ArrayList)blocksAndNames[blocksAndNames.Count - 1]), name) != null)
            {
                MainProgram.AddToLogString(path, ("you have used the same name for multiple variables in row " + sr.curRow + ". name - " + name));
                throw new Exception(("you have used the same name for multiple variables in row " + sr.curRow + ". name - " + name));
                
            }
            else
            {
                ((ArrayList)blocksAndNames[blocksAndNames.Count - 1]).Add(new ParametersType(name, parameterType));
                //if its only in the functions scope right now so it adds it
                if(blocksAndNames.Count==2)
                {
                    variables.Add(new ParametersType(name, parameterType));
                }
                if (!IsScope)
                {
                    try
                    {
                        globalVariables.Add(new ParametersType(name, parameterType));
                    }
                    catch
                    {
                        ConnectionServer.CloseConnection(threadNumber, ("you have used the same name for multiple variables in row " + sr.curRow + ". name - " + name), GeneralConsts.ERROR);
                    }

                }
            }
            //if the declaration is also a equation.
            if (VariableEquation.IsMatch(codeLine))
            {
                DifferentTypes = VariableEquationHandler(sr,anciCWords, codeLine, blocksAndNames, threadNumber);
            }
            if (!DifferentTypes)
            {
                Server.ConnectionServer.CloseConnection(threadNumber, codeLine + " types of both variables are different in row : " + sr.curRow, GeneralConsts.ERROR);
                CompileError = true;
            }

            return found;
        }
        /// Function - getVariableTypeParameterFromArrayList
        /// <summary>
        /// Get the whole parameterType node out of the ArrayList.
        /// </summary>
        /// <param name="blocksAndNames"> ArrayList type.</param>
        /// <param name="name"> the name to get the whole node from.</param>
        /// <returns> returns parameterType type of the node named like "name".</returns>
        static ParametersType GetVariableTypeParameterFromArrayList(ArrayList blocksAndNames, string name)
        {
            bool endLoop = false;
            ParametersType result = null;
            for (int i = blocksAndNames.Count; i > 0 && !endLoop; i--)
            {
                if ((result = IsExistInArrayList((ArrayList)blocksAndNames[i - 1], name)) != null)
                {
                    endLoop = true;
                }
            }
            return result;
        }
        /// Function - VariableEquationHandler
        /// <summary>
        /// Handling the variable equation part in the function "ChecksInSyntaxCheck" by
        /// make sure every variable is exist in the code and that their type of the equation
        /// is the same.
        /// </summary>
        /// <param name="threadNumber"> the number of the current thread.</param>
        /// <param name="sr"> buffer type MyStream.</param>
        /// <param name="codeLine"> the code line type string.</param>
        /// <param name="blocksAndNames"> ArrayList of variables.</param>
        /// <returns>returns if the variable equation is good.</returns>
        static bool VariableEquationHandler(MyStream sr,Hashtable anciCWords, string codeLine, ArrayList blocksAndNames, int threadNumber)
        {
            char[] trimChars = { '\t', ' ' };
            bool isSameType = true;
            //splits the equation to 2 lines before the '=' and after it.
            string temp = Regex.Split(codeLine, GeneralConsts.EQUAL_SIGN)[0].Trim(trimChars);
            if(temp.Length>0)
            {
                if (temp.Length >codeLine.Split('+','-')[0].Trim(trimChars).Length && codeLine.Split('+','-')[0].Trim(trimChars).Length > 0)
                {
                    temp = codeLine.Split('+','-')[0].Trim(trimChars);
                }
                temp = temp.Trim();
                if(temp.IndexOf(' ')!=GeneralConsts.NOT_FOUND_STRING)
                {
                    temp = temp.Substring(temp.IndexOf(' '));
                }
            }
            
            //takes the first param name.
            string varName1 = temp;
            varName1=varName1.Trim();
            temp = Regex.Split(codeLine, GeneralConsts.EQUAL_SIGN)[1];
            char[] searchingChars = { ';' };
            //takes the second param name.
            string varName2 = temp.Substring(0, temp.IndexOfAny(searchingChars));
            varName2 = varName2.Trim(trimChars);
            //takes the whole parameterType type by the function - "getVariableTypeParameterFromArrayList".
            ParametersType var1 = GetVariableTypeParameterFromArrayList(blocksAndNames, varName1.Trim(GeneralConsts.ASTERIX));
            ParametersType var2 = GetVariableTypeParameterFromArrayList(blocksAndNames, varName2.Trim(GeneralConsts.ASTERIX));
            //make sures the variable 2 is exist.
            //checks if their type is the same.
            if (var2!=null&&isSameType && var1.parameterType != var2.parameterType)
            {
                isSameType = false;
                if(anciCWords.ContainsKey(CreateMD5(var1.parameterType))&&anciCWords.ContainsKey(CreateMD5(var2.parameterType)))
                {
                    isSameType = true;
                }
            }
            return isSameType;
        }
        /// Function - cleanLineFromDoc
        /// <summary>
        /// cleaning the codeLine from documentation options so it gets only the pure coding.
        /// </summary>
        /// <param name="codeLine"> the line of the code type string.</param>
        /// <returns> returns the pure code with no documentation string.</returns>
        static string cleanLineFromDoc(string codeLine)
        {
            char[] trimChar = { '\t', ' ' };
            string result = "";
            result=Regex.Replace(codeLine, Documentation, "");
            result = result.Trim(trimChar);
            return result;
        }
        /// Function - IsParameterNameInArrayList
        /// <summary>
        /// Checks if the parameter name is in the array list.
        /// </summary>
        /// <param name="a"> array type ArrayList.</param>
        /// <param name="parameterName"> parameter name type string.</param>
        /// <returns></returns>
        static int IsParameterNameInArrayList(ArrayList a,string parameterName)
        {
            bool found = false;
            int indexFound=-1;
            for(int i=0;i<a.Count&&!found;i++)
            {
                if(((ParametersType)a[i]).parameterName==parameterName)
                {
                    found = true;
                    indexFound = i;
                }
            }
            return indexFound;
        }
        /// Function - IsNumber
        /// <summary>
        /// checks if the string is a number.
        /// </summary>
        /// <param name="aNumber"> a string that might be a number.</param>
        /// <returns> returns a boolean true if its a number and false otherwise</returns>
        static bool IsNumber(this string aNumber)
        {
            int temp_big_int;
            var is_number = int.TryParse(aNumber, out temp_big_int);
            return is_number;
        }
        /// Function - AmountOfFuncWithThisName
        /// <summary>
        /// Checks how much functions with the same name are in the dictionary "calledFromFunc" a dictionary who saves for every function
        /// the functions they are being called from.
        /// </summary>
        /// <param name="funcName"> function name type string.</param>
        /// <param name="calledFromFunc"> called from function Dictionary.</param>
        /// <returns> returns an arrayList of the function names.</returns>
        static ArrayList AmountOfFuncWithThisName(string funcName, Dictionary<string, ArrayList> calledFromFunc)
        {
            ArrayList results=new ArrayList();
            foreach (var item in calledFromFunc.Keys)
            {
                if(item.IndexOf(funcName)!=-1)
                {
                    results.Add(item.ToString());
                }
            }
            return results;
        }
        /// Function - ConverterFromRegularCallToTypeCall
        /// <summary>
        /// The dictionary of the calledFromFunc works like that : 
        /// Every key is built like - FuncName(ParamType,ParamType).
        /// this function is converting from a functionCall for an example - Function1(1,"hey",param3)
        /// to how to key is being built - Function1(int,string,paramType)
        /// this function is being used in order to take care of functions with 2 names and different params..
        /// </summary>
        /// <param name="callFuncLine"> the line of the call of the function type string.</param>
        /// <param name="variables"> variables arrayList (all variables of the function are in it).</param>
        /// <param name="globalVariables"> all of the global variables in the code type array list.</param>
        /// <param name="calledFromFunc"> called from func type dictionary.</param>
        /// <returns></returns>
        static string ConverterFromRegularCallToTypeCall(string callFuncLine,ArrayList variables,ArrayList globalVariables, Dictionary<string, ArrayList> calledFromFunc)
        {
            callFuncLine = cleanLineFromDoc(callFuncLine);
            string [] tempSplitForNameAndTypes = callFuncLine.Split('(');
            string convertedFuncLine = tempSplitForNameAndTypes[0] + "(";
            string[] allParameters = tempSplitForNameAndTypes[1].Split(',');
            allParameters[allParameters.Length - 1] = allParameters[allParameters.Length - 1].Trim(')');
            int temporaryIndex;
            bool found = false;
            bool exit = false;
            for (int i=0;i<allParameters.Length&&!found&&!exit;i++)
            {
                //if there is only 1 function with the current name in the dict it automatically takes the key from the dictionary.
                if(AmountOfFuncWithThisName(convertedFuncLine,calledFromFunc).Count==1)
                {
                    convertedFuncLine = AmountOfFuncWithThisName(convertedFuncLine, calledFromFunc)[0].ToString();
                    found = true;
                }
                //if there is none it exits immediately.
                else if (AmountOfFuncWithThisName(convertedFuncLine, calledFromFunc).Count == 0)
                {
                    exit = true;
                }
                //if there are more than 1 function with the same function name it goes and start checking those if's.
                //this if is checking in the variables the type of the variable if it exists.
                else if((temporaryIndex=IsParameterNameInArrayList(variables,allParameters[i]))!=-1)
                {
                    convertedFuncLine += ((ParametersType)variables[temporaryIndex]).parameterType + ",";
                }
                //same on global variables.
                else if((temporaryIndex = IsParameterNameInArrayList(globalVariables, allParameters[i])) != -1)
                {
                    convertedFuncLine += ((ParametersType)globalVariables[temporaryIndex]).parameterType + ",";
                }
                //string type.
                else if(allParameters[i].IndexOf(@"""")!=-1)
                {
                    convertedFuncLine += "string";
                }
                //number type.
                else if(IsNumber(allParameters[i]))
                {
                    convertedFuncLine += "int";
                }
            }
            if(found==false)
            {
                convertedFuncLine += ")";
            }
            if(exit)
            {
                convertedFuncLine = "error";
            }
            return convertedFuncLine;
        }
        /// Function - checkIfEvarIsTurned
        /// <summary>
        /// returns if one of the environment variables that are open is being checked 
        /// in the ifdef.
        /// </summary>
        /// <param name="codeLine"> the line of the code.</param>
        /// <param name="eVars"> evars Array (all evars that are being turned).</param>
        /// <returns></returns>
        public static bool checkIfEvarIsTurned(string codeLine,string [] eVars)
        {
            //suppose to give me the environment variable it means in the ifdef.
            bool found=false;
            string eVar;
            eVar = Regex.Split(codeLine, @"\s")[1].Trim();
            for (int i=0;i<eVars.Length&&!found;i++)
            {
                if(eVar==eVars[i])
                {
                    found = true;
                }
            }
            return found;
        }
        /// Function - CheckInDefines
        /// <summary>
        /// Function checks if the define in the ifdef/ifndef in the codeLine is already defined.
        /// </summary>
        /// <param name="blocksAndDefines"> blocksAndDefines is all defines ordered by blocks. first block is 
        ///                                 Is the "global defines" and so on.</param>
        /// <param name="blockNumber"> the index to start with the checking (current block).</param>
        /// <param name="codeLine"> codeLine type string.</param>
        /// <returns></returns>
        public static bool CheckInDefines(ArrayList blocksAndDefines,int blockNumber,string codeLine)
        {
            bool found = false;
            string ifdef;
            ifdef = codeLine.Split(' ')[1].Trim();
            for (int i=blockNumber;i>=0&&!found;i--)
            {
                for(int j=0;j<((ArrayList)blocksAndDefines[i]).Count && !found; j++)
                {
                    if(ifdef==(string)((ArrayList)blocksAndDefines[i])[j])
                    {
                        found = true;
                    }
                }
            }
            return found;
        }
        /// Function - Skip_Ifdef_Or_Ifndef
        /// <summary>
        /// Function skips ifdef or ifndef if needed.
        /// </summary>
        /// <param name="sr"> MyStream type buffer.</param>
        /// <param name="codeLine"> CodeLine type string.</param>
        /// <param name="threadNumber"> thread number type int.</param>
        /// <returns></returns>
        public static int Skip_Ifdef_Or_Ifndef(MyStream sr,string codeLine,int threadNumber)
        {
            //stack to count the blocks.
            Stack myStack = new Stack();
            int count=0;
            //saving the current position of the buffer.
            codeLine = sr.ReadLine();
            codeLine = cleanLineFromDoc(codeLine);
            myStack.Push(codeLine);
            while ((codeLine != null && myStack.Count > 0))
            {
                codeLine = sr.ReadLine();
                codeLine = cleanLineFromDoc(codeLine);
                count++;
                if (IfdefPattern.IsMatch(codeLine)||IfndefPattern.IsMatch(codeLine)||IfPattern.IsMatch(codeLine))
                {
                    myStack.Push(codeLine);
                }
                if (EndifPattern.IsMatch(codeLine))
                {
                    myStack.Pop();
                }
            }
            if (myStack.Count != 0)
            {
                ConnectionServer.CloseConnection(threadNumber, "endif did not close correctly", GeneralConsts.ERROR);
            }
            count = count +1;
            //checking the bool for seeking.
            myStack.Clear();
            return count;
        }
        /// Function - ChecksInSyntaxCheck
        /// <summary>
        /// this function take cares of the whole syntax check of the program.
        /// it uses the functions mentioned in the documentations before.
        /// it take cares scopes and no scopes with the parameter IsScope type bool.
        /// </summary>
        /// <param name="path"> the path of the file.</param>
        /// <param name="globalVariables"> variables that all file have.</param>
        /// <param name="threadNumber"> the number of the current thread running.</param>
        /// <param name="variables"> all of the variables that the scope knows.</param>
        /// <param name="sr"> buffer type MyStream.</param>
        /// <param name="codeLine"> code line type string.</param>
        /// <param name="IsScope"> bool type IsScope.</param>
        /// <param name="keywords"> keywords type Hashtable that conatins the code keywords.</param>
        /// <param name="blocksAndNames"> blocksAndNames type ArrayList that conatins the code variables in the scope.</param>
        /// <param name="parameters"> parameters type ArrayList conatins the function parameters.</param>
        /// <param name="functionLength"> scopeLength type int default is 0 if the code line is outside any scopes.</param>
        /// <param name="typeEnding"> the file type (for an example h or c).</param>the called from func dictionary.
        /// <param name="calledFromFunc"> the called from func dictionary.</param>
        /// <param name="FreeMemoryPattern"> the pattern of the free memory.</param>
        /// <param name="functionName"> the name of the function.</param>
        /// <param name="memoryHandleFunc"> all of the memory handles type arrayList.</param>
        /// <param name="MemoryPattern"> Regex of the memory pattern.</param>
        static void ChecksInSyntaxCheck(string path,string destPath, MyStream sr, string codeLine, bool IsScope, Hashtable keywords,Hashtable memoryHandleFunc, int threadNumber,string typeEnding,string [] eVars, ArrayList variables, ArrayList globalVariables, ArrayList blocksAndNames,ArrayList blocksAndDefines,Regex MemoryPattern, Regex FreeMemoryPattern,ref string codeContent, ArrayList parameters = null, Dictionary<string, ArrayList> calledFromFunc = null, Dictionary<string, Dictionary<string, string[]>> callsFromThisFunction = null, int functionLength = 0,string functionName="",Dictionary<string,string>functionsContent=null,bool isFunction=false,Hashtable anciCWords=null)
        {
            try
            {
                //adds the parameters of the function to the current ArrayList of variables.
                int i;
                bool keywordCheck = true;
                bool memoryAllocation = false;
                bool memoryRelease = false;
                if (parameters != null)
                {
                    //if its type ending c.
                    if (typeEnding == "c")
                    {
                        //adds the parameters of the function to the blocksAndNames.
                        if(isFunction)
                        {
                            blocksAndNames.Add(new ArrayList());
                            blocksAndDefines.Add(new ArrayList());
                            ((ArrayList)blocksAndNames[blocksAndNames.Count-1]).AddRange(parameters);
                        }
                    }
                    for (i = 0; i < parameters.Count; i++)
                    {
                        //checks if there is an error in the parameters of the function.

                        if (!keywords.ContainsKey(CreateMD5(((ParametersType)parameters[i]).parameterType.Trim(CharsToTrim))))
                        {
                            string error = (codeLine + " keyword does not exist. row : " + sr.curRow);
                            MainProgram.AddToLogString(path, "warning - " + error);
                        }
                    }

                }
                //cleaning code.
                codeLine = cleanLineFromDoc(codeLine).Trim();
                if (StructPattern.IsMatch(codeLine))
                {
                    //Add struct keywords to the keywords Hashtable.
                    AddStructNames(sr, codeLine, keywords);
                    blocksAndNames.Add(new ArrayList());
                    blocksAndDefines.Add(new ArrayList());
                }
                if (codeLine.Trim(GeneralConsts.TAB_SPACE).IndexOf("{") != GeneralConsts.NOT_FOUND_STRING)
                {
                    codeLine = sr.ReadLine();
                    codeContent += codeLine + GeneralConsts.NEW_LINE;
                    if (functionName != "")
                    {
                        functionsContent[functionName] += codeLine + GeneralConsts.NEW_LINE;
                    }
                }
                //how to convert to array list

                bool DifferentTypesCheck = true;
                int pos = 0;
                ArrayList keywordResults = new ArrayList();
                for (i = 0; i < functionLength+1 && !CompileError && codeLine != null; i++)
                {
                    try
                    {
                        AllChecksInSyntaxCheck(path,sr,ref codeLine,IsScope,keywords,memoryHandleFunc,threadNumber,typeEnding,eVars,variables,globalVariables,blocksAndNames,blocksAndDefines,MemoryPattern,FreeMemoryPattern,ref codeContent,ref i,keywordCheck,memoryAllocation,memoryRelease,DifferentTypesCheck,pos,keywordResults, ref functionLength,calledFromFunc,callsFromThisFunction,functionName,functionsContent,isFunction,anciCWords);
                    }
                    catch(Exception e)
                    {
                        MainProgram.AddToLogString(path,"an error have accured in line "+sr.curRow+"\n"+e.Message);
                        codeLine = sr.ReadLine();
                        codeContent += codeLine + GeneralConsts.NEW_LINE;
                        if (functionName != "")
                        {
                            functionsContent[functionName] += codeLine + GeneralConsts.NEW_LINE;
                        }
                    }
                        
                    
                    
                }
                //if that was a scope it removes all the keywords of the scope.
                if (IsScope)
                {
                    for (i = 0; i < keywordResults.Count; i++)
                    {
                        keywords.Remove(keywordResults[i]);
                    }

                }
            }
            catch(Exception e)
            {
                MainProgram.CleanBeforeCloseThread(threadNumber, e.ToString(), GeneralConsts.ERROR, path,destPath);
            }
            
        }
        /// Function  - AllChecksInSyntaxCheck
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="destPath"></param>
        /// <param name="sr"></param>
        /// <param name="codeLine"></param>
        /// <param name="IsScope"></param>
        /// <param name="keywords"></param>
        /// <param name="memoryHandleFunc"></param>
        /// <param name="threadNumber"></param>
        /// <param name="typeEnding"></param>
        /// <param name="eVars"></param>
        /// <param name="variables"></param>
        /// <param name="globalVariables"></param>
        /// <param name="blocksAndNames"></param>
        /// <param name="blocksAndDefines"></param>
        /// <param name="MemoryPattern"></param>
        /// <param name="FreeMemoryPattern"></param>
        /// <param name="codeContent"></param>
        /// <param name="i"></param>
        /// <param name="keywordCheck"></param>
        /// <param name="memoryAllocation"></param>
        /// <param name="memoryRelease"></param>
        /// <param name="DifferentTypesCheck"></param>
        /// <param name="pos"></param>
        /// <param name="keywordResults"></param>
        /// <param name="functionLength"></param>
        /// <param name="parameters"></param>
        /// <param name="calledFromFunc"></param>
        /// <param name="callsFromThisFunction"></param>
        /// <param name="functionName"></param>
        /// <param name="functionsContent"></param>
        /// <param name="isFunction"></param>
        /// <param name="anciCWords"></param>
        static void AllChecksInSyntaxCheck(string path, MyStream sr, ref string codeLine, bool IsScope, Hashtable keywords, Hashtable memoryHandleFunc, int threadNumber, string typeEnding, string[] eVars, ArrayList variables, ArrayList globalVariables, ArrayList blocksAndNames, ArrayList blocksAndDefines, Regex MemoryPattern, Regex FreeMemoryPattern, ref string codeContent,ref int i,bool keywordCheck,bool memoryAllocation,bool memoryRelease, bool DifferentTypesCheck, int pos, ArrayList keywordResults, ref int functionLength, Dictionary<string, ArrayList> calledFromFunc = null, Dictionary<string, Dictionary<string, string[]>> callsFromThisFunction = null, string functionName = "", Dictionary<string, string> functionsContent = null, bool isFunction = false, Hashtable anciCWords = null)
        {
            codeLine = cleanLineFromDoc(codeLine).Trim();
            if (functionName != "")
            {
                codeLine = ConcatenateIfNeeded(codeLine, ref i, sr, functionsContent, functionName);
            }
            //take cares to all of those situations.
            if (StructPattern.IsMatch(codeLine) || TypedefOneLine.IsMatch(codeLine))
            {
                keywordResults = AddStructNames(sr, codeLine, keywords);
            }
            if (DefineDecleration.IsMatch(codeLine))
            {
                (string, string) temp = DefineHandler(keywords, codeLine);
                if (temp.Item1 != "")
                {
                    if (!blocksAndDefines.Contains(temp.Item1))
                    {
                        ((ArrayList)blocksAndDefines[blocksAndDefines.Count - 1]).Add(temp.Item1);
                    }

                }

            }
            if (CallFunction.IsMatch(codeLine) && !functionPatternInH.IsMatch(codeLine))
            {
                MatchCollection m = Regex.Matches(codeLine, CallFunction.ToString());
                string convertedLine = m[0].ToString();
                convertedLine = ConverterFromRegularCallToTypeCall(codeLine, variables, globalVariables, calledFromFunc);
                if (convertedLine != "error" && calledFromFunc.ContainsKey(convertedLine))
                {
                    try
                    {
                        calledFromFunc[convertedLine].Add(functionName);
                        Console.WriteLine(SetCallsFromThisFunctionValue(convertedLine, callsFromThisFunction), FindCallingFunctionParameters(codeLine));
                        callsFromThisFunction[functionName].Add(SetCallsFromThisFunctionValue(convertedLine, callsFromThisFunction), FindCallingFunctionParameters(codeLine));
                    }
                    catch (Exception e)
                    {
                        MainProgram.AddToLogString(path, e.ToString());
                    }
                }

            }
            //checks for memory allocation.
            if (!memoryAllocation && MemoryPattern.IsMatch(codeLine))
            {
                if (!memoryHandleFunc.ContainsKey(CreateMD5(functionName)))
                {
                    memoryHandleFunc.Add(CreateMD5(functionName), GeneralConsts.MEMORY_ALLOCATION);
                    memoryAllocation = true;

                }
                else
                {
                    if (memoryRelease)
                    {
                        memoryHandleFunc[CreateMD5(functionName)] = GeneralConsts.MEMORY_MANAGEMENT;
                    }
                }
            }
            //checks for memory releases (free,etc..).
            if (!memoryRelease && FreeMemoryPattern.IsMatch(codeLine))
            {
                if (!memoryHandleFunc.ContainsKey(CreateMD5(functionName)))
                {
                    memoryHandleFunc.Add(CreateMD5(functionName), GeneralConsts.MEMORY_FREE);
                    memoryRelease = true;
                }
                else
                {
                    if (memoryAllocation)
                    {
                        memoryHandleFunc[CreateMD5(functionName)] = GeneralConsts.MEMORY_MANAGEMENT;
                    }
                }
            }
            //for all variable declerations.
            if (VariableDecleration.IsMatch(codeLine) && !(codeLine.IndexOf("typedef") != GeneralConsts.NOT_FOUND_STRING))
            {
                keywordCheck = VariableDeclarationHandler(path,ref codeLine, ref pos, anciCWords, keywords, threadNumber, IsScope, sr, typeEnding, variables, globalVariables, blocksAndNames);
                if (!keywordCheck)
                {
                    string error = (codeLine + " keyword does not exist. row : " + sr.curRow);
                    MainProgram.AddToLogString(path, error);

                }
            }
            else if (typeEnding == "c" && VariableEquation.IsMatch(codeLine))
            {
                DifferentTypesCheck = VariableEquationHandler(sr, anciCWords, codeLine, blocksAndNames, threadNumber);
                if (!DifferentTypesCheck)
                {
                    string error = (codeLine + " types of both variables might be different in row : " + sr.curRow);
                    /*CompileError = true;
                    throw new Exception(error);*/
                    MainProgram.AddToLogString(path, "warning - " + error);
                }
            }
            codeLine = codeLine.Trim();
            //checks if any of the error bools is on.
            pos = 0;
            //resets the error bools.
            keywordCheck = DifferentTypesCheck = true;
            if (IfdefPattern.IsMatch(codeLine))
            {
                if (!checkIfEvarIsTurned(codeLine, eVars))
                {
                    if (!CheckInDefines(blocksAndDefines, blocksAndDefines.Count - 1, codeLine))
                    {
                        i += Skip_Ifdef_Or_Ifndef(sr, codeLine, threadNumber);
                    }
                }
            }
            else if (IfndefPattern.IsMatch(codeLine))
            {
                if (checkIfEvarIsTurned(codeLine, eVars))
                {
                    if (CheckInDefines(blocksAndDefines, blocksAndDefines.Count - 1, codeLine))
                    {
                        i += Skip_Ifdef_Or_Ifndef(sr, codeLine, threadNumber);
                    }
                }
            }
            if (codeLine.IndexOf("//") != GeneralConsts.NOT_FOUND_STRING && codeLine.IndexOf("//") < 4 || codeLine.IndexOf("/*") != GeneralConsts.NOT_FOUND_STRING && codeLine.IndexOf("/*") < 4)
            {
                //skips documentation if needed.
                i += skipDocumentation(sr, codeLine);
            }
            //adds a new ArrayList inside the keywordsAndNames ArrayList for the scope that has started.
            if (OpenBlockPattern.IsMatch(codeLine))
            {
                blocksAndNames.Add(new ArrayList());
                blocksAndDefines.Add(new ArrayList());
            }
            if (CloseBlockPattern.IsMatch(codeLine))
            {
                try
                {
                    //close the last scope that just closed.
                    blocksAndNames.RemoveAt(blocksAndNames.Count - 1);
                    blocksAndDefines.RemoveAt(blocksAndDefines.Count - 1);
                }
                catch (Exception e)
                {
                    //bad scoping causes the function to remove from an ArrayList something while its already 0.
                    Server.ConnectionServer.CloseConnection(threadNumber, "bad scoping in function in row " + sr.curRow + "error message = " + e.ToString(), GeneralConsts.ERROR);
                    CompileError = true;
                    throw new Exception("Bad Scoping in Row " + sr.curRow);

                }
            }
            //if the code line is in a scope or if its not the last line in the scope continute to the next line.
            if (IsScope && i < functionLength)
            {
                codeLine = sr.ReadLine();
                codeContent += codeLine + GeneralConsts.NEW_LINE;
                if (functionName != "")
                {
                    functionsContent[functionName] += codeLine + GeneralConsts.NEW_LINE;
                }
            }
        
    }
        /// Function - FindNameFromCodeLine
        /// <summary>
        /// Get function name from function call code line.
        /// </summary>
        /// <param name="codeLine"> code line that has inside the function call.</param>
        /// <returns></returns>
        public static string FindNameFromCodeLine(string codeLine)
        {
            codeLine = codeLine.Split('(')[0];
            codeLine = codeLine.Substring(codeLine.LastIndexOf(" "));
            codeLine = codeLine.Trim();
            return codeLine;
        }
        /// Function - SetCallsFromThisFunctionValue
        /// <summary>
        /// creates the key that needs to be for this function on the dictionary of functionInfoJson. if they match return the key
        /// </summary>
        /// <param name="name"> function name</param>
        /// <param name="callsFromThisFunction"> type dictionary that have this function and what functions are being called from this.</param>
        /// <returns></returns>
        public static string SetCallsFromThisFunctionValue(string name, Dictionary<string, Dictionary<string, string[]>> callsFromThisFunction)
        {
            string result="";
            foreach(string key in callsFromThisFunction.Keys)
            {
                ArrayList parameters = new ArrayList();
                parameters.AddRange(GeneralRestApiServerMethods.FindParameters(key));
                string funcKeyInDict = FindNameFromCodeLine(key);
                funcKeyInDict += "(";
                for (int i = 0; i < parameters.Count - 1; i++)
                {
                    funcKeyInDict += ((ParametersType)parameters[i]).parameterType + ",";
                }
                if(parameters.Count>0)
                {
                    funcKeyInDict += ((ParametersType)parameters[parameters.Count - 1]).parameterType;
                }
                funcKeyInDict += ")";
                if (funcKeyInDict==name)
                {
                    result = key;
                }

            }
            return result;

        }
        /// Function - SyntaxCheck
        /// <summary>
        /// that function uses the Function "ChecksInSyntaxCheck" if that is in a scope
        /// or outside a scope according to the situation.
        /// </summary>
        /// <param name="path"> The path of the c code type string.</param>
        /// <param name="keywords"> keywords type Hashtable that conatins the code keywords.</param>
        public static bool SyntaxCheck(string path,string destPath,Hashtable anciCWords,ArrayList globalVariable,Dictionary<string,ArrayList>calledFromFunc, Dictionary<string, Dictionary<string, string[]>> callsFromThisFunction, Hashtable memoryHandleFunc, Hashtable keywords,Dictionary<string,ArrayList> funcVariables,string [] eVars,int threadNumber,string typeEnding,Regex MemoryPattern,Regex FreeMemoryPattern,Dictionary<string,string> functionsContent,ref string codeContent)
        {
            MyStream sr=null;
            try
            {
                 sr= new MyStream(path, System.Text.Encoding.UTF8);
            }
            catch(Exception e)
            {
                MainProgram.CleanBeforeCloseThread(threadNumber, FILE_NOT_FOUND, GeneralConsts.ERROR, path,destPath);
                CompileError = true;
            }
            if(sr!=null&&!CompileError)
            {
                //in order to delete struct keywords when they come in a function at the end of the function.
                ArrayList parameters = new ArrayList();
                ArrayList blocksAndNames = new ArrayList();
                ArrayList blocksAndDefines = new ArrayList();
                ArrayList variables = new ArrayList();
                //adds an ArrayList inside blocksAndNames ArrayList for the action outside the scopes.
                blocksAndNames.Add(new ArrayList());
                blocksAndDefines.Add(new ArrayList());
                string codeLine=sr.ReadLine();
                codeContent += codeLine + GeneralConsts.NEW_LINE;
                int scopeLength = 0;
                string lastFuncLine = "";
                bool isFunction = false;
                while (((codeLine = sr.ReadLine())!=null) && !CompileError)
                {
                    codeContent += codeLine + GeneralConsts.NEW_LINE;
                    scopeLength = 0; 
                    //handling the scopes.
                    if (OpenBlockPattern.IsMatch(codeLine))
                    {
                        
                        NextScopeLength(sr, ref codeLine, ref scopeLength, true);
                        ChecksInSyntaxCheck(path,destPath, sr, codeLine, true, keywords,memoryHandleFunc, threadNumber,typeEnding,eVars, variables, globalVariable, blocksAndNames,blocksAndDefines, MemoryPattern, FreeMemoryPattern,ref codeContent, parameters,calledFromFunc,callsFromThisFunction, scopeLength + 1,lastFuncLine,functionsContent,isFunction,anciCWords);
                        parameters.Clear();
                        isFunction = false;
                    }
                    // if there is a function it saves its parameters (only if its C)..
                    else if (FunctionPatternInC.IsMatch(codeLine)&&!functionPatternInH.IsMatch(codeLine))
                    {
                        isFunction = true;
                        parameters.AddRange(GeneralRestApiServerMethods.FindParameters(cleanLineFromDoc(codeLine)));
                        if (lastFuncLine != ""&&!(lastFuncLine.IndexOf(";")!=GeneralConsts.NOT_FOUND_STRING))
                        {
                            funcVariables.Add(lastFuncLine, new ArrayList(variables));
                            variables.Clear();
                        }
                        lastFuncLine = codeLine;
                        try
                        {
                            functionsContent.Add(lastFuncLine, "{ \n");
                        }
                        catch (Exception e)
                        {
                            MainProgram.CleanBeforeCloseThread(threadNumber, e.ToString(), GeneralConsts.ERROR, path,destPath);
                        }
                        string funcKeyInDict = GeneralRestApiServerMethods.FindNameFromCodeLine(codeLine);
                        funcKeyInDict += "(";
                        for (int i = 0; i < parameters.Count - 1; i++)
                        {
                            funcKeyInDict += ((ParametersType)parameters[i]).parameterType + ",";
                        }
                        if(parameters.Count>0)
                        {
                            funcKeyInDict += ((ParametersType)parameters[parameters.Count - 1]).parameterType + ")";
                        }
                        else
                        {
                            funcKeyInDict += ")";
                        }
                        calledFromFunc.Add(funcKeyInDict, new ArrayList());
                        callsFromThisFunction.Add(lastFuncLine, new Dictionary<string, string[]>());

                    }
                    // if there is a function in h it checks it keywords to see if they are good.
                    else if (functionPatternInH.IsMatch(codeLine))
                    {
                        //parameters.Clear();
                        parameters.AddRange(GeneralRestApiServerMethods.FindParameters(cleanLineFromDoc(codeLine)));
                        lastFuncLine = codeLine;
                        ChecksInSyntaxCheck(path, destPath, sr, codeLine, false, keywords,memoryHandleFunc, threadNumber, typeEnding,eVars, variables, globalVariable, blocksAndNames, blocksAndDefines, MemoryPattern, FreeMemoryPattern, ref codeContent, parameters);
                        parameters.Clear();
                    }
                    //handling outside the scopes.
                    else
                    {
                        ChecksInSyntaxCheck(path, destPath, sr, codeLine, false, keywords,memoryHandleFunc, threadNumber,typeEnding,eVars, variables, globalVariable, blocksAndNames, blocksAndDefines, MemoryPattern, FreeMemoryPattern, ref codeContent);
                    }

                }
                if (lastFuncLine != "")
                {
                    funcVariables.Add(lastFuncLine, new ArrayList(variables));
                    variables.Clear();
                }
            }
            return CompileError;
        }
        /// Function - FindCallingFunctionParameters
        /// <summary>
        /// searching for all calling function in the code.
        /// </summary>
        /// <param name="codeLine"> the code line type string.</param>
        /// <returns></returns>
        static string [] FindCallingFunctionParameters(string codeLine)
        {
            ArrayList result = new ArrayList();
            var pattern = @"\((.*?)\)";
            var matches = Regex.Matches(codeLine, pattern);
            string [] parameters = matches[0].Groups[1].Value.Split(',');
            for(int i=0;i<parameters.Length;i++)
            {
                parameters[i] = parameters[i].Trim();
            }
            return parameters;
        }
        /// Function - AddToHashFromFile
        /// <summary>
        /// Adds from a file splited by "splitBy" parameter to the Hashtable fromt he path.
        /// </summary>
        /// <param name="path"> The path for the file.</param>
        /// <param name="a"> Hashtable to store the keywords.</param>
        /// <param name="splitBy"> String that the file needs to split by.</param>
        public static void AddToHashFromFile(string path, Hashtable a,Hashtable anciCFile, string splitBy,int threadNumber)
        {

            MyStream sr = null;
            try
            {
                sr = new MyStream(path, System.Text.Encoding.UTF8);
            }
            catch(Exception e)
            {
                CompileError = true;
                MainProgram.AddToLogString(path, e.Message);
            }
            if(!CompileError)
            {
                string line = sr.ReadLine();
                string[] keysArr = Regex.Split(line, splitBy);
                ICollection keys = a.Keys;
                for (int i = 0; i < keysArr.Length; i++)
                {
                    a.Add(CreateMD5(keysArr[i]), keysArr[i]);
                    anciCFile.Add(CreateMD5(keysArr[i]), keysArr[i]);
                }
                sr.Close();
            }
        }
        /// Function - AddToKeywords
        /// <summary>
        /// Function checks if it can add a newKeyword to the keywords and if it is able it adds.
        /// </summary>
        /// <param name="newKeyword"> type string.</param>
        /// <param name="keywords"> keywords type Hashtable.</param>
        /// <param name="codeLine"> codeLine type string.</param>
        /// <returns></returns>
        static string AddToKeywords(string newKeyword,Hashtable keywords,string codeLine)
        {
            char[] trim = { ' ', '\"' };
            newKeyword = Regex.Split(codeLine, GeneralConsts.SPACEBAR)[1].Trim(trim);
            newKeyword = newKeyword.Trim();
            if (!keywords.ContainsKey(CreateMD5(newKeyword)))
            {
                keywords.Add(CreateMD5(newKeyword), newKeyword);
            }
            return newKeyword;
        }
        /// Function - DefineHandler
        /// <summary>
        /// Function take cares of the defines. takes the line of the define declaration and
        /// break it into a tuple of 2 strings (newKeyword,OldKeyword).
        /// </summary>
        /// <param name="keywords"> keywords type Hashtable.</param>
        /// <param name="codeLine"> codeLine type string.</param>
        /// <returns></returns>
        static (string,string) DefineHandler(Hashtable keywords,string codeLine)
        {
            string firstNewVariableWord;
            string secondNewVariableWord;
            string[] newkeyWords;
            string newKeyword="";
            string defineOriginalWord;
            (string, string) result=("","");
            //getting both of the names in two variables.
            firstNewVariableWord = codeLine.Substring(codeLine.IndexOf(' '), codeLine.Length - codeLine.IndexOf(' '));
            firstNewVariableWord = firstNewVariableWord.Trim();
            firstNewVariableWord = firstNewVariableWord.Substring(firstNewVariableWord.IndexOf(GeneralConsts.SPACEBAR), firstNewVariableWord.Length - firstNewVariableWord.IndexOf(GeneralConsts.SPACEBAR));
            firstNewVariableWord = firstNewVariableWord.Trim(CharsToTrim);
            //old definition.
            defineOriginalWord = firstNewVariableWord;

            if (firstNewVariableWord.IndexOf(GeneralConsts.SPACEBAR) != GeneralConsts.NOT_FOUND_STRING)
            {
                //checks if the definition exists.
                if (keywords.ContainsKey(CreateMD5(firstNewVariableWord)))
                {
                    //new keyword
                    //takes the part after the first space.
                    newKeyword = Regex.Split(codeLine, GeneralConsts.SPACEBAR)[1];
                    newKeyword = newKeyword.Trim();
                    //make sure that the keyword isn't already existed.
                    if (!keywords.ContainsKey(CreateMD5(newKeyword)))
                    {
                        //adds her if not
                        keywords.Add(CreateMD5(newKeyword), newKeyword);
                        //adds the new definition.
                        
                        //types the dont mind the variable are being ingored. for an example static : so if
                        //the type for example is static and i define a new static definition it will add it to
                        //the ignoreVariablesType.
                        if (ignoreVarialbesType.Contains(defineOriginalWord))
                        {
                            ignoreVarialbesType.Add(newKeyword);
                        }
                    }
                    result = (newKeyword, defineOriginalWord);
                }
                else
                {
                    //splits when there are 2 types that you define for an example "unsinged int"
                    //so what this section is doing is checking if both of the types exist.
                    newkeyWords = Regex.Split(firstNewVariableWord, GeneralConsts.SPACEBAR);
                    secondNewVariableWord = newkeyWords[1];
                    firstNewVariableWord = newkeyWords[0];
                    //checks both types.
                    if (CheckIfStringInHash(keywords, firstNewVariableWord) && CheckIfStringInHash(keywords, secondNewVariableWord))
                    {
                        newKeyword = AddToKeywords(newKeyword, keywords, codeLine);
                        result = (newKeyword, defineOriginalWord);
                    }
                }

            }
            else
            {
                newKeyword = AddToKeywords(newKeyword, keywords, codeLine);
                result = (newKeyword, defineOriginalWord);
            }
            return result;
        }
        /// Function - PreprocessorActions
        /// <summary>
        /// this function is handeling all of the things that come before the code the "preproccessning part".
        /// </summary>
        /// <param name="path"> the path of the file that is being checked.</param>
        /// <param name="threadNumber"> the thread number of the file checking.</param>
        /// <param name="keywords"> all of the keywords that are in the code.</param>
        /// <param name="includes"> all includes of the keywords in the code.</param>
        /// <param name="defines"> all the defines in the code.</param>
        /// <param name="eVars"> all the environment variables in the code.</param>
        /// <param name="pathes"> all the pathes in the code that have files in it.</param>
        /// <param name="fileThreadNumber"> the actual thread number.</param>
        static void PreprocessorActions(string path, int threadNumber, Hashtable keywords, Hashtable includes, Dictionary<string, string> defines, string[] eVars, string[] pathes, int fileThreadNumber)
        {
            bool endLoop = false;
            MyStream sr = null;
            //try to open the buffer.
            try
            {
                sr = new MyStream(path, System.Text.Encoding.UTF8);
            }
            catch (Exception e)
            {
                endLoop = true;
            }
            try
            {
                string codeLine;

                while (!endLoop && (codeLine = sr.ReadLine()) != null)
                {
                    codeLine = cleanLineFromDoc(codeLine);
                    if (DefineDecleration.IsMatch(codeLine)&&threadNumber!=0)
                    {
                        (string, string) temp = DefineHandler(keywords, codeLine);
                        if (!defines.ContainsKey(temp.Item1))
                        {
                            defines.Add(temp.Item1, temp.Item2);
                            
                        }
                        if(!keywords.ContainsKey(CreateMD5(temp.Item1)))
                        {
                            keywords.Add(CreateMD5(temp.Item1), temp.Item1);
                        }
                    }
                    //Handling almost the same patterns as the syntaxCheck function.
                    if ((StructPattern.IsMatch(codeLine) || EnumPattern.IsMatch(codeLine)) && threadNumber != 0)
                    {
                        AddStructNames(sr, codeLine, keywords);
                    }
                    else if (TypedefOneLine.IsMatch(codeLine) && threadNumber != 0)
                    {
                        AddStructNames(sr, codeLine, keywords);
                    }
                    else if (TypdedefNoStruct.IsMatch(codeLine) && threadNumber != 0)
                    {
                        AddStructNames(sr, codeLine, keywords);
                    }
                    //if the code line is an include it creates a thread and enters to the defines , structs and more to 
                    //the Hashtables and Dictionaries.
                    else if (IncludeTrianglesPattern.IsMatch(codeLine) || IncludeRegularPattern.IsMatch(codeLine))
                    {
                        string currentPath = GeneralConsts.EMPTY_STRING;
                        string result;
                        if (codeLine.IndexOf("<") != -1 && codeLine.IndexOf(">") != -1)
                        {
                            result = CutBetween2Strings(codeLine, "<", ">");
                        }
                        else
                        {
                            result = CutBetween2Strings(codeLine, "\"", "\"");
                        }
                        //only enters an include if it didnt already included him.
                        if (!includes.Contains(CreateMD5(result)))
                        {
                            includes.Add(CreateMD5(result), result);
                            //if the include includes a path inside of it.
                            if (result.IndexOf("\\") != -1)
                            {
                                //opens the thread (thread number +1).
                                PreprocessorActions(result, threadNumber + 1, keywords, includes, defines, eVars, pathes, fileThreadNumber);
                            }
                            //if it does not include an exact path.
                            else
                            {
                                //runs on the pathes that the import files might be in.
                                for (int i = 0; i < pathes.Length; i++)
                                {
                                    //checks if the file exists in one of those folders.
                                    if (Directory.GetFiles(pathes[i], result, SearchOption.AllDirectories).Length > 0)
                                    {
                                        currentPath = Directory.GetFiles(pathes[i], result, SearchOption.AllDirectories)[0];
                                        break;
                                    }
                                }
                                //creats a thread.
                                if (currentPath != "" && currentPath != null && currentPath.Length > 0)
                                {
                                    PreprocessorActions(currentPath, threadNumber + 1, keywords, includes, defines, eVars, pathes, fileThreadNumber);
                                }
                            }
                        }
                    }

                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            

        }
        /// Function - ConcatenateIfNeeded
        /// <summary>
        /// if someone wrote a line that continues in the next line it concatenates it. and creates a new line.
        /// </summary>
        /// <param name="codeLine"> codeLine type string</param>
        /// <param name="buffer"> buffer type MyStream</param>
        /// <param name="functionContent"> the content of the function</param>
        /// <param name="functionName">function Name</param>
        /// <returns></returns>
        static string ConcatenateIfNeeded(string codeLine,ref int index,MyStream buffer,Dictionary<string,string>functionContent,string functionName)
        {
            bool stop = false;
            string newCodeLine = codeLine;
            if(codeLine.IndexOf("#")!=-1)
            {
                stop = true;
            }
            for(int i=0;i<ignoreWhenCheckingEndRow.Length&&!stop;i++)
            {
                if (Regex.IsMatch(codeLine, ignoreWhenCheckingEndRow[i]))
                    stop = true;
            }
            if(codeLine.IndexOf("/*")!=-1|| codeLine.IndexOf("//") != -1|| codeLine.IndexOf("*/") != -1||codeLine.IndexOf("{")!=-1|| codeLine.IndexOf("}") != -1|| codeLine=="")
            {
                stop = true;
            }
            while(!stop&&codeLine!=null&&newCodeLine.IndexOf(";")==-1&&newCodeLine.IndexOf("{")==-1&& newCodeLine.IndexOf("{") == -1)
            {
                newCodeLine += buffer.ReadLine();
                index += 1;
            }
            if(codeLine!=newCodeLine)
            {
                newCodeLine = newCodeLine.Replace("\t", "");
                functionContent[functionName]=functionContent[functionName].Replace(codeLine, newCodeLine);
            }
            return newCodeLine;
        }
        /// Function - CutBetween2Strings
        /// <summary>
        /// Cut a string s from the first parameter to the second parameter.
        /// </summary>
        /// <param name="codeLine"> string s that the function cuts.</param>
        /// <param name="first"> first cut.</param>
        /// <param name="second"> second cut.</param>
        /// <returns> returns the cutted string.</returns>
        public static string CutBetween2Strings(string codeLine, string first, string second)
        {
            int startCut = codeLine.IndexOf(first) + first.Length;
            int endCut= codeLine.LastIndexOf(second);
            string result = codeLine.Substring(startCut, endCut - startCut);
            result = result.Trim();
            return result;
        }
        /// Function - AddKeywordsFromArray
        /// <summary>
        /// add keyword to array.
        /// </summary>
        /// <param name="arrayString"></param>
        /// <param name="keywords"></param>
        /// <param name="keywords2"></param>
        public static void AddKeywordsFromArray(string[] arrayString, Hashtable keywords, ArrayList keywords2 = null)
        {
            for (int i = 0; i < arrayString.Length; i++)
            {
                arrayString[i] = arrayString[i].Trim(CharsToTrim);
                if (!keywords.ContainsKey(CreateMD5(arrayString[i])))
                {
                    //adds the keywords.
                    keywords.Add(CreateMD5(arrayString[i]), arrayString[i]);
                    keywords2.Add(CreateMD5(arrayString[i]));
                }

            }
        }
        /// Function - AddStructNames
        /// <summary>
        /// adds all the names of the struct or typedef struct to the keywords Hashtable.
        /// </summary>
        /// <param name="sr"> buffer type MyStream.</param>
        /// <param name="s"> struct first line</param>
        /// <param name="keywords"> Hashtable to store the keywords.</param>
        /// <returns> returns an ArrayList with the keywords.</returns>
        static ArrayList AddStructNames(MyStream sr, string codeLine, Hashtable keywords)
        {
            ArrayList results = new ArrayList();
            int count = 0;
            int spaceIndex;
            string[] tempSplit;
            string tempString;
            string newVariableName;
            //if thats a typedef.
            uint curpos = sr.Pos;

            if (codeLine.IndexOf("typedef") != GeneralConsts.NOT_FOUND_STRING)
            {
                //if thats not a typedef declaration.
                if (!TypedefOneLine.IsMatch(codeLine) && !TypdedefNoStruct.IsMatch(codeLine))
                {
                    string structKeyword = codeLine.Trim(CharsToTrim);

                    structKeyword = (structKeyword.IndexOf("enum") != -1) ? structKeyword.Substring(structKeyword.IndexOf("enum")) : structKeyword.Substring(structKeyword.IndexOf("struct"));
                    if (!keywords.Contains(CreateMD5(structKeyword)) && (structKeyword != "struct" || structKeyword != "enum"))
                    {
                        //adds the new keyword.
                        keywords.Add(CreateMD5(structKeyword), structKeyword);
                        results.Add(CreateMD5(structKeyword));
                    }
                    if (codeLine.IndexOf("}") != -1)
                    {
                        codeLine = codeLine.Substring(codeLine.IndexOf("}") + 1);
                        codeLine = codeLine.Trim(';');
                        if (!keywords.Contains(CreateMD5(codeLine)))
                        {
                            keywords.Add(CreateMD5(codeLine), codeLine);
                        }
                    }
                    else if (NextScopeLength(sr, ref codeLine, ref count, false))
                    {
                        codeLine = codeLine.Trim(CharsToTrim);
                        tempSplit = Regex.Split(codeLine, @",");
                        AddKeywordsFromArray(tempSplit, keywords, results);
                    }
                }
                else
                {
                    if (TypedefOneLine.IsMatch(codeLine))
                    {
                        //if thats one line of typedef.
                        spaceIndex = codeLine.IndexOf(GeneralConsts.SPACEBAR) + 1;
                        //take after the line after the first space.
                        tempString = newVariableName = codeLine.Substring(spaceIndex);
                        tempString = tempString.TrimEnd(' ').Remove(tempString.LastIndexOf(GeneralConsts.SPACEBAR) + 1);
                        tempString = tempString.Trim(CharsToTrim);
                        newVariableName = CutBetween2Strings(codeLine, tempString, ";");
                        newVariableName = newVariableName.Trim(CharsToTrim);
                        if (keywords.Contains(CreateMD5(tempString)) && !keywords.Contains(CreateMD5(newVariableName)))
                        {
                            //adds the keyword for the line.
                            keywords.Add(CreateMD5(newVariableName), newVariableName);
                            results.Add(CreateMD5(newVariableName));
                        }

                    }
                    else
                    {
                        tempString = codeLine.Substring(codeLine.LastIndexOf(" "), codeLine.IndexOf(";") - codeLine.LastIndexOf(" "));
                        tempString = tempString.Trim();
                        if (!keywords.Contains(CreateMD5(tempString)))
                        {
                            keywords.Add(CreateMD5(tempString), tempString);
                            results.Add(CreateMD5(tempString));
                        }
                    }

                }
            }
            //if thats a regular struct.
            else
            {
                codeLine = codeLine.Trim(CharsToTrim);
                if (!keywords.Contains(CreateMD5(codeLine)))
                {
                    //adds the new keyword.
                    keywords.Add(CreateMD5(codeLine), codeLine);
                    results.Add(CreateMD5(codeLine));
                }


            }
            //returns the ArrayList.
            sr.Seek(curpos);
            return results;



        }
        /// Function - AddToArrayListFromFile
        /// <summary>
        /// Adds from a file the names that split by "split by" in the path "path"
        /// </summary>
        /// <param name="path"> path of the c code type string.</param>
        /// <param name="a"> ArrayList </param>
        /// <param name="splitBy"> What to split by type string.</param>
        public static void AddToArrayListFromFile(string path,ArrayList a,string splitBy,int threadNumber)
        {
            MyStream sr = null;
            try
            {
                sr = new MyStream(path, System.Text.Encoding.UTF8);
            }
            catch(Exception e)
            {
                MainProgram.AddToLogString(path, e.ToString());
                CompileError = true;
            }
            if(!CompileError)
            {
                string s = sr.ReadLine();
                string[] temp = Regex.Split(s, splitBy);
                foreach (string i in temp)
                {
                    a.Add(i);
                }
                sr.Close();
            }
            
        }
        /// Function - initializeKeywordsAndSyntext
        /// <summary>
        /// Function initialize all of the functions that needs to come before the syntax Check.
        /// </summary>
        /// <param name="ansiPath">  Path for the file the conatins the ansi c keywords.</param>
        /// <param name="cFilePath"> Path for the c file that contains the file.</param>
        /// <param name="CSyntextPath"> Path of all the ansi c syntext file (not sure yet if it is needed.)</param>
        /// <param name="ignoreVariablesTypesPath"> Path for all the variables types that i ignore in the checking.</param>
        /// <param name="keywords"> Hashtable to store the keywords.</param>
        /// <param name="includes"> Hashtable to store the includes.</param>
        /// <param name="defines"> Dictionary to store the defines.</param>
        /// <param name="pathes"> Paths for all the places where the imports might be.</param>
        public static bool initializeKeywordsAndSyntext(string ansiPath,string destPath, string cFilePath, string CSyntextPath, string ignoreVariablesTypesPath, Hashtable keywords, Hashtable includes,Dictionary<string,string> defines,string [] eVars,string [] pathes,int threadNumber,Hashtable anciCWords)
        {
            try
            {
                //ansiC File To Keywords ArrayList.
                AddToHashFromFile(ansiPath, keywords,anciCWords, ",", threadNumber);
                AddToArrayListFromFile(ignoreVariablesTypesPath, ignoreVarialbesType, ",", threadNumber);
                //C Syntext File To Syntext ArrayList.
                //AddToListFromFile(CSyntextPath, syntext, " ");
                //PreprocessorActions(cFilePath, cFilePath, 0, keywords, includes, defines, eVars, pathes, threadNumber);
                PreprocessorActions(cFilePath, 0, keywords, includes, defines, eVars, pathes, threadNumber);
            }
            catch(Exception e)
            {
                MainProgram.CleanBeforeCloseThread(threadNumber, e.Message, GeneralConsts.ERROR, cFilePath,destPath);
            }
            return CompileError;
            
        }
    }
}





