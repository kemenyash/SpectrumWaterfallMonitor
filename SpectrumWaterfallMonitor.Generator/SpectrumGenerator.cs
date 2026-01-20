using SpectrumWaterfallMonitor.Core.Models;
using SpectrumWaterfallMonitor.Generator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumWaterfallMonitor.Generator
{
    public sealed class SpectrumGenerator
    {
        public SpectrumGenerator(SpectrumGeneratorOptions options, int? randomSeed = null)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));

            if (Options.BinCount < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(options), "BinCount must be >= 2.");
            }

            if (Options.UpdateRateHz <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options), "UpdateRateHz must be > 0.");
            }

            _random = randomSeed is null ? new Random() : new Random(randomSeed.Value);
            _currentBasePowerDbm = Options.BasePowerDbm;
            _stepMhz = (Options.FrequencyEndMhz - Options.FrequencyStartMhz) / (Options.BinCount - 1);

            _peakStates = new PeakState[Options.Peaks.Count];
            for (var i = 0; i < _peakStates.Length; i++)
            {
                _peakStates[i] = new PeakState(Options.Peaks[i], _random);
            }
        }

        public SpectrumGeneratorOptions Options { get; }

        /// <summary>
        /// Pre-calculated bin step (MHz) so UI can display it if needed.
        /// </summary>
        public double StepMhz => _stepMhz;

        private readonly Random _random;
        private readonly double _stepMhz;
        private long _frameIndex;
        private double _currentBasePowerDbm;
        private readonly PeakState[] _peakStates;

        public SpectrumFrame CreateNextFrame()
        {
            _frameIndex++;

            UpdatePeakDrift();

            // Slow drift (optional): keeps the signal alive without hardcoding any "scenes".
            if (Options.DriftStdDbPerFrame > 0)
            {
                _currentBasePowerDbm = Clamp(
                    _currentBasePowerDbm + NextNormal(0, Options.DriftStdDbPerFrame),
                    Options.MinimumPowerDbm,
                    Options.MaximumPowerDbm);
            }

            var values = new double[Options.BinCount];

            for (var i = 0; i < values.Length; i++)
            {
                var noise = NextNormal(0, Options.NoiseStdDb);
                var v = _currentBasePowerDbm + noise;

                if (Options.Peaks.Count > 0)
                {
                    var f = Options.FrequencyStartMhz + (i * _stepMhz);
                    v = ApplyPeaks(v, f);
                }

                values[i] = Clamp(v, Options.MinimumPowerDbm, Options.MaximumPowerDbm);
            }

            return new SpectrumFrame(
                Options.FrequencyStartMhz,
                Options.FrequencyEndMhz,
                Options.MinimumPowerDbm,
                Options.MaximumPowerDbm,
                values);
        }

        private double NextNormal(double mean, double stdDev)
        {
            // Box–Muller
            var u1 = 1.0 - _random.NextDouble();
            var u2 = 1.0 - _random.NextDouble();
            var r = Math.Sqrt(-2.0 * Math.Log(u1));
            var theta = 2.0 * Math.PI * u2;
            var z = r * Math.Cos(theta);
            return mean + stdDev * z;
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private double ApplyPeaks(double currentDbm, double frequencyMhz)
        {
            var v = currentDbm;

            foreach (var peakState in _peakStates)
            {
                var peak = peakState.Options;
                var width = peakState.CurrentWidthMhz;
                if (width <= 0)
                {
                    continue;
                }

                var d = frequencyMhz - peakState.CurrentCenterMhz;

                // UI-friendly approximation:
                // - peak.PeakPowerDbm is the maximum at the center.
                // - as we move away from the center, we attenuate in dB quadratically.
                // This is not a physical RF model, but it produces stable, clear waterfall bands.
                var sigma = width * 0.5;
                if (sigma <= 0)
                {
                    continue;
                }

                var peakDbm = peakState.CurrentPeakPowerDbm;

                if (width >= 1.0)
                {
                    // Flat-top wideband: plateau + smooth edges (no parabola)
                    var half = width * 0.5;
                    var x = Math.Abs(d);

                    // how soft the edge is (15% of half-width)
                    var edge = Math.Max(0.001, half * 0.15);
                    var inner = Math.Max(0.0, half - edge);

                    if (x <= inner)
                    {
                        // plateau
                        peakDbm = peakState.CurrentPeakPowerDbm;
                    }
                    else if (x >= half)
                    {
                        // outside band
                        peakDbm = Options.MinimumPowerDbm;
                    }
                    else
                    {
                        // smooth drop on the edge
                        var t = (x - inner) / (half - inner); // 0..1
                        t = t * t * (3.0 - 2.0 * t);         // smoothstep
                        var dropDb = 50.0 * t;               // edge drop depth
                        peakDbm = peakState.CurrentPeakPowerDbm - dropDb;
                    }
                }
                else
                {
                    // Narrow carriers: keep current "gaussian-like" shape
                    sigma = width * 0.5;
                    if (sigma > 0)
                    {
                        var k = d / sigma;
                        var attenuationDb = (k * k) * 10.0;
                        peakDbm = peakState.CurrentPeakPowerDbm - attenuationDb;
                    }
                    else
                    {
                        peakDbm = Options.MinimumPowerDbm;
                    }
                }


                // "Max" overlay makes the band clearly visible above noise.
                if (peakDbm > v)
                {
                    v = peakDbm;
                }
            }

            return v;
        }

        private void UpdatePeakDrift()
        {
            if (_peakStates.Length == 0)
            {
                return;
            }

            var dt = 1.0 / Options.UpdateRateHz;
            var minF = Options.FrequencyStartMhz;
            var maxF = Options.FrequencyEndMhz;

            for (var i = 0; i < _peakStates.Length; i++)
            {
                _peakStates[i].Update(dt, minF, maxF, NextNormal);
            }
        }

        private sealed class PeakState
        {
            public PeakState(SpectrumPeakOptions options, Random random)
            {
                Options = options;
                BaseCenterMhz = options.CenterMhz;
                CurrentCenterMhz = options.CenterMhz;
                CurrentPeakPowerDbm = options.PeakPowerDbm;
                CurrentWidthMhz = options.WidthMhz;

                _phase = random.NextDouble() * 2.0 * Math.PI;
            }

            public SpectrumPeakOptions Options { get; }
            public double BaseCenterMhz { get; }
            public double CurrentCenterMhz { get; private set; }
            public double CurrentPeakPowerDbm { get; private set; }
            public double CurrentWidthMhz { get; private set; }

            private double _phase;

            public void Update(double dt, double minF, double maxF, Func<double, double, double> nextNormal)
            {
                switch (Options.DriftMode)
                {
                    case SpectrumPeakDriftMode.None:
                        CurrentCenterMhz = BaseCenterMhz;
                        break;

                    case SpectrumPeakDriftMode.Sine:
                        {
                            var amp = Options.DriftAmplitudeMhz;
                            var hz = Options.DriftSpeedHz;
                            if (amp <= 0 || hz <= 0)
                            {
                                CurrentCenterMhz = BaseCenterMhz;
                                break;
                            }

                            _phase += 2.0 * Math.PI * hz * dt;
                            if (_phase > 2.0 * Math.PI)
                            {
                                _phase -= 2.0 * Math.PI;
                            }

                            CurrentCenterMhz = BaseCenterMhz + (amp * Math.Sin(_phase));
                            CurrentCenterMhz = ClampToRange(CurrentCenterMhz, minF, maxF, CurrentWidthMhz);
                            break;
                        }

                    case SpectrumPeakDriftMode.RandomWalk:
                        {
                            var std = Options.RandomWalkStdMhzPerFrame;
                            if (std <= 0)
                            {
                                CurrentCenterMhz = BaseCenterMhz;
                                break;
                            }

                            CurrentCenterMhz = CurrentCenterMhz + nextNormal(0, std);
                            CurrentCenterMhz = ClampToRange(CurrentCenterMhz, minF, maxF, CurrentWidthMhz);
                            break;
                        }
                }

                if (Options.PowerFadingStdDbPerFrame > 0)
                {
                    CurrentPeakPowerDbm = CurrentPeakPowerDbm + nextNormal(0, Options.PowerFadingStdDbPerFrame);
                }
                else
                {
                    CurrentPeakPowerDbm = Options.PeakPowerDbm;
                }

                if (Options.WidthFadingStdMhzPerFrame > 0)
                {
                    CurrentWidthMhz = CurrentWidthMhz + nextNormal(0, Options.WidthFadingStdMhzPerFrame);
                }
                else
                {
                    CurrentWidthMhz = Options.WidthMhz;
                }

                CurrentWidthMhz = ClampWidth(CurrentWidthMhz, Options.WidthMhz);

                if (Options.CenterJitterStdMhzPerFrame > 0)
                {
                    CurrentCenterMhz = CurrentCenterMhz + nextNormal(0, Options.CenterJitterStdMhzPerFrame);
                    CurrentCenterMhz = ClampToRange(CurrentCenterMhz, minF, maxF, CurrentWidthMhz);
                }
            }

            private static double ClampWidth(double width, double baseWidth)
            {
                var min = Math.Max(0.01, baseWidth * 0.4);
                var max = baseWidth * 2.5;
                if (width < min) return min;
                if (width > max) return max;
                return width;
            }

            private static double ClampToRange(double center, double minF, double maxF, double width)
            {
                var margin = Math.Max(0.01, width * 2.0);
                var min = minF + margin;
                var max = maxF - margin;
                if (center < min) return min;
                if (center > max) return max;
                return center;
            }
        }
    }
}
