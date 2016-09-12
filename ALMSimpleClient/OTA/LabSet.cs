using ALMSimpleClient.IO;
using System.Collections.ObjectModel;
using System.IO;
using TDAPIOLELib;

namespace ALMSimpleClient.OTA
{
    public class LabSet : LabItem
    {
        public ObservableCollection<LabTestInstance> Tests;
        public bool IsDiscovered { get; set; }

        private readonly AlmConnection _connection;

        public LabSet(AlmConnection almConnection, int id, string name, LabFolder parent)
        {
            Type = LabItemType.TestSet;
            Id = id;
            Name = name;
            Tests = new ObservableCollection<LabTestInstance>();

            Parent = parent;
            _connection = almConnection;
        }

        public void DiscoverTests()
        {
            if (IsDiscovered) return;

            Tests.Clear();

            var setObject = _connection.GetTestLabSet(Id);

            TSTestFactory factory = setObject.TSTestFactory;

            var tests = factory.NewList("");
            foreach (dynamic test in tests)
            {
                var newTest = new LabTestInstance(_connection, Name, test.Status)
                {
                    HasAttachments = test.HasAttachment,
                    HasLinkage = test.HasLinkage,
                    TestId = test.TestConfiguration.TestId,
                    TestName = test.TestConfiguration.Name,
                    Id = test.TestConfiguration.ID,
                    Name = test.Name,
                    Type = test.Type
                };
                Tests.Add(newTest);
            }              

            IsDiscovered = true;
        }

        public override void CreateFolder(string path = "")
        {
            var currentPath = Path.Combine(path, DirectoryName());
            currentPath = FolderCreator.CreateFolder(currentPath, false);

            if (!IsDiscovered) DiscoverTests();

            foreach (var item in Tests)
            {
                item.CreateFolder(currentPath);
            }
        }
    }
}
