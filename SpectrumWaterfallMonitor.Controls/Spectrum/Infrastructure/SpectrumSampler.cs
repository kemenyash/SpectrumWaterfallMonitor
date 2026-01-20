namespace SpectrumWaterfallMonitor.Controls.Spectrum.Infrastructure
{
    public class SpectrumSampler
    {
        public double SampleMax(double[] values, int x, int width, double minimum)
        {
            if (values.Length == 0)
            {
                return minimum;
            }

            if (width <= 0)
            {
                return minimum;
            }

            var fromIndex = (int)((long)x * values.Length / width);
            var toIndex = (int)((long)(x + 1) * values.Length / width);

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

            var maximumInRange = minimum;

            for (var index = fromIndex; index < toIndex; index++)
            {
                var candidate = values[index];
                if (candidate > maximumInRange)
                {
                    maximumInRange = candidate;
                }
            }

            return maximumInRange;
        }
    }
}
