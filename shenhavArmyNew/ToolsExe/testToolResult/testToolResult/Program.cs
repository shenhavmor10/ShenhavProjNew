using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace testToolResult
{
    class Program
    {
        //This whole project is for testing a "tool".
        //Server info declaration.
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";
        /// Function - Main
        /// <summary>
        /// Handles the info recieving from the rest api server (Platform).
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task GetFromRestApi(string sourcePath, string destPath, string eVar)
        {
            //Communicating with rest api server
            Console.WriteLine("entered ");
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //Functions GET.
            Console.WriteLine("before async");
            Console.WriteLine("Evar = " + eVar);
            Console.WriteLine("destPath = " + destPath);
            HttpResponseMessage response = await client.GetAsync(string.Format("http://127.0.0.1:8081/functions?filePath={0}&eVar={1}", sourcePath, eVar));
            //HttpResponseMessage response = await client.GetAsync(string.Format("http://127.0.0.1:8081/functions?filePath={0}",sourcePath);
            Console.WriteLine("after async");
            string result = "result of tool something something";
            var json = JsonConvert.SerializeObject(result);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var responseForPost = await client.PostAsync(string.Format("http://127.0.0.1:8081/result?toolName={0}&filePath={1}&eVar={2}","toolTest",sourcePath, eVar), data);
            string result3 = responseForPost.Content.ReadAsStringAsync().Result;
            Console.WriteLine(result3);

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseBody);
            string logs = "logs logs logs logs \n logs logs logs \n another logs and another logs \n yay !";
            var json2 = JsonConvert.SerializeObject(logs);
            var data2 = new StringContent(json2, Encoding.UTF8, "application/json");
            var responseForPost2 = await client.PostAsync(string.Format("http://127.0.0.1:8081/logs?filePath={0}&eVar={1}", sourcePath, eVar), data2);
            string result2 = responseForPost.Content.ReadAsStringAsync().Result;
            Console.WriteLine(result2);
            //Deserialize.
            //Dictionary<string, FunctionInfoJson> dict = JsonConvert.DeserializeObject<Dictionary<string, FunctionInfoJson>>(responseBody);
            //Checking if it works (it does).
            /*Console.WriteLine(dict["static int* main(int* podd, int** odpdf, char a, char* retval)"].documentation);
            string documentationTemplate = new MyStream(documentationPath, System.Text.Encoding.UTF8).ReadToEnd();
            string tempDocumentation = GeneralConsts.EMPTY_STRING;
            MyStream myStream = new MyStream(sourcePath, System.Text.Encoding.UTF8);
            StreamWriter destStream = new StreamWriter(Path.Combine(destPath, "newFile.c"),false);
            destStream.AutoFlush = true;
            string newFile=myStream.ReadToEnd();
            foreach (string key in dict.Keys)
            {
                regexPattern = GeneralConsts.EMPTY_STRING;
                Console.WriteLine(key);
                ParametersType[] parameters = (ParametersType[])dict[key].parameters;
                for (int i=0;i<parameters.Length;i++)
                {
                    Console.WriteLine(parameters[i].parameterName);
                }
                regexPattern += @"(?s).*\@params.*\n";
                for (int i = 0; i < parameters.Length; i++)
                {
                    regexPattern += @".*" + parameters[i].parameterName + @".*\n";
                }
                regexPattern += @".*\@returns.*\n.*";
                Regex documentation = new Regex(regexPattern);
                if (!documentation.IsMatch(dict[key].documentation))
                {
                    tempDocumentation = createParameters(parameters);
                    string newDocumentation = documentationTemplate;
                    newDocumentation = newDocumentation.Replace("{0}", tempDocumentation);
                    newDocumentation = newDocumentation.Replace("{1}", "type - " + dict[key].returnType);
                    if(dict[key].documentation!= GeneralConsts.EMPTY_STRING)
                    {
                        newFile = newFile.Replace(dict[key].documentation, newDocumentation);
                    }
                    else
                    {
                        newFile = newFile.Replace(key, newDocumentation + "\r\n" + key);
                    }
                    


                }
                Console.WriteLine(dict[key].documentation);


            }
            destStream.Write(newFile);
            destStream.Close();
            /*Console.WriteLine(dict["void spoi()"].documentation);
            //Code Info GET.
            response = await client.GetAsync("http://127.0.0.1:8081/codeInfo");
            response.EnsureSuccessStatusCode();
            responseBody = await response.Content.ReadAsStringAsync();
            //Deserialize.
            CodeInfoJson code = JsonConvert.DeserializeObject<CodeInfoJson>(responseBody);
            //Checking if it works (it does).
            Console.WriteLine(code.definesAmount);
            Console.ReadLine();*/

        }
        static async Task Main(string[] args)
        {
            Console.WriteLine(args[0] + "\n" + args[1]);
            string destPath = args[1];
            string sourcePath = args[0];
            string eVar = args[2];//.Split(' ')[0];
            await GetFromRestApi(sourcePath, destPath, eVar);
        }
    }
}
