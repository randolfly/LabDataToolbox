using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;

namespace LabDataToolbox;

public class AppConfig
{
    public AdsConfig AdsConfig { get; set; } = new();
    public DataLogConfig DataLogConfig { get; set; } = new();

    public static readonly string ConfigurationFolder;
    public static readonly string ConfigurationFileName;

    static AppConfig()
    {
        ConfigurationFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        ConfigurationFileName = Assembly.GetCallingAssembly().FullName!.Split(',')[0] + ".json";
    }

    [JsonIgnore]
    public static string ConfigurationFileFullName => Path.Combine(ConfigurationFolder, ConfigurationFileName);
}