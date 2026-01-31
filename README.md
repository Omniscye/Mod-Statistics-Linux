# Mod Statistics (Linux Edition)

A Linux native version of the Mod Statistics dashboard, forked from AtomicTyler and rebuilt using C# and Avalonia UI.
This tool provides an easy to use interface for tracking downloads and ratings for your mods.

## Features
* Linux Native: Rebuilt with Avalonia UI to run natively on Linux environments, including Arch Linux.
* Thunderstore Tracking: Fetches and aggregates statistics specifically for mods listed in your thunderstore.json.
* Local Data: Processes your mod data locally through the application interface.
* Embedded Configuration: Uses an embedded JSON file for mod tracking to keep the executable portable.

## Configuration
To add or remove mods, edit the following file before building:
* thunderstore.json
  Add your mod names, community IDs, and store links.

Note: Please keep the JSON structure consistent to ensure the parser handles the data correctly.

## Building
This project targets .NET 8.0.

1. Clone the repository
2. Install dependencies
   dotnet restore
3. Build the project
   dotnet build

## Running on Linux
After building, the application produces a native Linux executable.

Option 1: Run normally 
Navigate to the output directory bin/Debug/net8.0/ and double click the ModStatistics executable.

Option 2: Run from terminal 
cd bin/Debug/net8.0/ 
./ModStatistics

Both methods launch the same native executable.

## Changing the Application Name (GUI)

To change the name displayed in the application, modify the MainWindow constructor where the window properties and title are defined.

1. Open MainWindow.cs

2. Window Title Bar
Change the string assigned to Title.

Find:
Title = "Omniscye Mod Statistics";

Replace the string with your preferred application name.

3. Dashboard Header
Change the text assigned to the title block.

Find:
Text = "Mod Statistics Dashboard";

Update this string to whatever you want displayed at the top of the window.

## Linux Specifics
The application includes a fallback for the DISPLAY environment variable to ensure compatibility with various X11 and Wayland configurations.

## Credits
* Original C# project by AtomicTyler
* Linux port and Avalonia UI implementation by Omniscye
