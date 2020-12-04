using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SaneWpf.Framework
{
    public class AsyncCommand : ICommand
    {
        private readonly Func<object, Task> _commandAction;
        private readonly Func<object, bool> _canExecute;

        public AsyncCommand(Func<object, Task> commandAction) : this(commandAction, null) { }

        public AsyncCommand(Func<object, Task> commandAction, Func<object, bool> canExecute)
        {
            _commandAction = commandAction;
            _canExecute = canExecute;
        }

        bool ICommand.CanExecute(object parameter) => !_running && (_canExecute?.Invoke(parameter) ?? true);

        async void ICommand.Execute(object parameter) => await ExecuteAsync(parameter).ConfigureAwait(false);

        public async Task ExecuteAsync(object parameter)
        {
            _running = true;
            ExecuteChanged?.Invoke(this, EventArgs.Empty);

            await _commandAction(parameter);

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
