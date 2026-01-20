using System.Windows.Media;

namespace SpectrumWaterfallMonitor.Controls.Spectrum.Infrastructure
{
    public class SpectrumStyle
    {
        public Pen GridPen { get; }
        public Pen LinePen { get; }

        private SpectrumStyle(Pen gridPen, Pen linePen)
        {
            GridPen = gridPen;
            LinePen = linePen;
        }

        public static SpectrumStyle CreateDefault()
        {
            var gridBrush = new SolidColorBrush(Color.FromRgb(40, 40, 40));
            gridBrush.Freeze();

            var lineBrush = new SolidColorBrush(Color.FromRgb(220, 220, 80));
            lineBrush.Freeze();

            var gridPen = new Pen(gridBrush, 1);
            gridPen.Freeze();

            var linePen = new Pen(lineBrush, 1);
            linePen.Freeze();

            return new SpectrumStyle(gridPen, linePen);
        }
    }
}
