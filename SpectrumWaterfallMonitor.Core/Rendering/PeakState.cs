using SpectrumWaterfallMonitor.Core.Enums;
using SpectrumWaterfallMonitor.Core.Records;
using System;

namespace SpectrumWaterfallMonitor.Core.Rendering
{
    internal class PeakState
    {
        private double phase;

        public SpectrumPeakOptions Options { get; }
        public double BaseCenterMhz { get; }
        public double CurrentCenterMhz { get; private set; }
        public double CurrentPeakPowerDbm { get; private set; }
        public double CurrentWidthMhz { get; private set; }


        public PeakState(SpectrumPeakOptions options, Random random)
        {
            Options = options;
            BaseCenterMhz = options.CenterMhz;
            CurrentCenterMhz = options.CenterMhz;
            CurrentPeakPowerDbm = options.PeakPowerDbm;
            CurrentWidthMhz = options.WidthMhz;

            phase = random.NextDouble() * 2.0 * Math.PI;
        }


        public void Update(double dt, double minF, double maxF, Func<double, double, double> nextNormal)
        {
            switch (Options.DriftMode)
            {
                case SpectrumPeakDriftMode.None:
                    CurrentCenterMhz = BaseCenterMhz;
                    break;

                case SpectrumPeakDriftMode.Sine:
                    {
                        var amp = Options.DriftAmplitudeMhz;
                        var hz = Options.DriftSpeedHz;
                        if (amp <= 0 || hz <= 0)
                        {
                            CurrentCenterMhz = BaseCenterMhz;
                            break;
                        }

                        phase += 2.0 * Math.PI * hz * dt;
                        if (phase > 2.0 * Math.PI)
                        {
                            phase -= 2.0 * Math.PI;
                        }

                        CurrentCenterMhz = BaseCenterMhz + (amp * Math.Sin(phase));
                        CurrentCenterMhz = ClampToRange(CurrentCenterMhz, minF, maxF, CurrentWidthMhz);
                        break;
                    }

                case SpectrumPeakDriftMode.RandomWalk:
                    {
                        var std = Options.RandomWalkStdMhzPerFrame;
                        if (std <= 0)
                        {
                            CurrentCenterMhz = BaseCenterMhz;
                            break;
                        }

                        CurrentCenterMhz = CurrentCenterMhz + nextNormal(0, std);
                        CurrentCenterMhz = ClampToRange(CurrentCenterMhz, minF, maxF, CurrentWidthMhz);
                        break;
                    }
            }

            if (Options.PowerFadingStdDbPerFrame > 0)
            {
                CurrentPeakPowerDbm = CurrentPeakPowerDbm + nextNormal(0, Options.PowerFadingStdDbPerFrame);
            }
            else
            {
                CurrentPeakPowerDbm = Options.PeakPowerDbm;
            }

            if (Options.WidthFadingStdMhzPerFrame > 0)
            {
                CurrentWidthMhz = CurrentWidthMhz + nextNormal(0, Options.WidthFadingStdMhzPerFrame);
            }
            else
            {
                CurrentWidthMhz = Options.WidthMhz;
            }

            CurrentWidthMhz = ClampWidth(CurrentWidthMhz, Options.WidthMhz);

            if (Options.CenterJitterStdMhzPerFrame > 0)
            {
                CurrentCenterMhz = CurrentCenterMhz + nextNormal(0, Options.CenterJitterStdMhzPerFrame);
                CurrentCenterMhz = ClampToRange(CurrentCenterMhz, minF, maxF, CurrentWidthMhz);
            }
        }

        private static double ClampWidth(double width, double baseWidth)
        {
            var min = Math.Max(0.01, baseWidth * 0.4);
            var max = baseWidth * 2.5;
            if (width < min) return min;
            if (width > max) return max;
            return width;
        }

        private static double ClampToRange(double center, double minF, double maxF, double width)
        {
            var margin = Math.Max(0.01, width * 2.0);
            var min = minF + margin;
            var max = maxF - margin;
            if (center < min) return min;
            if (center > max) return max;
            return center;
        }
    }
}
