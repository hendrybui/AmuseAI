using OnnxStack.StableDiffusion.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace Amuse.UI.Models
{
    public record StatisticsModel : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _updateTimer;
        private readonly long _totalElapsedTimestamp;
        private readonly List<double> _iterationTimes;

        private TimeSpan _elapsed;
        private TimeSpan _totalElapsed;
        private double _iterationsSec;
        private long _elapsedTimestamp;
        private double _elapsedTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsModel"/> class.
        /// </summary>
        public StatisticsModel()
        {
            _updateTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Background, UpdateElapsed, App.Current.Dispatcher);
            _iterationTimes = new List<double>();
            _totalElapsedTimestamp = Stopwatch.GetTimestamp();
            _updateTimer.Start();
        }


        /// <summary>
        /// Gets or sets the elapsed time.
        /// </summary>
        public TimeSpan Elapsed
        {
            get { return _elapsed; }
            set { _elapsed = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the total elapsed.
        /// </summary>
        public TimeSpan TotalElapsed
        {
            get { return _totalElapsed; }
            set { _totalElapsed = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the iterations sec.
        /// </summary>
        public double IterationsSec
        {
            get { return _iterationsSec; }
            set { _iterationsSec = value; NotifyPropertyChanged(); }
        }


        /// <summary>
        /// Updates the statistics.
        /// </summary>
        public void Update(DiffusionProgress progress)
        {
            if (progress.StepValue == 0 && progress.StepMax > 0)
            {
                _elapsedTime = 0;
                _iterationTimes.Clear();
                _elapsedTimestamp = Stopwatch.GetTimestamp();
            }
            else
            {
                if (progress.Elapsed > 0 && progress.StepMax > 0)
                {
                    _iterationTimes.Add(progress.Elapsed);
                }
                else
                {
                    _elapsedTime = progress.Elapsed;
                }
            }

            CalculateIterations();
        }


        public void Reset()
        {
            _elapsedTime = 0;
            _elapsedTimestamp = 0;
            _iterationTimes.Clear();
        }


        /// <summary>
        /// Completes this instance.
        /// </summary>
        public void Complete()
        {
            _updateTimer.Stop();
            CalculateElapsed();
            CalculateIterations();
        }


        /// <summary>
        /// Calculates the elapsed time.
        /// </summary>
        /// <param name="baseTimes">The base times.</param>
        private void CalculateElapsed()
        {
            Elapsed = _elapsedTime > 0 || _elapsedTimestamp == 0
                ? TimeSpan.FromMilliseconds(_elapsedTime)
                : Stopwatch.GetElapsedTime(_elapsedTimestamp);
            TotalElapsed = Stopwatch.GetElapsedTime(_totalElapsedTimestamp);
        }

        /// <summary>
        /// Calculates the iterations.
        /// </summary>
        /// <returns></returns>
        private void CalculateIterations()
        {
            if (_iterationTimes.Count == 0)
                return;

            IterationsSec = 1000d / _iterationTimes.Average();
        }


        private void UpdateElapsed(object sender, EventArgs e)
        {
            CalculateElapsed();
        }


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion
    }
}