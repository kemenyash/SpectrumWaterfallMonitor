using System;

namespace SpectrumWaterfallMonitor.Controls.Waterfall.Infrastructure
{
    public class SpectrumRowAccumulator
    {
        private double[] maximumRow;
        private int accumulationCount;
        private int accumulationTarget;

        public SpectrumRowAccumulator()
        {
            maximumRow = Array.Empty<double>();
            accumulationCount = 0;
            accumulationTarget = 1;
        }

        public void SetTarget(int outputHeight, int historyLineCount)
        {
            if (outputHeight <= 0)
            {
                accumulationTarget = 1;
                return;
            }

            var target = (int)Math.Ceiling((double)historyLineCount / outputHeight);
            accumulationTarget = Math.Max(1, target);
        }

        public bool TryAccumulateMaxRow(double[] values, int outputWidth, double minimum, double maximum, out double[] row)
        {
            row = maximumRow;

            if (values.Length == 0)
            {
                return false;
            }

            if (outputWidth <= 0)
            {
                return false;
            }

            if (maximumRow.Length != outputWidth)
            {
                maximumRow = new double[outputWidth];
                row = maximumRow;
                accumulationCount = 0;
            }

            for (var screenX = 0; screenX < outputWidth; screenX++)
            {
                var fromIndex = (int)((long)screenX * values.Length / outputWidth);
                var toIndex = (int)((long)(screenX + 1) * values.Length / outputWidth);

                if (fromIndex < 0)
                {
                    fromIndex = 0;
                }

                if (fromIndex >= values.Length)
                {
                    fromIndex = values.Length - 1;
                }

                if (toIndex <= fromIndex)
                {
                    toIndex = fromIndex + 1;
                }

                if (toIndex > values.Length)
                {
                    toIndex = values.Length;
                }

                var best = minimum;

                for (var index = fromIndex; index < toIndex; index++)
                {
                    var candidate = values[index];
                    if (candidate > best)
                    {
                        best = candidate;
                    }
                }

                if (best < minimum)
                {
                    best = minimum;
                }

                if (best > maximum)
                {
                    best = maximum;
                }

                if (accumulationCount == 0)
                {
                    maximumRow[screenX] = best;
                }
                else
                {
                    if (best > maximumRow[screenX])
                    {
                        maximumRow[screenX] = best;
                    }
                }
            }

            accumulationCount++;

            if (accumulationCount < accumulationTarget)
            {
                return false;
            }

            accumulationCount = 0;
            return true;
        }

        public void Reset()
        {
            maximumRow = Array.Empty<double>();
            accumulationCount = 0;
            accumulationTarget = 1;
        }
    }
}
