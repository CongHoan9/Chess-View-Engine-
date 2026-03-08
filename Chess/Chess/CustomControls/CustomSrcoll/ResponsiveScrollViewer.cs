using System.Windows;
using System.Windows.Controls;
namespace Chess
{
    public class ResponsiveScrollViewer : ScrollViewer
    {
        public static readonly DependencyProperty FollowWindowProperty = DependencyProperty.Register(nameof(FollowWindow), typeof(bool), typeof(ResponsiveScrollViewer), new(false));
        public static readonly DependencyProperty NewHorizontalVisibilityProperty = DependencyProperty.Register(nameof(NewHorizontalVisibility), typeof(ScrollBarVisibility), typeof(ResponsiveScrollViewer), new(ScrollBarVisibility.Auto));
        public static readonly DependencyProperty NewVerticalVisibilityProperty = DependencyProperty.Register(nameof(NewVerticalVisibility), typeof(ScrollBarVisibility),  typeof(ResponsiveScrollViewer), new(ScrollBarVisibility.Auto));
        public bool FollowWindow
        {
            get => (bool)GetValue(FollowWindowProperty);
            set => SetValue(FollowWindowProperty, value);
        }
        public ScrollBarVisibility NewHorizontalVisibility
        {
            get => (ScrollBarVisibility)GetValue(NewHorizontalVisibilityProperty);
            set => SetValue(NewHorizontalVisibilityProperty, value);
        }
        public ScrollBarVisibility NewVerticalVisibility
        {
            get => (ScrollBarVisibility)GetValue(NewVerticalVisibilityProperty);
            set => SetValue(NewVerticalVisibilityProperty, value);
        }
        private ScrollBarVisibility OldVerticalVisibility;
        private ScrollBarVisibility OldHorizontalVisibility;
        private double OldHeight;
        private double OldWidth;
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Dispatcher.BeginInvoke(() =>
            {
                OldWidth = ActualWidth;
                OldHeight = ActualHeight;
                OldVerticalVisibility = VerticalScrollBarVisibility;
                OldHorizontalVisibility = HorizontalScrollBarVisibility;        
                if (Parent is FrameworkElement framework)
                {
                    if (FollowWindow && Window.GetWindow(framework) is Window window)
                    {
                        window.SizeChanged += OnSizeChanged;
                        Update(window.ActualWidth, window.ActualHeight);
                    }
                    else
                    {
                        framework.SizeChanged += OnSizeChanged;
                        Update(framework.ActualWidth, framework.ActualHeight);
                    }
                }
            });
        }
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Update(e.NewSize.Width, e.NewSize.Height - (sender is Window ? (SystemParameters.CaptionHeight + SystemParameters.WindowResizeBorderThickness.Top) * 2 : 0));
        }
        private void Update(double width, double height)
        {
            if (!double.IsNaN(OldWidth) && !double.IsNaN(width))
            {
                if (width < OldWidth)
                {
                    HorizontalScrollBarVisibility = NewHorizontalVisibility;
                }
                else
                {
                    HorizontalScrollBarVisibility = OldHorizontalVisibility;
                }
                if (VerticalScrollBarVisibility != ScrollBarVisibility.Disabled)
                {
                    Width = width;
                }
                else
                {
                    ClearValue(WidthProperty);
                }
            }
            if (!double.IsNaN(OldHeight) && !double.IsNaN(height))
            {
                if (height < OldHeight)
                {
                    VerticalScrollBarVisibility = NewVerticalVisibility;
                }
                else
                {
                    VerticalScrollBarVisibility = OldVerticalVisibility;
                }
                if (VerticalScrollBarVisibility != ScrollBarVisibility.Disabled)
                {
                    Height = height;
                }
                else
                {
                    ClearValue(HeightProperty);
                }
            }
        }
    }
}