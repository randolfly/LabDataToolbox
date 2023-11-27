namespace LabDataToolbox.Model;

public class SymbolInfoTree
{
    public SymbolInfo SymbolInfo { get; set; }
    public List<SymbolInfoTree> Children { get; set; }
}