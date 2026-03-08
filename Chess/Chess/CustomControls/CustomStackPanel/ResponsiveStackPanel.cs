using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    public abstract class ResponsiveStackPanel : StackPanel
    {
        public static readonly DependencyProperty WidthResponsiveProperty = DependencyProperty.Register(nameof(WidthResponsive), typeof(double), typeof(ResponsiveStackPanel), new(double.NaN));
        public static readonly DependencyProperty HeightResponsiveProperty = DependencyProperty.Register(nameof(HeightResponsive), typeof(double), typeof(ResponsiveStackPanel), new(double.NaN));
        public static readonly DependencyProperty FollowWindowProperty = DependencyProperty.Register(nameof(FollowWindow), typeof(bool), typeof(ResponsiveStackPanel), new(false));
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
