using System.Windows;
using System.Windows.Controls;
namespace Chess
{
    public class CollapsedGrid : ResponsiveGrid
    {
        protected Visibility OldVisibility;
        public static readonly DependencyProperty NewVisibilityProperty = DependencyProperty.Register(nameof(NewVisibility), typeof(Visibility), typeof(CollapsedGrid), new(Visibility.Visible));
        public Visibility NewVisibility
        {
            get => (Visibility)GetValue(NewVisibilityProperty);
            set => SetValue(NewVisibilityProperty, value);
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Dispatcher.BeginInvoke(() =>
            {
                OldVisibility = Visibility;
                if (Parent is FrameworkElement framework)
                {
                    if(FollowWindow)
                    {
                        if (Window.GetWindow(framework) is Window window)
                        {
                            window.SizeChanged += Check;
                            Update(window.ActualWidth);
                        }
                    }
                    else
                    {
                        framework.SizeChanged += Check;
                        Update(framework.ActualWidth);
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
                    Visibility = NewVisibility;
                }
                else
                {
                    Visibility = OldVisibility;
                }
            }
        }
    }
}
