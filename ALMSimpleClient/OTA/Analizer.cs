using System.Collections.Generic;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace ALMSimpleClient.OTA
{
    public static class Analizer
    {
        public static Dictionary<string, int> SummaryStatus;
        public static Dictionary<string, int> SummaryRunStatus;
        public static Dictionary<string, int> SummaryStepStatus;
        public static int TotalRuns;
        public static int TotalSteps;

        public static int TotalAttachments;
        public static int TotalRunAttachments;

        public static int TotalDefects;
        public static int RunDefects;

        public static async Task PerformAnalysis(AlmConnection connection, CancellationToken token, FilterField filterField,
    string filterValue, bool discoverRuns, bool discoverSteps, bool discoverAttachments, bool discoverDefects)
        {
            await Task.Run(() =>
            {
                SummaryStatus = new Dictionary<string, int>();
                SummaryRunStatus = new Dictionary<string, int>();
                SummaryStepStatus = new Dictionary<string, int>();
                TotalRuns = 0;
                TotalSteps = 0;

                TotalAttachments = 0;
                TotalRunAttachments = 0;

                TotalDefects = 0;
                RunDefects = 0;

                var factory = connection.GetTestSetFactory();
                var filter = factory.Filter;
                filter.Filter[filterField.Code] = filterValue;
                var sets = filter.NewList();

                foreach (var set in sets)
                {
                    token.ThrowIfCancellationRequested();

                    var setName = set.Name;
                    var setTests = set.TSTestFactory.NewList("");

                    foreach (var test in setTests)
                    {
                        token.ThrowIfCancellationRequested();

                        var newTest = new LabTestInstance(connection, setName, test.Status)
                        {
                            HasAttachments = test.HasAttachment,
                            HasLinkage = test.HasLinkage,
                            Id = test.TestConfiguration.ID,
                            TestId = test.TestConfiguration.TestId,
                            Name = test.Name,
                            Type = test.Type
                        };

                        if (discoverAttachments)
                            TotalAttachments += test.Attachments.NewList("").Count;

                        if (discoverDefects)
                            TotalDefects += test.BugLinkFactory.NewList("").Count;

                        if (discoverRuns)
                        {
                            var testRuns = test.RunFactory.NewList("");

                            foreach (var run in testRuns)
                            {
                                var runStatus = run.Status;
                                int current;
                                if (SummaryRunStatus.TryGetValue(runStatus, out current))
                                    SummaryRunStatus[runStatus] = current + 1;
                                else
                                    SummaryRunStatus[runStatus] = 1;

                                if (run.HasAttachment)
                                    newTest.RunsHaveAttachments = true;

                                if (discoverAttachments)
                                    TotalRunAttachments += run.Attachments.NewList("").Count;

                                if (run.HasLinkage)
                                    newTest.RunHaveLinkage = true;

                                if (discoverDefects)
                                    RunDefects += run.BugLinkFactory.NewList("").Count;

                                if (discoverSteps)
                                {
                                    var runSteps = run.StepFactory.NewList("");

                                    foreach (var step in runSteps)
                                    {
                                        var stepStatus = step.Status;
                                        int curr;
                                        if (SummaryStepStatus.TryGetValue(stepStatus, out curr))
                                            SummaryStepStatus[stepStatus] = curr + 1;
                                        else
                                            SummaryStepStatus[stepStatus] = 1;
                                    }

                                    TotalSteps += runSteps.Count;
                                }
                            }

                            newTest.Runs = testRuns.Count;
                        }


                        int value;
                        if (SummaryStatus.TryGetValue(newTest.Status, out value))
                            SummaryStatus[newTest.Status] = value + 1;
                        else
                            SummaryStatus[newTest.Status] = 1;

                        TotalRuns += newTest.Runs;

                        MainWindow.AddAnalysisTest(newTest);
                    }
                }
            }, token);
        }
    }
}
