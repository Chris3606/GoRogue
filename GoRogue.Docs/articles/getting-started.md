---
title: Getting Started
---

# Getting Started
GoRogue is a .NET Standard library, and as such, can be used with any .NET projects running on a platform that supports .NET Standard 2.0.  Compatible platforms include, but are not limited to, .NET Framework 4.6.1 or higher, Mono 5.4 or higher, and .NET Core 2.0 or higher.  Additional compatibility information for .NET Standard 2.0 can be found at Microsoft's site [here](https://docs.microsoft.com/en-us/dotnet/standard/net-standard).

The library is distributed as a NuGet package, so installation is exactly like that of any other NuGet package.  Instructions below outline the process for popular platforms.

# .NET Core
GoRogue may be used in a .NET Core project like any other .NET Core-compatible nuget package.  Because GoRogue targets .NET Standard 2.0, .NET Core 2.0 or higher is required.

# [Windows](#tab/tabid-win)
On Windows, setup is easiest using Visual Studio Community 2017, which can be downloaded for free [here](https://www.visualstudio.com/downloads/).  With Visual Studio installed, perform the following steps:
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
    System.Console.WriteLine(new GoRogue.Coord(1, 2));
    // Used to stop window from closing until a key is pressed.
    int c = System.Console.Read();
}
```

6. Run the project and you should see the coordinate printed out.

![run program](~/images/getting_started/windows_project/4_Run_Program.PNG)

# [Linux](#tab/tabid-lin)
On Linux distributions, a .NET Core project requires an installation of the .NET Core SDK.  The download and installation instructions can easily be found for various distributions [here](https://www.microsoft.com/net/learn/get-started/linuxredhat). NOTE: The version the commands install by default may not be the most recent. Feel free to install the most recent version, however any version 2.0 or greater should work fine.
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
    System.Console.WriteLine(new GoRogue.Coord(1, 2));
    // Used to stop window from closing until a key is pressed.
    int c = System.Console.Read();
}
```

4. Run the project with `dotnet run`.  You should see the coordinate printed out.

![run_program linux core](~/images/getting_started/linux_project/4_Core_Run_Program.PNG)


# [Mac](#tab/tabid-mac)
On MacOS, the newest version of [Visual Studio for Mac](https://www.visualstudio.com/vs/visual-studio-mac/) has proper versions of .NET Core packaged with it, and can be used to create a .NET Core template (with or without the XCode portion).  If .NET Core is installed manually, a project can also be creatd from the command line, by following the commands in the Linux tab.

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
    System.Console.WriteLine(new GoRogue.Coord(1, 2));
    // Used to stop window from closing until a key is pressed.
    int c = System.Console.Read();
}
```

6. Run the project and you should see the coordinate printed out.

![mac core run program](~/images/getting_started/linux_project/4_Framework_Run_Program.PNG)

***

# .NET Framework
GoRogue may be used in a .NET Framework project like any other .NET Framework-compatible nuget package.  Because GoRogue targets .NET Standard 2.0, .NET Framework v4.6.1 or higher is required.

# [Windows](#tab/tabid-win)
On Windows, setup is easiest using Visual Studio Community 2017, which can be downloaded for free [here](https://www.visualstudio.com/downloads/).  With Visual Studio installed, perform the following steps:
1. Open Visual Studio, and select **File->New->Project...**.  From templates, choose **Visual C#->Console App (.NET Framework)**.  Note that any other .NET Framework project type should also suffice.

![create framework project](~/images/getting_started/windows_project/1_Framework_Create_Project.PNG)

2. Give the project any name and location you want.  Although it is not required, you may want to ensure **Create Directory For Solution** is unchecked, as it will simplify the resulting file structure for single-project solutions (causing the project file to reside in the same location as the solution file, rather than a subdirectory). Click **OK**.
3. Next, we must add the NuGet package.  Right click on the project in the Solution explorer, and choose **Manage NuGet Packages**.

![manage nuget](~/images/getting_started/windows_project/2_Manage_Nuget.PNG)

4. Ensure that the **Browse** tab is selected, and that **Package Source** is set to nuget.org, then search **GoRogue**.  Install the package.

![install nuget](~/images/getting_started/windows_project/3_Install_Nuget.PNG)

5. Replace the Main function in Program.cs with the following:

```C#
static void Main(string[] args)
{
    System.Console.WriteLine(new GoRogue.Coord(1, 2));
    // Used to stop window from closing until a key is pressed.
    int c = System.Console.Read();
}
```

6. Run the project and you should see the coordinate printed out.

![run program](~/images/getting_started/windows_project/4_Run_Program.PNG)

# [Linux](#tab/tabid-lin)
On Linux distributions, a .NET Framework project can be compiled using Mono, and is easiest created with MonoDevelop.  The newest mainstream version of MonoDevelop (7.3 at the time of this writing) has a Mono installation packaged with it, and can be installed using [flatpak](https://flatpak.org/getting).  See [this link](http://www.monodevelop.com/download/#fndtn-download-lin) for the MonoDevelop flatpak file, and [this link](http://docs.flatpak.org/en/latest/) for details on flatpak usage, or follow the steps below.
1. Visit the [MonoDevelop downloads](http://www.monodevelop.com/download/#fndtn-download-lin) page, and download the newest MonoDevelop.flatpakref file.
2. In terminal, from the directory in which the MonoDevelop.flatpakref file is located, run `flatpak install MonoDevelop.flatpakref` to begin the install.
3. Run the command `flatpak run com.xamarin.MonoDevelop`.  This will run MonoDevelop for the first time, and ensure a shortcut is placed in your programs menu.

Once MonoDevelop is installed, perform the following steps:
1. Open MonoDevelop, and select **File->New Solution...**.  From templates, choose **.NET->Console Project (C#)**.  Note that any other .NET Framework project type should also suffice.  Then, select next.

![linux framework create project](~/images/getting_started/linux_project/1_Framework_Create_Project.PNG)

2. Give the project any name and location you want.  Although it is not required, you may want to ensure **Create a project directory within the solution directory** is unchecked, as it will simplify the resulting file structure for single-project solutions (causing the project file to reside in the same location as the solution file, rather than a subdirectory). Click **Create**.

3. Next, we must add the NuGet package.  In the Solution view, under the project dropdown, right-click on **Packages**, and choose **Add Packages...**.

![linux framework manage nuget](~/images/getting_started/linux_project/2_Framework_Manage_Nuget.PNG)

4. Ensure that the **Package Source** dropdown is set to nuget.org, then search **GoRogue**.  Install the package.

![linux install nuget](~/images/getting_started/linux_project/3_Framework_Install_Nuget.PNG)

5. Replace the Main function in Program.cs with the following:

```C#
static void Main(string[] args)
{
    System.Console.WriteLine(new GoRogue.Coord(1, 2));
    // Used to stop window from closing until a key is pressed.
    int c = System.Console.Read();
}
```

6. Run the project and you should see the coordinate printed out.

![linux framework run program](~/images/getting_started/linux_project/4_Framework_Run_Program.PNG)

# [Mac](#tab/tabid-mac)
On MacOS, the newest version of [Visual Studio for Mac](https://www.visualstudio.com/vs/visual-studio-mac/) has proper versions of Mono packaged with it, and can be used to create a .NET Framework template (with or without the XCode portion).

With Visual Studio installed, perform the following steps:
1. Open Visual Studio, and select **File->New Solution...**.  From templates, choose **.NET->Console Project (C#)**.  Note that any other .NET Framework project type should also suffice.  Then, select next.

![mac framework create project](~/images/getting_started/linux_project/1_Framework_Create_Project.PNG)

2. Give the project any name and location you want.  Although it is not required, you may want to ensure **Create a project directory within the solution directory** is unchecked, as it will simplify the resulting file structure for single-project solutions (causing the project file to reside in the same location as the solution file, rather than a subdirectory). Click **Create**.
3. Next, we must add the NuGet package.  In the Solution view, under the project dropdown, right-click on **Packages**, and choose **Add Packages...**.

![mac framework manage nuget](~/images/getting_started/linux_project/2_Framework_Manage_Nuget.PNG)

4. Ensure that the **Package Source** dropdown is set to nuget.org, then search **GoRogue**.  Install the package.

![mac framework install nuget](~/images/getting_started/linux_project/3_Framework_Install_Nuget.PNG)

5. Replace the Main function in Program.cs with the following:

```C#
static void Main(string[] args)
{
    System.Console.WriteLine(new GoRogue.Coord(1, 2));
    // Used to stop window from closing until a key is pressed.
    int c = System.Console.Read();
}
```

6. Run the project and you should see the coordinate printed out.

![mac framework run program](~/images/getting_started/linux_project/4_Framework_Run_Program.PNG)
***

# Unity
GoRogue is fully capable of functioning with the Unity game engine.  However, since Unity does not natively support the NuGet package management system, installation will need to be done manually using the GoRogue dlls.  GoRogue provides dll builds on its GitHub releases page for each version to make this easy:

1. Navigate to the [GitHub releases page](https://github.com/Chris3606/GoRogue/releases), and download the most recent versions `GoRogue_x.y.z_MinimalDependencies.zip`.
> [!WARNING]
> Ensure you download the zip file labeled MinimalDependencies or it will not work correctly!  The "minimal depedencies" builds are missing no functionality that is relevant in the Unity environment -- see [Common Issues](#common-issues) for details as to what these builds are and how they differ from the regular build.

2. Extract the zip file, then drag and drop all files contained within it into your Unity project.

#Godot
Because it supports .NET Standard 2.0 for its C# scripting, GoRogue also fully functions within the Godot game engine:

1. Make sure you have a version of Godot that supports C#.

2. Add the GoRogue NuGet package.  Methods of doing this will vary with your development.  See the [Godot docs](https://docs.godotengine.org/en/3.1/getting_started/scripting/c_sharp/c_sharp_basics.html#using-nuget-packages-in-godot) on using nuget packages for details.

# Enabling SourceLink (Optional)
Starting with GoRogue 2.0, the library supports [SourceLink](https://github.com/dotnet/sourcelink), and distributes debugging symbols packages with each release in the form of _.snuget_ packages.  Enabling this functionality is optional, but if configured will allow you to step into GoRogue code using the debugger, just as you would your own code.  This may be extremely helpful for identifying and tracking down issues with your code.  The use of this feature requires Visual Studio 2017 version 15.9 or greater.

1. Add the NuGet debugging symbols source to your Visual Studio debugging settings by following the instructions in the "Consume snupkg from NuGet.org in Visual Studio" section of [this webpage](https://blog.nuget.org/20181116/Improved-debugging-experience-with-the-NuGet-org-symbol-server-and-snupkg.html).

2. In Visual Studio, go to _Tools->Options->Debugging_, and ensure that "Just My Code" is _disabled_, and that "SourceLink Support" is _enabled_.

##  Utilizing Debug Builds
The support of SourceLink and symbols packages in GoRogue can make debugging code much easier.  However, since the default GoRogue package for each version is still a "Release" build, it can still be challenging to debug code involving GoRogue function calls, as optimizations that occur during the release build process can limit the usefulness of debugging symbols.  Thus, with each version of GoRogue, a "Debug" build is also provided. The debug build is categorized as a prerelease by nuget, so you will need to enable prereleases to see it.  Once you do so, if you look at versions of GoRogue available, you will see two listings for each version -- x.y.z, which is the release build and x.y.z-debug, which is categorized as a prerelease, and is the debug build.  If you need to perform debugging involving stepping into GoRogue code, simply switch your package version to the "-debug" version corresponding to the GoRogue version you are using.  Then, you can switch back to the regular version when debugging is complete.

# Common Issues
Below are some common issues that have occured during installation, and how to fix them.

## Dependency Issues 
GoRogue is designed to be highly portable, and can function in any environment that supports .NET Standard 2.0.  While it does not depend on any libraries except Troschuetz.Random and OptimizedPriorityQueue (which NuGet will automatically install), it does provide some type conversions that makes the library more interoperable with other libraries, notably MonoGame.  The project is set up in such a way as to avoid creating a hard dependency on MonoGame in the process, so the compiler should _not_ attempt to resolve MonoGame as a required dependency of GoRogue.  However, the method of doing this involves the use of a newer project file tag, which some older or non-standard compilers (notably, the Unity compiler), may be unable to handle properly.  As such, when a project using GoRogue is compiled using these compilers, you may get an error saying that it cannot resolve the MonoGame dependency.

To counteract this, on the [GitHub releases page](https://github.com/Chris3606/GoRogue/releases), GoRogue provides .zip files that contain the GoRogue DLL and its dependencies.  Specifically, it provides two zip files for each release -- `GoRogue_x.y.z.zip` and `GoRogue_x.y.z_MinimalDependencies.zip`.  The "minimal dependency" builds are exactly like the regular builds, except they do not contain the type conversions for third-party libraries such as MonoGame.  As such, these builds should be used in those cases where dependency errors related to MonoGame and other graphics libraries are encountered.