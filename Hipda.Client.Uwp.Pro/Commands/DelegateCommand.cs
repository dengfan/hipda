using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hipda.Client.Uwp.Pro.Commands
{
    public class DelegateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (this.CanExecuteFunc == null)
            {
                return true;
            }
            return this.CanExecuteFunc(parameter);
        }

        public void Execute(object parameter)
        {
            if (this.ExecuteAction == null)
            {
                return;
            }
            this.ExecuteAction(parameter);
        }

        public Action<object> ExecuteAction { get; set; }
        public Func<object, bool> CanExecuteFunc { get; set; }
    }

    public class DelegateCommand<T> : ICommand
    {
        /// <summary>
        /// 命令
        /// </summary>
        private Action<T> _Command;
        /// <summary>
        /// 命令可否执行判断
        /// </summary>
        private Func<T, bool> _CanExecute;
        /// <summary>
        /// 可执行判断结束后通知命令执行
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="command">命令</param>
        public DelegateCommand(Action<T> command) : this(command, null)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="command">命令</param>
        /// <param name="canexecute">命令可执行判断</param>
        public DelegateCommand(Action<T> command, Func<T, bool> canexecute)
        {
            if (command == null)
            {
                throw new ArgumentException("command");
            }
            _Command = command;
            _CanExecute = canexecute;
        }

        /// <summary>
        /// 命令执行判断
        /// </summary>
        /// <param name="parameter">判断数据</param>
        /// <returns>判定结果（True：可执行，False：不可执行）</returns>
        public bool CanExecute(object parameter)
        {
            return _CanExecute == null ? true : _CanExecute((T)parameter);
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">参数</param>
        public void Execute(object parameter)
        {
            _Command((T)parameter);
        }
    }

}
