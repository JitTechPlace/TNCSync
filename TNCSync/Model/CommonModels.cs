using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TNCSync.Model
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            if (this.CanExecuteEvaluator == null)
            { return true; }
            else
            {
                bool result = this.CanExecuteEvaluator.Invoke(parameter);
                return result;
            }
        }

        public void Execute(object parameter)
        {
            this.MethodToExecute.Invoke(parameter);
        }

        private Action<object> MethodToExecute;
        private Func<object, bool> CanExecuteEvaluator;

        public RelayCommand(Action<object> ActualMethodToExecute, Func<object, bool> ActualCanExecuteEvaluator)
        {
            MethodToExecute = ActualMethodToExecute;
            CanExecuteEvaluator = ActualCanExecuteEvaluator;
        }

        public RelayCommand(Action<object> ActualMethodToExecute)
        {
            new RelayCommand(ActualMethodToExecute, null);
        }
    }
}
