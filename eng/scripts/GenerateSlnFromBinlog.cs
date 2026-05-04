// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#:package MSBuild.StructuredLogger@2.*
#:property RollForward=LatestMajor

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Logging.StructuredLogger;

if (args.Length == 0 || args[0] is "-h" or "--help")
{
    Console.WriteLine("""
        Usage: dotnet run --file eng/scripts/GenerateSlnFromBinlog.cs -- <binlog-path> [output.slnx]

        Generates a .slnx file from all projects found in an MSBuild binary log.

        Arguments:
          binlog-path       Path to the .binlog file
          output.slnx       Output path (default: everything.slnx in current directory)
        """);
    return;
}

string binlogPath = Path.GetFullPath(args[0]);
if (!File.Exists(binlogPath))
{
    Console.Error.WriteLine($"Error: Binlog file not found: {binlogPath}");
    Environment.Exit(1);
}

string outputPath = args.Length > 1 ? Path.GetFullPath(args[1]) : Path.GetFullPath("everything.slnx");

Console.WriteLine($"Binlog: {binlogPath}");
Console.WriteLine($"Output: {outputPath}");

// Extract projects from binlog
Console.WriteLine("Reading binlog...");
var build = BinaryLog.ReadBuild(binlogPath);
Console.WriteLine("Scanning for project nodes...");
var projects = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
int nodesVisited = 0;

build.VisitAllChildren<Project>(project =>
{
    nodesVisited++;
    if (nodesVisited % 100 == 0)
    {
        Console.Write($"\r  Visited {nodesVisited} project nodes, found {projects.Count} unique projects...");
    }

    var path = project.ProjectFile;
    if (!string.IsNullOrEmpty(path) && File.Exists(path))
    {
        var ext = Path.GetExtension(path);
        if (ext.Equals(".csproj", StringComparison.OrdinalIgnoreCase) ||
            ext.Equals(".vbproj", StringComparison.OrdinalIgnoreCase) ||
            ext.Equals(".fsproj", StringComparison.OrdinalIgnoreCase))
        {
            projects.Add(Path.GetFullPath(path));
        }
    }
});

Console.WriteLine($"\r  Visited {nodesVisited} project nodes, found {projects.Count} unique projects.   ");

if (projects.Count == 0)
{
    Console.Error.WriteLine("Error: No projects found in binlog.");
    Environment.Exit(1);
}

Console.WriteLine($"Found {projects.Count} unique projects.");

// Generate SLNX with paths relative to the directory containing the .slnx file
string slnxDir = Path.GetDirectoryName(outputPath)!;
Directory.CreateDirectory(slnxDir);

using var writer = new StreamWriter(outputPath, append: false, encoding: new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
writer.WriteLine("<Solution>");

// Group projects by their relative directory to create solution folders
var projectsByFolder = projects
    .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
    .Select(p => new { FullPath = p, RelativePath = Path.GetRelativePath(slnxDir, p).Replace('\\', '/') })
    .GroupBy(p => GetFolderPath(p.RelativePath))
    .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

foreach (var folder in projectsByFolder)
{
    writer.WriteLine($"  <Folder Name=\"/{folder.Key}/\">");
    foreach (var proj in folder)
    {
        writer.WriteLine($"    <Project Path=\"{proj.RelativePath}\" />");
    }
    writer.WriteLine("  </Folder>");
}

writer.WriteLine("</Solution>");

Console.WriteLine($"Solution created: {outputPath}");
Console.WriteLine($"Projects added: {projects.Count}");

static string GetFolderPath(string relativePath)
{
    var dir = relativePath.Substring(0, relativePath.LastIndexOf('/'));
    // Strip leading "../" segments for cleaner folder names
    while (dir.StartsWith("../"))
    {
        dir = dir.Substring(3);
    }

    return dir;
}
