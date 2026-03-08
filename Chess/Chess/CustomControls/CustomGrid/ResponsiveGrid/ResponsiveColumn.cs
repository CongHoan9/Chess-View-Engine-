using System.Windows;
using System.Windows.Controls;
namespace Chess
{
    internal class ResponsiveColumn : ColumnDefinition
    {
        protected double OldMin;
        protected double OldMax;
        protected GridLength OldSize;
        public static readonly DependencyProperty ParentLevelProperty = DependencyProperty.Register(nameof(ParentLevel), typeof(int), typeof(ResponsiveColumn), new(1));
        public static readonly DependencyProperty ResponsiveSizeProperty = DependencyProperty.Register(nameof(ResponsiveSize), typeof(double), typeof(ResponsiveColumn), new(double.NaN));
        public static readonly DependencyProperty NewSizeProperty = DependencyProperty.Register(nameof(NewSize), typeof(GridLength), typeof(ResponsiveColumn), new(new GridLength(1, GridUnitType.Star)));
        public static readonly DependencyProperty FollowWindowProperty = DependencyProperty.Register(nameof(FollowWindow), typeof(bool), typeof(ResponsiveColumn), new(false));
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
                OldMin = MinWidth;
                OldMax = MaxWidth;
                OldSize = Width;
                if (Parent is FrameworkElement framework)
                {
                    if (FollowWindow)
                    {
                        if (Window.GetWindow(framework) is Window window)
                        {
                            window.SizeChanged += CheckToShow;
                        }
                    }
                    else
                    {
                        framework.SizeChanged += CheckToShow;
                    }
                }
            });
        }
        public void CheckToShow(object sender, SizeChangedEventArgs e)
        {
            double size = e.NewSize.Width;
            if (!double.IsNaN(size) && !double.IsNaN(ResponsiveSize))
            {
                if (size < ResponsiveSize)
                {
                    ClearValue(MinWidthProperty);
                    ClearValue(MaxWidthProperty);
                    Width = NewSize;
                }
                else
                {
                    MinWidth = OldMin;
                    MaxWidth = OldMax;
                    Width = OldSize;
                }
            }
        }
    }
}
