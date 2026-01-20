using System.Windows.Media.Imaging;

namespace SpectrumWaterfallMonitor.Controls.Waterfall.Infrastructure
{
    public class WaterfallBitmapBuffer
    {
        public WriteableBitmap? Bitmap { get; private set; }

        public int BinWidth { get; private set; }
        public int HistoryHeight { get; private set; }

        public int NextWriteRow { get; private set; }
        public int NewestRow { get; private set; }

        public WaterfallBitmapBuffer()
        {
            Bitmap = null;
            BinWidth = 0;
            HistoryHeight = 0;

            NewestRow = -1;
            NextWriteRow = 0;
        }

        public bool Ensure(int binWidth, int historyHeight)
        {
            if (binWidth <= 0 || historyHeight <= 0)
            {
                return false;
            }

            if (Bitmap is not null && BinWidth == binWidth && HistoryHeight == historyHeight)
            {
                return false;
            }

            BinWidth = binWidth;
            HistoryHeight = historyHeight;

            Bitmap = new WriteableBitmap(BinWidth, HistoryHeight, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);

            NewestRow = -1;
            NextWriteRow = HistoryHeight > 0 ? HistoryHeight - 1 : 0;

            return true;
        }

        public void AdvanceWriteRow()
        {
            if (Bitmap is null)
            {
                return;
            }

            var height = Bitmap.PixelHeight;
            if (height <= 0)
            {
                return;
            }

            var rowIndex = NextWriteRow;

            NewestRow = rowIndex;
            NextWriteRow = rowIndex == 0 ? height - 1 : rowIndex - 1;
        }

        public void FillWithFloorColor(uint floorColor)
        {
            if (Bitmap is null)
            {
                return;
            }

            Bitmap.Lock();
            try
            {
                unsafe
                {
                    var pointer = (uint*)Bitmap.BackBuffer;
                    var count = (Bitmap.BackBufferStride / 4) * HistoryHeight;

                    for (var index = 0; index < count; index++)
                    {
                        pointer[index] = floorColor;
                    }
                }

                Bitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, BinWidth, HistoryHeight));

                NewestRow = -1;
                NextWriteRow = HistoryHeight > 0 ? HistoryHeight - 1 : 0;
            }
            finally
            {
                Bitmap.Unlock();
            }
        }

        public void Reset()
        {
            Bitmap = null;
            BinWidth = 0;
            HistoryHeight = 0;

            NewestRow = -1;
            NextWriteRow = 0;
        }
    }
}
