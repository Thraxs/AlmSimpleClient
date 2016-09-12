namespace ALMSimpleClient.OTA
{
    public class TestRun
    {
        //Boolean properties
        public bool HasAttachments { get; set; }
        public bool HasLinkage { get; set; }

        public TestRun(dynamic tsRunObject, string parent, bool discoverSteps)
        {
            HasAttachments = tsRunObject.HasAttachment;
            HasLinkage = tsRunObject.HasLinkage;

            if (discoverSteps)
            {
                
            }
        }

        public void DiscoverSteps()
        {
            //TODO
        }
    }
}
