
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Globalization;

public static class UnusedAssetsFinder
{
    private static readonly HashSet<string> extensions = new()
    {
        ".png", ".jpg", ".jpeg", ".tga", ".psd",
        ".wav", ".mp3", ".ogg",
        ".mat", ".anim", ".controller",
        ".fbx", ".obj",
        ".asset", ".rendertexture",
        ".shader", ".shadergraph",
        ".ttf", ".otf"
    };

    private static readonly string[] protectedFolders =
    {
        "Resources",
        "Editor",
        "StreamingAssets",
        "AddressableAssetsData",
        "Settings",
        "Plugins",
        "TextMesh Pro"
    };

    private static readonly HashSet<string> specialExtensions = new()
    {
        ".cs", ".js", ".dll", ".asmdef",
        ".asmref", ".unity", ".prefab",
        ".uxml", ".uss", ".compute"
    };

    [MenuItem("Tools/Find Unused Assets")]
    public static void FindUnusedAssets()
    {
        string[] paths = AssetDatabase.GetAllAssetPaths()
            .Where(p => p.StartsWith("Assets/"))
            .Where(p => !AssetDatabase.IsValidFolder(p))
            .ToArray();

        HashSet<string> referenced = new();

        // Dependencia -> archivos que la referencian.
        Dictionary<string, HashSet<string>> reverseReferences = new();

        try
        {
            for (int i = 0; i < paths.Length; i++)
            {
                string source = paths[i];

                EditorUtility.DisplayProgressBar(
                    "Analizando referencias",
                    source,
                    (float)i / paths.Length
                );

                string[] dependencies =
                    AssetDatabase.GetDependencies(source, false);

                foreach (string dependency in dependencies)
                {
                    if (dependency == source)
                        continue;

                    referenced.Add(dependency);

                    if (!reverseReferences.TryGetValue(
                        dependency, out var sources))
                    {
                        sources = new HashSet<string>();
                        reverseReferences[dependency] = sources;
                    }

                    sources.Add(source);
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        // Archivos sin referencias detectadas.
        List<(string path, long size)> unused = new();

        foreach (string path in paths)
        {
            string extension =
                Path.GetExtension(path).ToLowerInvariant();

            if (!extensions.Contains(extension))
                continue;

            if (referenced.Contains(path))
                continue;

            unused.Add((path, GetFileSize(path)));
        }

        string projectPath =
            Directory.GetParent(Application.dataPath).FullName;

        string assetsReportPath = Path.Combine(
            projectPath, "UnusedAssetsReport.csv"
        );

        string foldersReportPath = Path.Combine(
            projectPath, "UnusedFoldersReport.csv"
        );

        GenerateAssetsReport(unused, assetsReportPath);

        int candidateFolders = GenerateFoldersReport(
            paths,
            reverseReferences,
            foldersReportPath
        );

        double totalSizeMB =
            unused.Sum(a => a.size) / (1024.0 * 1024.0);

        Debug.Log(
            $"Análisis completado.\n" +
            $"Archivos sin referencias: {unused.Count}\n" +
            $"Tamaño: {totalSizeMB:F2} MB\n" +
            $"Carpetas sin referencias externas: {candidateFolders}\n" +
            $"Informe de archivos: {assetsReportPath}\n" +
            $"Informe de carpetas: {foldersReportPath}"
        );

        EditorUtility.RevealInFinder(foldersReportPath);
    }

    private static void GenerateAssetsReport(
        List<(string path, long size)> unused,
        string outputPath)
    {
        var groupedAssets = unused
            .GroupBy(a =>
                Path.GetDirectoryName(a.path)?
                    .Replace('\\', '/')
                ?? "Sin carpeta"
            )
            .OrderBy(g => g.Key);

        StringBuilder report = new();

        report.AppendLine("Carpeta,Archivo,Tamaño (MB)");

        foreach (var group in groupedAssets)
        {
            long folderSize = group.Sum(a => a.size);

            report.AppendLine();
            report.AppendLine(
                $"{Csv(group.Key + "/")},,"
            );

            foreach (var asset in group.OrderByDescending(a => a.size))
            {
                report.AppendLine(
                    $",{Csv(Path.GetFileName(asset.path))}," +
                    ToMB(asset.size)
                );
            }

            report.AppendLine(
                $",{Csv("TOTAL CARPETA")},{ToMB(folderSize)}"
            );
        }

        report.AppendLine();
        report.AppendLine(
            $"{Csv("TOTAL GENERAL")},," +
            ToMB(unused.Sum(a => a.size))
        );

        File.WriteAllText(
            outputPath, report.ToString(), Encoding.UTF8
        );
    }

    private static int GenerateFoldersReport(
        string[] paths,
        Dictionary<string, HashSet<string>> reverseReferences,
        string outputPath)
    {
        string[] folders = AssetDatabase.GetAllAssetPaths()
            .Where(AssetDatabase.IsValidFolder)
            .Where(p => p.StartsWith("Assets/"))
            .OrderBy(p => p)
            .ToArray();

        HashSet<string> enabledScenes = new(
            EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
        );

        StringBuilder report = new();

        report.AppendLine(
            "Carpeta,Estado,Archivos,Referencias externas," +
            "Archivos especiales,Tamaño (MB),Ejemplo de referencia"
        );

        int candidateCount = 0;

        foreach (string folder in folders)
        {
            string prefix = folder + "/";

            string[] folderFiles = paths
                .Where(p => p.StartsWith(
                    prefix, StringComparison.Ordinal))
                .ToArray();

            if (folderFiles.Length == 0)
                continue;

            // Contar referencias provenientes de fuera.
            HashSet<string> externalSources = new();

            foreach (string file in folderFiles)
            {
                if (!reverseReferences.TryGetValue(
                    file, out var sources))
                    continue;

                foreach (string source in sources)
                {
                    if (!source.StartsWith(
                        prefix, StringComparison.Ordinal))
                    {
                        externalSources.Add(source);
                    }
                }
            }

            int externalCount = externalSources.Count;

            int specialCount = folderFiles.Count(p =>
                specialExtensions.Contains(
                    Path.GetExtension(p).ToLowerInvariant()
                )
            );

            bool protectedFolder = IsProtectedFolder(folder);

            bool containsEnabledScene =
                folderFiles.Any(enabledScenes.Contains);

            bool containsCode = folderFiles.Any(p =>
            {
                string ext =
                    Path.GetExtension(p).ToLowerInvariant();

                return ext == ".cs" ||
                       ext == ".dll" ||
                       ext == ".asmdef" ||
                       ext == ".asmref";
            });

            long folderSize =
                folderFiles.Sum(GetFileSize);

            string status;

            if (protectedFolder)
            {
                status = "PROTEGIDA - Revisar manualmente";
            }
            else if (containsEnabledScene)
            {
                status = "ESCENA EN BUILD - Conservar";
            }
            else if (externalCount > 0)
            {
                status = "TIENE REFERENCIAS EXTERNAS";
            }
            else if (containsCode)
            {
                status = "REVISAR - Contiene codigo";
            }
            else if (specialCount > 0)
            {
                status = "REVISAR - Sin referencias externas, contiene prefabs o escenas";
                candidateCount++;
            }
            else
            {
                status = "CANDIDATA - Sin referencias externas detectadas";
                candidateCount++;
            }

            string exampleReference = externalSources
                .OrderBy(p => p)
                .FirstOrDefault() ?? "";

            report.AppendLine(
                $"{Csv(folder)}," +
                $"{Csv(status)}," +
                $"{folderFiles.Length}," +
                $"{externalCount}," +
                $"{specialCount}," +
                $"{ToMB(folderSize)}," +
                $"{Csv(exampleReference)}"
            );
        }

        File.WriteAllText(
            outputPath, report.ToString(), Encoding.UTF8
        );

        return candidateCount;
    }

    private static bool IsProtectedFolder(string folder)
    {
        string[] segments = folder.Split('/');

        return protectedFolders.Any(name =>
            segments.Any(segment =>
                segment.Equals(
                    name,
                    StringComparison.OrdinalIgnoreCase
                )
            )
        );
    }

    private static long GetFileSize(string path)
    {
        return File.Exists(path)
            ? new FileInfo(path).Length
            : 0;
    }

    private static string ToMB(long bytes)
    {
        return (bytes / (1024.0 * 1024.0))
            .ToString("F3", CultureInfo.InvariantCulture);
    }

    private static string Csv(string value)
    {
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
#endif
