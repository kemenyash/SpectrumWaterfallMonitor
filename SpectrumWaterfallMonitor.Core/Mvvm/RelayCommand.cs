using System;
using System.Windows.Input;

namespace SpectrumWaterfallMonitor.Core.Mvvm
{
    public class RelayCommand : ICommand
    {
        private readonly Action executeAction;
        private readonly Func<bool>? canExecuteFunction;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            executeAction = execute ?? throw new ArgumentNullException(nameof(execute));
            canExecuteFunction = canExecute;
        }

        public bool CanExecute(object? parameter) => canExecuteFunction?.Invoke() ?? true;

        public void Execute(object? parameter) => executeAction();

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
