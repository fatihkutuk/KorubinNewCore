using KorubinNewCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KorubinNewCore.Managers
{
    public class TimerManager
    {
        private List<ExtendedTimer> _timers;

        public bool IsTimersStopped { get; private set; }

        public TimerManager()
        {
            _timers = new List<ExtendedTimer>();
        }

        public void AddTimer(ExtendedTimer timer)
        {
            if (!IsTimerExist(timer.Name))
            {
                if (timer.Interval != 0d)
                {
                    _timers.Add(timer);
                }
            }
            else
            {
                var ex = new Exception("Timer is already exist: Name:" + timer.Name)
                {
                    Source = "TimerManager",
                };
                ex.Data.Add("TimerName", timer.Name);
                throw ex;
            }
        }

        public void AddTimer(ICollection<ExtendedTimer> timers)
        {
            foreach (var item in timers)
            {
                AddTimer(item);
            }
        }

        public void StartTimer(string timerName)
        {
            if (IsTimersStopped.Equals(false))
            {
                _timers.Find(t => t.Name == timerName).Start();
            }
        }

        public void StartAll()
        {
            try
            {
                _timers.ForEach(a => a.Start());
                IsTimersStopped = false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void StopTimer(string timerName)
        {
            if (IsTimersStopped.Equals(false))
            {
                _timers.Find(t => t.Name == timerName).Stop();
            }
        }

        public void StopAll()
        {
            try
            {
                _timers.ForEach(a => a.Enabled = false);
                IsTimersStopped = true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool IsTimerExist(string timerName)
        {
            return _timers.Any(a => a.Name == timerName);
        }
    }
}
