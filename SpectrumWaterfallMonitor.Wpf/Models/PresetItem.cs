using SpectrumWaterfallMonitor.Core.Records;

namespace SpectrumWaterfallMonitor.Wpf.Models
{
    public class PresetItem
    {
        public string Name { get; }
        public Func<SpectrumGeneratorOptions> Factory { get; }
        public override string ToString() => Name;

        public PresetItem(string name, Func<SpectrumGeneratorOptions> factory)
        {
            Name = name;
            Factory = factory;
        }

    }
}
