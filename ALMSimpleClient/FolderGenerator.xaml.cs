using System.IO;
using System.Windows;
using System.Windows.Controls;
using ALMSimpleClient.IO;
using ALMSimpleClient.OTA;
using ALMSimpleClient.Util;
using Directory = Alphaleonis.Win32.Filesystem.Directory;
using File = Alphaleonis.Win32.Filesystem.File;

namespace ALMSimpleClient
{
    /// <summary>
    /// Interaction logic for FolderGenerator.xaml
    /// </summary>
    public partial class FolderGenerator
    {
        private readonly LabItem _selectedFolder;
        public FolderGenerator(LabItem folder)
        {
            InitializeComponent();

            //Initialize folder generator window
            _selectedFolder = folder;
            textBox_labFolder.Text = folder.Name;

            //Load settings
            if (Directory.Exists(Settings.TargetDirectory))
            {
                textBox_targetDirectory.Text = Settings.TargetDirectory;
                FolderCreator.TargetDirectory = Settings.TargetDirectory;
            }

            checkBox_enableTemplate.IsChecked = bool.Parse(Settings.IncludeTemplate);
            if (File.Exists(Settings.TemplateFilePath))
            {
                textBox_templateFile.Text = Settings.TemplateFilePath;
                FolderCreator.TemplateFilePath = Settings.TemplateFilePath;
            }

            checkBox_longPaths.IsChecked = bool.Parse(Settings.LongPaths);
            checkBox_renameFolders.IsChecked = bool.Parse(Settings.RenameFolders);
            checkBox_useTestNames.IsChecked = bool.Parse(Settings.UseTestNames);
        }

        private void button_browseDirectory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folder = FolderCreator.BrowseFolder();
                if (folder != null)
                {
                    textBox_targetDirectory.Text = folder;
                    FolderCreator.TargetDirectory = folder;
                }
            }
            catch (PathTooLongException ex)
            {
                MessageBox.Show("The selected path is too long for the directory browser, please write it manually.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.LogException(ex);
            }
        }

        private void checkBox_enableTemplate_Checked(object sender, RoutedEventArgs e)
        {
            button_browseTemplate.IsEnabled = true;
            textBox_templateFile.IsEnabled = true;
        }

        private void checkBox_enableTemplate_Unchecked(object sender, RoutedEventArgs e)
        {
            button_browseTemplate.IsEnabled = false;
            textBox_templateFile.IsEnabled = false;
        }

        private void button_browseTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var template = FolderCreator.BrowseFile();
                textBox_templateFile.Text = template;
                FolderCreator.TemplateFilePath = template;
            }
            catch (PathTooLongException ex)
            {
                MessageBox.Show("The selected path is too long for the directory browser, please write it manually.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.LogException(ex);
            }
        }

        private void button_createFolders_Click(object sender, RoutedEventArgs e)
        {
            //Preconditions check
            if (!Directory.Exists(FolderCreator.TargetDirectory))
            {
                MessageBox.Show("The selected target directory is not valid", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Creation settings
            FolderCreator.TargetDirectory = textBox_targetDirectory.Text;
            FolderCreator.TemplateFilePath = textBox_templateFile.Text;
            FolderCreator.IncludeTemplate = checkBox_enableTemplate.IsChecked.GetValueOrDefault(false);
            FolderCreator.LongPaths = checkBox_longPaths.IsChecked.GetValueOrDefault(false);
            FolderCreator.RenameFolders = checkBox_renameFolders.IsChecked.GetValueOrDefault(false);
            FolderCreator.UseTestNames = checkBox_useTestNames.IsChecked.GetValueOrDefault(false);

            //Save settings
            Settings.TargetDirectory = FolderCreator.TargetDirectory;
            Settings.IncludeTemplate = FolderCreator.IncludeTemplate.ToString();
            Settings.TemplateFilePath = FolderCreator.TemplateFilePath;
            Settings.LongPaths = FolderCreator.LongPaths.ToString();
            Settings.RenameFolders = FolderCreator.RenameFolders.ToString();
            Settings.UseTestNames = FolderCreator.UseTestNames.ToString();
            Settings.Save();

            //Clear results
            expander.IsExpanded = false;
            listBox_log.Items.Clear();

            //Create the folders
            FolderCreator.GenerateFolders(_selectedFolder);

            //Display results
            foreach (var item in FolderCreator.ResultsDictionary)
            {
                var content = item.Key.GetDescription() + ": " + item.Value;
                listBox_log.Items.Add(new ListBoxItem { Content = content });
            }

            if (FolderCreator.ResultsExceptions.Count != 0)
            {
                listBox_log.Items.Add(new ListBoxItem { Content = "Exceptions", FontWeight = FontWeights.Bold });
                listBox_log.Items.Add(new Separator());

                foreach (var item in FolderCreator.ResultsExceptions)
                {
                    var content = item.Value.GetDescription() + ": " + item.Key;
                    listBox_log.Items.Add(new ListBoxItem { Content = content });
                }
            }
            expander.IsExpanded = true;

            MessageBox.Show("Folders created", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
