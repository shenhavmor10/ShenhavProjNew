using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClassesSolution;
using Newtonsoft.Json;

namespace MemoryHandleTool
{
    class Program
    {
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";
        static Dictionary<string, FunctionInfoJson> resultsJson;
        static string logs="";
        static async Task GetFromRestApi(string sourcePath, string destPath, string eVar)
        {
            //Communicating with rest api server
            Console.WriteLine("entered ");
            HttpClient client = new HttpClient();
            string regexPattern = "";
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //Functions GET.
            Console.WriteLine("before async");
            Console.WriteLine("Evar = " + eVar);
            Console.WriteLine("destPath = " + destPath);
            HttpResponseMessage response = await client.GetAsync(string.Format("http://127.0.0.1:8081/functions?filePath={0}&eVar={1}", sourcePath, eVar));
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseBody);
            Console.WriteLine(resultsJson);
            Console.WriteLine("after json");
            resultsJson = JsonConvert.DeserializeObject<Dictionary<string, FunctionInfoJson>> (responseBody);
            Console.WriteLine("after desrialize");
            foreach(string key in resultsJson.Keys)
            {
                string mallocPattern = "";
                if(resultsJson[key].memoryAllocation)
                {
                    Console.WriteLine("entered if");
                    var response2 = await client.GetAsync(string.Format("http://127.0.0.1:8081?filePath={0}&eVar={1}&functionName={2}&readyPattern={3}&returnSize={4}", sourcePath, eVar, System.Net.WebUtility.UrlEncode(resultsJson[key].fName), "CustomMalloc", "line"));
                    string responseBody2 = await response2.Content.ReadAsStringAsync();
                    string[] mallocs = JsonConvert.DeserializeObject<string[]>(responseBody2);
                    Console.WriteLine("after getting mallocs");
                    response2= await client.GetAsync(string.Format("http://127.0.0.1:8081?filePath={0}&eVar={1}&functionName={2}&readyPattern={3}&returnSize={4}", sourcePath, eVar, System.Net.WebUtility.UrlEncode(resultsJson[key].fName), "CustomFree", "line"));
                    responseBody2 = await response2.Content.ReadAsStringAsync();
                    string [] frees = JsonConvert.DeserializeObject<string[]>(responseBody2);
                    ArrayList freesArrayList = new ArrayList();
                    Console.WriteLine("before mallocs");
                    //mallocs = RemoveAllFreedMallocs(ChangeAllFreeNames(frees), ChangeAllMallocNames(mallocs));
					Console.WriteLine("Before before mallocs");
                    mallocs = ChangeAllMallocNames(mallocs);
					Console.Read();
                    frees = ChangeAllFreeNames(frees);
                    freesArrayList.AddRange(frees);
                    mallocs =mallocs.Distinct<string>().ToArray();
                    Console.WriteLine("after mallocs");
                    Dictionary<string, string[]> final_mallocs = new Dictionary<string, string[]>();
                    for(int i=0;i<mallocs.Length;i++)
                    {
                        if(!final_mallocs.ContainsKey(mallocs[i]))
                        {
                            final_mallocs.Add(mallocs[i], new string[0]);
                        }
                    }
                    
                    int indexOfFree;
                    List<string> keys = new List<string>(final_mallocs.Keys);
                    foreach (string keyName in keys)
                    {
                        response2 = await client.GetAsync(string.Format("http://127.0.0.1:8081?filePath={0}&eVar={1}&functionName={2}&variable={3}", sourcePath, eVar, resultsJson[key].fName, keyName));
                        responseBody2 = await response2.Content.ReadAsStringAsync();
                        Console.WriteLine("deserialize");
                        string[] equalMallocs = JsonConvert.DeserializeObject<string[]>(responseBody2);
                        final_mallocs[keyName] = equalMallocs;
                        printArray((string[])freesArrayList.ToArray(typeof(string)));
                        if ((indexOfFree=checkIfContains((string [])freesArrayList.ToArray(typeof(string)), final_mallocs[keyName]))!=-1)
                        {
                            final_mallocs.Remove(keyName);
                            freesArrayList.RemoveAt(indexOfFree);
                        }
                    }
                    Console.WriteLine("before first foreach");
                    foreach (string keyName in final_mallocs.Keys)
                    {
                        Console.WriteLine(keyName);
                        for(int i=0;i<final_mallocs[keyName].Length;i++)
                        {
                            Console.WriteLine(final_mallocs[keyName][i]);
                        }
                    }
                    Console.WriteLine("after final_mallocs");
                    foreach (string keyName in final_mallocs.Keys)
                    {
                        Console.WriteLine("entered foreach");
                        response2 = await client.GetAsync(string.Format("http://127.0.0.1:8081?filePath={0}&eVar={1}&functionName={2}&variable={3}", sourcePath, eVar, resultsJson[key].fName, keyName));
                        responseBody2 = await response2.Content.ReadAsStringAsync();
                        Console.WriteLine("deserialize");
                        string [] equalMallocs= JsonConvert.DeserializeObject<string []>(responseBody2);
                        Console.WriteLine("after second deserialize");
                        var tuple = checkIfContainsInAllArray(resultsJson[key].callsFromThisFunction, equalMallocs);
                        Console.WriteLine(tuple.Item1);
                        if (tuple.Item1=="")
                        {
                            logs += "Variable " + keyName + " that was created in function " + resultsJson[key].fName + " was not being free'd.";
                            Console.WriteLine(keyName+" Not Free");
                        }
                        else
                        {
                            if(await CheckIfFreed(tuple.Item2, tuple.Item1, client, sourcePath, eVar)==false)
                            {
                                logs += "Variable " + keyName + " that was created in function " + resultsJson[key].fName + " was not being free'd.";
                                Console.WriteLine(keyName + " Not Free");
                            }
                        }
                    }
                }
            }
            Console.WriteLine("finished");
            var json = JsonConvert.SerializeObject(logs);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var responseForPost = await client.PostAsync(string.Format("http://127.0.0.1:8081/logs?filePath={0}&eVar={1}", sourcePath, eVar), data);
            string result = responseForPost.Content.ReadAsStringAsync().Result;
            Console.WriteLine(result);
        }
        static void printArray(string [] a)
        {
            for(int i=0;i<a.Length;i++)
            {
                Console.WriteLine(a[i]);
            }
        }
        static async Task<bool> CheckIfFreed(int paramIndex,string funcKey,HttpClient client,string sourcePath,string eVar)
        {
            Console.WriteLine("entered recursion");
            Console.WriteLine(funcKey);
            Console.WriteLine(paramIndex);
            Console.WriteLine(resultsJson[funcKey].fName);
            Dictionary<string, string[]> final_mallocs = new Dictionary<string, string[]>();
            var response2 = await client.GetAsync(string.Format("http://127.0.0.1:8081?filePath={0}&eVar={1}&functionName={2}&variable={3}", sourcePath, eVar, resultsJson[funcKey].fName, resultsJson[funcKey].parameters[paramIndex].parameterName));
            string responseBody2 = await response2.Content.ReadAsStringAsync();
            string mallocOriginalName = resultsJson[funcKey].parameters[paramIndex].parameterName;
            Console.WriteLine("malloc param atm "+mallocOriginalName);
            final_mallocs.Add(mallocOriginalName, JsonConvert.DeserializeObject<string []>(responseBody2));
            response2 = await client.GetAsync(string.Format("http://127.0.0.1:8081?filePath={0}&eVar={1}&functionName={2}&readyPattern={3}&returnSize={4}", sourcePath, eVar, System.Net.WebUtility.UrlEncode(resultsJson[funcKey].fName), "CustomFree", "line"));
            responseBody2 = await response2.Content.ReadAsStringAsync();
            string[] frees = JsonConvert.DeserializeObject<string []>(responseBody2);
            frees = ChangeAllFreeNames(frees);
            printArray(frees);
            if(checkIfContains(frees,final_mallocs[mallocOriginalName])!=-1)
            {
                return true;
            }
            string funcNameResult; 
            int newParamIndex;
            

            (funcNameResult,newParamIndex)= checkIfContainsInAllArray(resultsJson[funcKey].callsFromThisFunction, final_mallocs[mallocOriginalName]);
            Console.WriteLine("funcResult = " + funcNameResult);
            Console.WriteLine("new param "+newParamIndex);
            if (funcNameResult == "")
            {
                return false;
            }
            return CheckIfFreed(newParamIndex, funcNameResult, client, sourcePath, eVar).Result;

        }
        static (string,int) checkIfContainsInAllArray(Dictionary<string,string[]> callsFromThisFunction,string [] final_mallocs)
        {
            string result = "";
            int index = -1;
            for(int i=callsFromThisFunction.Count-1;i>=0;i--)
            {
                if((index=checkIfContains(callsFromThisFunction.ElementAt(i).Value,final_mallocs))!=-1)
                {
                    result = callsFromThisFunction.ElementAt(i).Key;
                }
            }
            return (result,index);
        }
        static int checkIfContains(string [] a,string [] b)
        {
            int found = -1;
            for(int i=0;i<a.Length&&found==-1;i++)
            {
                if(b.Contains(a[i]))
                {
                    found = i;
                }
            }
            return found;
        }
        static async Task Main(string[] args)
        {
            Console.WriteLine(args[0] + "\n" + args[1]);
            string destPath = args[1];
            string sourcePath = args[0];
            string eVar = args[2];//.Split(' ')[0];
            await GetFromRestApi(sourcePath, destPath, eVar);
        }
        static string GetFreeName(string freeCall)
        {
            var pattern = @"\((.*?)\)";
            var matches = Regex.Matches(freeCall, pattern);
            string result = matches[0].Groups[1].Value;
            return result;
        }
        static string [] ChangeAllFreeNames(string [] frees)
        {
            string [] result = new string [frees.Length];
            for(int i=0;i<frees.Length;i++)
            {
                result[i] = GetFreeName(frees[i]).Trim();
            }
            return result;
        }
        static string[] ChangeAllMallocNames(string[] mallocs)
        {
            string[] result = new string[mallocs.Length];
            for (int i = 0; i < mallocs.Length; i++)
            {
                result[i] = GetMallocName(mallocs[i]).Trim();
            }
            return result;
        }
        static string GetMallocName(string mallocCall)
        {
            string result = mallocCall.Split('=')[0];
            result = result.Trim();
            result = result.Substring(result.LastIndexOf(' '));
            return result;
        }
        static string [] RemoveAllFreedMallocs(string [] frees,string [] mallocs)
        {
            ArrayList result = new ArrayList();
            result.AddRange(mallocs);
            for(int i=0;i<mallocs.Length;i++)
            {
                for(int j=0;j<frees.Length;j++)
                {
                    if(mallocs[i]==frees[j])
                    {
                        result.Remove(mallocs[i]);
                    }
                }
            }
            return (string[])result.ToArray(typeof(string));
        }
    }
}
