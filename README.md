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

Add Tool Client - 

1. In the categories of the GUI select Add tool or Update tool.

Add - 
A. Write tool name priority etc...
B. Browse Project folder for the tool.
C. Press Apply and thats it.

Update -
A. Write tool name.
B. If you want to delete the tool select the checkox that means delete tool.
   If you want to update the tool then browse the new project folder.
C. Press Apply and thats it.

Have fun.



