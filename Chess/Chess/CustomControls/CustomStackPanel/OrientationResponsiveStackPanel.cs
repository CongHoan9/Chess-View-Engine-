using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    public class OrientationResponsiveStackPanel : ResponsiveStackPanel
    {
        private Orientation OldOrientation;
        public static readonly DependencyProperty NewOrientationProperty = DependencyProperty.Register(nameof(NewOrientation), typeof(Orientation), typeof(OrientationResponsiveStackPanel), new(Orientation.Vertical));
        public Orientation NewOrientation
        {
            get => (Orientation)GetValue(NewOrientationProperty);
            set => SetValue(NewOrientationProperty, value);
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Dispatcher.BeginInvoke(() =>
            {
                OldOrientation = Orientation;
                if (Parent is FrameworkElement framework)
                {
                    if (FollowWindow && Window.GetWindow(framework) is Window window)
                    {
                        window.SizeChanged += Check;
                        Update(window.ActualWidth);
                    }
                    else
                    {
                        framework.SizeChanged += Check;
                        Update(framework.ActualHeight);
                    }
                }
            });
        }
        private void Check(object sender, SizeChangedEventArgs e)
        {
            Update(e.NewSize.Width);
        }
        private void Update(double width)
        {
            if (!double.IsNaN(width) && !double.IsNaN(WidthResponsive))
            {
                if (width < WidthResponsive)
                {
                    Orientation = NewOrientation;
                }
                else
                {
                    Orientation = OldOrientation; 
                }
            }
        }
    }
}
