using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SpectrumWaterfallMonitor.Controls.Waterfall.Infrastructure
{
    public class WaterfallRenderer
    {
        private readonly ImageBrush topSegmentBrush;
        private readonly ImageBrush bottomSegmentBrush;

        public WaterfallRenderer()
        {
            topSegmentBrush = CreateImageBrush();
            bottomSegmentBrush = CreateImageBrush();
        }

        public void Render(
            DrawingContext drawingContext,
            WriteableBitmap bitmap,
            int newestRow,
            double actualWidth,
            double actualHeight,
            double zoom,
            double pan)
        {
            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, actualWidth, actualHeight)));

            var pushedTransform = false;

            if (zoom != 1.0 || pan != 0.0)
            {
                var maxShift = (zoom - 1.0) * actualWidth / 2.0;

                var matrix = Matrix.Identity;
                matrix.ScaleAt(zoom, 1.0, actualWidth / 2.0, 0.0);
                matrix.Translate(-pan * maxShift, 0.0);

                drawingContext.PushTransform(new MatrixTransform(matrix));
                pushedTransform = true;
            }

            DrawBitmapWithWrap(drawingContext, bitmap, newestRow, actualWidth, actualHeight);

            if (pushedTransform)
            {
                drawingContext.Pop();
            }

            drawingContext.Pop();
        }

        private void DrawBitmapWithWrap(DrawingContext drawingContext, WriteableBitmap bitmap, int newestRow, double w, double h)
        {
            var pixelHeight = bitmap.PixelHeight;
            if (pixelHeight <= 0 || newestRow < 0)
            {
                drawingContext.DrawImage(bitmap, new Rect(0, 0, w, h));
                return;
            }

            var topRows = pixelHeight - newestRow;
            var bottomRows = newestRow;

            var topHeight = h * topRows / pixelHeight;

            topSegmentBrush.ImageSource = bitmap;
            topSegmentBrush.Viewbox = new Rect(0, (double)newestRow / pixelHeight, 1, (double)topRows / pixelHeight);
            drawingContext.DrawRectangle(topSegmentBrush, null, new Rect(0, 0, w, topHeight));

            if (bottomRows > 0)
            {
                bottomSegmentBrush.ImageSource = bitmap;
                bottomSegmentBrush.Viewbox = new Rect(0, 0, 1, (double)bottomRows / pixelHeight);
                drawingContext.DrawRectangle(bottomSegmentBrush, null, new Rect(0, topHeight, w, h - topHeight));
            }
        }

        private static ImageBrush CreateImageBrush()
        {
            var brush = new ImageBrush
            {
                ViewboxUnits = BrushMappingMode.RelativeToBoundingBox,
                Stretch = Stretch.Fill,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top
            };

            RenderOptions.SetBitmapScalingMode(brush, BitmapScalingMode.NearestNeighbor);
            return brush;
        }
    }
}
