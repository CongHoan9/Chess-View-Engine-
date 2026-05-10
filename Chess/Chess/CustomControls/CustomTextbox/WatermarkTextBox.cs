using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
namespace Chess
{
    public class WatermarkTextBox : TextBox, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public static readonly DependencyProperty WatermarkTextProperty = DependencyProperty.Register(nameof(WatermarkText), typeof(string), typeof(WatermarkTextBox), new("Search"));
        public static readonly DependencyProperty InputColorProperty = DependencyProperty.Register(nameof(InputColor), typeof(SolidColorBrush), typeof(WatermarkTextBox), new(new SolidColorBrush(System.Windows.Media.Color.FromRgb(48, 48, 48))));
        public static readonly DependencyProperty WatermarkColorProperty = DependencyProperty.Register(nameof(WatermarkColor), typeof(SolidColorBrush), typeof(WatermarkTextBox), new(new SolidColorBrush(System.Windows.Media.Color.FromRgb(128, 128, 128))));
        public static readonly DependencyProperty NewFontWeightProperty = DependencyProperty.Register(nameof(NewFontWeight), typeof(FontWeight), typeof(WatermarkTextBox), new(default));
        public string WatermarkText
        {
            get => (string)GetValue(WatermarkTextProperty);
            set => SetValue(WatermarkTextProperty, value);
        }
        public SolidColorBrush InputColor
        {
            get => (SolidColorBrush)GetValue(InputColorProperty);
            set => SetValue(InputColorProperty, value);
        }
        public SolidColorBrush WatermarkColor
        {
            get => (SolidColorBrush)GetValue(WatermarkColorProperty);
            set => SetValue(WatermarkColorProperty, value);
        }
        public FontWeight NewFontWeight
        {
            get => (FontWeight)GetValue(NewFontWeightProperty);
            set => SetValue(NewFontWeightProperty, value);
        }
        protected FontWeight OldFontWeight;
        public ICommand ClearText { get; set; }
        public virtual bool IsWatermarkShowing => Text == WatermarkText && Equals(Foreground, WatermarkColor);
        public Visibility ClearVisibility => IsWatermarkShowing ? Visibility.Collapsed : Visibility.Visible;
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Dispatcher.BeginInvoke(() =>
            {
                OldFontWeight = FontWeight;
                ClearText = new RelayCommand(() => Text = string.Empty, () => !IsWatermarkShowing);
                if (string.IsNullOrEmpty(Text))
                {
                    Text = WatermarkText;
                    Foreground = WatermarkColor;
                    OnPropertyChanged(nameof(ClearVisibility));
                }
            });
        }
        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            if (string.IsNullOrEmpty(Text))
            {
                ShowWatermark();
            }
        }
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (IsWatermarkShowing)
            {
                SetupToInput(e);
            }
            else
            {
                base.OnPreviewMouseDown(e);
            }
        }
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (IsWatermarkShowing)
            {
                SetupToInput(e);
            }
            else
            {
                base.OnPreviewMouseUp(e);
            }
        }
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            if (string.IsNullOrEmpty(Text))
            {
                ShowWatermark();
            }
        }
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (IsWatermarkShowing)
            {
                if (e.Key == Key.Space)
                {
                    HideWatermark();
                }
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    if (e.Key == Key.V)
                    {
                        HideWatermark();
                        return;
                    }
                    else if (e.Key == Key.A || e.Key == Key.C || e.Key == Key.X)
                    {
                        e.Handled = true;
                        return;
                    }
                }
                if (e.Key == Key.Enter || e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Home || e.Key == Key.End || e.Key == Key.PageUp || e.Key == Key.PageDown)
                {
                    e.Handled = true;
                    return;
                }
            }

        }
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (IsWatermarkShowing)
            {
                HideWatermark();
            }
            base.OnPreviewTextInput(e);
        }
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            if (string.IsNullOrEmpty(Text) && Foreground != WatermarkColor)
            {
                ShowWatermark();
            }
        }
        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            base.OnContextMenuOpening(e);
            if (IsWatermarkShowing)
            {
                HideWatermark();
            }
        }
        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            if (e.Data.GetData(DataFormats.Text) is string input)
            {
                Text = input;
                Foreground = InputColor; 
                OnPropertyChanged(nameof(ClearVisibility));
                e.Handled = true;
            }
            else
            {
                ShowWatermark();
            }
        }
        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            if (e.Data.GetData(DataFormats.Text) is string)
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }
        protected override void OnDragLeave(DragEventArgs e)
        {
            base.OnDragLeave(e);
            if (string.IsNullOrEmpty(Text))
            {
                ShowWatermark();
            }
        }
        protected void SetupToInput(RoutedEventArgs e)
        {
            OnPropertyChanged(nameof(ClearVisibility));
            Dispatcher.InvokeAsync(() => { CaretIndex = 0; SelectionLength = 0; });
            e.Handled = true;
            Focus();
        }
        protected void ShowWatermark()
        {
            Text = WatermarkText;
            FontWeight = OldFontWeight;
            Foreground = WatermarkColor;
            OnPropertyChanged(nameof(ClearVisibility));
        }
        protected void HideWatermark()
        {
            Text = string.Empty;
            Foreground = InputColor;
            FontWeight = NewFontWeight;
            Select(0, 0);
            OnPropertyChanged(nameof(ClearVisibility));
        }
    }
    public class ClearVisibilityConverter : MarkupExtension, IValueConverter
    {
        private static ClearVisibilityConverter Instance;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance ??= new ClearVisibilityConverter();
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WatermarkTextBox textbox)
            {
                return textbox.IsWatermarkShowing ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}