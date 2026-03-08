using System.Windows.Input;
namespace Chess
{
    public abstract class CommandBase : ICommand
    {
        public abstract bool CanExecute(object parameter);
        public abstract void Execute(object parameter);
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        protected static void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
    public class RelayCommand(Action execute, Func<bool> canExecute = null) : CommandBase
    {
        private readonly Action ActionExecute = execute;
        private readonly Func<bool> FuncCanExecute = canExecute;
        public override bool CanExecute(object parameter) => FuncCanExecute?.Invoke() ?? true;
        public override void Execute(object parameter) => ActionExecute();
    }
    public class RelayCommand<T>(Action<T> execute, Func<T, bool> canExecute = null) : CommandBase
    {
        private readonly Action<T> ActionExecute = execute;
        private readonly Func<T, bool> FuncCanExecute = canExecute;
        public override bool CanExecute(object parameter)
        {
            if (parameter == null && typeof(T).IsValueType)
            {
                return FuncCanExecute?.Invoke(default) ?? true;
            }
            else
            {
                return FuncCanExecute?.Invoke((T)parameter) ?? true;
            }
        }

        public override void Execute(object parameter)
        {
            if (parameter == null && typeof(T).IsValueType)
            {
                ActionExecute(default);
            }
            else
            {
                ActionExecute((T)parameter);
            }
                
        }
    }
}
