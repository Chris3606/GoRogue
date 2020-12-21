---
title: Getting Started
---

# Getting Started
GoRogue is a .NET Standard library, and as such, can be used with any .NET projects running on a platform that supports .NET Standard 2.1.  Compatible platforms include, but are not limited to, .NET Core 3.0 or higher, Mono 6.4 or higher, and .NET 5 or higher.  Additional compatibility information for .NET Standard 2.1 can be found at Microsoft's site [here](https://docs.microsoft.com/en-us/dotnet/standard/net-standard).

The library is distributed as a NuGet package, so installation is exactly like that of any other NuGet package.  Instructions below outline the process for popular platforms.

# .NET Core/.NET
GoRogue may be used in a .NET Core (now called simply .NET as of .NET 5) project like any other compatible nuget package.  Because GoRogue targets .NET Standard 2.1, .NET Core 3+, or .NET 5+, is required.

# [Windows](#tab/tabid-win)
On Windows, setup is easiest using Visual Studio Community 2019, which can be downloaded for free [here](https://www.visualstudio.com/downloads/).  With Visual Studio installed, perform the following steps:
1. Open Visual Studio, and select **File->New->Project...**.  From templates, choose **Visual C#->Console App (.NET Core)**.  Note that any other .NET Core project type should also suffice.

![create project](~/images/getting_started/windows_project/1_Core_Create_Project.PNG)

2. Give the project any name and location you want.  Although it is not required, you may want to ensure **Create Directory For Solution** is unchecked, as it will simplify the resulting file structure for single-project solutions (causing the project file to reside in the same location as the solution file, rather than a subdirectory). Click **OK**.
3. Next, we must add the NuGet package.  Right click on the project in the Solution explorer, and choose **Manage NuGet Packages**.

![manage nuget](~/images/getting_started/windows_project/2_Manage_Nuget.PNG)

4. Ensure that the **Browse** tab is selected, and that **Package Source** is set to nuget.org, then search **GoRogue**.  Install the package.

![install nuget](~/images/getting_started/windows_project/3_Install_Nuget.PNG)

5. Replace the Main function in Program.cs with the following:

```C#
static void Main(string[] args)
{
    System.Console.WriteLine(new SadRogue.Primitives.Point(1, 2));
    // Used to stop window from closing until a key is pressed.
    int c = System.Console.Read();
}
```

6. Run the project and you should see the coordinate printed out.

![run program](~/images/getting_started/windows_project/4_Run_Program.PNG)

# [Linux](#tab/tabid-lin)
On Linux distributions, a .NET Core project requires an installation of the .NET Core SDK (now called simply .NET 5).  Installation instructions vary by distribution and platform.

1. In terminal, in the directory you want to create the project, run the following commands:

```bash
dotnet new console -o TestProjGoRogue
cd TestProjGoRogue
```

This creates a new console project called TestProjGoRogue, and changes to its directory.  Note that although console is used here, any other .NET Core project type should work fine.

2. Now we must add the NuGet package.  Run the command `dotnet package add GoRogue`.

3. Replace the Main function in Program.cs with the following:

```C#
static void Main(string[] args)
{
    System.Console.WriteLine(new SadRogue.Primitives.Point(1, 2));
    // Used to stop window from closing until a key is pressed.
    int c = System.Console.Read();
}
```

4. Run the project with `dotnet run`.  You should see the coordinate printed out.

![run_program linux core](~/images/getting_started/linux_project/4_Core_Run_Program.PNG)


# [Mac](#tab/tabid-mac)
On MacOS, the newest version of [Visual Studio for Mac](https://www.visualstudio.com/vs/visual-studio-mac/) has proper versions of .NET Core/.NET 5 packaged with it, and can be used to create a .NET Core template (with or without the XCode portion).  If .NET Core/.NET v5+ is installed manually, a project can also be creatd from the command line, by following the commands in the Linux tab.

With Visual Studio installed, perform the following steps:
1. Open Visual Studio, and select **File->New Solution...**.  From templates, choose **.NET Core->App->Console Project (C#)**.  Note that any other .NET Core project type should also suffice.  Then, select next.

![mac core create project](~/images/getting_started/mac_project/1_Core_Create_Project.PNG)

2. Give the project any name and location you want.  Although it is not required, you may want to ensure **Create a project directory within the solution directory** is unchecked, as it will simplify the resulting file structure for single-project solutions (causing the project file to reside in the same location as the solution file, rather than a subdirectory). Click **Create**.
3. Next, we must add the NuGet package.  In the Solution view, under the project dropdown, right-click on **Dependencies**, and choose **Add Packages...**.

![mac core manage nuget](~/images/getting_started/mac_project/2_Core_Manage_Nuget.PNG)

4. Ensure that the **Package Source** dropdown is set to nuget.org, then search **GoRogue**.  Install the package.

![mac core install nuget](~/images/getting_started/linux_project/3_Framework_Install_Nuget.PNG)

5. Replace the Main function in Program.cs with the following:

```C#
static void Main(string[] args)
{
    System.Console.WriteLine(new SadRogue.Primitives.Point(1, 2));
    // Used to stop window from closing until a key is pressed.
    int c = System.Console.Read();
}
```

6. Run the project and you should see the coordinate printed out.

![mac core run program](~/images/getting_started/linux_project/4_Framework_Run_Program.PNG)

***

# Godot
Because it supports .NET Standard 2.1 for its C# scripting, GoRogue also fully functions within the Godot game engine:

1. Make sure you have a version of Godot that supports C#/.NET Standard 2.1 or higher.

2. Add the GoRogue NuGet package.  Methods of doing this will vary with your development.  See the [Godot docs](https://docs.godotengine.org/en/3.2/getting_started/scripting/c_sharp/c_sharp_basics.html#using-nuget-packages-in-godot) on using nuget packages for details.

# Enabling SourceLink (Optional)
GoRogue natively supports [SourceLink](https://github.com/dotnet/sourcelink), and distributes debugging symbols packages with each release in the form of _.snuget_ packages.  Enabling this functionality is optional, but if configured will allow you to step into GoRogue code using the debugger, just as you would your own code.  This may be extremely helpful for identifying and tracking down issues with your code.  The use of this feature requires Visual Studio 2017 version 15.9 or greater.

1. Add the NuGet debugging symbols source to your Visual Studio debugging settings by following the instructions in the "Consume snupkg from NuGet.org in Visual Studio" section of [this webpage](https://blog.nuget.org/20181116/Improved-debugging-experience-with-the-NuGet-org-symbol-server-and-snupkg.html).

2. In Visual Studio, go to _Tools->Options->Debugging_, and ensure that "Just My Code" is _disabled_, and that "SourceLink Support" is _enabled_.

##  Utilizing Debug Builds
The support of SourceLink and symbols packages in GoRogue can make debugging code much easier.  However, since the default GoRogue package for each version is still a "Release" build, it can still be challenging to debug code involving GoRogue function calls, as optimizations that occur during the release build process can limit the usefulness of debugging symbols.  Thus, with each version of GoRogue, a "Debug" build is also provided. The debug build is categorized as a prerelease by nuget, so you will need to enable prereleases to see it.  Once you do so, if you look at versions of GoRogue available, you will see two listings for each version -- x.y.z, which is the release build and x.y.z-debug, which is categorized as a prerelease, and is the debug build.  If you need to perform debugging involving stepping into GoRogue code, simply switch your package version to the "-debug" version corresponding to the GoRogue version you are using.  Then, you can switch back to the regular version when debugging is complete.
