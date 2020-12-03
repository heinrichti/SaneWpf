using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SaneWpf.Framework
{
    public class AsyncCommand<T> : ICommand
    {
        private readonly Func<T, Task> _commandAction;
        private readonly Func<T, bool> _canExecute;

        public AsyncCommand(Func<T, Task> commandAction) : this(commandAction, null) { }

        public AsyncCommand(Func<T, Task> commandAction, Func<T, bool> canExecute)
        {
            _commandAction = commandAction;
            _canExecute = canExecute;
        }

        bool ICommand.CanExecute(object parameter) => !_running && (_canExecute?.Invoke((T) parameter) ?? true);

        async void ICommand.Execute(object parameter)
        {
            _running = true;
            ExecuteChanged?.Invoke(this, EventArgs.Empty);

            await _commandAction((T) parameter);

            _running = false;
            ExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                ExecuteChanged += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
                ExecuteChanged -= value;
            }
        }

        private volatile bool _running;
        private event EventHandler ExecuteChanged;

    }
}
