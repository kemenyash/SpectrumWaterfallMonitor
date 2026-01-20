using System;

namespace SpectrumWaterfallMonitor.Controls.Waterfall.Infrastructure
{
    public class WaterfallViewport
    {
        private const double MinZoomFactor = 1.0;
        private const double MaxZoomFactor = 50.0;

        public double Zoom { get; private set; }
        public double Pan { get; private set; }

        public WaterfallViewport()
        {
            Zoom = 1.0;
            Pan = 0.0;
        }

        public void Set(double zoom, double pan)
        {
            Zoom = CoerceZoom(zoom);
            Pan = CoercePan(pan);
        }

        public void Reset()
        {
            Zoom = 1.0;
            Pan = 0.0;
        }

        public void ZoomAt(double mouseX, double width, int wheelDelta, double speed)
        {
            if (width <= 0)
            {
                return;
            }

            var zoomOld = CoerceZoom(Zoom);
            var panOld = CoercePan(Pan);

            var steps = wheelDelta / 120.0;
            var zoomNew = CoerceZoom(zoomOld * Math.Pow(speed, steps));

            if (Math.Abs(zoomNew - zoomOld) < 0.000001)
            {
                return;
            }

            var clampedMouseX = Clamp(mouseX, 0, width);
            var contentX = ScreenToContentX(clampedMouseX, zoomOld, panOld, width);
            var panNew = ContentXToPanForAnchor(contentX, clampedMouseX, zoomNew, width);

            Zoom = zoomNew;
            Pan = panNew;
        }

        public void PanByDrag(double startPan, double dxPixels, double width, double zoom)
        {
            zoom = CoerceZoom(zoom);

            if (width <= 0)
            {
                return;
            }

            if (zoom <= 1.0)
            {
                Pan = 0.0;
                return;
            }

            var maxShift = (zoom - 1.0) * width / 2.0;
            if (maxShift <= 0)
            {
                return;
            }

            var panNew = startPan - (dxPixels / maxShift);
            Pan = CoercePan(panNew);
        }

        private static double ScreenToContentX(double screenX, double zoom, double pan, double width)
        {
            zoom = CoerceZoom(zoom);
            pan = CoercePan(pan);

            if (width <= 0)
            {
                return screenX;
            }

            if (zoom == 1.0)
            {
                return screenX;
            }

            var maxShift = (zoom - 1.0) * width / 2.0;
            return (screenX - (1.0 - zoom) * width / 2.0 + pan * maxShift) / zoom;
        }

        private static double ContentXToPanForAnchor(double contentX, double anchorScreenX, double zoom, double width)
        {
            zoom = CoerceZoom(zoom);

            if (width <= 0)
            {
                return 0.0;
            }

            if (zoom == 1.0)
            {
                return 0.0;
            }

            var maxShift = (zoom - 1.0) * width / 2.0;
            var pan = (zoom * contentX + (1.0 - zoom) * width / 2.0 - anchorScreenX) / maxShift;
            return CoercePan(pan);
        }

        private static double CoerceZoom(double zoom)
        {
            if (double.IsNaN(zoom) || double.IsInfinity(zoom))
            {
                return MinZoomFactor;
            }

            return Clamp(zoom, MinZoomFactor, MaxZoomFactor);
        }

        private static double CoercePan(double pan)
        {
            if (double.IsNaN(pan) || double.IsInfinity(pan))
            {
                return 0.0;
            }

            return Clamp(pan, -1.0, 1.0);
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}
