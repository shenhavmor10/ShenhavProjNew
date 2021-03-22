"# ShenhavProjNew" 

Description -

The project is a platform that does a static analysis for a c code, The platform has an option to update or add tools, The platform first checks for some small compile checks and right after it starts the analysis part. all of the information gets to a rest api that can be accessed from everywhere (as long as you got internet connection :)). 
tools might be individual or they might be connected to each other
It have a GUI for the client that checks his c code in the platform and a GUI for adding or updating a tool.
the project is mostly written in c# and the database is built with sql server.

Installation - 

1. First install the whole folder in your computer.
2. if you want you can config the location of things in the configuration files altough it has a default.
3. run the ShenhavProjNew\shenhavArmyNew\GUI\bin\Debug\GUI.exe in order to use the client and if you are the installer of the platform itself then run the platform aswell (at the moment the name is = ShenhavProjNew\shenhavArmyNew\testServer2\bin\Debug\testServer2).
4. thats it now enter the files select the tools and the paths needed and run.

Usage - 

Regular client -

1. Browse the files needed . (c file, project file ,etc...)
2. (Optional) if you have a specific malloc or frees you need to write the functions in the place needed.
3. Select the tools you want by click on the checkbox of them.
4. Press Connect button and thats it. :)

Tools that are currently in the platform - 
MemoryHandleTool - this tool will check in all the code if there is a memory that wanst being free'd.


Add Tool Client - 

1. In the categories of the GUI select Add tool or Update tool.

Add - 
A. Write tool name description etc...
B. Browse Project folder for the tool.
C. Add Tools Result (All results that the tools need. for an example : tool1,tool2)
D. Press Apply And Thats It !

Update -
A. Write tool name.
B. Browse new project for the tool
C. Press Apply and thats it.

Remove - 
A. Write tool name.
B. Press Apply and thats it.


Programmer use -

If you want to write a tool you will need to know what the rest api can do.

Rest API - 

For every get request you will have to give as parameters the file path and the environmentVariable (quick tip. the platform turns on the tool
and send him the exact environment Variable and file path. use it).

Lets start with the GET Requests -

you have 4 paths you can request from -

1. regular path - http://localhost:8081/ or ip instead of localhost. 
2. functions path - http://localhost:8081/functions.
3. codeInfo Path - http://localhost:8081/codeInfo
4. result path - (in order to get a different tool result) - http://localhost:8081/result

functions are being used in order to get you a big json of the functions information. you can get the information for only one function by using
the parameter "name" and you can get them by a return type by using the parameter "returnType" otherwise you will get the whole json for all functions information in 
the code. an example for information you recieve - function parameters, return type, content, variables, pattern, function calls from this function and much more.

codeInfo is a path in order to get information about the whole code. for an example includes, defines, global variables and much more.

result path is being used in order to get a result of another tool to another tool. so if tool1 wants the result of tool 2 he will send a GET REQUEST to the path
result and add the parameter "toolName" in order to tell the exact tool that you want his result. by doing that the rest will send you back the result 
that the other tool send.

regular path is being used for most cases and for a lot of things. all will be mentioned.

regular path parameters.

"functionName" - in order to search for something inside a function.

"variable" - this parameters has to come with "functionName" parameter and will give you back all of the variables that are equal to "variable".

"pattern" - in order to search for a specific pattern use the parameter and send a regex inside. (tip in order to send the pattern use URL ENCODE). you can use "functionName" 
parameter in order to search the pattern only in the specific function given and you can search the pattern for the whole code if you wont use the parameter "functionName".

"readyPattern" - there are some readyPatterns that are in the platform itself you can use them by writing the name of them in the "readyPattern" parameter. same as the
"pattern" parameter you can use here the "functionName" parameter.

All ReadyPatterns - 
VariableDecleration
VariableEquation
DefineDecleration
IncludeTrianglesPattern
IncludeRegularPattern
functionPatternInH
StructPattern
CustomMallocPattern
CustomFreePattern

POST REQUESTS - 

you have 2 things you can do with POST.

1. logs
2. result

by sending a POST request to logs you need to attach string that is the log and the platform will add it to it logs.
by sending a POST request to result you need to attach the file you want to save as a result and the platform will save it so if another toll will need it it can
give it to him.

Have fun.
