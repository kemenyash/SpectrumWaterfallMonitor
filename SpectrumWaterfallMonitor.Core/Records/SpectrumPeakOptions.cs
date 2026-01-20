using SpectrumWaterfallMonitor.Core.Enums;

namespace SpectrumWaterfallMonitor.Core.Records
{
    public record SpectrumPeakOptions(
        double CenterMhz,
        double WidthMhz,
        double PeakPowerDbm,
        SpectrumPeakDriftMode DriftMode = SpectrumPeakDriftMode.None,
        double DriftAmplitudeMhz = 0.0,
        double DriftSpeedHz = 0.0,
        double RandomWalkStdMhzPerFrame = 0.0,
        double PowerFadingStdDbPerFrame = 0.0,
        double WidthFadingStdMhzPerFrame = 0.0,
        double CenterJitterStdMhzPerFrame = 0.0);
}
