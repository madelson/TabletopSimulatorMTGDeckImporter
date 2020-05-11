using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TabletopMtgImporter.Wpf
{
    internal class DelegateCommand : ICommand
    {
        private readonly Func<object, bool> _canExecute;
        private readonly Action<object> _execute;

        public DelegateCommand(Action<object> execute, Func<object, bool>? canExecute = null)
        {
            this._execute = execute;
            this._canExecute = canExecute ?? (_ => true);
        }

        public bool CanExecute(object parameter) => this._canExecute(parameter);

        public void Execute(object parameter) => this._execute(parameter);

        public event EventHandler CanExecuteChanged; // never changes
    }
}
