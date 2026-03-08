using System.Windows;
using System.Windows.Controls;
namespace Chess
{
    internal class ResponsiveRow : RowDefinition
    {
        protected double OldMin;
        protected double OldMax;
        protected GridLength OldSize;
        public static readonly DependencyProperty ParentLevelProperty = DependencyProperty.Register(nameof(ParentLevel), typeof(int), typeof(ResponsiveRow), new(1));
        public static readonly DependencyProperty ResponsiveSizeProperty = DependencyProperty.Register(nameof(ResponsiveSize), typeof(double), typeof(ResponsiveRow), new(double.NaN));
        public static readonly DependencyProperty NewSizeProperty = DependencyProperty.Register(nameof(NewSize), typeof(GridLength), typeof(ResponsiveRow), new(new GridLength(1, GridUnitType.Star)));
        public static readonly DependencyProperty FollowWindowProperty = DependencyProperty.Register(nameof(FollowWindow), typeof(bool), typeof(ResponsiveRow), new(false));
        public int ParentLevel
        {
            get => (int)GetValue(ParentLevelProperty);
            set => SetValue(ParentLevelProperty, value);
        }
        public double ResponsiveSize
        {
            get => (double)GetValue(ResponsiveSizeProperty);
            set => SetValue(ResponsiveSizeProperty, value);
        }
        public GridLength NewSize
        {
            get => (GridLength)GetValue(NewSizeProperty);
            set => SetValue(NewSizeProperty, value);
        }
        public bool FollowWindow
        {
            get => (bool)GetValue(FollowWindowProperty);
            set => SetValue(FollowWindowProperty, value);
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Dispatcher.BeginInvoke(() =>
            {
                OldMin = MinHeight;
                OldMax = MaxHeight;
                OldSize = Height;
                if (Parent is FrameworkElement framework)
                {
                    if (FollowWindow)
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
            if (!double.IsNaN(width) && !double.IsNaN(ResponsiveSize))
            {
                if (width < ResponsiveSize)
                {
                    ClearValue(MinHeightProperty);
                    ClearValue(MaxHeightProperty);
                    Height = NewSize;
                }
                else
                {
                    MinHeight = OldMin;
                    MaxHeight = OldMax;
                    Height = OldSize;
                }
            }
        }
    }
}
