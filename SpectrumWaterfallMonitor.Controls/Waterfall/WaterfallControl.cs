using SpectrumWaterfallMonitor.Controls.Waterfall.Infrastructure;
using SpectrumWaterfallMonitor.Core.Models;
using SpectrumWaterfallMonitor.Core.Rendering;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SpectrumWaterfallMonitor.Controls.Waterfall
{
    public class WaterfallControl : FrameworkElement
    {
        private readonly WaterfallViewport viewport;
        private readonly WaterfallDragController dragController;

        private readonly WaterfallBitmapBuffer bitmapBuffer;
        private readonly SpectrumRowAccumulator accumulator;
        private readonly WaterfallRowWriter rowWriter;
        private readonly WaterfallRenderer renderer;

        private readonly Random randomSource;

        private double displayMinimumPowerDbm;
        private double displayMaximumPowerDbm;

        private double ditherDb;
        private double gamma;

        private GradientLookup gradientLookup;

        public WaterfallControl()
        {
            Focusable = true;
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;

            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);

            displayMinimumPowerDbm = -120;
            displayMaximumPowerDbm = -20;

            ditherDb = 0.25;
            gamma = 1.0;

            gradientLookup = GradientLookupFactory.CreateDefault(1024);

            randomSource = new Random();

            viewport = new WaterfallViewport();
            dragController = new WaterfallDragController(viewport);

            bitmapBuffer = new WaterfallBitmapBuffer();
            accumulator = new SpectrumRowAccumulator();
            rowWriter = new WaterfallRowWriter(randomSource);
            renderer = new WaterfallRenderer();
        }

        public SpectrumFrame? SpectrumFrame
        {
            get => (SpectrumFrame?)GetValue(SpectrumFrameProperty);
            set => SetValue(SpectrumFrameProperty, value);
        }

        public static readonly DependencyProperty SpectrumFrameProperty =
            DependencyProperty.Register(
                nameof(SpectrumFrame),
                typeof(SpectrumFrame),
                typeof(WaterfallControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnSpectrumFrameChanged));

        public int HistoryLineCount
        {
            get => (int)GetValue(HistoryLineCountProperty);
            set => SetValue(HistoryLineCountProperty, value);
        }

        public static readonly DependencyProperty HistoryLineCountProperty =
            DependencyProperty.Register(
                nameof(HistoryLineCount),
                typeof(int),
                typeof(WaterfallControl),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender, OnHistoryLineCountChanged));

        public double ZoomFactor
        {
            get => (double)GetValue(ZoomFactorProperty);
            set => SetValue(ZoomFactorProperty, value);
        }

        public static readonly DependencyProperty ZoomFactorProperty =
            DependencyProperty.Register(
                nameof(ZoomFactor),
                typeof(double),
                typeof(WaterfallControl),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double PanX
        {
            get => (double)GetValue(PanXProperty);
            set => SetValue(PanXProperty, value);
        }

        public static readonly DependencyProperty PanXProperty =
            DependencyProperty.Register(
                nameof(PanX),
                typeof(double),
                typeof(WaterfallControl),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        private static void OnHistoryLineCountChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not WaterfallControl control)
            {
                return;
            }

            control.bitmapBuffer.Reset();
            control.accumulator.Reset();
            control.InvalidateVisual();
        }

        private static void OnSpectrumFrameChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not WaterfallControl control)
            {
                return;
            }

            var frame = e.NewValue as SpectrumFrame;
            if (frame is null)
            {
                return;
            }

            control.HandleFrame(frame);
            control.InvalidateVisual();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (ActualWidth <= 0 || ActualHeight <= 0)
            {
                return;
            }

            viewport.Set(ZoomFactor, PanX);

            var speed = Keyboard.Modifiers.HasFlag(ModifierKeys.Control) ? 1.35 : 1.15;
            viewport.ZoomAt(e.GetPosition(this).X, ActualWidth, e.Delta, speed);

            ZoomFactor = viewport.Zoom;
            PanX = viewport.Pan;

            e.Handled = true;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            Focus();

            if (e.ClickCount >= 2)
            {
                viewport.Reset();
                ZoomFactor = viewport.Zoom;
                PanX = viewport.Pan;

                e.Handled = true;
                return;
            }

            viewport.Set(ZoomFactor, PanX);

            dragController.BeginDrag(this, e);
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!dragController.IsDragging)
            {
                return;
            }

            viewport.Set(ZoomFactor, PanX);

            if (dragController.UpdateDrag(this, e, ActualWidth))
            {
                ZoomFactor = viewport.Zoom;
                PanX = viewport.Pan;
                e.Handled = true;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            dragController.EndDrag(this);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            dragController.EndDrag(this);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
            dragController.EndDrag(this);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            EnsureBitmap();

            var bitmap = bitmapBuffer.Bitmap;
            if (bitmap is null)
            {
                drawingContext.DrawRectangle(Brushes.Black, null, new Rect(0, 0, ActualWidth, ActualHeight));
                return;
            }

            viewport.Set(ZoomFactor, PanX);

            renderer.Render(
                drawingContext,
                bitmap,
                bitmapBuffer.NewestRow,
                ActualWidth,
                ActualHeight,
                viewport.Zoom,
                viewport.Pan);
        }

        private void EnsureBitmap()
        {
            var frame = SpectrumFrame;
            var binWidth = frame is null ? 0 : frame.PowerDbmValues.Length;
            var historyHeight = HistoryLineCount;

            if (binWidth <= 0 || historyHeight <= 0)
            {
                return;
            }

            if (bitmapBuffer.Ensure(binWidth, historyHeight))
            {
                bitmapBuffer.FillWithFloorColor(gradientLookup.ResolvePackedBgra32(0));
                accumulator.Reset();
            }
        }

        private void HandleFrame(SpectrumFrame frame)
        {
            EnsureBitmap();

            var bitmap = bitmapBuffer.Bitmap;
            if (bitmap is null)
            {
                return;
            }

            var outputWidth = bitmap.PixelWidth;
            var outputHeight = bitmap.PixelHeight;

            if (outputWidth <= 0 || outputHeight <= 0)
            {
                return;
            }

            accumulator.SetTarget(outputHeight, HistoryLineCount);

            var rowReady = accumulator.TryAccumulateMaxRow(
                frame.PowerDbmValues,
                outputWidth,
                displayMinimumPowerDbm,
                displayMaximumPowerDbm,
                out var row);

            if (!rowReady)
            {
                return;
            }

            rowWriter.WriteRowToBitmap(
                bitmapBuffer,
                bitmap,
                row,
                displayMinimumPowerDbm,
                displayMaximumPowerDbm,
                ditherDb,
                gamma,
                gradientLookup);
        }
    }
}
