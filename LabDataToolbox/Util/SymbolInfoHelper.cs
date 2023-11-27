namespace LabDataToolbox.Util;

using LabDataToolbox.Model;
using TwinCAT.Ads.TypeSystem;

public static class SymbolInfoHelper
{
    public static SymbolInfo ParseSymbol(this Symbol symbol)
    {
        return new SymbolInfo
        {
            Path = symbol.InstancePath,
            ByteSize = symbol.ByteSize,
            PlcType = symbol.DataType?.Name,
            NetType = ConvertBitSizeToNetType(symbol.ByteSize)
        };
    }

    public static List<SymbolInfo> ConvertToSymbolInfo(this SymbolInfoTree symbolInfoTree)
    {
        var symbolInfos = new List<SymbolInfo> { symbolInfoTree.SymbolInfo };
        foreach (var subSymbolInfoTree in symbolInfoTree.Children)
            symbolInfos.AddRange(subSymbolInfoTree.ConvertToSymbolInfo());

        return symbolInfos;
    }

    public static Type? ConvertPlcTypeToNetType(this SymbolInfo symbolInfo)
    {
        return ConvertBitSizeToNetType(symbolInfo.ByteSize);
    }

    private static Type? ConvertBitSizeToNetType(int bitSize)
    {
        return bitSize switch
        {
            16 => typeof(short),
            32 => typeof(int),
            64 => typeof(double),
            _ => null
        };
    }
}