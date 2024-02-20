# pws-web-api

Standing up a .NET server:

If encountering a "Could not load file or assembly 'DataScope.Select.Rest Api.Client' or one of its dependencies. An attempt was made to load a program with an incorrect format." error, please do the following:
- You’ll need to enable 64-bit IIS Express from Visual Studio settings to do this go to Tools ==> Options ==> Projects and Solutions ==> Web Projects and enable "Use the 63 bit version of IIS Express for web sites and projects"
- by clickicking the checkbox next to it. Click "ok" to save changes
