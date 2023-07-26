---
title: Getting Started
---

# Getting Started
GoRogue is a .NET Standard library, and as such, can be used with any .NET projects running on a platform that supports .NET Standard 2.1.  Compatible platforms include, but are not limited to, .NET Core 3.0 or higher, Mono 6.4 or higher, and .NET 5 or higher.  Additional compatibility information for .NET Standard 2.1 can be found at Microsoft's site [here](https://docs.microsoft.com/en-us/dotnet/standard/net-standard).

The library is distributed as a NuGet package, so installation is exactly like that of any other NuGet package.  Instructions below outline the process for popular platforms.

## Traditional Dotnet Projects
GoRogue may be used in a .NET project like any other compatible NuGet package.  Because GoRogue targets .NET Standard 2.1, .NET Core 3+ or .NET 5+ is required.

# [New Project UI](#tab/tabid-new-project-ui)
GoRogue may be easily introduced into a new or existing project created using Visual Studio (which can be downloaded [here](https://www.visualstudio.com/downloads/)) or any other supported IDE.  The following steps outline the process with Visual Studio 2019, although other versions of Visual Studio and other IDEs will have similar project creation steps:
1. Open Visual Studio, and select **File->New->Project...**.  From templates, choose **Visual C#->Console App**.  Note that any .NET project type should suffice, though "Console" is a good default.

![create project](~/images/getting_started/ide_project/1_Core_Create_Project.PNG)

2. Give the project a name and location, and finish project creation.
3. Next, you must add the GoRogue NuGet package.  Right click on the project in the Solution explorer, and choose **Manage NuGet Packages**.

![manage nuget](~/images/getting_started/ide_project/2_Manage_Nuget.PNG)

4. Ensure that the **Browse** tab is selected, and that **Package Source** is set to nuget.org, then search **GoRogue**.  Install the package.

![install nuget](~/images/getting_started/ide_project/3_Install_Nuget.PNG)

5. Replace the contents of the `Main` function (or top level statements, if you're using them) in `Program.cs` with the following:

[!code-csharp[](../../GoRogue.Snippets/GettingStarted.cs#ExampleMainFunction)]

6. Run the project and you should see the coordinate printed out; this should validate that GoRogue and its dependencies are installed properly.

![run program](~/images/getting_started/ide_project/4_Run_Program.PNG)

# [Dotnet CLI Tool](#tab/tabid-dotnet-cli)
You can also use the "dotnet" CLI tool to create a new project and install GoRogue.

1. In terminal, in the directory you want to create the project, run the following commands:

```bash
dotnet new console -o GoRogueTestProject
cd GoRogueTestProject
```

This creates a new console project called "GoRogueTestProject".  Any .NET project type should allow the installation of GoRogue; "console" is simply chosen here for simplicity.

2. ext, you must add the GoRogue NuGet package.  Run the command `dotnet package add GoRogue`.

3. Replace the `Main` function (or top level statements, if you're using them) in `Program.cs` with the following:

[!code-csharp[](../../GoRogue.Snippets/GettingStarted.cs#ExampleMainFunction)]

4. Run the project with `dotnet run`.  You should see the coordinate printed out; this should validate that GoRogue and its dependencies are installed properly.

![run_program dotnet core](~/images/getting_started/dotnet_cli_project/4_Core_Run_Program.PNG)
***

## Usage With SadConsole
Although GoRogue is designed to be portable and function in any supported .NET environment, users of [SadConsole](https://sadconsole.com/) should be aware that there are some extra dotnet templates and projects which may be helpful.  One option is to simply follow SadConsole's "getting started" instructions, then install GoRogue into the created project; however there is also an "integration" library which is designed to help integrate the two libraries, as well as several code examples showing common ways to integrate the two.  Although not applicable to all use cases, it may be useful to look through some of these.  Details can be found [here](https://github.com/Chris3606/SadConsole_RogueLike_Info).

## Other Engines/Platforms/Runtimes
Any platform which supports .NET Standard 2.1 or higher will generally be compatible with GoRogue.  This includes (but is not limited to) frameworks such as MonoGame,a s well as game engines like Godot, Stride, and Unity.  Specific instructions for these platforms aren't provided here; however as outlined above, usage will typically be as simple as adding the NuGet package to the project.  Refer to your engine's specific documentation for installation of NuGet packages for details.

## Godot
Because it supports .NET Standard 2.1 for its C# scripting, GoRogue also fully functions within the Godot game engine:

1. Make sure you have a version of Godot that supports C#/.NET Standard 2.1 or higher.

2. Add the GoRogue NuGet package.  See the [Godot docs](https://docs.godotengine.org/en/4.1/tutorials/scripting/c_sharp/c_sharp_basics.html#using-nuget-packages-in-godot) on using nuget packages for details.

## Enabling SourceLink (Optional)
GoRogue natively supports [SourceLink](https://github.com/dotnet/sourcelink), and distributes debugging symbols packages with each release in the form of _.snuget_ packages.  Enabling this functionality is optional, but if enabled it will allow you to step into GoRogue code using the debugger, just as you would your own code.  This may be extremely helpful for identifying and tracking down issues with your code.  The use of this feature requires Visual Studio 2017 version 15.9 or greater, or another IDE/platform supporting SourceLink.  The following instructions will assume you are using Visual Studio.

1. Add the NuGet debugging symbols source to your Visual Studio debugging settings by following the instructions in the "Consume snupkg from NuGet.org in Visual Studio" section of [this webpage](https://blog.nuget.org/20181116/Improved-debugging-experience-with-the-NuGet-org-symbol-server-and-snupkg.html).

2. In Visual Studio, go to _Tools->Options->Debugging_, and ensure that "Just My Code" is _disabled_, and that "SourceLink Support" is _enabled_.

###  Utilizing Debug Builds
The support of SourceLink and symbols packages in GoRogue can make debugging code much easier.  However, since the default GoRogue package for each version is still a "Release" build, it can still be challenging to debug code involving GoRogue function calls, as optimizations that occur during the release build process can limit the usefulness of debugging symbols.  Thus, with each version of GoRogue, a "Debug" build is also provided. The debug build is categorized as a prerelease by nuget, so you will need to enable pre-releases to see it.  Once you do so, if you look at versions of GoRogue available, you will see two listings for each version -- x.y.z, which is the release build and x.y.z-debug, which is categorized as a prerelease, and is the debug build.  If you need to perform debugging involving stepping into GoRogue code, simply switch your package version to the "-debug" version corresponding to the GoRogue version you are using.  Then, you can switch back to the regular version when debugging is complete.

## Next Steps
To learn more about GoRogue, we recommend that you check out the other items in the "Articles" section, which provide basic documentation for specific features.  A good starting point for these is the [grid view documentation](~/articles/howtos/grid-view-concepts.md); the concept of a "grid view" is a foundational concept for many GoRogue algorithms.

It also also recommended that you look through the API Documentation for specific features; it is fairly complete and will provide detailed descriptions, usage considerations, etc.