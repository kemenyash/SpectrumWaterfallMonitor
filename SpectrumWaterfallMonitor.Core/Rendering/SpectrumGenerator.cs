using SpectrumWaterfallMonitor.Core.Models;
using SpectrumWaterfallMonitor.Core.Records;
using System;

namespace SpectrumWaterfallMonitor.Core.Rendering
{
    public class SpectrumGenerator
    {
        private readonly Random randomGenerator;
        private readonly double frequencyStepMhz;
        private double currentBasePowerDbm;
        private readonly PeakState[] peakStates;

        public double StepMhz => frequencyStepMhz;
        public SpectrumGeneratorOptions Options { get; }

        public SpectrumGenerator(SpectrumGeneratorOptions options, int? randomSeed = null)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));

            if (Options.BinCount < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(options));
            }

            if (Options.UpdateRateHz <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options));
            }

            randomGenerator = randomSeed is null ? new Random() : new Random(randomSeed.Value);

            currentBasePowerDbm = Options.BasePowerDbm;

            frequencyStepMhz = (Options.FrequencyEndMhz - Options.FrequencyStartMhz) / (Options.BinCount - 1);

            peakStates = new PeakState[Options.Peaks.Count];
            for (var index = 0; index < peakStates.Length; index++)
            {
                peakStates[index] = new PeakState(Options.Peaks[index], randomGenerator);
            }
        }

        public SpectrumFrame CreateNextFrame()
        {
            UpdatePeakDrift();

            if (Options.DriftStdDbPerFrame > 0)
            {
                currentBasePowerDbm = Clamp(currentBasePowerDbm + GenerateNormalRandom(0, Options.DriftStdDbPerFrame), Options.MinimumPowerDbm, Options.MaximumPowerDbm);
            }

            var spectrumValues = new double[Options.BinCount];

            for (var binIndex = 0; binIndex < spectrumValues.Length; binIndex++)
            {
                var noiseDb = GenerateNormalRandom(0, Options.NoiseStdDb);
                var currentPowerDbm = currentBasePowerDbm + noiseDb;

                if (Options.Peaks.Count > 0)
                {
                    var frequencyMhz = Options.FrequencyStartMhz + (binIndex * frequencyStepMhz);

                    currentPowerDbm = ApplyPeaks(currentPowerDbm, frequencyMhz);
                }

                spectrumValues[binIndex] = Clamp(currentPowerDbm, Options.MinimumPowerDbm, Options.MaximumPowerDbm);
            }

            return new SpectrumFrame(Options.FrequencyStartMhz, Options.FrequencyEndMhz, Options.MinimumPowerDbm, Options.MaximumPowerDbm, spectrumValues);
        }

        private double GenerateNormalRandom(double mean, double standardDeviation)
        {
            var uniformRandom1 = 1.0 - randomGenerator.NextDouble();
            var uniformRandom2 = 1.0 - randomGenerator.NextDouble();

            var radius = Math.Sqrt(-2.0 * Math.Log(uniformRandom1));
            var angleRadians = 2.0 * Math.PI * uniformRandom2;

            var standardNormalValue = radius * Math.Cos(angleRadians);

            return mean + standardDeviation * standardNormalValue;
        }

        private static double Clamp(double value, double minimum, double maximum)
        {
            if (value < minimum) return minimum;
            if (value > maximum) return maximum;
            return value;
        }

        private double ApplyPeaks(double basePowerDbm, double frequencyMhz)
        {
            var resultingPowerDbm = basePowerDbm;

            foreach (var peakState in peakStates)
            {
                var peakWidthMhz = peakState.CurrentWidthMhz;
                if (peakWidthMhz <= 0)
                {
                    continue;
                }

                var frequencyOffsetMhz =
                    frequencyMhz - peakState.CurrentCenterMhz;

                var sigmaMhz = peakWidthMhz * 0.5;
                if (sigmaMhz <= 0)
                {
                    continue;
                }

                var peakPowerDbm = peakState.CurrentPeakPowerDbm;

                if (peakWidthMhz >= 1.0)
                {
                    var halfWidthMhz = peakWidthMhz * 0.5;
                    var absoluteOffsetMhz = Math.Abs(frequencyOffsetMhz);

                    var edgeWidthMhz = Math.Max(0.001, halfWidthMhz * 0.15);
                    var innerRegionMhz = Math.Max(0.0, halfWidthMhz - edgeWidthMhz);

                    if (absoluteOffsetMhz <= innerRegionMhz)
                    {
                        peakPowerDbm = peakState.CurrentPeakPowerDbm;
                    }
                    else if (absoluteOffsetMhz >= halfWidthMhz)
                    {
                        peakPowerDbm = Options.MinimumPowerDbm;
                    }
                    else
                    {
                        var normalizedTransitionFactor = (absoluteOffsetMhz - innerRegionMhz) / (halfWidthMhz - innerRegionMhz);

                        normalizedTransitionFactor = normalizedTransitionFactor * normalizedTransitionFactor * (3.0 - 2.0 * normalizedTransitionFactor);

                        var attenuationDb = 50.0 * normalizedTransitionFactor;

                        peakPowerDbm = peakState.CurrentPeakPowerDbm - attenuationDb;
                    }
                }
                else
                {
                    var normalizedDistance = frequencyOffsetMhz / sigmaMhz;
                    var attenuationDb = (normalizedDistance * normalizedDistance) * 10.0;

                    peakPowerDbm = peakState.CurrentPeakPowerDbm - attenuationDb;
                }

                if (peakPowerDbm > resultingPowerDbm)
                {
                    resultingPowerDbm = peakPowerDbm;
                }
            }

            return resultingPowerDbm;
        }

        private void UpdatePeakDrift()
        {
            if (peakStates.Length == 0)
            {
                return;
            }

            var deltaTimeSeconds = 1.0 / Options.UpdateRateHz;

            var minFrequencyMhz = Options.FrequencyStartMhz;
            var maxFrequencyMhz = Options.FrequencyEndMhz;

            for (var index = 0; index < peakStates.Length; index++)
            {
                peakStates[index].Update(deltaTimeSeconds, minFrequencyMhz, maxFrequencyMhz, GenerateNormalRandom);
            }
        }
    }
}
