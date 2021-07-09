using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace KorubinNewCore.Models
{
    public class ExtendedTimer : Timer
    {
        public string Name { get; internal set; }
        public ExtendedTimer(string name, double interval)
        {
            AutoReset = true;
            Enabled = false;
            Name = name;
            Interval = interval;
        }
    }
}
