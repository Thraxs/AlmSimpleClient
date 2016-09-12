using System.Collections.Generic;
using System.IO;
using ALMSimpleClient.IO;

namespace ALMSimpleClient.OTA
{
    public class LabTestInstance
    {
        //Boolean properties
        public bool HasAttachments { get; set; }
        public bool HasLinkage { get; set; }

        //Test identification properties
        public int TestId { get; set; }
        public string TestName { get; set; }

        //Instance identification properties
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Status { get; set; } 
        public LabRunStatus LabRunStatus { get; set; }

        //Relationships
        public string ParentName { get; set; }

        //Test runs
        public List<TestRun> TestRuns { get; set; }
        public bool IsDiscovered { get; set; }
        public int Runs { get; set; }
        public bool RunsHaveAttachments { get; set; }
        public bool RunHaveLinkage { get; set; }

        //Test object properties
        private readonly AlmConnection _connection; //TODO remove if not needed

        public LabTestInstance(AlmConnection almConnection, string parent, string status)
        {
            Status = status;
            ParentName = parent;
            TestRuns = new List<TestRun>();

            _connection = almConnection;

            switch (Status)
            {
                case "Blocked":
                    LabRunStatus = LabRunStatus.Blocked;
                    break;
                case "Failed":
                    LabRunStatus = LabRunStatus.Failed;
                    break;
                case "N/A":
                    LabRunStatus = LabRunStatus.NotAvailable;
                    break;
                case "No Run":
                    LabRunStatus = LabRunStatus.NoRun;
                    break;
                case "Not Completed":
                    LabRunStatus = LabRunStatus.NotCompleted;
                    break;
                case "Passed":
                    LabRunStatus = LabRunStatus.Passed;
                    break;
                default:
                    LabRunStatus = LabRunStatus.Other;
                    break;
            }
        }

        public void CreateFolder(string path = "")
        {
            var name = FolderCreator.UseTestNames ? TestName : Name;
            var currentPath = Path.Combine(path, name);
            FolderCreator.CreateFolder(currentPath, true);
        }
    }
}
