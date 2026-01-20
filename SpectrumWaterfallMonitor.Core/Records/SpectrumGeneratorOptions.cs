using System;
using System.Collections.Generic;

namespace SpectrumWaterfallMonitor.Core.Records
{
    public record SpectrumGeneratorOptions(
        int BinCount,
        double FrequencyStartMhz,
        double FrequencyEndMhz,
        double MinimumPowerDbm,
        double MaximumPowerDbm,
        double BasePowerDbm,
        double NoiseStdDb,
        double DriftStdDbPerFrame,
        int UpdateRateHz)
    {
        public IReadOnlyList<SpectrumPeakOptions> Peaks { get; set; } = Array.Empty<SpectrumPeakOptions>();
    }

}
