using System;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using ClassesSolution;
using Server;
using System.Linq;

namespace testServer2
{
    public class SyncServer
    {
        //Patterns Declaration.
        static Regex FunctionPatternInC = new Regex(@"^([^ ]+\s)?[^ ]+\s(.*\s)?[^ ]+\([^()]*\)$");
        static Regex functionPatternInH = new Regex(@"^[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]\;$");
        static Regex staticFunctionPatternInC = new Regex(@"^.*static.*\s.*[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]$");
        const string NOT_FOUND_500 = "Could not found pattern";
        const string NOT_FOUND_404 = "Not found";
        static Dictionary<string, Dictionary<string, object>> ToolsData = new Dictionary<string, Dictionary<string, object>>();

        /// Function - TakeOnlyNameNeeded
        /// <summary>
        /// Take only function by the name given.
        /// </summary>
        /// <param name="final_json"> the final json Dictionary.</param>
        /// <param name="parameterName"> Name of the function type string.</param>
        /// <param name="parameterType"></param>
        /// <param name="filePath"></param>
        /// <param name="eVar"></param>
        static string TakeOnlyNameNeeded(Dictionary<string, Dictionary<string, object>> final_json, string parameterName,string parameterType,string eVar)
        {
            string result="";
            bool found = false;
            for (int i=0;i< ((Dictionary<string, FunctionInfoJson>)final_json[eVar]["function"]).Count&&!found;i++)
            {
                var item = ((Dictionary<string, FunctionInfoJson>)final_json[eVar]["function"]).ElementAt(i);
                switch (parameterType)
                {
                    case "name":
                        if (((Dictionary<string, FunctionInfoJson>)final_json[eVar]["function"])[item.Key].fName == parameterName)
                        {
                            result = item.Key;
                        }
                        break;
                    case "returnType":
                        if (((Dictionary<string, FunctionInfoJson>)final_json[eVar]["function"])[item.Key].returnType == parameterName)
                        {
                            result = item.Key;
                        }
                        break;
                }
            }
            return result;
        }
        /// Function - SendData
        /// <summary>
        /// Send data tot the context given. if not found flag is on it returns status code 500.
        /// </summary>
        /// <param name="data"> data type string.</param>
        /// <param name="not_found"> type bool.</param>
        /// <param name="context"> context type HttpListenerContext.</param>
        static void SendData(string data,bool not_found,HttpListenerContext context)
        {
            byte[] bytes;
            if (not_found)
            {
                bytes = Encoding.UTF8.GetBytes(NOT_FOUND_500);
                context.Response.StatusCode = 500;
            }
            else
            {
                if (data.Length < 0)
                {
                    bytes = Encoding.UTF8.GetBytes(NOT_FOUND_404);
                    context.Response.StatusCode = 404;
                }
                else
                {
                    bytes = Encoding.UTF8.GetBytes(data);
                }
            }
            Stream OutputStream = context.Response.OutputStream;
            //sends the message back.
            OutputStream.Write(bytes, 0, bytes.Length);
            //Close connection.
            OutputStream.Close();
        }
        static void Rd_isResetDictionary(object sender, ResetDictEventArgs e)
        {
            if(ToolsData.ContainsKey(e.filePath))
            {
                ToolsData[e.filePath].Clear();
            }
            
        }
        /// Function - SyncServer
        /// <summary>
        /// Creation of the rest api server.
        /// </summary>
        /// <param name="filePath"> Path for the code file.</param>
        /// <param name="includes"> Hashtable for all of the includes in the code.</param>
        /// <param name="defines"> Dictionary that stores all of the defines in the code.</param>
        public SyncServer(ResetDictionary rd)
        {
            rd.isResetDictionary += new IsResetDict(Rd_isResetDictionary);
            var listener = new HttpListener();
            //add prefixes.
            listener.Prefixes.Add("http://localhost:8081/");
            listener.Prefixes.Add("http://127.0.0.1:8081/");
            //start listening.
            listener.Start();

            while (ConnectionServer.GetCloseAllBool()==false)
            {
                try
                {
                    //if gets connection.
                    var context = listener.GetContext(); //Block until a connection comes in
                    context.Response.StatusCode = 200;
                    context.Response.SendChunked = true;
                    context.Response.ContentType = "application/json";
                    string dataJson = GeneralConsts.EMPTY_STRING;
                    Dictionary<string,Dictionary<string, Dictionary<string, object>>> final_json = MainProgram.GetFinalJson();
                    Dictionary<string, Dictionary<string,string>> customReadyPatterns = MainProgram.GetReadyPatterns();
                    string filePath = context.Request.QueryString["filePath"];
                    if(!ToolsData.ContainsKey(filePath))
                    {
                        ToolsData.Add(filePath,new Dictionary<string, object>());
                    }
                    string eVar = context.Request.QueryString["eVar"];
                    bool not_found_pattern = false;
                    bool not_found = false;
                    char[] trimChars = { '/', ' '};
                    int totalTime = 0;
                    string path = GeneralConsts.EMPTY_STRING;
                    Regex r;
                    //All GET commands.
                    if (context.Request.HttpMethod == "GET")
                    {
                        //make sure eVars get in here aswell.
                        //taking care of the option of EqualVariables which means that you give a variable from a function and a function name
                        //and get all of the variables that are equal to him (memory equal).
                        if(context.Request.QueryString["functionName"]!=null&&context.Request.QueryString["variable"]!=null)
                        {
                            try
                            {
                                string keyName = TakeOnlyNameNeeded(final_json[filePath], context.Request.QueryString["functionName"], "name", eVar);
                                if (!((Dictionary<string, FunctionInfoJson>)final_json[filePath][eVar]["function"]).ContainsKey(keyName))
                                {
                                    not_found = true;
                                }
                                string[] result = GeneralRestApiServerMethods.GetEqualVariables(((Dictionary<string, FunctionInfoJson>)final_json[filePath][eVar]["function"])[keyName], context.Request.QueryString["variable"]);
                                dataJson = JsonConvert.SerializeObject(result);
                            }
                            catch(Exception e)
                            {
                                SendData(JsonConvert.SerializeObject(e.Message), not_found, context);
                            }
                        }
                        //taking care of a pattern that the user wants to search.
                        if(context.Request.QueryString["pattern"]!=null)
                        {
                            string returnSize = context.Request.QueryString["returnSize"];
                            string[] result;
                            try
                            {
                                if (context.Request.QueryString["functionName"] != null)
                                {
                                    string functionKeyName = TakeOnlyNameNeeded(final_json[filePath], context.Request.QueryString["functionName"], "name", eVar);
                                    result = GeneralRestApiServerMethods.SearchPattern(context.Request.QueryString["pattern"], returnSize, ((Dictionary<string, FunctionInfoJson>)final_json[filePath][eVar]["function"])[functionKeyName].content);
                                }
                                else
                                {
                                    result = GeneralRestApiServerMethods.SearchPattern(context.Request.QueryString["pattern"], returnSize, ((CodeInfoJson)final_json[filePath][eVar]["codeInfo"]).codeContent);
                                }
                                dataJson = JsonConvert.SerializeObject(result);
                            }
                            catch(Exception e)
                            {
                                SendData(JsonConvert.SerializeObject(e.Message), not_found, context);
                            }
                            
                        }
                        //taking care of a ready pattern that is being taked or from the patterns file or from the customReadyPatterns Dictionary.
                        else if(context.Request.QueryString["readyPattern"] != null)
                        {
                            if (GeneralRestApiServerMethods.TakePatternFromFile(context.Request.QueryString["readyPattern"]) == GeneralConsts.EMPTY_STRING&&!customReadyPatterns[filePath].ContainsKey(context.Request.QueryString["readyPattern"]))
                                not_found_pattern = true;
                            else
                            {
                                string patternFromFile;
                                if (customReadyPatterns[filePath].ContainsKey(context.Request.QueryString["readyPattern"]))
                                {
                                    patternFromFile = customReadyPatterns[filePath][context.Request.QueryString["readyPattern"]];
                                }
                                else
                                {
                                    patternFromFile = (GeneralRestApiServerMethods.TakePatternFromFile(context.Request.QueryString["readyPattern"]));
                                }
                                string returnSize = context.Request.QueryString["returnSize"];
                                string[] result;
                                if (context.Request.QueryString["functionName"] != null)
                                {
                                    Console.WriteLine(context.Request.QueryString["functionName"]);
                                    string funcKey=TakeOnlyNameNeeded(final_json[filePath],context.Request.QueryString["functionName"],"name",eVar);
                                    result = GeneralRestApiServerMethods.SearchPattern(patternFromFile, returnSize, ((Dictionary<string, FunctionInfoJson>)final_json[filePath][eVar]["function"])[funcKey].content);
                                }
                                else
                                {
                                    result = GeneralRestApiServerMethods.SearchPattern(patternFromFile, returnSize, ((CodeInfoJson)final_json[filePath][eVar]["codeInfo"]).codeContent);
                                }
                                
                                dataJson = JsonConvert.SerializeObject(result);
                            }
                        }
                        else
                        {
                            path = context.Request.RawUrl;
                            path = path.Trim(trimChars);
                            path = path.Split('?')[0];
                            //switch case for get commands.
                            switch (path)
                            {
                                case "functions":
                                    //this is wrong needs to fix.
                                    string parameterName;
                                    if (context.Request.QueryString["name"] != null)
                                    {
                                        parameterName=TakeOnlyNameNeeded(final_json[filePath], context.Request.QueryString["name"],"name",eVar);
                                        dataJson = JsonConvert.SerializeObject(((Dictionary<string,FunctionInfoJson>)final_json[filePath][eVar]["function"])[parameterName]);
                                    }
                                    //return type means all the functions which return that type.
                                    else if (context.Request.QueryString["returnType"] != null)
                                    {
                                        parameterName=TakeOnlyNameNeeded(final_json[filePath], context.Request.QueryString["returnType"], "returnType", eVar);
                                        dataJson = JsonConvert.SerializeObject(((Dictionary<string, FunctionInfoJson>)final_json[filePath][eVar]["function"])[parameterName]);
                                    }
                                    else
                                    {
                                        dataJson = JsonConvert.SerializeObject(final_json[filePath][eVar]["function"]);
                                    }
                                    break;
                                    //code info situation option.
                                case "codeInfo":
                                    dataJson = JsonConvert.SerializeObject(final_json[filePath][eVar]["codeInfo"]);
                                    break;
                                case "result":
                                    //in order to get result of another tool.
                                    string toolName=context.Request.QueryString["toolName"];
                                    if(ToolsData[filePath].ContainsKey(toolName))
                                    {
                                        dataJson = JsonConvert.SerializeObject(ToolsData[filePath][toolName]);
                                    }
                                    else
                                    {
                                        not_found = true;
                                    }
                                    break;
                                default:
                                    break;
                                    

                            }
                        }
                        SendData(dataJson, not_found, context);

                    }
                    if(context.Request.HttpMethod=="POST")
                    {
                        path = context.Request.RawUrl;
                        path = path.Trim(trimChars);
                        path = path.Split('?')[0];
                        dataJson = GeneralConsts.EMPTY_STRING;
                        switch (path)
                        {
                            case "logs":
                                using (var reader = new StreamReader(context.Request.InputStream))
                                    dataJson = reader.ReadToEnd();
                                if(dataJson != "")
                                {
                                    MainProgram.AddToLogString(filePath, dataJson);
                                }
                                else
                                {
                                    not_found = true;
                                }
                                break;
                            case "result":
                                MainProgram.AddToLogString(filePath, context.Request.QueryString["result"]);
                                string toolName=context.Request.QueryString["toolName"];
                                using (var reader = new StreamReader(context.Request.InputStream))
                                    dataJson = reader.ReadToEnd();
                                ToolsData[filePath].Add(toolName, dataJson);
                                break;
                            default:
                                break;
                        }
                        SendData(dataJson, not_found, context);
                    }
                }


                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    // Client disconnected or some other error - ignored for this example
                }
            }
        }
        private void Rd_isResetDictionary1(object sender, ResetDictEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
