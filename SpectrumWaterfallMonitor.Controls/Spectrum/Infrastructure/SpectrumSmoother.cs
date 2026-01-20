using System;

namespace SpectrumWaterfallMonitor.Controls.Spectrum.Infrastructure
{
    public class SpectrumSmoother
    {
        private readonly double escalation;
        private readonly double release;

        private double[] smoothedByX;

        public SpectrumSmoother(double escalation, double release)
        {
            this.escalation = escalation;
            this.release = release;
            smoothedByX = Array.Empty<double>();
        }

        public void EnsureWidth(int width)
        {
            if (width <= 0)
            {
                return;
            }

            if (smoothedByX.Length != width)
            {
                smoothedByX = new double[width];
            }
        }

        public double Apply(int x, double value)
        {
            var previous = smoothedByX[x];
            var smoothing = value >= previous ? escalation : release;
            var smoothed = previous * smoothing + value * (1.0 - smoothing);

            smoothedByX[x] = smoothed;
            return smoothed;
        }
    }
}
