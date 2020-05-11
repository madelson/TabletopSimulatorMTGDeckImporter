using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public DelegateCommand(Action<object> execute, Func<object, bool>? canExecute = null, INotifyPropertyChanged? canExecuteChangedSource = null)
        {
            if ((canExecute == null) != (canExecuteChangedSource == null))
            {
                throw new ArgumentException($"{nameof(canExecuteChangedSource)} must be provided alongside {nameof(canExecute)}");
            }

            this._execute = execute;
            this._canExecute = canExecute ?? (_ => true);
            if (canExecuteChangedSource != null)
            {
                canExecuteChangedSource.PropertyChanged += (o, e) => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool CanExecute(object parameter) => this._canExecute(parameter);

        public void Execute(object parameter) => this._execute(parameter);

        public event EventHandler? CanExecuteChanged; // never changes
    }
}
