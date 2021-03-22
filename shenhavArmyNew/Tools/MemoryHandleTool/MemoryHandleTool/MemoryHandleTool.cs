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
    class MemoryHandleTool
    {
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";
        static Dictionary<string, FunctionInfoJson> resultsJson;
        static string logs="";
        /// Function - GetFromRestApi
        /// <summary>
        /// the main area where the actaull tool is working on. most of the requests are being sent from here.
        /// </summary>
        /// <param name="sourcePath"> the path of the file.</param>
        /// <param name="destPath"> the destination file (if needed).</param>
        /// <param name="eVar"> the encironment variables that are being checked at the moment.</param>
        /// <returns></returns>
        static async Task GetFromRestApi(string sourcePath, string destPath, string eVar)
        {
            //Communicating with rest api server
            HttpClient client = new HttpClient();
            string regexPattern = "";
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //Functions GET.
            HttpResponseMessage response = await client.GetAsync(string.Format("http://127.0.0.1:8081/functions?filePath={0}&eVar={1}", sourcePath, eVar));
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            resultsJson = JsonConvert.DeserializeObject<Dictionary<string, FunctionInfoJson>> (responseBody);
            foreach(string key in resultsJson.Keys)
            {
                string mallocPattern = "";
                // enters only if there is a memory allocation in the function.
                if(resultsJson[key].memoryAllocation)
                {
                    //get all mallocs.
                    var response2 = await client.GetAsync(string.Format("http://127.0.0.1:8081?filePath={0}&eVar={1}&functionName={2}&readyPattern={3}&returnSize={4}", sourcePath, eVar, System.Net.WebUtility.UrlEncode(resultsJson[key].fName), "CustomMalloc", "line"));
                    string responseBody2 = await response2.Content.ReadAsStringAsync();
                    string[] mallocs = JsonConvert.DeserializeObject<string[]>(responseBody2);
                    //get all frees.
                    response2= await client.GetAsync(string.Format("http://127.0.0.1:8081?filePath={0}&eVar={1}&functionName={2}&readyPattern={3}&returnSize={4}", sourcePath, eVar, System.Net.WebUtility.UrlEncode(resultsJson[key].fName), "CustomFree", "line"));
                    responseBody2 = await response2.Content.ReadAsStringAsync();
                    string [] frees = JsonConvert.DeserializeObject<string[]>(responseBody2);
                    //in order to remove the free's changing to an arrayList for the easier solution.
                    ArrayList freesArrayList = new ArrayList();
                    //change names from free(x) to x. and from the whole malloc line to only the name of the variable.
                    mallocs = ChangeAllMallocNames(mallocs);
                    frees = ChangeAllFreeNames(frees);
                    freesArrayList.AddRange(frees);
                    //making sure that we are seeing only from each allocation.
                    mallocs =mallocs.Distinct<string>().ToArray();
                    //creating the final mallocs dictionary.
                    Dictionary<string, string[]> final_mallocs = new Dictionary<string, string[]>();
                    for(int i=0;i<mallocs.Length;i++)
                    {
                        if(!final_mallocs.ContainsKey(mallocs[i]))
                        {
                            //adding the whole mallocs from mallocs array.
                            final_mallocs.Add(mallocs[i], new string[0]);
                        }
                    }
                    int indexOfFree;
                    List<string> keys = new List<string>(final_mallocs.Keys);
                    //loop on the dictionary keys.
                    foreach (string keyName in keys)
                    {
                        //get all equal mallocs - all the cariables that are being equal to the malloc given in this situation 
                        //(memory equal means they have the same memory so if you free one they are both being free'd).
                        response2 = await client.GetAsync(string.Format("http://127.0.0.1:8081?filePath={0}&eVar={1}&functionName={2}&variable={3}", sourcePath, eVar, resultsJson[key].fName, keyName));
                        responseBody2 = await response2.Content.ReadAsStringAsync();
                        string[] equalMallocs = JsonConvert.DeserializeObject<string[]>(responseBody2);
                        //putting in the final mallocs value the array of all of the equal variables.
                        final_mallocs[keyName] = equalMallocs;
                        //if there is a free on the same scope check it and if there is remove the malloc and the free and keep going.
                        if ((indexOfFree=checkIfContains((string [])freesArrayList.ToArray(typeof(string)), final_mallocs[keyName]))!=-1)
                        {
                            final_mallocs.Remove(keyName);
                            freesArrayList.RemoveAt(indexOfFree);
                        }
                    }
                    //now checking only for other scopes (function calls).
                    foreach (string keyName in final_mallocs.Keys)
                    {
                        //getting the equal mallocs one more time for each malloc that is still didnt got free'd.
                        response2 = await client.GetAsync(string.Format("http://127.0.0.1:8081?filePath={0}&eVar={1}&functionName={2}&variable={3}", sourcePath, eVar, resultsJson[key].fName, keyName));
                        responseBody2 = await response2.Content.ReadAsStringAsync();
                        string [] equalMallocs= JsonConvert.DeserializeObject<string []>(responseBody2);
                        var tuple = checkIfContainsInAllArray(resultsJson[key].callsFromThisFunction, equalMallocs);
                        //if the first item of the tuple which is the function call name is "" it means there is no function
                        //where this variable is being sent to so there is nothing to check in this situation. the allocation
                        //isnt being free'd.
                        if (tuple.Item1=="")
                        {
                            logs += "Variable " + keyName + " that was created in function " + resultsJson[key].fName + " was not being free'd.";
                        }
                        else
                        {
                            //recursion check if in other function calls the allocation is being free'd.
                            if(await CheckIfFreed(tuple.Item2, tuple.Item1, client, sourcePath, eVar)==false)
                            {
                                logs += "Variable " + keyName + " that was created in function " + resultsJson[key].fName + " was not being free'd.";
                            }
                        }
                    }
                }
            }
            var json = JsonConvert.SerializeObject(logs);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var responseForPost = await client.PostAsync(string.Format("http://127.0.0.1:8081/logs?filePath={0}&eVar={1}", sourcePath, eVar), data);
            string result = responseForPost.Content.ReadAsStringAsync().Result;
        }
        /// Function - CheckIfFreed
        /// <summary>
        /// this function is checking if there are function calls that variables with an unfree'd allocation is being
        /// sent as a parameter and if there is it checks if the variable is being free'd there and if it dont it continues
        /// in recursion and keep checking untill there is no more function calls with this variable.
        /// </summary>
        /// <param name="paramIndex"> the index of the parameter in this new function.</param>
        /// <param name="funcKey"> the name of the function which is also the key of the dictionary.</param>
        /// <param name="client"> HttpClient type client.</param>
        /// <param name="sourcePath"> the file path.</param>
        /// <param name="eVar"> current environment variables.</param>
        /// <returns></returns>
        static async Task<bool> CheckIfFreed(int paramIndex,string funcKey,HttpClient client,string sourcePath,string eVar)
        {
            Dictionary<string, string[]> final_mallocs = new Dictionary<string, string[]>();
            //taking the equal mallocs like before.
            var response2 = await client.GetAsync(string.Format("http://127.0.0.1:8081?filePath={0}&eVar={1}&functionName={2}&variable={3}", sourcePath, eVar, resultsJson[funcKey].fName, resultsJson[funcKey].parameters[paramIndex].parameterName));
            string responseBody2 = await response2.Content.ReadAsStringAsync();
            string mallocOriginalName = resultsJson[funcKey].parameters[paramIndex].parameterName;
            //adding the original malloc name which is now the name of the new parameter in the function which the function gets.
            //for an example func(a,b) so if you get paramIndex 1 the param will now be 'b'.
            final_mallocs.Add(mallocOriginalName, JsonConvert.DeserializeObject<string []>(responseBody2));
            //get all frees.
            response2 = await client.GetAsync(string.Format("http://127.0.0.1:8081?filePath={0}&eVar={1}&functionName={2}&readyPattern={3}&returnSize={4}", sourcePath, eVar, System.Net.WebUtility.UrlEncode(resultsJson[funcKey].fName), "CustomFree", "line"));
            responseBody2 = await response2.Content.ReadAsStringAsync();
            string[] frees = JsonConvert.DeserializeObject<string []>(responseBody2);
            //change names like before.
            frees = ChangeAllFreeNames(frees);
            //if it is being free'd return true.
            if(checkIfContains(frees,final_mallocs[mallocOriginalName])!=-1)
            {
                return true;
            }
            string funcNameResult; 
            int newParamIndex;
            (funcNameResult,newParamIndex)= checkIfContainsInAllArray(resultsJson[funcKey].callsFromThisFunction, final_mallocs[mallocOriginalName]);
            //same as before.
            if (funcNameResult == "")
            {
                return false;
            }
            return CheckIfFreed(newParamIndex, funcNameResult, client, sourcePath, eVar).Result;

        }
        /// Function - checkIfContainsInAllArray
        /// <summary>
        /// check if there is a function that is getting our allocated parameter as a parameter.
        /// </summary>
        /// <param name="callsFromThisFunction"> a dictionary which have a string of the func key as mentioned before
        /// and the array is its params that are being called so if we have a function call - func(a,b) it will have in the
        /// dictionary - [void func(string a,string b);:(a,b)].
        /// </param>
        /// <param name="final_mallocs">the all variation of the variable (memory equal).</param>
        /// <returns></returns>
        static (string,int) checkIfContainsInAllArray(Dictionary<string,string[]> callsFromThisFunction,string [] final_mallocs)
        {
            string result = "";
            int index = -1;
            //scanning from the end to the start.
            for(int i=callsFromThisFunction.Count-1;i>=0;i--)
            {
                //checks if contain one of the params.
                if((index=checkIfContains(callsFromThisFunction.ElementAt(i).Value,final_mallocs))!=-1)
                {
                    //return his index in the function.
                    result = callsFromThisFunction.ElementAt(i).Key;
                }
            }
            return (result,index);
        }
        /// Function - checkIfContains
        /// <summary>
        /// check between 2 arrays if they have 1 in common. if no it returns -1 if yes it returns the index of the 1.
        /// </summary>
        /// <param name="a">array type string.</param>
        /// <param name="b">array type string.</param>
        /// <returns>returns the index of the result or -1.</returns>
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
        //main.
        static async Task Main(string[] args)
        {
            string destPath = args[1];
            string sourcePath = args[0];
            string eVar = args[2];//.Split(' ')[0];
            await GetFromRestApi(sourcePath, destPath, eVar);
        }
        /// Function - GetFreeName
        /// <summary>
        /// gets the free name so if the string was free(asa); it will return asa.
        /// </summary>
        /// <param name="freeCall"> the call of the free.</param>
        /// <returns>returns the name of the variable being free'd.</returns>
        static string GetFreeName(string freeCall)
        {
            var pattern = @"\((.*?)\)";
            var matches = Regex.Matches(freeCall, pattern);
            string result = matches[0].Groups[1].Value;
            return result;
        }
        /// Function - ChangeAllFreeNames
        /// <summary>
        /// change all the free names like mentioned before.
        /// </summary>
        /// <param name="frees"> array type string with all frees inside.</param>
        /// <returns> returns the new array with the changes.</returns>
        static string [] ChangeAllFreeNames(string [] frees)
        {
            string [] result = new string [frees.Length];
            for(int i=0;i<frees.Length;i++)
            {
                result[i] = GetFreeName(frees[i]).Trim();
            }
            return result;
        }
        /// Function - ChangeAllMallocNames
        /// <summary>
        /// changes all malloc names of the array to be only the name and not the code line.
        /// same idea as the change frees.
        /// </summary>
        /// <param name="mallocs"> array type string with all allocations in it.</param>
        /// <returns>returns a new array type string with the changes.</returns>
        static string[] ChangeAllMallocNames(string[] mallocs)
        {
            string[] result = new string[mallocs.Length];
            for (int i = 0; i < mallocs.Length; i++)
            {
                result[i] = GetMallocName(mallocs[i]).Trim();
            }
            return result;
        }
        /// Function - GetMallocName
        /// <summary>
        /// Get the malloc name instead of the malloc code line that is being given to the function.
        /// </summary>
        /// <param name="mallocCall"> the malloc code line type string.</param>
        /// <returns> returns the malloc name type string.</returns>
        static string GetMallocName(string mallocCall)
        {
            string result = mallocCall.Split('=')[0];
            result = result.Trim();
            result = result.Substring(result.LastIndexOf(' '));
            return result;
        }
    }
}
