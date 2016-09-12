using System;
using System.Collections.Generic;
using ALMSimpleClient.IO;
using TDAPIOLELib;

namespace ALMSimpleClient.OTA
{
    public class AlmConnection
    {
        public string ConnectionUrl { get; set; }
        public string User { get; set; }
        public Dictionary<string, List<string>> Domains { get; set; }

        private readonly TDConnection _connection;

        public AlmConnection(string url)
        {
            ConnectionUrl = url;
            _connection = new TDConnection();
            _connection.InitConnectionEx(ConnectionUrl);
        }

        public void Authenticate(string user, string password)
        {
            User = user;
            _connection.Login(User, password);

            GetProjects();
        }

        public void Connect(string domain, string project)
        {
            _connection.Connect(domain, project);
        }

        public void ReleaseConnection()
        {
            try
            {
                if (_connection == null) return;

                if (_connection.Connected)
                    _connection.Disconnect();

                if (_connection.LoggedIn)
                    _connection.Logout();

                _connection.ReleaseConnection();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public LabFolder GetRootFolder()
        {
            TestLabFolderFactory factory = _connection.TestLabFolderFactory;
            var rootFolder = factory.Root;
            var labFolder = new LabFolder(this, rootFolder.ID, "Root", null);

            return labFolder;
        }

        public TestSetFactory GetTestSetFactory()
        {
            return _connection.TestSetFactory;
        }

        public dynamic GetTestLabFolder(int id)
        {
            TestLabFolderFactory factory = _connection.TestLabFolderFactory;
            TDFilter filter = factory.Filter;
            filter["CF_ITEM_ID"] = id.ToString();
            var folders = filter.NewList();
            dynamic folder = folders[1];

            return folder;
        }

        public dynamic GetTestLabSet(int id)
        {
            TestSetFactory factory = _connection.TestSetFactory;
            TDFilter filter = factory.Filter;
            filter["CY_CYCLE_ID"] = id.ToString();
            var sets = filter.NewList();
            dynamic set = sets[1];

            return set;
        }

        public List<FilterField> GetFields()
        {
            var fields = new List<FilterField>();

            var fieldList = _connection.TestSetFactory.Fields;
            foreach (var field in fieldList)
            {
                if (!field.Property.IsCanFilter || !field.Property.IsActive) continue;

                var name = field.Name;
                var label = field.Property.UserLabel;
                if (label != string.Empty)
                    fields.Add(new FilterField(name, label));
            }

            return fields;
        }

        private void GetProjects()
        {
            var res = _connection.GetAllVisibleProjectDescriptors();

            Domains = new Dictionary<string, List<string>>();
            foreach (dynamic item in res)
            {
                string domain = item.DomainName;
                string project = item.Name;
                if (!Domains.ContainsKey(domain))
                {
                    Domains.Add(domain, new List<string>());
                }

                Domains[domain].Add(project);
            }
        }
    }
}
