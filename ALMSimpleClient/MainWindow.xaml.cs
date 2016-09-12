using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using ALMSimpleClient.IO;
using ALMSimpleClient.OTA;

namespace ALMSimpleClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly AlmConnection _almConnection;
        private LabFolder _rootFolder;

        private static ObservableCollection<LabTestInstance> _summaryTests;

        private CancellationTokenSource _cancellationToken;

        public MainWindow(AlmConnection connection)
        {
            InitializeComponent();

            _almConnection = connection;

            //Load settings
            textBox_filterValue.Text = Settings.LastFilterValue;
            checkBox_runAnalysis.IsChecked = bool.Parse(Settings.RunAnalysis);
            checkBox_stepAnalysis.IsChecked = bool.Parse(Settings.StepAnalysis);
            checkBox_attachmentAnalysis.IsChecked = bool.Parse(Settings.AttachmentAnalysis);
            checkBox_defectAnalysis.IsChecked = bool.Parse(Settings.DefectAnalysis);

            //Initialize test lab
            _rootFolder = _almConnection.GetRootFolder();
            treeView.ItemsSource = new ObservableCollection<LabFolder> { _rootFolder };
            instancesPanel.Visibility = Visibility.Hidden;

            //Initialize test summary
            var filterFields = _almConnection.GetFields();
            comboBox_filterFields.ItemsSource = filterFields;
            if (comboBox_filterFields.Items.Count != 0)
                comboBox_filterFields.SelectedIndex = 0;

            _summaryTests = new ObservableCollection<LabTestInstance>();
            resultsGrid.ItemsSource = _summaryTests;

            analysisResultsPanel.Visibility = Visibility.Hidden;
        }

        private void menu_disconnect_Click(object sender, RoutedEventArgs e)
        {
            _almConnection.ReleaseConnection();
            new LoginWindow().Show();
            Close();
        }

        private void menu_exit_Click(object sender, RoutedEventArgs e)
        {
            _almConnection.ReleaseConnection();
            Application.Current.Shutdown();
        }

        private void treeView_Expanded(object sender, RoutedEventArgs e)
        {
            var treeViewItem = e.OriginalSource as TreeViewItem;
            var item = (LabItem) treeViewItem?.Header;

            if (item?.Type == LabItemType.Folder)
            {
                ((LabFolder) item).DiscoverChildren();
            }
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = (LabItem) treeView.SelectedItem;

            if (item.Type == LabItemType.TestSet)
            {
                var labSet = (LabSet) item;
                labSet.DiscoverTests();
                instancesGrid.ItemsSource = labSet.Tests;

                instancesPanel.Visibility = Visibility.Visible;
            }
            else
            {
                instancesPanel.Visibility = Visibility.Hidden;
            }
        }

        private void refreshInstances_Click(object sender, RoutedEventArgs e)
        {
            var item = (LabItem)treeView.SelectedItem;

            instancesPanel.Visibility = Visibility.Hidden;

            var labSet = (LabSet)item;
            labSet.IsDiscovered = false;
            labSet.DiscoverTests();
            instancesGrid.ItemsSource = labSet.Tests;

            instancesPanel.Visibility = Visibility.Visible;
        }

        private void refreshFolders_Click(object sender, RoutedEventArgs e)
        {
            _rootFolder = _almConnection.GetRootFolder();
            treeView.ItemsSource = new ObservableCollection<LabFolder> { _rootFolder };
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _almConnection.ReleaseConnection();
        }

        private void expandFolders_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (FrameworkElement)sender;
            var folder = (LabFolder)menuItem.DataContext;

            if (folder.Parent == null)
            {
                var messageBoxResult =
                    MessageBox.Show("Expanding the root folder may take a while. Do you want to continue?",
                        "Expand folder confirmation", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.No)
                    return;
            }

            if (!folder.IsSelected)
                folder.IsSelected = true;

            folder.Expand();
        }

        private void collapseFolders_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (FrameworkElement)sender;
            var folder = (LabFolder)menuItem.DataContext;

            if (!folder.IsSelected)
                folder.IsSelected = true;

            folder.Collapse();
        }

        private void setFolderFilter_Click(object sender, RoutedEventArgs e)
        {

        }

        private void exportFolders_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (LabItem)treeView.SelectedItem;
            if (selectedItem == null)
            {
                MessageBox.Show("You must select a folder or test set",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var createFolderWindow = new FolderGenerator(selectedItem);
            createFolderWindow.ShowDialog();
        }

        private void exportFolderContext_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (FrameworkElement)sender;
            var selectedItem = (LabItem)menuItem.DataContext;

            if (!selectedItem.IsSelected)
                selectedItem.IsSelected = true;

            var createFolderWindow = new FolderGenerator(selectedItem);
            createFolderWindow.ShowDialog();
        }

        private void checkBox_runAnalysis_Checked(object sender, RoutedEventArgs e)
        {
            checkBox_stepAnalysis.IsEnabled = true;
        }

        private void checkBox_runAnalysis_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBox_stepAnalysis.IsChecked = false;
            checkBox_stepAnalysis.IsEnabled = false;
        }

        private async void button_startAnalysis_Click(object sender, RoutedEventArgs e)
        {
            _cancellationToken = new CancellationTokenSource();
            button_startAnalysis.IsEnabled = false;
            button_cancelAnalysis.IsEnabled = true;
            analysisResultsPanel.Visibility = Visibility.Visible;
            label_status.Content = "Running analysis...";
            label_results.Content = "Results";

            var filterField = (FilterField)comboBox_filterFields.SelectedItem;
            var filterValue = textBox_filterValue.Text;
            var analizeRuns = checkBox_runAnalysis.IsChecked.GetValueOrDefault(false);
            var analizeSteps = checkBox_stepAnalysis.IsChecked.GetValueOrDefault(false);
            var analizeAttachments = checkBox_attachmentAnalysis.IsChecked.GetValueOrDefault(false);
            var analizeDefects = checkBox_defectAnalysis.IsChecked.GetValueOrDefault(false);
            var canceled = false;

            //Save settings
            Settings.LastFilterValue = filterValue;
            Settings.RunAnalysis = analizeRuns.ToString();
            Settings.StepAnalysis = analizeSteps.ToString();
            Settings.AttachmentAnalysis = analizeAttachments.ToString();
            Settings.DefectAnalysis = analizeDefects.ToString();
            Settings.Save();

            try
            {
                _summaryTests.Clear();
                listBox_status.Items.Clear();
                listBox_runs.Items.Clear();
                listBox_steps.Items.Clear();
                listBox_attachments.Items.Clear();
                listBox_defects.Items.Clear();
                await Analizer.PerformAnalysis(_almConnection, _cancellationToken.Token, filterField, 
                    filterValue, analizeRuns, analizeSteps, analizeAttachments, analizeDefects);
            }
            catch (OperationCanceledException)
            {
                canceled = true;
                label_status.Content = "Analysis canceled";
                label_results.Content = "Results";
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            finally
            {
                _cancellationToken.Dispose();
                button_startAnalysis.IsEnabled = true;
                button_cancelAnalysis.IsEnabled = false;

                var total = _summaryTests.Count;
                foreach (var item in Analizer.SummaryStatus)
                {
                    var name = item.Key + ": " + item.Value + " (" + ((item.Value * 100)/total) + "%)";
                    listBox_status.Items.Add(new ListBoxItem {Content = name });
                }
                listBox_status.Items.Add(new Separator());
                listBox_status.Items.Add(new ListBoxItem {Content = "Total: " + total});

                foreach (var item in Analizer.SummaryRunStatus)
                {
                    var name = item.Key + ": " + item.Value + " (" + ((item.Value * 100) / Analizer.TotalRuns) + "%)";
                    listBox_runs.Items.Add(new ListBoxItem { Content = name });
                }
                listBox_runs.Items.Add(new Separator());
                listBox_runs.Items.Add(new ListBoxItem { Content = "Total: " + Analizer.TotalRuns});

                foreach (var item in Analizer.SummaryStepStatus)
                {
                    var name = item.Key + ": " + item.Value + " (" + ((item.Value * 100) / Analizer.TotalSteps) + "%)";
                    listBox_steps.Items.Add(new ListBoxItem { Content = name });
                }
                listBox_steps.Items.Add(new Separator());
                listBox_steps.Items.Add(new ListBoxItem { Content = "Total: " + Analizer.TotalSteps });

                listBox_attachments.Items.Add(new ListBoxItem { Content = "Test attachments: " + Analizer.TotalAttachments });
                listBox_attachments.Items.Add(new ListBoxItem { Content = "Run attachments: " + Analizer.TotalRunAttachments });
                listBox_attachments.Items.Add(new Separator());
                listBox_attachments.Items.Add(new ListBoxItem { Content = "Total: " + (Analizer.TotalAttachments + Analizer.TotalRunAttachments) });

                listBox_defects.Items.Add(new ListBoxItem { Content = "Test defects: " + (Analizer.TotalDefects - Analizer.RunDefects) });
                listBox_defects.Items.Add(new ListBoxItem { Content = "Run defects: " + Analizer.RunDefects });
                listBox_defects.Items.Add(new Separator());
                listBox_defects.Items.Add(new ListBoxItem { Content = "Total: " + Analizer.TotalDefects });

                if (!canceled)
                {
                    label_status.Content = "Last analysis performed at " + DateTime.Now;
                    label_results.Content = "Results (" + total + " tests)";
                }
            }
        }

        public static void AddAnalysisTest(LabTestInstance test)
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                _summaryTests.Add(test);
            });
        }

        private void button_cancelAnalysis_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _cancellationToken.Cancel();
            }
            catch (ObjectDisposedException) {}
        }
    }
}
