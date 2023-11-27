using System.IO;
using System.Text.Json.Serialization;

namespace LabDataToolbox.Model;

public class DataLogConfig
{
    public string DataLogFolderName { get; set; } =
        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

    public string DataLogFileName { get; set; } = "hello.csv";

    public List<string> RecordAdsSymbolName { get; set; } = new();
    public List<string> GraphAdsSymbolName { get; set; } = new();

    [JsonIgnore]
    public string TempFileFullName => Path.Combine(DataLogFolderName, DataLogFileName);

    [JsonIgnore]
    public string RecordFileFullName
    {
        get
        {
            var datetime = DateTime.Now;
            var fileName = DataLogFileName + "_" + datetime.ToString("yyyyMMddHHmmss") + ".csv";
            return Path.Combine(DataLogFolderName, fileName);
        }
    }
}