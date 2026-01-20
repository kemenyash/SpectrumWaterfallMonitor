using SpectrumWaterfallMonitor.Core.Models;
using SpectrumWaterfallMonitor.Core.Records;

namespace SpectrumWaterfallMonitor.Core.Rendering
{
    public class GradientLookup
    {
        public ColorGradient Gradient { get; }
        public int TableSize { get; }
        public uint[] PackedBgra32Table { get; }

        public GradientLookup(ColorGradient gradient, int tableSize)
        {
            Gradient = gradient;
            TableSize = tableSize;
            PackedBgra32Table = BuildPackedBgra32Table(gradient, tableSize);
        }


        public uint ResolvePackedBgra32(double normalized01)
        {
            var clamped = normalized01 < 0 ? 0 : (normalized01 > 1 ? 1 : normalized01);
            var index = (int)(clamped * (PackedBgra32Table.Length - 1));

            if (index < 0)
            {
                index = 0;
            }

            if (index >= PackedBgra32Table.Length)
            {
                index = PackedBgra32Table.Length - 1;
            }

            return PackedBgra32Table[index];
        }

        private static uint[] BuildPackedBgra32Table(ColorGradient gradient, int tableSize)
        {
            var table = new uint[tableSize];

            for (var index = 0; index < tableSize; index++)
            {
                var position = (double)index / (tableSize - 1);
                var color = MapToColor(gradient, position);
                table[index] = PackBgra32(color);
            }

            return table;
        }

        private static uint PackBgra32(RgbColor color)
        {
            var alpha = 0xFFu;
            var red = (uint)color.Red;
            var green = (uint)color.Green;
            var blue = (uint)color.Blue;

            return (alpha << 24) | (red << 16) | (green << 8) | blue;
        }

        private static RgbColor MapToColor(ColorGradient gradient, double position)
        {
            var stops = gradient.Stops;
            if (stops.Count == 0)
            {
                return new RgbColor(0, 0, 0);
            }

            if (position <= stops[0].Offset)
            {
                return stops[0].Color;
            }

            var lastIndex = stops.Count - 1;
            if (position >= stops[lastIndex].Offset)
            {
                return stops[lastIndex].Color;
            }

            for (var index = 0; index < stops.Count - 1; index++)
            {
                var left = stops[index];
                var right = stops[index + 1];

                if (position < left.Offset || position > right.Offset)
                {
                    continue;
                }

                var span = right.Offset - left.Offset;
                var local = span <= 0 ? 0 : (position - left.Offset) / span;

                var red = (byte)(left.Color.Red + (right.Color.Red - left.Color.Red) * local);
                var green = (byte)(left.Color.Green + (right.Color.Green - left.Color.Green) * local);
                var blue = (byte)(left.Color.Blue + (right.Color.Blue - left.Color.Blue) * local);

                return new RgbColor(red, green, blue);
            }

            return stops[lastIndex].Color;
        }
    }
}
