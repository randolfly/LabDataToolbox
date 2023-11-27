using TwinCAT.Ads;

namespace LabDataToolbox.Model;

public class AdsConfig
{
    public string NetId { get; set; } = AmsNetId.Local.ToString();
    public int PortId { get; set; } = 851;
    public int Period { get; set; } = 5;
}