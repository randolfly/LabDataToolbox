// @Author: randolf
// @LastEditors: randolf
// @Description:
// @Created: 2023-07-28 19:20
// @Date: 2023-07-28 19:20
// @Modify:

namespace LabDataToolbox.Service;

using BlazorComponent;
using LabDataToolbox.Model;
using LabDataToolbox.Pages.Window;
using LabDataToolbox.Util;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.ValueAccess;

public class AdsDataLogService : IDisposable
{
    private readonly object _recordDataLock = new();

    public AdsDataLogService(AppConfigService appConfigService)
    {
        AppConfigService = appConfigService;
        AdsConfig = appConfigService.AppConfig.AdsConfig;
        PeriodicActionTimer = new PeriodicActionTimer(BackupCsvLog);
    }

    private AppConfigService AppConfigService { get; set; }
    private bool IsAdsConnected => AdsClient.IsConnected;
    
    public bool IsAdsPortRun
    {
        get
        {
            var state = false;
            try
            {
                state = AdsClient.ReadState().AdsState == AdsState.Run;
            }
            catch (Exception)
            {
                return false;
            }

            return state;
        }
    }

    private AdsClient AdsClient { get; } = new();
    private AdsConfig AdsConfig { get; set; } = new();
    private List<List<double>> RecordData { get; } = new();
    public List<SymbolInfo> RecordSymbolInfos { get; set; } = new();
    public List<SymbolInfo> GraphSymbolInfos { get; set; } = new();
    private Dictionary<uint, SymbolInfo> RecordSymbolInfoDictionary { get; } = new();
    public Dictionary<SymbolInfo, FigureViewWindow> FigureViewWindowDictionary { get; set; } = new();

    // Add recurrent save file action
    public PeriodicActionTimer PeriodicActionTimer { get; set; }
    public bool ExportCsvTempWithHeader { get; set; } = true;
    public int StartId { get; set; } = 0;
    public int EndId { get; set; }

    public void Dispose()
    {
        AdsClient.Dispose();
        PeriodicActionTimer.Dispose();
    }

    public void ConnectAdsServer()
    {
        AdsClient.Connect(AdsConfig.NetId, AdsConfig.PortId);
    }

    public void DisconnectAdsServer()
    {
        if (IsAdsConnected) AdsClient.Disconnect();
    }

    public void RegisterNotification()
    {
        if (RecordSymbolInfos.Count <= 0) return;
        // update ads config
        AdsConfig = AppConfigService.AppConfig.AdsConfig;
        RecordData.Clear();
        RecordSymbolInfoDictionary.Clear();
        FigureViewWindowDictionary.Values.ForEach(w => w.Close());
        FigureViewWindowDictionary.Clear();
        AdsClient.AdsNotification += AdsNotificationHandler;
        try
        {
            lock (_recordDataLock)
            {
                foreach (var symbol in RecordSymbolInfos)
                {
                    var notificationHandle = AdsClient.AddDeviceNotification(symbol.Path, symbol.ByteSize,
                        new NotificationSettings(AdsTransMode.Cyclic, AdsConfig.Period, 0), null);
                    RecordSymbolInfoDictionary.Add(notificationHandle, symbol);
                    RecordData.Add(new List<double>());
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

        ShowGraphView(GraphSymbolInfos);
        ExportCsvTempWithHeader = true;
        StartId = 0;
        PeriodicActionTimer.StartTimer();
    }

    public async Task UnregisterNotification(string logDataFullName)
    {
        RecordSymbolInfoDictionary.Keys.ToList().ForEach(handle => AdsClient.DeleteDeviceNotification(handle));
        RecordSymbolInfoDictionary.Clear();
        foreach (var figureViewWindow in FigureViewWindowDictionary.Values)
        {
            figureViewWindow.ScaleRangeToMax();
        }

        AdsClient.AdsNotification -= AdsNotificationHandler;
        await ExportCsvLog(logDataFullName);
        PeriodicActionTimer.StopTimer();
    }

    public List<SymbolInfoTree> GetSymbolInfoTree()
    {
        var settings = new SymbolLoaderSettings(SymbolsLoadMode.VirtualTree, ValueAccessMode.InstancePath);
        var symbolLoader = SymbolLoaderFactory.Create(AdsClient, settings);
        var symbols = symbolLoader.Symbols;

        // var symbolInfoTreeList = new List<SymbolInfoTree>(symbols.Count);
        var symbolInfoTreeList = new List<SymbolInfoTree>(2);
        var symbolIndex = 0;
        for (var i = 0; i < symbols.Count; i++)
        {
            var symbol = (Symbol)symbols[i];
            // if (symbol.InstanceName is not ("MAIN" or "GVL")) continue;
            symbolInfoTreeList.Add(new SymbolInfoTree());
            var symbolInfoTree = symbolInfoTreeList[symbolIndex];
            symbolIndex++;
            LoadSymbolInfoTree(symbol, ref symbolInfoTree);
        }

        return symbolInfoTreeList;

        static void LoadSymbolInfoTree(Symbol symbol, ref SymbolInfoTree symbolInfoTree)
        {
            symbolInfoTree.SymbolInfo = symbol.ParseSymbol();
            symbolInfoTree.Children = new List<SymbolInfoTree>(symbol.SubSymbolCount);
            for (var i = 0; i < symbol.SubSymbolCount; i++)
            {
                symbolInfoTree.Children.Add(new SymbolInfoTree());
                var infoTree = symbolInfoTree.Children[i];
                LoadSymbolInfoTree((Symbol)symbol.SubSymbols[i], ref infoTree);
            }
        }
    }

    private void ShowGraphView(List<SymbolInfo> graphSymbolInfos)
    {
        foreach (var figureViewWindow in FigureViewWindowDictionary.Values)
        {
            figureViewWindow.CloseWindow();
        }

        FigureViewWindowDictionary.Clear();
        for (var i = 0; i < graphSymbolInfos.Count; i++)
        {
            var symbolInfo = graphSymbolInfos[i];
            FigureViewWindow.LogPeriod = AdsConfig.Period;
            var (left, top) = GetGraphViewWindowPos(i, graphSymbolInfos.Count);
            var figureViewWindow = new FigureViewWindow { Title = symbolInfo.Path, Left = left, Top = top };
            FigureViewWindowDictionary.Add(symbolInfo, figureViewWindow);
            figureViewWindow.Show();
        }
    }

    private static (double x, double y) GetGraphViewWindowPos(int id, int windowNum, int windowWidth = 600,
        int windowHeight = 300)
    {
        const int windowRowSize = 3;
        var desktopWorkingArea = SystemParameters.WorkArea;
        var left = desktopWorkingArea.Right - ((int)(id / windowRowSize) + 1) * windowWidth;
        var top = desktopWorkingArea.Bottom - (id % windowRowSize + 1) * windowHeight;
        return (left, top);
    }

    private void AdsNotificationHandler(object? sender, AdsNotificationEventArgs e)
    {
        double data = 0;
        data = e.Data.Length switch
        {
            1 => BitConverter.ToBoolean(e.Data.Span) ? 1 : 0,
            2 => BinaryPrimitives.ReadInt16LittleEndian(e.Data.Span),
            4 => BinaryPrimitives.ReadInt32LittleEndian(e.Data.Span),
            8 => BinaryPrimitives.ReadDoubleLittleEndian(e.Data.Span),
            _ => 0
        };
        var symbol = RecordSymbolInfoDictionary[e.Handle];
        lock (_recordDataLock)
        {
            RecordData[RecordSymbolInfoDictionary.Keys.ToList().IndexOf(e.Handle)].Add(data);
        }

        if (FigureViewWindowDictionary.Keys.Contains(symbol))
        {
            var index = FigureViewWindowDictionary.Keys.ToList().IndexOf(symbol);
            FigureViewWindowDictionary.Values.ToList()[index].UpdateData(data);
        }
    }

    private async void BackupCsvLog()
    {
        var backupFileName = AppConfigService.AppConfig.DataLogConfig.TempFileFullName + ".tmp";
        EndId = StartId + (int)((double)(PeriodicActionTimer.TimeInterval) / (double)(AdsConfig.Period));
        await ExportCsvLog(backupFileName, StartId, EndId, ExportCsvTempWithHeader);
        StartId = EndId;
        ExportCsvTempWithHeader = false;
    }

    private async Task ExportCsvLog(string logDataFullName)
    {
        var recordString = GetRecordString();
        await using var outputFile = new StreamWriter(logDataFullName);
        await outputFile.WriteAsync(recordString);
    }

    private async Task ExportCsvLog(string logDataFullName, int startId, int endId, bool withHeader = false)
    {
        var recordString = GetRecordString(startId, endId, withHeader);
        await using var outputFile = new StreamWriter(logDataFullName, append: true);
        await outputFile.WriteAsync(recordString);
    }

    private string GetRecordString(int startId, int endId, bool withHeader = false)
    {
        var sb = new StringBuilder();
        if (withHeader)
        {
            sb.AppendLine(string.Join(',', RecordSymbolInfos.Select(s => s.Path)));
        }

        var maxLength = RecordData.Where(d => d.Count > 0).Select(d => d.Count).Min();
        endId = endId < maxLength ? endId : maxLength;
        for (var i = startId; i < endId; i++)
            sb.AppendLine($"{string.Join(',', RecordData.Select(d => d[i].ToString(CultureInfo.InvariantCulture)))}");
        return sb.ToString();
    }

    private string GetRecordString()
    {
        var sb = new StringBuilder();
        var maxLength = RecordData.Where(d => d.Count > 0).Select(d => d.Count).Min();
        sb.AppendLine(string.Join(',', RecordSymbolInfos.Select(s => s.Path)));
        for (var i = 0; i < maxLength; i++)
            sb.AppendLine($"{string.Join(',', RecordData.Select(d => d[i].ToString(CultureInfo.InvariantCulture)))}");
        return sb.ToString();
    }
}