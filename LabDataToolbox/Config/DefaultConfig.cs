namespace LabDataToolbox.Config;

public static class DefaultConfig
{
    public static List<string> DataExportTypes { get; } = new()
    {
        "csv",
        "mat"
    };
}