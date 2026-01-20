using System.Windows;
using System.Windows.Media;

namespace SpectrumWaterfallMonitor.Controls.Spectrum.Infrastructure
{
    public class SpectrumGridRenderer
    {
        public void Draw(DrawingContext drawingContext, Pen gridPen, int width, int height)
        {
            for (var index = 1; index < 10; index++)
            {
                var x = index * (width / 10.0);
                drawingContext.DrawLine(gridPen, new Point(x, 0), new Point(x, height));
            }

            for (var index = 1; index < 10; index++)
            {
                var y = index * (height / 10.0);
                drawingContext.DrawLine(gridPen, new Point(0, y), new Point(width, y));
            }
        }
    }
}
