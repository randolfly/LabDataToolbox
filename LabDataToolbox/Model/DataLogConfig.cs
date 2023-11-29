using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Documents;
using LabDataToolbox.Config;

namespace LabDataToolbox.Model;

public class DataLogConfig
{
    public string DataLogFolderName { get; set; } =
        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

    public string DataLogFileName { get; set; } = "hello";
    public List<string> DataExportTypes { get; set; } = DefaultConfig.DataExportTypes;
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
            var fileName = DataLogFileName + "_" + datetime.ToString("yyyyMMddHHmmss");
            return Path.Combine(DataLogFolderName, fileName);
        }
    }
}