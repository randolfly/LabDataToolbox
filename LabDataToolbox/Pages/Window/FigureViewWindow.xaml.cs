// @Author: randolf
// @LastEditors: randolf
// @Description:
// @Created: 2023-07-28 19:34
// @Date: 2023-07-28 19:34
// @Modify:

namespace LabDataToolbox.Pages.Window;

using ScottPlot.Plottable;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Threading;

/// <summary>
///     Interaction logic for FigureViewWindow.xaml
/// </summary>
public partial class FigureViewWindow : System.Windows.Window
{
    private const int Maxlength = 500_0000;
    // private const int Maxlength = 500;

    public static double LogPeriod = 5;
    private readonly DispatcherTimer _renderTimer;

    private int _nextDataIndex;
    private readonly double[] LogData = new double[Maxlength];

    public FigureViewWindow()
    {
        InitializeComponent();
        DataContext = this;

        MaxRenderLengthTextBox.Text = (MaxRenderLength * LogPeriod).ToString(CultureInfo.InvariantCulture);
        SignalPlot = FigureView.Plot.AddSignal(LogData);

        FigureView.Plot.XAxis.TickLabelFormat(CustomTickFormatter);
        Crosshair = FigureView.Plot.AddCrosshair(0, 0);
        FigureView.MouseMove += OnMouseMove;
        FigureView.MouseEnter += OnMouseEnter;
        FigureView.MouseLeave += OnMouseLeave;
        FigureView.Refresh();

        // create a timer to update the GUI
        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(20)
        };
        _renderTimer.Tick += UpdateFigure!;
        _renderTimer.Start();
    }


    private int MaxRenderLength { get; set; } = 2_000;

    private Crosshair Crosshair { get; }
    private SignalPlot SignalPlot { get; set; }

    private void UpdateFigure(object sender, EventArgs e)
    {
        FigureView.Plot.AxisAuto();
        FigureView.Refresh();
    }

    public void ScaleRangeToMax()
    {
        SignalPlot.MaxRenderIndex = _nextDataIndex - 1;
        SignalPlot.MinRenderIndex = 0;
        FigureView.Plot.AxisAuto();
        FigureView.Refresh();
        _renderTimer.Stop();
    }

    // create a custom formatter as a static class
    private static string CustomTickFormatter(double position)
    {
        return $"{position * LogPeriod}";
    }

    public void UpdateData(double data)
    {
        if (_nextDataIndex >= Maxlength)
        {
            // throw new OverflowException("data array isn't long enough to accomodate new data");
            // clear all data and begin new figure

            _nextDataIndex = 0;
        }

        LogData[_nextDataIndex] = data;
        SignalPlot.MaxRenderIndex = _nextDataIndex;
        if (_nextDataIndex > MaxRenderLength)
            SignalPlot.MinRenderIndex = _nextDataIndex - MaxRenderLength;
        else
            SignalPlot.MinRenderIndex = 0;

        _nextDataIndex += 1;

        Debug.WriteLine($"nextDataIndex: {_nextDataIndex}");
    }


    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        // var pixelX = (int)e.MouseDevice.GetPosition(FigureView).X;
        // var pixelY = (int)e.MouseDevice.GetPosition(FigureView).Y;

        var (coordinateX, coordinateY) = FigureView.GetMouseCoordinates();

        Crosshair.X = coordinateX;
        Crosshair.Y = coordinateY;

        FigureView.Refresh();
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        Crosshair.IsVisible = true;
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        Crosshair.IsVisible = false;
    }

    public void CloseWindow()
    {
        Close();
    }

    private void OnMaxRenderLengthChanged(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            MaxRenderLength = Convert.ToInt32(double.Parse(MaxRenderLengthTextBox.Text) / LogPeriod);
    }
}