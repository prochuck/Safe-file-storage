using System;
using System.Windows.Input;

namespace Safe_file_storage.ViewModels
{
    public class Command : ICommand
    {
        Action<object> _execute;
        Func<object, bool> _canExecute;

        public Command(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute is null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
           _execute(parameter);
        }
    }
}
