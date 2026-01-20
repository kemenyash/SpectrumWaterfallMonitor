using SpectrumWaterfallMonitor.Core.Records;
using System.Collections.Generic;

namespace SpectrumWaterfallMonitor.Core.Models
{
    public class ColorGradient
    {
        public IReadOnlyList<GradientStopModel> Stops { get; }
        public ColorGradient(IReadOnlyList<GradientStopModel> stops)
        {
            Stops = stops;
        }
    }
}
