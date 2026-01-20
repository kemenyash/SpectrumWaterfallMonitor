using SpectrumWaterfallMonitor.Core.Models;
using SpectrumWaterfallMonitor.Core.Records;
using SpectrumWaterfallMonitor.Core.Rendering;

namespace SpectrumWaterfallMonitor.Controls.Waterfall.Infrastructure
{
    public static class GradientLookupFactory
    {
        public static GradientLookup CreateDefault(int tableSize)
        {
            var blue = new RgbColor(0x00, 0x00, 0xFF);
            var cyan = new RgbColor(0x00, 0xFF, 0xFF);
            var green = new RgbColor(0x00, 0xFF, 0x00);
            var yellow = new RgbColor(0xFF, 0xFF, 0x00);
            var red = new RgbColor(0xFF, 0x00, 0x00);

            var gradient = new ColorGradient(
                new[]
                {
                    new GradientStopModel(0.00, blue),
                    new GradientStopModel(0.25, cyan),
                    new GradientStopModel(0.50, green),
                    new GradientStopModel(0.75, yellow),
                    new GradientStopModel(1.00, red)
                });

            return new GradientLookup(gradient, tableSize);
        }
    }
}
