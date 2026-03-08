using System;
using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    public abstract class ResponsiveGrid : Grid
    {
        public static readonly DependencyProperty WidthResponsiveProperty = DependencyProperty.Register(nameof(WidthResponsive), typeof(double), typeof(ResponsiveGrid), new(double.NaN));
        public static readonly DependencyProperty HeightResponsiveProperty = DependencyProperty.Register(nameof(HeightResponsive), typeof(double), typeof(ResponsiveGrid), new(double.NaN));
        public static readonly DependencyProperty FollowWindowProperty = DependencyProperty.Register(nameof(FollowWindow), typeof(bool), typeof(ResponsiveGrid), new(false));
        public virtual double WidthResponsive
        {
            get => (double)GetValue(WidthResponsiveProperty);
            set => SetValue(WidthResponsiveProperty, value);
        }
        public virtual double HeightResponsive
        {
            get => (double)GetValue(HeightResponsiveProperty);
            set => SetValue(HeightResponsiveProperty, value);
        }
        public bool FollowWindow
        {
            get => (bool)GetValue(FollowWindowProperty);
            set => SetValue(FollowWindowProperty, value);
        }
    }
}
