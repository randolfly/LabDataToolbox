using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace LabDataToolbox.Util;

public class PeriodicActionTimer
{
    private Timer _timer;
    public int TimeInterval { get; set; } = 5000;
    public Action TimerAction { get; set; }
    public PeriodicActionTimer(Action timerAction) => TimerAction = timerAction;

    public void StartTimer()
    {
        _timer = new Timer(TimeInterval);
        // Hook up the Elapsed event for the timer. 
        _timer.Elapsed += OnTimedEvent;
        _timer.AutoReset = true;
        _timer.Enabled = true;
    }

    public void StopTimer()
    {
        _timer.Stop();
    }

    private void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        // Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}", e.SignalTime);
        TimerAction();
    }


    public void Dispose()
    {
        _timer.Dispose();
    }

}