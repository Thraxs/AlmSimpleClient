namespace ALMSimpleClient.OTA
{
    public static class OleException
    {
        private const int VbObjectError = 2147221504;

        public static int GetCode(int exceptionCode)
        {
            return exceptionCode + VbObjectError;
        }
    }
}
