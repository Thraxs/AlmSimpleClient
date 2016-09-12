namespace ALMSimpleClient.OTA
{
    public class FilterField
    {
        public string Code { get; set; }
        public string Label { get; set; }

        public FilterField(string code, string label)
        {
            Code = code;
            Label = label;
        }
    }
}
