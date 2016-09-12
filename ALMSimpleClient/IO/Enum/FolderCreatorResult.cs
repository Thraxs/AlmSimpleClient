using System.ComponentModel;

namespace ALMSimpleClient.IO
{
    public enum FolderCreatorResult
    {
        [Description("Created normally")]
        Normal,
        [Description("Skipped (long path)")]
        LongPathSkipped,
        [Description("Created with long path")]
        LongPathCreated,
        [Description("Skipped (duplicate)")]
        DuplicatedSkipped,
        [Description("Created (renamed duplicate)")]
        DuplicatedRenamed,
        [Description("Created (renamed with long path)")]
        LongPathAndRenamed
    }
}
