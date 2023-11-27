// @Author: randolf
// @LastEditors: randolf
// @Description:
// @Created: 2023-07-28 19:19
// @Date: 2023-07-28 19:19
// @Modify:


namespace LabDataToolbox.Service;

using LabDataToolbox.Model;
using System.IO;
using System.Text.Json;

public class AppConfigService
{
    public AppConfigService()
    {
        UpdateConfiguration();
    }

    public AppConfig AppConfig { get; set; } = new();

    public static void SaveConfiguration(AppConfig appConfig)
    {
        var jsonString = JsonSerializer.Serialize(appConfig, new JsonSerializerOptions { WriteIndented = true });
        if (!Directory.Exists(AppConfig.ConfigurationFolder))
            Directory.CreateDirectory(AppConfig.ConfigurationFolder);
        File.WriteAllText(AppConfig.ConfigurationFileFullName, jsonString);
    }

    public void UpdateConfiguration()
    {
        LoadConfiguration(AppConfig.ConfigurationFileFullName);
    }

    private void LoadConfiguration(string configPath)
    {
        if (!File.Exists(configPath)) return;
        var configString = File.ReadAllText(configPath);
        AppConfig = JsonSerializer.Deserialize<AppConfig>(configString)!;
    }
}