using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TNCSync.RulerControl.PositionManagers
{
    public enum RulerPosition
    {
        Top,
        Left
    }

    public abstract class RulerPositionManager
    {
        protected RulerPositionManager(RulerBase control) => Control = control;

        public RulerBase Control { get; private set; }

        public abstract Line CreateMajorLine(double offset);
        public abstract Line CreateMinorLine(double offset);
        public abstract TextBlock CreateText(double value, double offset);
        public abstract double GetSize();
        public abstract double GetHeight();

        public bool UpdateMarkerPosition(Line marker, Point position)
        {
            if (marker == null) return false;
            return OnUpdateMarkerPosition(marker, position);
        }

        public void UpdateFirstStepControl(Canvas control, double stepSize)
        {
            if (control == null) return;

            OnUpdateFirstStepControl(control, stepSize);
        }

        public void UpdateStepRepeaterControl(Rectangle control, VisualBrush brush, double stepSize)
        {
            if (control == null) return;
            if (brush == null) return;

            OnUpdateStepRepeaterControl(control, brush, stepSize);
        }

        protected abstract bool OnUpdateMarkerPosition(Line marker, Point position);
        protected abstract void OnUpdateFirstStepControl(Canvas control, double stepSize);
        protected abstract void OnUpdateStepRepeaterControl(Rectangle control, VisualBrush brush, double stepSize);

        protected virtual Line GetBaseLine() => new Line
        {
            Stroke = Control.StepColor,
            StrokeThickness = 1,
            Stretch = Stretch.None,
        };

        protected virtual TextBlock GetTextBlock(string text) => new TextBlock { Text = text };

        protected virtual CultureInfo GetTextCulture() => Control.TextCulture ?? CultureInfo.CurrentUICulture;

    }
}
