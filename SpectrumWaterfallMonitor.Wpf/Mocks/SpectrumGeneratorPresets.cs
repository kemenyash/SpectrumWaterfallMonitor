using SpectrumWaterfallMonitor.Core.Enums;
using SpectrumWaterfallMonitor.Core.Records;

namespace SpectrumWaterfallMonitor.Wpf.Mocks
{
    public static class SpectrumGeneratorPresets
    {
        public static SpectrumGeneratorOptions RadioEther_90To110MHz_20Hz()
        {
            return new SpectrumGeneratorOptions(
                BinCount: 1024,
                FrequencyStartMhz: 90.0,
                FrequencyEndMhz: 110.0,
                MinimumPowerDbm: -120.0,
                MaximumPowerDbm: -20.0,
                BasePowerDbm: -108.0,
                NoiseStdDb: 4.0,
                DriftStdDbPerFrame: 0.08,
                UpdateRateHz: 20)
            {
                Peaks = new[]
                {
                    new SpectrumPeakOptions(CenterMhz: 91.1, WidthMhz: 0.15, PeakPowerDbm: -55,
                        PowerFadingStdDbPerFrame: 0.06,
                        CenterJitterStdMhzPerFrame: 0.003),
                    new SpectrumPeakOptions(CenterMhz: 92.4, WidthMhz: 0.16, PeakPowerDbm: -58,
                        PowerFadingStdDbPerFrame: 0.05,
                        CenterJitterStdMhzPerFrame: 0.002),
                    new SpectrumPeakOptions(CenterMhz: 94.8, WidthMhz: 0.27, PeakPowerDbm: -54,
                        PowerFadingStdDbPerFrame: 0.07,
                        CenterJitterStdMhzPerFrame: 0.003),
                    new SpectrumPeakOptions(CenterMhz: 96.3, WidthMhz: 0.06, PeakPowerDbm: -56,
                        PowerFadingStdDbPerFrame: 0.06,
                        CenterJitterStdMhzPerFrame: 0.002),
                    new SpectrumPeakOptions(CenterMhz: 98.6, WidthMhz: 0.06, PeakPowerDbm: -55,
                        PowerFadingStdDbPerFrame: 0.06,
                        CenterJitterStdMhzPerFrame: 0.002),
                    new SpectrumPeakOptions(CenterMhz: 101.7, WidthMhz: 0.06, PeakPowerDbm: -57,
                        PowerFadingStdDbPerFrame: 0.05,
                        CenterJitterStdMhzPerFrame: 0.002),
                    new SpectrumPeakOptions(CenterMhz: 103.6, WidthMhz: 0.06, PeakPowerDbm: -56,
                        PowerFadingStdDbPerFrame: 0.06,
                        CenterJitterStdMhzPerFrame: 0.002),
                    new SpectrumPeakOptions(CenterMhz: 107.2, WidthMhz: 0.06, PeakPowerDbm: -57,
                        PowerFadingStdDbPerFrame: 0.06,
                        CenterJitterStdMhzPerFrame: 0.002),

                    new SpectrumPeakOptions(CenterMhz: 93.2, WidthMhz: 0.08, PeakPowerDbm: -35.0,
                        PowerFadingStdDbPerFrame: 0.12,
                        CenterJitterStdMhzPerFrame: 0.004),

                    new SpectrumPeakOptions(CenterMhz: 99.7, WidthMhz: 0.10, PeakPowerDbm: -28.0,
                        DriftMode: SpectrumPeakDriftMode.Sine,
                        DriftAmplitudeMhz: 0.20,
                        DriftSpeedHz: 0.02,
                        PowerFadingStdDbPerFrame: 0.10,
                        CenterJitterStdMhzPerFrame: 0.003),

                    new SpectrumPeakOptions(CenterMhz: 106.4, WidthMhz: 0.08, PeakPowerDbm: -40.0,
                        DriftMode: SpectrumPeakDriftMode.RandomWalk,
                        RandomWalkStdMhzPerFrame: 0.006,
                        PowerFadingStdDbPerFrame: 0.08),

                    new SpectrumPeakOptions(CenterMhz: 97.5, WidthMhz: 4.5, PeakPowerDbm: -62.0,
                        PowerFadingStdDbPerFrame: 0.18,
                        WidthFadingStdMhzPerFrame: 0.03,
                        CenterJitterStdMhzPerFrame: 0.02),
                }
            };
        }

        public static SpectrumGeneratorOptions NoiseOnly_90To110MHz_20Hz()
        {
            return new SpectrumGeneratorOptions(
                BinCount: 1024,
                FrequencyStartMhz: 90.0,
                FrequencyEndMhz: 110.0,
                MinimumPowerDbm: -120.0,
                MaximumPowerDbm: -20.0,
                BasePowerDbm: -105.0,
                NoiseStdDb: 6.0,
                DriftStdDbPerFrame: 0.2,
                UpdateRateHz: 20);
        }

        public static SpectrumGeneratorOptions SingleCarrier_90To110MHz_20Hz()
        {
            return new SpectrumGeneratorOptions(
                BinCount: 1024,
                FrequencyStartMhz: 90.0,
                FrequencyEndMhz: 110.0,
                MinimumPowerDbm: -120.0,
                MaximumPowerDbm: -20.0,
                BasePowerDbm: -110.0,
                NoiseStdDb: 3.0,
                DriftStdDbPerFrame: 0.05,
                UpdateRateHz: 20)
            {
                Peaks = new[]
                {
            new SpectrumPeakOptions(
                CenterMhz: 100.0,
                WidthMhz: 0.08,
                PeakPowerDbm: -30.0,
                PowerFadingStdDbPerFrame: 0.05
            )
        }
            };
        }

        public static SpectrumGeneratorOptions DriftingCarrier_90To110MHz_20Hz()
        {
            return new SpectrumGeneratorOptions(
                BinCount: 1024,
                FrequencyStartMhz: 90.0,
                FrequencyEndMhz: 110.0,
                MinimumPowerDbm: -120.0,
                MaximumPowerDbm: -20.0,
                BasePowerDbm: -110.0,
                NoiseStdDb: 4.0,
                DriftStdDbPerFrame: 0.05,
                UpdateRateHz: 20)
            {
                Peaks = new[]
                {
            new SpectrumPeakOptions(
                CenterMhz: 95.0,
                WidthMhz: 0.08,
                PeakPowerDbm: -28.0,
                DriftMode: SpectrumPeakDriftMode.Sine,
                DriftAmplitudeMhz: 2.5,
                DriftSpeedHz: 0.015,
                PowerFadingStdDbPerFrame: 0.06
            )
        }
            };
        }

        public static SpectrumGeneratorOptions WidebandBlock_90To110MHz_20Hz()
        {
            return new SpectrumGeneratorOptions(
                BinCount: 1024,
                FrequencyStartMhz: 90.0,
                FrequencyEndMhz: 110.0,
                MinimumPowerDbm: -120.0,
                MaximumPowerDbm: -20.0,
                BasePowerDbm: -112.0,
                NoiseStdDb: 3.5,
                DriftStdDbPerFrame: 0.1,
                UpdateRateHz: 20)
            {
                Peaks = new[]
                {
            new SpectrumPeakOptions(
                CenterMhz: 100.0,
                WidthMhz: 6.0,
                PeakPowerDbm: -55.0,
                PowerFadingStdDbPerFrame: 0.2,
                WidthFadingStdMhzPerFrame: 0.05,
                CenterJitterStdMhzPerFrame: 0.02
            )
        }
            };
        }

        public static SpectrumGeneratorOptions StressTest_90To110MHz_20Hz()
        {
            return new SpectrumGeneratorOptions(
                BinCount: 1024,
                FrequencyStartMhz: 90.0,
                FrequencyEndMhz: 110.0,
                MinimumPowerDbm: -120.0,
                MaximumPowerDbm: -20.0,
                BasePowerDbm: -100.0,
                NoiseStdDb: 10.0,
                DriftStdDbPerFrame: 1.0,
                UpdateRateHz: 20)
            {
                Peaks = new[]
                {
            new SpectrumPeakOptions(95.0, 0.2, -25.0,
                DriftMode: SpectrumPeakDriftMode.RandomWalk,
                RandomWalkStdMhzPerFrame: 0.05,
                PowerFadingStdDbPerFrame: 0.3),

            new SpectrumPeakOptions(105.0, 5.0, -50.0,
                WidthFadingStdMhzPerFrame: 0.2,
                PowerFadingStdDbPerFrame: 0.4)
        }
            };
        }

    }
}
