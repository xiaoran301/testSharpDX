using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WindowsFormsApp1.Foundation
{
   public class TimeManager
    {
        private Stopwatch _StopWatch;
        private float m_ticksPerMs;
        private long m_LastFrameTime = 0;

        public float FrameTime { get; private set; }
        public float TotalFrameTime { get; private set; }

        public void Init()
        {
            if (!Stopwatch.IsHighResolution)
                Debug.Assert(false);
            if (Stopwatch.Frequency == 0)
                Debug.Assert(false);

            _StopWatch = Stopwatch.StartNew();

            // Find out how many times the frequency counter ticks every millisecond.
            m_ticksPerMs = (float)(Stopwatch.Frequency / 1000.0f);
        }
        public void Update()
        {

            // Query the current time.
            long currentTime = _StopWatch.ElapsedTicks;

            // Calculate the difference in time since the last time we queried for the current time.
            float timeDifference = currentTime - m_LastFrameTime;

            // Calculate the frame time by the time difference over the timer speed resolution.
            FrameTime = timeDifference / m_ticksPerMs;
            TotalFrameTime += FrameTime;

            // Restart the timer.
            m_LastFrameTime = currentTime;

        }
    }
}
