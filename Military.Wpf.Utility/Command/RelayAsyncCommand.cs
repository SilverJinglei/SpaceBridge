using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;

namespace Military.Wpf.Utility.Command
{
    public class RelayAsyncCommand<T> : RelayCommand<T>
    {
        public RelayAsyncCommand(Action<T> execute) : base(execute)
        {
        }

        public RelayAsyncCommand(Action<T> execute, Func<T, bool> canExecute) : base(execute, canExecute)
        {
        }

        public async override void Execute(object parameter)
        {
            await Task.Run(() => base.Execute(parameter));
        }
    }

    public class RelayAsyncCommand : RelayCommand
    {
        public RelayAsyncCommand(Action execute) : base(execute)
        {
        }

        public RelayAsyncCommand(Action execute, Func<bool> canExecute) : base(execute, canExecute)
        {
        }

        public async override void Execute(object parameter)
        {
            await Task.Run(() => base.Execute(parameter));
        }
    }
}