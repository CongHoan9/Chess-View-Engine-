using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    public class UnIncreaseScrollViewer : ScrollViewer
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Dispatcher.BeginInvoke(() =>
            {
                if (Parent is FrameworkElement framework)
                {
                    framework.SizeChanged += Check;
                    Update(framework.ActualWidth);
                }
            });
        }
        private void Check(object sender, SizeChangedEventArgs e)
        {
            Update(e.NewSize.Width);
        }
        private void Update(double width)
        {
            if (!double.IsNaN(width))
            {
                MaxWidth = width - (Margin.Left + Margin.Right);
            }
        }
    }
}
