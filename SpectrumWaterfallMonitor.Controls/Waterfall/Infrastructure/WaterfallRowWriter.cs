using SpectrumWaterfallMonitor.Core.Rendering;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SpectrumWaterfallMonitor.Controls.Waterfall.Infrastructure
{
    public class WaterfallRowWriter
    {
        private readonly Random randomSource;

        public WaterfallRowWriter(Random randomSource)
        {
            this.randomSource = randomSource;
        }

        public void WriteRowToBitmap(
            WaterfallBitmapBuffer buffer,
            WriteableBitmap bitmap,
            double[] rowPowerDbm,
            double minimum,
            double maximum,
            double ditherDb,
            double gamma,
            GradientLookup gradientLookup)
        {
            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;

            if (width <= 0 || height <= 0)
            {
                return;
            }

            if (rowPowerDbm.Length < width)
            {
                return;
            }

            buffer.AdvanceWriteRow();

            var rowIndex = buffer.NewestRow;
            if (rowIndex < 0)
            {
                return;
            }

            var inverseRange = 1.0 / (maximum - minimum);

            bitmap.Lock();
            try
            {
                unsafe
                {
                    var basePointer = (byte*)bitmap.BackBuffer;
                    var stride = bitmap.BackBufferStride;

                    var rowPointer = (uint*)(basePointer + rowIndex * stride);

                    for (var x = 0; x < width; x++)
                    {
                        var value = rowPowerDbm[x];

                        if (ditherDb > 0)
                        {
                            value += ((randomSource.NextDouble() - 0.5) * 2.0) * ditherDb;
                        }

                        var normalized = (value - minimum) * inverseRange;

                        if (normalized < 0)
                        {
                            normalized = 0;
                        }

                        if (normalized > 1)
                        {
                            normalized = 1;
                        }

                        if (gamma != 1.0)
                        {
                            normalized = Math.Pow(normalized, gamma);
                        }

                        rowPointer[x] = gradientLookup.ResolvePackedBgra32(normalized);
                    }
                }

                bitmap.AddDirtyRect(new Int32Rect(0, rowIndex, width, 1));
            }
            finally
            {
                bitmap.Unlock();
            }
        }
    }
}
