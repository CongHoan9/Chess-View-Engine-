using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    public class CanJumpGrid : ResponsiveGrid
    {
        public static readonly DependencyProperty NewColumnProperty = DependencyProperty.Register(nameof(NewColumn), typeof(int), typeof(CanJumpGrid), new(0));
        public static readonly DependencyProperty NewRowProperty = DependencyProperty.Register(nameof(NewRow), typeof(int), typeof(CanJumpGrid), new(0));
        public int NewColumn
        {
            get => (int)GetValue(NewColumnProperty);
            set => SetValue(NewColumnProperty, value);
        }
        public int NewRow
        {
            get => (int)GetValue(NewRowProperty);
            set => SetValue(NewRowProperty, value);
        }
        private int OldColumn;
        private int OldRow;
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Dispatcher.BeginInvoke(() =>
            {
                OldRow = GetRow(this);
                OldColumn = GetColumn(this);
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
                    SetColumn(this, NewColumn);
                    SetRow(this, NewRow);
                }
                else
                {
                    SetColumn(this, OldColumn);
                    SetRow(this, OldRow);
                }
            }
        }
    }
}
