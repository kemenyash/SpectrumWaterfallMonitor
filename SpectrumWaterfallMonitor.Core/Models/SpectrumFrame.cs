namespace SpectrumWaterfallMonitor.Core.Models
{
    public class SpectrumFrame
    {
        public double FrequencyStartMhz { get; }
        public double FrequencyEndMhz { get; }
        public double MinimumPowerDbm { get; }
        public double MaximumPowerDbm { get; }
        public double[] PowerDbmValues { get; }
        public SpectrumFrame(double frequencyStartMhz, double frequencyEndMhz, double minimumPowerDbm, double maximumPowerDbm, double[] powerDbmValues)
        {
            FrequencyStartMhz = frequencyStartMhz;
            FrequencyEndMhz = frequencyEndMhz;
            MinimumPowerDbm = minimumPowerDbm;
            MaximumPowerDbm = maximumPowerDbm;
            PowerDbmValues = powerDbmValues;
        }

    }
}
