---
title: Getting Started
---

# Getting Started
GoRogue is a .NET Standard library, and as such, can be used with any .NET projects running on a platform that supports .NET Standard 2.1.  Compatible platforms include, but are not limited to, .NET Core 3.0 or higher, Mono 6.4 or higher, and .NET 5 or higher.  Additional compatibility information for .NET Standard 2.1 can be found at Microsoft's site [here](https://docs.microsoft.com/en-us/dotnet/standard/net-standard).

Note that GoRogue is _not_ a full game engine; it's a collection of tools, data structures, and algorithms which are helpful in creating a 2D grid-based game and can easily integrate with other frameworks.  There are a number of crucial "game engine" features which GoRogue, by design, does not provide any facilities for, including rendering and playing audio.  The intent is for you to pair GoRogue with some other framework or library which handles those aspects.  An all-inclusive list will not be provided here; but if you're looking for suggestions, some compatible options include [SadConsole](), [MonoGame](https://www.monogame.net/), [Unity](https://unity.com/), [Godot](https://godotengine.org/), and [Stride](https://www.stride3d.net/).  GoRogue is distributed as a NuGet package which provides targets for .NET Standard 2.1 compatible platforms, which includes the Mono, .NET, and .NET Core runtimes; so it will generally be compatible with any framework using these platforms.

Because the library is distributed as a NuGet package, installation is straightforward.  Instructions below outline the process for popular platforms/runtimes; however it really is as simple as "install the NuGet package" in the vast majority of circumstances.

## Traditional Dotnet Projects
GoRogue may be used in a traditional .NET project like any other compatible NuGet package.  Because GoRogue targets .NET Standard 2.1, .NET Core 3+ or .NET 5+ is required.

# [New Project UI](#tab/tabid-new-project-ui)
GoRogue may be easily introduced into a new or existing project created using Visual Studio (which can be downloaded [here](https://www.visualstudio.com/downloads/)) or any other supported IDE.  The following steps outline the process with Visual Studio 2019, although other versions of Visual Studio and other IDEs will have similar project creation steps:
1. Open Visual Studio, and select **File->New->Project...**.  From templates, choose **Visual C#->Console App**.  Note that any .NET project type should suffice, though "Console" is a good default.

![create project](~/images/getting_started/ide_project/1_Core_Create_Project.PNG)

2. Give the project a name and location, and finish project creation.

3. Next, you must add the GoRogue NuGet package.  Right click on the project in the Solution explorer, and choose **Manage NuGet Packages**.

![manage nuget](~/images/getting_started/ide_project/2_Manage_Nuget.PNG)

4. Ensure that the **Browse** tab is selected, and that **Package Source** is set to nuget.org, and ensure "include prereleases" is checked (this is required because GoRogue v3 is still in beta).  Then, search **GoRogue**.  Select a version which **does not end in -debug**, and install the package.

![install nuget](~/images/getting_started/ide_project/3_Install_Nuget.PNG)

5. Replace the contents of the `Main` function (or top level statements, if you're using them) in `Program.cs` with the following:

[!code-csharp[](../../GoRogue.Snippets/GettingStarted.cs#ExampleMainFunction)]


Note that the above code assumes the following "using" statements are present in the file in which it was placed:

[!code-csharp[](../../GoRogue.Snippets/GettingStarted.cs#RequiredIncludes)]

6. Run the project and you should see a grid filled with "T" (true values) printed out; this should validate that GoRogue and its dependencies are installed properly.

![run program](~/images/getting_started/ide_project/4_Run_Program.PNG)

# [Dotnet CLI Tool](#tab/tabid-dotnet-cli)
You can also use the "dotnet" CLI tool to create a new project and install GoRogue.

1. In terminal, in the directory you want to create the project, run the following commands:

```bash
dotnet new console -o GoRogueTestProject
cd GoRogueTestProject
```

This creates a new console project called "GoRogueTestProject".  Any .NET project type should allow the installation of GoRogue; "console" is simply chosen here for simplicity.

2. Next, you must add the GoRogue NuGet package.  Run the command `dotnet add package GoRogue -v 3.0.0-beta08`.  Note that until GoRogue v3 is out of beta, you will **must** manually specify the version in order to install GoRogue v3 (since the NuGet package is marked as a "prerelease").

3. Replace the `Main` function (or top level statements, if you're using them) in `Program.cs` with the following:

[!code-csharp[](../../GoRogue.Snippets/GettingStarted.cs#ExampleMainFunction)]

Note that the above code assumes the following "using" statements are present in the file in which it was placed:

[!code-csharp[](../../GoRogue.Snippets/GettingStarted.cs#RequiredIncludes)]

4. Run the project with `dotnet run`.  You should see a grid filled with "T" (true values) printed out; this should validate that GoRogue and its dependencies are installed properly.

![run_program dotnet core](~/images/getting_started/dotnet_cli_project/4_Core_Run_Program.PNG)
***

## Usage With SadConsole
Users which choose to use GoRogue with [SadConsole](https://sadconsole.com/) should be aware that there are some extra dotnet templates and projects which may be helpful.  One option is to simply follow SadConsole's "getting started" instructions, then install GoRogue into the created project; however there is also an "integration" library which is designed to help integrate the two libraries, as well as several code examples showing ways to use them together.  Although not applicable to all use cases, the integration library and its examples may be useful when getting started, even if just as a reference.

Details on using GoRogue and SadConsole together (both with and without the integration library) can be found [here](https://github.com/Chris3606/SadConsole_RogueLike_Info).

## Unity
GoRogue is compatible with modern versions of Unity; however it is not distributed on the Unity Asset Store, and Unity does not have built-in support for NuGet.  Two possible options for using GoRogue with Unity are:

1. There is a [third-party package for Unity](https://github.com/GlitchEnzo/NuGetForUnity) which allows the installation of NuGet packages.  This is, in many cases, the easiest way to use GoRogue in Unity.

2. On GoRogue's GitHub, in the [releases section](https://github.com/Chris3606/GoRogue/releases), each release will have .zip file attached to it.  This .zip file will contain all DLLs and XML documentation files for both GoRogue itself, and all of its dependencies (assuming a .NET Standard 2.1 target).  You can simply download this .zip file, and add the DLLs to your Unity project via drag and drop.

## Other Engines/Platforms/Runtimes
Any platform which supports .NET Standard 2.1 or higher will generally be compatible with GoRogue.  This includes (but is not limited to) frameworks such as MonoGame, as well as game engines like Godot, Stride, and Unity.  Specific instructions for these platforms aren't provided here; however as outlined above, usage will typically be as simple as adding the NuGet package to the project.  Refer to your engine's specific documentation for installation of NuGet packages for details.

### Platforms Not Supporting NuGet
If your platform does not support NuGet, in the ["Releases" section of GoRogue's GitHub](https://github.com/Chris3606/GoRogue/releases), each release will have .zip file attached to it.  This .zip file will contain all DLLs and XML documentation files for both GoRogue itself, and all of its dependencies (assuming a .NET Standard 2.1 target); so you may manually add references to the DLLs.

## Enabling SourceLink (Optional)
GoRogue natively supports [SourceLink](https://github.com/dotnet/sourcelink), and distributes debugging symbols packages with each release in the form of _.snuget_ packages.  Enabling this functionality is optional, but if enabled it will allow you to step into GoRogue code using the debugger, just as you would your own code.  This may be extremely helpful for identifying and tracking down issues with your code.  The use of this feature requires Visual Studio 2017 version 15.9 or greater, or another IDE/platform supporting SourceLink.  The following instructions will assume you are using Visual Studio.

1. Add the NuGet debugging symbols source to your Visual Studio debugging settings by following the instructions in the "Consume snupkg from NuGet.org in Visual Studio" section of [this webpage](https://blog.nuget.org/20181116/Improved-debugging-experience-with-the-NuGet-org-symbol-server-and-snupkg.html).

2. In Visual Studio, go to _Tools->Options->Debugging_, and ensure that "Just My Code" is _disabled_, and that "SourceLink Support" is _enabled_.

###  Utilizing Debug Builds
The support of SourceLink and symbols packages in GoRogue can make debugging code much easier.  However, since the default GoRogue package for each version is still a "Release" build, it can still be challenging to debug code involving GoRogue function calls, as optimizations that occur during the release build process can limit the usefulness of debugging symbols.  Thus, with each version of GoRogue, a "Debug" build is also provided. The debug build is categorized as a prerelease by nuget, so you will need to enable pre-releases to see it.  Once you do so, if you look at versions of GoRogue available, you will see two listings for each version -- x.y.z, which is the release build and x.y.z-debug, which is categorized as a prerelease, and is the debug build.  If you need to perform debugging involving stepping into GoRogue code, simply switch your package version to the "-debug" version corresponding to the GoRogue version you are using.  Then, you can switch back to the regular version when debugging is complete.

## Next Steps
To learn more about GoRogue, we recommend that you check out the other items in the "Articles" section, which provide basic documentation for specific features.  A good starting point for these is the [grid view documentation](~/articles/howtos/grid-view-concepts.md); the concept of a "grid view" is a foundational concept for many GoRogue algorithms.

It also also recommended that you look through the API Documentation for specific features; it is fairly complete and will provide detailed descriptions, usage considerations, etc.