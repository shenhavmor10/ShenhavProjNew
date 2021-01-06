using System;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using ClassesSolution;
using Server;

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
        /// Function - SyncServer
        /// <summary>
        /// Creation of the rest api server.
        /// </summary>
        /// <param name="filePath"> Path for the code file.</param>
        /// <param name="includes"> Hashtable for all of the includes in the code.</param>
        /// <param name="defines"> Dictionary that stores all of the defines in the code.</param>
        static void TakeOnlyNameNeeded(Dictionary<string, Dictionary<string, object>> final_json, string parameterName,string parameterType,string filePath,string eVar)
        {
            foreach (string functionName in ((Dictionary<string,FunctionInfoJson>)final_json[eVar]["functions"]).Keys)
            {
                switch(parameterType)
                {
                    case "name":
                        if (((Dictionary<string, FunctionInfoJson>)final_json[eVar]["functions"])[functionName].fName != parameterName)
                        {
                            final_json["functions"].Remove(functionName);
                        }
                        break;
                    case "returnType":
                        if (((Dictionary<string, FunctionInfoJson>)final_json[eVar]["functions"])[functionName].returnType != parameterName)
                        {
                            final_json["functions"].Remove(functionName);
                        }
                        break;
                }
            }
        }
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
            ToolsData[e.filePath].Clear();
        }
        public SyncServer()
        {
            ResetDictionary rd = new ResetDictionary();
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
                    string filePath = context.Request.QueryString["filePath"];
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
                        if(context.Request.QueryString["pattern"]!=null)
                        {
                            MainProgram.AddToLogString(filePath,context.Request.QueryString["pattern"]);
                            MainProgram.AddToLogString(filePath, context.Request.QueryString["returnSize"]);
                            r = new Regex(context.Request.QueryString["pattern"]);
                            string returnSize = context.Request.QueryString["returnSize"];
                            string [] result=GeneralRestApiServerMethods.SearchPattern(r, returnSize, filePath);
                            dataJson = JsonConvert.SerializeObject(result);
                            MainProgram.AddToLogString(filePath, dataJson);
                        }
                        else if(context.Request.QueryString["readyPattern"] != null)
                        {
                            MainProgram.AddToLogString(filePath, context.Request.QueryString["readyPattern"]);
                            MainProgram.AddToLogString(filePath, context.Request.QueryString["returnSize"]);
                            if (GeneralRestApiServerMethods.TakePatternFromFile(context.Request.QueryString["readyPattern"]) == GeneralConsts.EMPTY_STRING)
                                not_found_pattern = true;
                            else
                            {
                                r = new Regex(GeneralRestApiServerMethods.TakePatternFromFile(context.Request.QueryString["readyPattern"]));
                                string returnSize = context.Request.QueryString["returnSize"];
                                string[] result = GeneralRestApiServerMethods.SearchPattern(r, returnSize, filePath);
                                dataJson = JsonConvert.SerializeObject(result);
                            }
                        }
                        else
                        {
                            path = context.Request.RawUrl;
                            path = path.Trim(trimChars);
                            path = path.Split('?')[0];
                            MainProgram.AddToLogString(filePath, path);
                            //switch case for get commands.
                            switch (path)
                            {
                                case "functions":
                                    //this is wrong needs to fix.
                                    if (context.Request.QueryString["name"] != null)
                                    {
                                        TakeOnlyNameNeeded(final_json[path], context.Request.QueryString["name"],"name",filePath,eVar);
                                    }
                                    if (context.Request.QueryString["returnType"] != null)
                                    {
                                        TakeOnlyNameNeeded(final_json[path], context.Request.QueryString["returnType"], "returnType", filePath, eVar);
                                    }
                                    dataJson = JsonConvert.SerializeObject(final_json[filePath][eVar]["function"]);
                                    MainProgram.AddToLogString(filePath, dataJson);
                                    break;
                                case "codeInfo":
                                    dataJson = JsonConvert.SerializeObject(final_json[filePath][eVar]["codeInfo"]);
                                    MainProgram.AddToLogString(filePath, dataJson);
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
                        if (path == "logs")
                        {
                            dataJson=GeneralConsts.EMPTY_STRING;
                            MainProgram.AddToLogString(filePath, context.Request.QueryString["logs"]);
                            MainProgram.AddToLogString(filePath, context.Request.QueryString["returnSize"]);
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
                        }
                        SendData(dataJson, not_found, context);
                    }
                }


                catch (Exception)
                {
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
