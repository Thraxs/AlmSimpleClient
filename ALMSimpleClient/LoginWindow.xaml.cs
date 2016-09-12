using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using ALMSimpleClient.IO;
using ALMSimpleClient.OTA;

namespace ALMSimpleClient
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class LoginWindow
    {
        private AlmConnection _almConnection;
        private List<string> _projects;
        private bool _loggedIn;

        public LoginWindow()
        {
            InitializeComponent();

            //Load settings
            Settings.Load();
            textBox_url.Text = Settings.LastUrl;
            textBox_username.Text = Settings.LastUser;

            //DEBUG
            /*
            _almConnection = new AlmConnection("http://192.168.1.10:8080/qcbin/");
            _almConnection.Authenticate("admin", "sdah236@#as3");
            _almConnection.Connect("TSTDOMAIN" , "TSTPROJECT");
            var mainWindow = new MainWindow(_almConnection);
            mainWindow.Show();
            _loggedIn = true;
            Close();*/
        }

        private void button_authenticate_Click(object sender, RoutedEventArgs e)
        {
            //Check ALM url
            var uri = textBox_url.Text;
            if (uri.Contains("qcbin") && !(uri.EndsWith("/qcbin/") || uri.EndsWith("/qcbin")))
            {
                var binIndex = uri.IndexOf("qcbin", StringComparison.Ordinal) + 5;
                textBox_url.Text = uri.Remove(binIndex, uri.Length - binIndex);
            }

            textBox_url.IsEnabled = false;
            textBox_username.IsEnabled = false;
            textBox_password.IsEnabled = false;
            button_authenticate.IsEnabled = false;
            button_cancel.IsEnabled = true;

            Settings.LastUrl = textBox_url.Text;
            Settings.LastUser = textBox_username.Text;
            Settings.Save();

            label_status.Content = "Authenticating";
            label_status.Foreground = Brushes.Black;
            label_status.Visibility = Visibility.Visible;
            progressBar_status.IsIndeterminate = true;

            try
            {
                var url = textBox_url.Text;
                var user = textBox_username.Text;

                _almConnection = new AlmConnection(url);
                _almConnection.Authenticate(user, textBox_password.Password);

                comboBox_domain.ItemsSource = _almConnection.Domains;
                comboBox_domain.IsEnabled = true;

                EndTask();
            }
            catch (COMException ex)
            {
                var exceptionCode = OleException.GetCode(ex.ErrorCode);
                EndTask(exceptionCode == 30006
                    ? "Wrong user name or password."
                    : "Connection error");
                textBox_url.IsEnabled = true;
                textBox_username.IsEnabled = true;
                textBox_password.IsEnabled = true;
                button_authenticate.IsEnabled = true;
                button_cancel.IsEnabled = false;

                Logger.LogException(ex);
            }
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            textBox_url.IsEnabled = true;
            textBox_username.IsEnabled = true;
            textBox_password.IsEnabled = true;
            button_authenticate.IsEnabled = true;
            button_cancel.IsEnabled = false;
            comboBox_domain.ItemsSource = null;
            comboBox_domain.IsEnabled = false;
            comboBox_project.ItemsSource = null;
            comboBox_project.IsEnabled = false;
        }

        private void EndTask(string error = "")
        {
            if (error != string.Empty)
            {
                label_status.Content = error;
                label_status.Foreground = Brushes.Red;
                label_status.Visibility = Visibility.Visible;
            }
            else
            {
                label_status.Content = "";
                label_status.Foreground = Brushes.Black;
                label_status.Visibility = Visibility.Hidden;
            }

            progressBar_status.IsIndeterminate = false;
        }

        private void comboBox_domain_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comboBox_domain.SelectedIndex == -1) return;

            var domain = comboBox_domain.SelectedValue.ToString();
            _projects = _almConnection.Domains[domain];
            comboBox_project.ItemsSource = _projects;

            comboBox_project.IsEnabled = true;
        }

        private void comboBox_project_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            button_login.IsEnabled = comboBox_project.SelectedIndex != -1;
        }

        private void button_login_Click(object sender, RoutedEventArgs e)
        {
            _almConnection.Connect(comboBox_domain.SelectedValue.ToString(), comboBox_project.SelectedValue.ToString());
            var mainWindow = new MainWindow(_almConnection);
            mainWindow.Show();
            _loggedIn = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_loggedIn)
                _almConnection?.ReleaseConnection();
        }
    }
}
