using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Server;
using ClassesSolution;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;

namespace testServer2
{
    class MainProgram
    {
        const int FILE_PATH_INDEX = 0;
        const int START_TOOLS_INDEX = 5;
        const int DEST_PATH_INDEX = 4;
        const int GCC_INCLUDE_FOLDER_INDEX = 2;
        const int EXTRA_INCLUDE_FOLDER_INDEX = 3;
        const int PROJECT_FOLDER_INDEX = 1;
        const int START_INDEX_OF_TOOLS = 0;
        const string MAIN_DICT_INDEX = "main";
        //paths for all files.
        const string configFile = @"..\..\..\ConfigurationFIle.txt";
        static string toolExeFolder, ignoreVariablesTypesPath, ansiCFile, CSyntextFile, logFile;
        const string FINISH_SUCCESFULL = "Finished succesfully code is ready at the destination path.";
        const int TIMEOUT_SLEEP = 1000;
        static Regex ToolsBlock = new Regex("tools={(.*?)}");
        static Regex MemoryBlock = new Regex("memory={(.*?)}");
        static Regex FreeBlock = new Regex("free={(.*?)}");
        static Regex EnvironmentVariablesPathBlock = new Regex("environmentVariablePath={(.*?)}");
        static Regex eVarsProtocol = new Regex(@"(?i)^(\s)?#define\s[^\s]*(\s)?$");
        static Mutex mutexAddLogFiles = new Mutex();
        static ArrayList currentDataList = new ArrayList();
        static int threadNumber = 0;
        static Dictionary<string,Dictionary<string, Dictionary<string, Object>>> final_json = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
        static Dictionary<string, string> readyPatterns = new Dictionary<string, string>();
        static Dictionary<string, string> logFiles = new Dictionary<string, string>();
        //static string librariesPath = @"C:\Users\Shenhav\Desktop\Check";
        //global variable declaration.

        //static ArrayList syntext = new ArrayList(); dont know if needed.

        /// Function - GetFinalJson
        /// <summary>
        /// Function returns the final json.
        /// final json is the json that is being created after the compilation checks, this json contains
        /// all information of the code and the rest api is prasing the information to the GET requests.
        /// </summary>
        /// <returns>final json type Dictionary<string,Dictionary<string,Object>> </returns>
        public static Dictionary<string, Dictionary<string, Dictionary<string, Object>>> GetFinalJson()
        {
            return final_json;
        }
        public static Dictionary<string,string> GetReadyPatterns()
        {
            return readyPatterns;
        }
        /// Function - CleanBeforeCloseThread
        /// <summary>
        /// function gets message threadNumber closeType and filePath cleans all resources that connects to the filePath 
        /// and after that close the connection with the path gotten.
        /// </summary>
        /// <param name="threadNumber"> the number of the thread type int.</param>
        /// <param name="message"> message to send to client type int.</param>
        /// <param name="closeType"> what is the type of the close connection for an example "ERROR".</param>
        /// <param name="filePath"> filePath type string.</param>
        public static void CleanBeforeCloseThread(int threadNumber,string message,int closeType,string filePath)
        {
            string logFilePath = createLogFile(threadNumber, filePath);
            AddToLogString(filePath, message);
            ConnectionServer.CloseConnection(threadNumber, message, closeType);
            if(final_json.ContainsKey(filePath))
            {
                final_json.Remove(filePath);
            }
            File.WriteAllText(logFilePath, logFiles[filePath]);
        }
        /// Function - AddToLogString
        /// <summary>
        /// logs is the string that saves all logs and at the end it writes it to the logs txt.
        /// </summary>
        /// <param name="content">what you add to the string</param>
        public static void AddToLogString(string filePath,string content)
        {
            mutexAddLogFiles.WaitOne();
            logFiles[filePath] += content + GeneralConsts.NEW_LINE;
            mutexAddLogFiles.ReleaseMutex();
        }
        
        /// Function - RunAllChecks
        /// <summary>
        /// Thread starts all checks.
        /// </summary>
        /// <param name="filePath"> the path of the file that is being checked.</param>
        /// <param name="pathes"> all pathes that the imports might be in.</param>
        /// <param name="destPath"> the destination path of the file.</param>
        /// <param name="fileType"> the type of the file.</param>
        /// <param name="freePatterns"> all free patterns.</param>
        /// <param name="memoryPatterns"> all memory handles patterns.</param>
        /// <param name="tools"> all tools type arrayList.</param>
        static void RunAllChecks(string filePath,string destPath, string [] pathes,ArrayList tools,ResetDictionary rd,string fileType,string [] memoryPatterns,string [] freePatterns)
        {
            //variable declaration.
            //create regex for all memory handles (malloc alloc etc... and custom memory handles aswell).
            bool compileError = false;
            int currentThreadNumber = threadNumber;
            string memoryPatternTemp = @"(?!.*return)(?= (\s)?([^\s()] + (\s)?((\*)*(\s))?)?[^\s()]+(\s ?= (\s)?(malloc|alloc|realloc|calloc|";
            string customMalloc = @"(?<=\n\r\t)(\s)*?[^\n]+(\s*?)=(\s*?)(malloc|calloc|alloc|realloc";
            for (int i=0;i<memoryPatterns.Length;i++)
            {
                memoryPatternTemp += memoryPatterns[i]+"|";
                customMalloc += memoryPatterns[i] + "|";
            }
            memoryPatternTemp=memoryPatternTemp.Substring(0, memoryPatternTemp.Length - 1);
            customMalloc = customMalloc.Substring(0, customMalloc.Length - 1);
            customMalloc += @")\([^\n]+\);(\s)*?(?=\n\r\t)";
            memoryPatternTemp += @")\(.+\);$))";
            readyPatterns.Add("CustomMalloc", customMalloc);
            Regex MemoryPattern = new Regex(memoryPatternTemp);
            //create regex for all free handles plus custom frees.
            string customFree = @"(?<=\n\r\t)(\s)*?(free|";
            string freePatternTemp = @"(?!.*return)(?=(\s)?(free|";
            for(int i=0;i<freePatterns.Length;i++)
            {
                freePatternTemp += freePatterns[i] + "|";
                customFree += freePatterns[i] + "|";
            }
            customFree = customFree.Substring(0, customFree.Length - 1);
            customFree += @")\(.+\);(\s)*?(?=\n\r)";
            freePatternTemp = freePatternTemp.Substring(0, freePatternTemp.Length - 1);
            freePatternTemp += @")\(.+\);$)";
            Console.WriteLine(customFree);
            readyPatterns.Add("CustomFree", customFree);
            Regex FreeMemoryPattern = new Regex(freePatternTemp);
            foreach(string eVars in final_json[filePath].Keys)
            {
                Hashtable memoryHandleFuncs = new Hashtable();
                Dictionary<string, ArrayList> calledFromFunc = new Dictionary<string, ArrayList>();
                Dictionary<string, Dictionary<string,string[]>> callsFromThisFunction = new Dictionary<string, Dictionary<string, string[]>>();
                Hashtable keywords = new Hashtable();
                Hashtable includes = new Hashtable();
                Dictionary<string, string> defines = new Dictionary<string, string>();
                Dictionary<string, ArrayList> funcVariables = new Dictionary<string, ArrayList>();
                Dictionary<string, string> functionsContent = new Dictionary<string, string>();
                ArrayList globalVariable = new ArrayList();
                string codeContent = "";
                //initialize 
                try
                {
                    compileError=GeneralCompilerFunctions.initializeKeywordsAndSyntext(ansiCFile, filePath, CSyntextFile, ignoreVariablesTypesPath, keywords, includes, defines, eVars.Split(','), pathes, currentThreadNumber);
                    Console.WriteLine("after initialize");
                }
                catch (Exception e)
                {
                    AddToLogString(filePath, "ERROR IN PREPROCESSOR "+e.Message);

                }
                if(!compileError)
                {
                    AddToLogString(filePath, keywords.Count.ToString());
                    //Syntax Check.
                    try
                    {
                        compileError = GeneralCompilerFunctions.SyntaxCheck(filePath, globalVariable, calledFromFunc,callsFromThisFunction, memoryHandleFuncs, keywords, funcVariables, eVars.Split(','), currentThreadNumber, fileType, MemoryPattern, FreeMemoryPattern,functionsContent, ref codeContent);
                    }
                    catch (Exception e)
                    {
                        AddToLogString(filePath, "ERROR IN SyntaxCheck " + e.Message);
                    }
                    if (!compileError)
                    {
                        GeneralCompilerFunctions.printArrayList(filePath, keywords);
                        AddToLogString(filePath, keywords.Count.ToString());
                        //just tests.
                        try
                        {
                            GeneralRestApiServerMethods.CreateFinalJson(filePath, includes, globalVariable, funcVariables, defines, final_json, eVars, fileType, memoryHandleFuncs, calledFromFunc,callsFromThisFunction,functionsContent,codeContent);
                            Console.WriteLine("after final json");
                        }
                        catch (Exception e)
                        {
                            AddToLogString(filePath, "ERROR Creating final json");
                            ConnectionServer.CloseConnection(currentThreadNumber, "ERROR Creating final json " + e.ToString(), GeneralConsts.ERROR);
                        }
                    }
                }
            }
            if(!compileError)
            {
                foreach (string eVars in final_json[filePath].Keys)
                {
                    
                    Thread threadOpenTools = new Thread(() => RunAllTasks(filePath, destPath, tools, currentThreadNumber,eVars));
                    threadOpenTools.Start();
                    Console.WriteLine("enter thread with evar = " + eVars);
                    threadOpenTools.Join();
                    Console.WriteLine("join "+eVars);
                    rd.Reset_Dictionary(filePath);
                    

                    /*AddToLogString(filePath, FINISH_SUCCESFULL);
                    Console.WriteLine(logFiles[filePath]);
                    Thread writeToFile = new Thread(() => File.WriteAllText(logFile, logFiles[filePath]));
                    writeToFile.Start();
                    writeToFile.Join(GeneralConsts.TIMEOUT_JOIN);*/
                }
                CleanBeforeCloseThread(currentThreadNumber, FINISH_SUCCESFULL, GeneralConsts.FINISHED_SUCCESFULLY, filePath);
            }
        }

        static void AddToolToLogFile(string filePath,string eVar,string toolName)
        {
            AddToLogString(filePath, eVar);
            AddToLogString(filePath, toolName);
        }

        /// Function - createLogFile
        /// <summary>
        /// creates the log file path and opens it.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns> Returns a path for the file type string.</returns>
        static string createLogFile(int threadNumber,string filePath)
        {
            filePath = filePath.Substring(0, filePath.LastIndexOf("\\"));
            System.DateTime moment = DateTime.Today;
            string newPath = filePath + @"\" + moment.Day + "-" + moment.Month+".txt";
            try
            {
                StreamWriter outputFile = new StreamWriter(newPath);
                outputFile.Close();
            }
            catch(Exception e)
            {
                CleanBeforeCloseThread(threadNumber, "Could not open file for writing", GeneralConsts.ERROR, filePath);
            }
            return newPath;
        }
        /// Function - RunAllTasks
        /// <summary>
        /// runs all tools picked by the client by the order.
        /// </summary>
        /// <param name="filePath"> the path of the file.</param>
        /// <param name="destPath"> the path of the destionation.</param>
        /// <param name="tools"> The array of the tools sorted from low to high priority.</param>
        static void RunAllTasks(string filePath,string destPath,ArrayList tools,int currentThreadNumber,string eVar)
        {
            ArrayList toolsScripts = new ArrayList();
            for (int i = START_INDEX_OF_TOOLS; i < tools.Count; i++)
            {
                toolsScripts.Add(File.ReadAllText(toolExeFolder + "\\" + tools[i]));
            }
            //runs the tools one by one.
            for (int i= START_INDEX_OF_TOOLS; i< toolsScripts.Count;i++)
            {
                AddToolToLogFile(filePath, eVar, (string)toolsScripts[i]);
                RunProcessAsync((string)toolsScripts[i],filePath,destPath,eVar);
            }
            toolsScripts.Clear();
        }
        /// Function - RunProcessAsync
        /// <summary>
        /// starts a tool (task) and sends him 2 parameters src path and dest path.
        /// </summary>
        /// <param name="fileName"> name of the file.</param>
        /// <param name="srcPath"> the source path of the file</param>
        /// <param name="destPath"> the destination of the new file.</param>
        /// <returns></returns>
        static Task<int> RunProcessAsync(string fileName,string srcPath,string destPath,string eVar)
        {
            Console.WriteLine(Directory.GetCurrentDirectory());
            Console.WriteLine("running async waiting for exit.");
            Console.WriteLine("FileName ="+fileName);
            var tcs = new TaskCompletionSource<int>();

            var process = new Process
            {
                StartInfo = { FileName = fileName, Arguments = String.Format("{0} {1} {2}",srcPath,destPath,eVar) },
                EnableRaisingEvents = true
            };

            /*process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };*/
            process.Start();
            process.WaitForExit();
            //process.WaitForExit(); might need for synchronize.
            Console.WriteLine("exited..");
            return tcs.Task;
        }
        /// Function - initializeConfig
        /// <summary>
        /// Function opens up config file and set all paths.
        /// </summary>
        static void initializeConfig()
        {
            Dictionary<string, string> configDict = new Dictionary<string, string>();
            try
            {
                using (var sr = new StreamReader(configFile))
                {
                    string line = null;

                    // while it reads a key
                    while ((line = sr.ReadLine()) != null)
                    {
                        // add the key and whatever it 
                        // can read next as the value
                        configDict.Add(line.Split('=')[0], line.Split('=')[1]);
                        
                    }
                }
                toolExeFolder = configDict["toolExeFolder"];
                ignoreVariablesTypesPath = configDict["ignoreVariablesTypesPath"];
                ansiCFile = configDict["ansiCFile"];
                CSyntextFile = configDict["CSyntextFile"];
                logFile = configDict["logFile"];
            }
            catch(Exception e)
            {
                AddToLogString(MAIN_DICT_INDEX, "error = " + e.ToString());
            }
            configDict.Clear();
        }
        /// Function - InitializeMainProgram
        /// <summary>
        /// This function initialize the whole main program. opens all threads needed and more.
        /// </summary>
        static ResetDictionary InitializeMainProgram()
        {
            initializeConfig();
            File.WriteAllText(logFile, GeneralConsts.EMPTY_STRING);
            ResetDictionary rd = new ResetDictionary();
            Thread restApi = new Thread(() => new SyncServer(rd));
            restApi.Start();
            
            AddToLogString(MAIN_DICT_INDEX, "started rest api");
            //Initialize all the things that needs to come before the syntax check.
            Thread serverThread;
            //start server socket.
            serverThread = new Thread(() => Server.ConnectionServer.ExecuteServer(11111));
            serverThread.Start();
            AddToLogString(MAIN_DICT_INDEX, "started socket for client listen");
            return rd;
        }
        /// Function - GetAllPossibilitiesWithoutDuplicates
        /// <summary>
        /// Function gets all possibilites. example = if we have three Evars - A,B,C
        /// It returns us an arrayList with all possibilities including nothing - |A|B|A,B|C|A,C|B,C|A,B,C|Nothing|.
        /// </summary>
        /// <param name="array"> an array that contains at first the environment variables, type array string.</param>
        /// <returns></returns>
        static ArrayList GetAllPossibilitiesWithoutDuplicates(string [] array)
        {
            ArrayList result = new ArrayList();
            int finalSize = (int)Math.Pow(array.Length, 2) - 1;
            int currentIndex = 0;
            int currentResultLength;
            while (finalSize>result.Count)
            {
                result.Add(array[currentIndex]);
                currentResultLength = result.Count;
                for(int i=0;i< currentResultLength - 1;i++)
                {
                    result.Add(string.Format((string)result[i] +","+array[currentIndex]));
                }
                currentIndex++;
            }
            return result;
        }
        /// Function - SetEnvironmentVariables
        /// <summary>
        /// Function sets all environmentVariables in the final_json.
        /// </summary>
        /// <param name="filePath"> the file path type string.</param>
        /// <param name="environmentVariablesPath"> the environment variables path.</param>
        /// <param name="currentThreadNumber"> the current thread number type int.</param>
        static void SetEnvironmentVariables(string filePath,string environmentVariablesPath,int currentThreadNumber)
        {
            MyStream sr = null;
            try
            {
                sr = new MyStream(environmentVariablesPath, System.Text.Encoding.UTF8);
            }
            catch(Exception e)
            {
                CleanBeforeCloseThread(currentThreadNumber, "error = Couldnt open the environment variables file. error explained - "+e.Message, GeneralConsts.ERROR, filePath);
            }
            string line = "";
            ArrayList tempArrayForEnvironmentVars = new ArrayList();
            while((line=sr.ReadLine())!=null)
            {
                if(!eVarsProtocol.IsMatch(line))
                {
                    AddToLogString(MAIN_DICT_INDEX, "error = Wrong environment variables protocol..");
                }
                else
                {
                    AddToLogString(MAIN_DICT_INDEX, "Evar - "+ line.Split(' ')[1].Trim());
                    tempArrayForEnvironmentVars.Add(line.Split(' ')[1].Trim());
                }
            }
            tempArrayForEnvironmentVars = GetAllPossibilitiesWithoutDuplicates((string[])tempArrayForEnvironmentVars.ToArray(typeof(string)));
            tempArrayForEnvironmentVars.Add("NoEvarTurnedOn");
            foreach (string eVar in tempArrayForEnvironmentVars)
            {
                try
                {
                    final_json[filePath].Add(eVar, new Dictionary<string, object>());
                }
                catch(Exception e)
                {
                    CleanBeforeCloseThread(currentThreadNumber, "error = there are more than one environment variable with the same name.", GeneralConsts.ERROR, filePath);
                }
            }


            
        }
        /// Function - MainLoopToGetFiles
        /// <summary>
        /// The function that runs the whole loop of the program that gets new files from the server and
        /// take cares of them and runs RunAllChecks Function.
        /// </summary>
        static void MainLoopToGetFiles(ResetDictionary rd)
        {
            while (ConnectionServer.GetCloseAllBool() == false)
            {
                //checks if something got added to the server list by the gui. if it did 
                //it copies it to the main current list and start to run all the checks on the paths
                //got by the gui (the data inside the List is the user paths.).
                ArrayList list = Server.ConnectionServer.GetThreadsData();
                if (list.Count > currentDataList.Count)
                {
                    //adds to the current data list the original server data list last node.
                    ArrayList tools = new ArrayList();
                    currentDataList.Add(list[currentDataList.Count]);
                    AddToLogString(MAIN_DICT_INDEX, currentDataList[currentDataList.Count - 1].ToString());
                    string infoServer = currentDataList[currentDataList.Count - 1].ToString();
                    string[] paths = Regex.Split(infoServer.Substring(0, infoServer.IndexOf("tools")), ",");
                    Console.WriteLine("resulttttt "+ToolsBlock.Match(infoServer).Groups[1].Value);
                    string [] toolsArray = Regex.Split(ToolsBlock.Match(infoServer).Groups[1].Value,",");
                    string [] memoryArray = Regex.Split(MemoryBlock.Match(infoServer).Groups[1].Value, ",");
                    string[] freeArray = Regex.Split(FreeBlock.Match(infoServer).Groups[1].Value, ",");
                    if(freeArray.Length==1)
                    {
                        if(freeArray[0]=="")
                        {
                            freeArray = new string[0];
                        }
                    }
                    if (memoryArray.Length == 1)
                    {
                        if (memoryArray[0] == "")
                        {
                            memoryArray = new string[0];
                        }
                    }
                    string environmentVariablePath= EnvironmentVariablesPathBlock.Match(infoServer).Groups[1].Value;
                    string filePath = paths[FILE_PATH_INDEX];
                    if (logFiles.ContainsKey(filePath))
                    {
                        logFiles[filePath] = GeneralConsts.EMPTY_STRING;
                    }
                    else
                    {
                        logFiles.Add(filePath, GeneralConsts.EMPTY_STRING);
                    }
                    AddToLogString(filePath, "FilePath - " + filePath);
                    string[] pathes = { paths[PROJECT_FOLDER_INDEX], paths[GCC_INCLUDE_FOLDER_INDEX], paths[EXTRA_INCLUDE_FOLDER_INDEX] };
                    string destPath = paths[DEST_PATH_INDEX];
                    tools.AddRange(toolsArray);
                    final_json.Add(filePath, new Dictionary<string, Dictionary<string, object>>());
                    SetEnvironmentVariables(filePath, environmentVariablePath, threadNumber);
                    //because i still dont have a prefect checks for headers so im giving the thread a default null so the program can run.
                    Thread runChecksThread=null;
                    runChecksThread = new Thread(() => RunAllChecks(filePath, destPath, pathes, tools, rd, filePath.Substring(filePath.Length - 1),memoryArray, freeArray));
                    runChecksThread.Start();

                }
                else
                {
                    Thread.Sleep(TIMEOUT_SLEEP);
                }

            }
        }
        /// Function - Main
        /// <summary>
        /// Main Function.
        /// </summary>
        /// <param name="args"> arguments.</param>
        static void Main(string[] args)
        {
            //open Rest API.
            logFiles.Add(MAIN_DICT_INDEX, GeneralConsts.EMPTY_STRING);
            ResetDictionary rd= InitializeMainProgram();
            MainLoopToGetFiles(rd);
        }
    }
}
