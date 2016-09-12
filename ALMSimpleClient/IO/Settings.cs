using System;
using System.IO;
using System.Xml;

namespace ALMSimpleClient.IO
{
    static class Settings
    {
        private static readonly string ConfigFolder =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ALM Simple Client";

        private static readonly string ConfigFile = ConfigFolder + "\\config.xml";

        //Login settings
        public static string LastUrl = "";
        public static string LastUser = "";

        //Folder creator settings
        public static string TargetDirectory = "";
        public static string IncludeTemplate = "False";
        public static string TemplateFilePath = "";
        public static string LongPaths = "False";
        public static string RenameFolders = "False";
        public static string UseTestNames = "False";

        //Test analysis settings
        public static string LastFilterValue = "";
        public static string RunAnalysis = "False";
        public static string StepAnalysis = "False";
        public static string AttachmentAnalysis = "False";
        public static string DefectAnalysis = "False";

        public static void Save()
        {
            if (!ConfigFolderExists())
                Directory.CreateDirectory(ConfigFolder);

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t"
            };

            using (var writer = XmlWriter.Create(ConfigFile, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Settings");

                writer.WriteStartElement("Login");
                writer.WriteElementString("LastUrl", LastUrl);
                writer.WriteElementString("LastUser", LastUser);
                writer.WriteEndElement();

                writer.WriteStartElement("FolderCreation");
                writer.WriteElementString("TargetDirectory", TargetDirectory);
                writer.WriteElementString("IncludeTemplate", IncludeTemplate);
                writer.WriteElementString("TemplateFilePath", TemplateFilePath);
                writer.WriteElementString("LongPaths", LongPaths);
                writer.WriteElementString("RenameFolders", RenameFolders);
                writer.WriteElementString("UseTestNames", UseTestNames);
                writer.WriteEndElement();

                writer.WriteStartElement("Analysis");
                writer.WriteElementString("LastFilterValue", LastFilterValue);
                writer.WriteElementString("RunAnalysis", RunAnalysis);
                writer.WriteElementString("StepAnalysis", StepAnalysis);
                writer.WriteElementString("AttachmentAnalysis", AttachmentAnalysis);
                writer.WriteElementString("DefectAnalysis", DefectAnalysis);
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        public static void Load()
        {
            if (!ConfigFileExists()) return;

            using (var reader = XmlReader.Create(ConfigFile))
            {
                var category = "";
                while (reader.Read())
                {
                    if (!reader.IsStartElement()) continue;

                    if (reader.Depth < 2)
                    {
                        category = reader.Name;
                        continue;
                    }

                    string value;
                    switch (category)
                    {
                        case "Login":
                            switch (reader.Name)
                            {
                                case "LastUrl":
                                    reader.Read();
                                    value = reader.Value.Trim();
                                    LastUrl = value;
                                    break;
                                case "LastUser":
                                    reader.Read();
                                    value = reader.Value.Trim();
                                    LastUser = value;
                                    break;
                            }
                            break;
                        case "FolderCreation":
                            switch (reader.Name)
                            {
                                case "TargetDirectory":
                                    reader.Read();
                                    value = reader.Value.Trim();
                                    TargetDirectory = value;
                                    break;
                                case "IncludeTemplate":
                                    reader.Read();
                                    value = reader.Value.Trim();
                                    IncludeTemplate = value;
                                    break;
                                case "TemplateFilePath":
                                    reader.Read();
                                    value = reader.Value.Trim();
                                    TemplateFilePath = value;
                                    break;
                                case "LongPaths":
                                    reader.Read();
                                    value = reader.Value.Trim();
                                    LongPaths = value;
                                    break;
                                case "RenameFolders":
                                    reader.Read();
                                    value = reader.Value.Trim();
                                    RenameFolders = value;
                                    break;
                                case "UseTestNames":
                                    reader.Read();
                                    value = reader.Value.Trim();
                                    UseTestNames = value;
                                    break;
                            }
                            break;
                        case "Analysis":
                            switch (reader.Name)
                            {
                                case "LastFilterValue":
                                    reader.Read();
                                    value = reader.Value.Trim();
                                    LastFilterValue = value;
                                    break;
                                case "RunAnalysis":
                                    reader.Read();
                                    value = reader.Value.Trim();
                                    RunAnalysis = value;
                                    break;
                                case "StepAnalysis":
                                    reader.Read();
                                    value = reader.Value.Trim();
                                    StepAnalysis = value;
                                    break;
                                case "AttachmentAnalysis":
                                    reader.Read();
                                    value = reader.Value.Trim();
                                    AttachmentAnalysis = value;
                                    break;
                                case "DefectAnalysis":
                                    reader.Read();
                                    value = reader.Value.Trim();
                                    DefectAnalysis = value;
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        private static bool ConfigFolderExists()
        {
            return Directory.Exists(ConfigFolder);
        }

        private static bool ConfigFileExists()
        {
            return File.Exists(ConfigFile);
        }
    }
}
