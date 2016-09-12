using System.Collections.Generic;
using System.Windows.Forms;
using ALMSimpleClient.OTA;
using Alphaleonis.Win32.Filesystem;

namespace ALMSimpleClient.IO
{
    public static class FolderCreator
    {
        public static bool IncludeTemplate;
        public static string TemplateFilePath;
        private static string _templateFileName;

        public static string TargetDirectory;

        public static bool LongPaths;
        public static bool RenameFolders;
        public static bool UseTestNames;

        public static Dictionary<FolderCreatorResult, int> ResultsDictionary;
        public static Dictionary<string, FolderCreatorResult> ResultsExceptions;

        public static string BrowseFolder()
        {
            var dialog = new FolderBrowserDialog();

            if (Directory.Exists(TargetDirectory))
                dialog.SelectedPath = TargetDirectory;

            dialog.ShowDialog();

            return dialog.SelectedPath;
        }

        public static string BrowseFile()
        {
            var dialog = new OpenFileDialog();

            var path = "";
            if (File.Exists(TemplateFilePath))
            {
                dialog.InitialDirectory = TemplateFilePath;
                path = TemplateFilePath;
            }

            if (dialog.ShowDialog() != DialogResult.OK) return path;

            var result = dialog.FileName;
            if (!string.IsNullOrWhiteSpace(result))
                path = result;

            return path;
        }

        public static void GenerateFolders(LabItem rootFolder)
        {
            if (IncludeTemplate & File.Exists(TemplateFilePath))
                _templateFileName = Path.GetFileName(TemplateFilePath);
            else
                IncludeTemplate = false;

            ResultsDictionary = new Dictionary<FolderCreatorResult, int>();
            ResultsExceptions = new Dictionary<string, FolderCreatorResult>();

            rootFolder.CreateFolder(TargetDirectory);
        }

        public static string CreateFolder(string path, bool withTemplate)
        {
            var folderPath = path;
            var createTemplate = IncludeTemplate && withTemplate;

            //Check if folder already exists
            var renamed = false;
            if (Directory.Exists(folderPath))
            {
                if (RenameFolders)
                {
                    //Find new name
                    while (Directory.Exists(folderPath))
                    {
                        var directoryPath = Path.GetDirectoryName(folderPath);
                        var directoryName = Path.GetFileName(folderPath);

                        folderPath = Path.Combine(directoryPath, ("[REP] " + directoryName));
                    }
                    renamed = true;
                }
                else
                {
                    AddResult(folderPath, FolderCreatorResult.DuplicatedSkipped);
                    return folderPath;
                }
            }

            var pathLength = createTemplate ? 
                Path.Combine(folderPath, _templateFileName).Length : folderPath.Length;

            //Check path length
            var longPathUsed = false;
            if (pathLength > 260)
            {
                if (LongPaths)
                {
                    //Create as long path
                    longPathUsed = true;
                }
                else
                {
                    AddResult(folderPath, FolderCreatorResult.LongPathSkipped);
                    return folderPath;
                }
            }

            //Create
            Directory.CreateDirectory(folderPath);
            if (createTemplate)
                File.Copy(TemplateFilePath, Path.Combine(folderPath, _templateFileName));
                
            if (renamed)
                AddResult(folderPath, longPathUsed ? FolderCreatorResult.LongPathAndRenamed : FolderCreatorResult.DuplicatedRenamed);
            else
                AddResult(folderPath, longPathUsed ? FolderCreatorResult.LongPathCreated : FolderCreatorResult.Normal);

            return folderPath;
        }

        private static void AddResult(string path, FolderCreatorResult result)
        {
            int value;
            if (ResultsDictionary.TryGetValue(result, out value))
                ResultsDictionary[result] = value + 1;
            else
                ResultsDictionary.Add(result, 1);

            if (result != FolderCreatorResult.Normal)
                ResultsExceptions.Add(path, result);
        }
    }
}
