using System.Windows;
using System.Windows.Input;

namespace SpectrumWaterfallMonitor.Controls.Waterfall.Infrastructure
{
    public class WaterfallDragController
    {
        private readonly WaterfallViewport viewport;

        private bool isDragging;
        private double dragStartMouseX;
        private double dragStartPan;
        private double dragStartZoom;
        private Cursor? previousCursor;

        public bool IsDragging
        {
            get
            {
                return isDragging;
            }
        }

        public WaterfallDragController(WaterfallViewport viewport)
        {
            this.viewport = viewport;
            isDragging = false;
            previousCursor = null;
        }


        public void BeginDrag(FrameworkElement element, MouseButtonEventArgs e)
        {
            isDragging = true;

            dragStartMouseX = e.GetPosition(element).X;
            dragStartPan = viewport.Pan;
            dragStartZoom = viewport.Zoom;

            previousCursor = element.Cursor;
            element.Cursor = Cursors.SizeWE;

            element.CaptureMouse();
        }

        public bool UpdateDrag(FrameworkElement element, MouseEventArgs e, double actualWidth)
        {
            if (!isDragging)
            {
                return false;
            }

            if (!element.IsMouseCaptured)
            {
                return false;
            }

            if (actualWidth <= 0)
            {
                return false;
            }

            if (dragStartZoom <= 1.0)
            {
                return false;
            }

            var x = e.GetPosition(element).X;
            var dx = x - dragStartMouseX;

            viewport.PanByDrag(dragStartPan, dx, actualWidth, dragStartZoom);
            return true;
        }

        public void EndDrag(FrameworkElement element)
        {
            if (!isDragging)
            {
                return;
            }

            isDragging = false;

            if (element.IsMouseCaptured)
            {
                element.ReleaseMouseCapture();
            }

            if (previousCursor is not null)
            {
                element.Cursor = previousCursor;
                previousCursor = null;
            }
            else
            {
                element.Cursor = null;
            }
        }
    }
}
