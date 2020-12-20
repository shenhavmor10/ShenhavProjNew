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
        enum MemoryPatternEnum
        {
            MALLOC_INDEX_MEMORYARRAY,
            CALLOC_INDEX_MEMORYARRAY,
            ALLOC_INDEX_MEMORYARRAY,
            REALLOC_INDEX_MEMORYARRAY,
            FREE_INDEX_MEMORYARRAY
        }
        const string MAIN_DICT_INDEX = "main";
        //paths for all files.
        const string toolExeFolder = @"..\..\..\ToolsExe";
        const string ignoreVariablesTypesPath = @"..\..\..\ignoreVariablesType.txt";
        //static string filePath = @"C:\Users\Shenhav\Desktop\Check\checkOne.c";
        const string ansiCFile = @"..\..\..\Ansikeywords.txt";
        const string CSyntextFile = @"..\..\..\CSyntext.txt";
        const string logFile = @"..\..\..\LogFile.txt";
        const string FINISH_SUCCESFULL = "Finished succesfully code is ready at the destination path.";
        const int TIMEOUT_SLEEP = 1000;
        static Regex ToolsBlock = new Regex("tools={(.*?)}");
        static Regex MemoryBlock = new Regex("memory={(.*?)}");
        static Regex FreeBlock = new Regex("free={(.*?)}");
        static Mutex mutexAddLogFiles = new Mutex();
        static bool compileError = false;
        static ArrayList currentDataList = new ArrayList();
        static int threadNumber = 0;
        static Dictionary<string, Dictionary<string, Object>> final_json = new Dictionary<string, Dictionary<string, Object>>();
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
        public static Dictionary<string,Dictionary<string,Object>> GetFinalJson()
        {
            return final_json;
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
        static void RunAllChecks(string filePath,string destPath, string [] pathes,ArrayList tools,string fileType,string [] memoryPatterns,string [] freePatterns)
        {
            //variable declaration.
            //create regex for all memory handles (malloc alloc etc... and custom memory handles aswell).
            string memoryPatternTemp = @"(?!.*return)(?= (\s)?([^\s()] + (\s)?((\*)*(\s))?)?[^\s()]+(\s ?= (\s)?(";
            for(int i=0;i<memoryPatterns.Length;i++)
            {
                memoryPatternTemp += memoryPatterns[i]+"|";
            }
            memoryPatternTemp=memoryPatternTemp.Substring(0, memoryPatternTemp.Length - 1);
            memoryPatternTemp += @")\(.+\);$))";
            Regex MemoryPattern = new Regex(memoryPatternTemp);
            //create regex for all free handles plus custom frees.
            string freePatternTemp = @"(?!.*return)(?=(\s)?)?";
            for(int i=0;i<freePatterns.Length;i++)
            {
                freePatternTemp += freePatterns[i] + "|";
            }
            freePatternTemp = freePatternTemp.Substring(0, freePatternTemp.Length - 1);
            Regex FreeMemoryPattern = new Regex(freePatternTemp);
            Hashtable memoryHandleFuncs=new Hashtable();
            Dictionary<string, ArrayList> calledFromFunc = new Dictionary<string, ArrayList>();
            Hashtable keywords = new Hashtable();
            Hashtable includes = new Hashtable();
            Dictionary<string, string> defines = new Dictionary<string, string>();
            Dictionary<string, ArrayList> funcVariables = new Dictionary<string, ArrayList>();
            ArrayList globalVariable = new ArrayList();
            
            //initialize 
            try
            {
                GeneralCompilerFunctions.initializeKeywordsAndSyntext(ansiCFile, filePath, CSyntextFile, ignoreVariablesTypesPath, keywords, includes, defines, pathes, threadNumber);
                Console.WriteLine("after initialize");
            }
            catch(Exception e)
            {
                AddToLogString(filePath, "ERROR IN PREPROCESSOR");
                ConnectionServer.CloseConnection(threadNumber,"ERROR IN PREPROCESSOR "+e.ToString() , GeneralConsts.ERROR);

            }

            AddToLogString(filePath, keywords.Count.ToString());
            //Syntax Check.
            try
            {
                compileError = GeneralCompilerFunctions.SyntaxCheck(filePath, globalVariable,calledFromFunc, memoryHandleFuncs, keywords, funcVariables, threadNumber, fileType, MemoryPattern, FreeMemoryPattern);
            }
            catch(Exception e)
            {
                AddToLogString(filePath, "ERROR IN SyntaxCheck");
                ConnectionServer.CloseConnection(threadNumber, "ERROR IN SyntaxCheck " + e.ToString(), GeneralConsts.ERROR);
            }
            Console.WriteLine("finished");

            if (!compileError)
            {
                GeneralCompilerFunctions.printArrayList(filePath,keywords);
                AddToLogString(filePath, keywords.Count.ToString());
                //just tests.
                try
                {
                    GeneralRestApiServerMethods.CreateFinalJson(filePath, includes, globalVariable, funcVariables, defines, final_json,fileType,memoryHandleFuncs,calledFromFunc);
                    Console.WriteLine("after final json");
                }
                catch (Exception e)
                {
                    AddToLogString(filePath, "ERROR Creating final json");
                    ConnectionServer.CloseConnection(threadNumber, "ERROR Creating final json " + e.ToString(), GeneralConsts.ERROR);
                }
               
                string dataJson = JsonConvert.SerializeObject(final_json[filePath]["codeInfo"]);
                AddToLogString(filePath, "new json " +dataJson);
                Thread threadOpenTools = new Thread(() => RunAllTasks(filePath, destPath, tools));
                threadOpenTools.Start();
                threadOpenTools.Join(GeneralConsts.TIMEOUT_JOIN);
                ConnectionServer.CloseConnection(threadNumber, FINISH_SUCCESFULL,GeneralConsts.FINISHED_SUCCESFULLY);
                AddToLogString(filePath, FINISH_SUCCESFULL);
                Console.WriteLine(logFiles[filePath]);
                Thread writeToFile = new Thread(() => File.WriteAllText(logFile, logFiles[filePath]));
                writeToFile.Start();
                writeToFile.Join(GeneralConsts.TIMEOUT_JOIN);

                
            }

        }
        /// Function - createLogFile
        /// <summary>
        /// creates the log file path and opens it.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns> Returns a path for the file type string.</returns>
        static string createLogFile(string filePath)
        {
            filePath = filePath.Substring(0, filePath.LastIndexOf("\\"));
            System.DateTime moment = DateTime.Today;
            string newPath = filePath + @"\" + moment.Day + "-" + moment.Month+".txt";
            StreamWriter outputFile = new StreamWriter(newPath);
            outputFile.Close();
            return newPath;
        }
        /// Function - RunAllTasks
        /// <summary>
        /// runs all tools picked by the client by the order.
        /// </summary>
        /// <param name="filePath"> the path of the file.</param>
        /// <param name="destPath"> the path of the destionation.</param>
        /// <param name="tools"> The array of the tools sorted from low to high priority.</param>
        static void RunAllTasks(string filePath,string destPath,ArrayList tools)
        {
            string logFilePath=createLogFile(filePath);
            for (int i = START_INDEX_OF_TOOLS; i < tools.Count; i++)
            {
                tools[i] = File.ReadAllText(toolExeFolder + "\\" + tools[i]);
            }
            //runs the tools one by one.
            for (int i= START_INDEX_OF_TOOLS; i<tools.Count;i++)
            {
                using (StreamWriter sw = File.AppendText(logFilePath))
                {
                    sw.WriteLine(string.Format("\n"+"script : "+tools[i] +"\n"));
                }
                RunProcessAsync((string)tools[i],filePath,destPath, logFilePath);
            }
        }
        /// Function - RunProcessAsync
        /// <summary>
        /// starts a tool (task) and sends him 2 parameters src path and dest path.
        /// </summary>
        /// <param name="fileName"> name of the file.</param>
        /// <param name="srcPath"> the source path of the file</param>
        /// <param name="destPath"> the destination of the new file.</param>
        /// <returns></returns>
        static Task<int> RunProcessAsync(string fileName,string srcPath,string destPath,string logTextPath)
        {
            var tcs = new TaskCompletionSource<int>();

            var process = new Process
            {
                StartInfo = { FileName = fileName, Arguments = String.Format("{0} {1} {2}",srcPath,destPath,logTextPath) },
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };
            process.Start();
            process.WaitForExit(20000);
            //process.WaitForExit(); might need for synchronize.
            return tcs.Task;
        }
        /// Function - InitializeMainProgram
        /// <summary>
        /// This function initialize the whole main program. opens all threads needed and more.
        /// </summary>
        static void InitializeMainProgram()
        {
            File.WriteAllText(logFile, GeneralConsts.EMPTY_STRING);
            Thread restApi = new Thread(() => new SyncServer());
            restApi.Start();
            logFiles.Add(MAIN_DICT_INDEX, GeneralConsts.EMPTY_STRING);
            AddToLogString(MAIN_DICT_INDEX, "started rest api");
            //Initialize all the things that needs to come before the syntax check.
            Thread serverThread;
            //start server socket.
            serverThread = new Thread(() => Server.ConnectionServer.ExecuteServer(11111));
            serverThread.Start();
            AddToLogString(MAIN_DICT_INDEX, "started socket for client listen");
        }
        /// Function - MainLoopToGetFiles
        /// <summary>
        /// The function that runs the whole loop of the program that gets new files from the server and
        /// take cares of them and runs RunAllChecks Function.
        /// </summary>
        static void MainLoopToGetFiles()
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
                   
                    //because i still dont have a prefect checks for headers so im giving the thread a default null so the program can run.
                    Thread runChecksThread=null;
                    runChecksThread = new Thread(() => RunAllChecks(filePath, destPath, pathes, tools, filePath.Substring(filePath.Length - 1),memoryArray, freeArray));
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
            InitializeMainProgram();
            MainLoopToGetFiles();
        }
    }
}
