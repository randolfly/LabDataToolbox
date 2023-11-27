namespace LabDataToolbox;

public class SymbolInfo
{
    public string Path { get; set; } = string.Empty;
    public string? PlcType { get; set; } = string.Empty;
    public Type? NetType { get; set; } = null;
    public int ByteSize { get; set; } = 0;
}
