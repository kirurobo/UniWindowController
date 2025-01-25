using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// PackageExporter
/// </summary>
public static class PackageExporter
{
    private const string RootDirectory = "Assets/Kirurobo/UniWindowController/Runtime";
    private const string FileName = "UniWindowController";

    /// <summary>
    ///     パッケージの書き出し(エディタ上でのテスト用)
    ///     メニュー 「Tools > Export Unitypackage」をクリックで実行
    /// </summary>
    [MenuItem("Tools/Export Unitypackage")]
    public static void ExportTestOnEditor()
    {
        var exportPath = EditorUtility.SaveFilePanel
        (
            "保存先を選択",
            Application.dataPath,
            $"{FileName}.unitypackage",
            "unitypackage"
        );

        CreatePackage(RootDirectory, exportPath);
    }

    /// <summary>
    ///     パッケージの書き出し
    ///     GitHub Actionsから呼ばれる
    /// </summary>
    public static void Export()
    {
        CreatePackage(RootDirectory, $"build/{FileName}.unitypackage");
    }

    /// <summary>
    ///    パッケージの書き出し
    /// </summary>
    /// <param name="rootDirectory"></param>
    /// <param name="exportPath"></param>
    private static void CreatePackage(string rootDirectory, string exportPath)
    {
        SafeCreateDirectory(exportPath);
        var assetsPaths = GetAllAssetsAtPath(rootDirectory);
        AssetDatabase.ExportPackage(assetsPaths, exportPath, ExportPackageOptions.Default);
        Debug.Log(
            $"Export complete: {Path.GetFullPath(exportPath)}\nExport below files:\n{string.Join("\n", assetsPaths)}");
    }

    /// <summary>
    ///    ディレクトリが存在しない場合にディレクトリを作成する
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static DirectoryInfo SafeCreateDirectory(string path)
    {
        var diParent = Directory.GetParent(path);
        if (diParent == null || Directory.Exists(diParent.FullName)) return null;
        return Directory.CreateDirectory(diParent.FullName);
    }

    /// <summary>
    ///    指定したパス以下の全てのファイルパスを取得する
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    private static string[] GetAllAssetsAtPath(string root)
    {
        return Directory.GetFiles(root, "*", SearchOption.AllDirectories)
            .Where(x => !string.IsNullOrEmpty(x))
            .Where(x => !x.EndsWith(".meta"))
            .Where(x => x != ".DS_Store")
            .ToArray();
    }
}