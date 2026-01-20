using System.Windows;
using System.Windows.Media;

namespace SpectrumWaterfallMonitor.Controls.Spectrum.Infrastructure
{
    public class SpectrumGeometryBuilder
    {
        public StreamGeometry? Build(
            double[] values,
            int width,
            int height,
            double minimum,
            double maximum,
            SpectrumSampler sampler,
            SpectrumSmoother smoother)
        {
            if (width <= 0 || height <= 0)
            {
                return null;
            }

            if (values.Length == 0)
            {
                return null;
            }

            var range = maximum - minimum;
            if (range <= 0)
            {
                return null;
            }

            var geometry = new StreamGeometry();

            using (var context = geometry.Open())
            {
                for (var x = 0; x < width; x++)
                {
                    var maxInRange = sampler.SampleMax(values, x, width, minimum);
                    var smoothed = smoother.Apply(x, maxInRange);

                    var normalized = (smoothed - minimum) / range;

                    if (normalized < 0)
                    {
                        normalized = 0;
                    }

                    if (normalized > 1)
                    {
                        normalized = 1;
                    }

                    var y = (1.0 - normalized) * (height - 1);

                    if (x == 0)
                    {
                        context.BeginFigure(new Point(0, y), false, false);
                    }
                    else
                    {
                        context.LineTo(new Point(x, y), true, false);
                    }
                }
            }

            geometry.Freeze();
            return geometry;
        }
    }
}
