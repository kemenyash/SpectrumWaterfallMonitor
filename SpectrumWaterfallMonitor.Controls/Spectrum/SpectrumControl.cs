using SpectrumWaterfallMonitor.Controls.Spectrum.Infrastructure;
using SpectrumWaterfallMonitor.Core.Models;
using System;
using System.Windows;
using System.Windows.Media;

namespace SpectrumWaterfallMonitor.Controls.Spectrum
{
    public class SpectrumControl : FrameworkElement
    {
        private readonly SpectrumStyle style;
        private readonly SpectrumGridRenderer gridRenderer;
        private readonly SpectrumSmoother smoother;
        private readonly SpectrumSampler sampler;
        private readonly SpectrumGeometryBuilder geometryBuilder;

        public SpectrumControl()
        {
            style = SpectrumStyle.CreateDefault();
            gridRenderer = new SpectrumGridRenderer();
            smoother = new SpectrumSmoother(escalation: 0.55, release: 0.12);
            sampler = new SpectrumSampler();
            geometryBuilder = new SpectrumGeometryBuilder();
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
                typeof(SpectrumControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnSpectrumFrameChanged));

        private static void OnSpectrumFrameChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not SpectrumControl control)
            {
                return;
            }

            control.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var width = Math.Max(1, (int)Math.Round(ActualWidth));
            var height = Math.Max(1, (int)Math.Round(ActualHeight));

            drawingContext.DrawRectangle(Brushes.Black, null, new Rect(0, 0, width, height));
            gridRenderer.Draw(drawingContext, style.GridPen, width, height);

            var frame = SpectrumFrame;
            if (frame is null)
            {
                return;
            }

            var values = frame.PowerDbmValues;
            if (values.Length == 0)
            {
                return;
            }

            smoother.EnsureWidth(width);

            var min = frame.MinimumPowerDbm;
            var max = frame.MaximumPowerDbm;

            var geometry = geometryBuilder.Build(
                values,
                width,
                height,
                min,
                max,
                sampler,
                smoother);

            if (geometry is null)
            {
                return;
            }

            drawingContext.DrawGeometry(null, style.LinePen, geometry);
        }
    }
}
