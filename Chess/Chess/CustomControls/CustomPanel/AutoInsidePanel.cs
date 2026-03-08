using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Chess
{
    public class AutoInsidePanel : StackPanel
    {
        private double OriginalTop;
        private bool HasOriginal;
        private bool IsAdjusted;
        public static readonly DependencyProperty FollowWindowProperty = DependencyProperty.Register(nameof(FollowWindow), typeof(bool), typeof(AutoInsidePanel), new(false));
        public bool FollowWindow
        {
            get => (bool)GetValue(FollowWindowProperty);
            set => SetValue(FollowWindowProperty, value);
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            if (Canvas.GetTop(this) is double top)
            { 
                OriginalTop = top;
                HasOriginal = true;
            }
            Dispatcher.BeginInvoke(() =>
            {
                if (Parent is FrameworkElement framework)
                {
                    if (FollowWindow && Window.GetWindow(framework) is Window window)
                    {
                        window.SizeChanged += CheckBounds;
                    }
                    else
                    {
                        framework.SizeChanged += CheckBounds;
                    }
                }
            });
        }
        private void CheckBounds(object sender, SizeChangedEventArgs e)
        {
            Console.WriteLine("Checking AutoInsidePanel bounds...");
            if (!HasOriginal || Parent is not FrameworkElement parent)
            {
                return;
            }
            double containerHeight = FollowWindow ? Window.GetWindow(parent)?.ActualHeight ?? parent.ActualHeight : parent.ActualHeight;
            double controlHeight = ActualHeight;
            double currentTop = Canvas.GetTop(this);
            if (double.IsNaN(currentTop))
            {
                currentTop = 0;
            }
            double bottom = currentTop + controlHeight;
            bool isOverflowing = controlHeight > containerHeight || currentTop < 0 || bottom > containerHeight;
            // Nếu không còn overflow → quay về vị trí gốc
            if (!isOverflowing && IsAdjusted)
            {
                Canvas.SetTop(this, OriginalTop);
                IsAdjusted = false;
                return;
            }
            // Nếu overflow thật sự → điều chỉnh
            if (isOverflowing)
            {
                double newTop = currentTop;

                if (controlHeight > containerHeight)
                {
                    newTop = (containerHeight - controlHeight) / 2;
                }
                else
                {
                    if (bottom > containerHeight)
                        newTop -= (bottom - containerHeight);

                    if (newTop < 0)
                        newTop = 0;
                }
                Canvas.SetTop(this, newTop);
                IsAdjusted = true;
            }
        }
    }
}
