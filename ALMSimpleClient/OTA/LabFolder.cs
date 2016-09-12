using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ALMSimpleClient.IO;
using TDAPIOLELib;

namespace ALMSimpleClient.OTA
{
    public class LabFolder : LabItem
    {
        public ObservableCollection<LabItem> Children { get; set; }
        public bool IsDiscovered { get; set; }

        private readonly AlmConnection _connection;

        public LabFolder(AlmConnection almConnection, int id, string name, LabFolder parent = null)
        {
            Type = LabItemType.Folder;
            Id = id;
            Name = name;
            Children = new ObservableCollection<LabItem> { new LabItem { Name = "Dummy" } };

            Parent = parent;
            _connection = almConnection;
        }

        public void DiscoverChildren()
        {
            if (IsDiscovered) return;

            Children.Clear();

            var folderObject = _connection.GetTestLabFolder(Id);

            List folders = folderObject.NewList();
            foreach (dynamic folder in folders)
            {
                if (folder == null) continue;

                var newFolder = new LabFolder(_connection, folder.ID, folder.Name, this);
                Children.Add(newFolder);
            }

            List testSets = folderObject.TestSetFactory.NewList("");
            foreach (dynamic testSet in testSets)
            {
                var newSet = new LabSet(_connection, testSet.ID, testSet.Name, this);
                Children.Add(newSet);
            }

            IsDiscovered = true;
        }

        public void Collapse()
        {
            IsExpanded = false;
            foreach (var item in Children.Where(item => item.Type == LabItemType.Folder))
            {
                ((LabFolder)item).Collapse();
            }
        }

        public void Expand()
        {
            if (!IsDiscovered) DiscoverChildren();

            IsExpanded = true;
            foreach (var item in Children.Where(item => item.Type == LabItemType.Folder))
            {
                ((LabFolder)item).Expand();
            }
        }

        public override void CreateFolder(string path = "")
        {
            var currentPath = Path.Combine(path, DirectoryName());
            currentPath = FolderCreator.CreateFolder(currentPath, false);

            if (!IsDiscovered) DiscoverChildren();

            foreach (var item in Children)
            {
                item.CreateFolder(currentPath);   
            }
        }
    }
}
