using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
namespace Chess
{
    public class AutoInsideBorder : Border
    {
        private double OldCanvasTop;
        public static readonly DependencyProperty FactorSizeProperty = DependencyProperty.Register(nameof(FactorSize), typeof(double), typeof(AutoInsideBorder), new(double.NaN));
        public static readonly DependencyProperty FollowWindowProperty = DependencyProperty.Register(nameof(FollowWindow), typeof(bool), typeof(AutoInsideBorder), new PropertyMetadata(false));
        public bool FollowWindow 
        { 
            get => (bool)GetValue(FollowWindowProperty);
            set => SetValue(FollowWindowProperty, value);
        }
        public double FactorSize
        {
            get => (double)GetValue(FactorSizeProperty);
            set => SetValue(FactorSizeProperty, value);
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Dispatcher.BeginInvoke(() =>
            {
                OldCanvasTop = Canvas.GetTop(this);
                if (Parent is FrameworkElement framework)
                {
                    if (FollowWindow && Window.GetWindow(framework) is Window window)
                    {
                        window.SizeChanged += OnSizeChanged;
                        Update(window.ActualHeight); 
                    }
                    else
                    {
                        framework.SizeChanged += OnSizeChanged;
                        Update(framework.ActualHeight);
                    }
                }
            });
        }
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Update(e.NewSize.Height);
        }
        private void Update(double height)
        {
            if (!double.IsNaN(FactorSize))
            {
                double overflow = ActualHeight + FactorSize - height;
                if (overflow <= 0)
                {
                    Canvas.SetTop(this, OldCanvasTop);
                    return;
                }
                double factor = height < ActualHeight ? .5 : 0;
                double delta = overflow - (ActualHeight - height) * factor;
                Canvas.SetTop(this, OldCanvasTop - delta);
            }
        }
    }
}