using System;
using System.Windows.Input;

namespace SaneWpf.Framework
{
    public class Command<T> : ICommand
    {
        private readonly Action<T> _commandAction;
        private readonly Func<T, bool> _canExecute;

        public Command(Action<T> commandAction) : this(commandAction, null) { }

        public Command(Action<T> commandAction, Func<T, bool> canExecute)
        {
            _commandAction = commandAction;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke((T) parameter) ?? true;

        public void Execute(object parameter) => _commandAction((T) parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
