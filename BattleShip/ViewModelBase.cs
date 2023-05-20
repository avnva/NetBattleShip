using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BattleShip
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<MessageBoxEventArgs> MessageBoxRequest;

        public Action Close { get; set; }
        protected void MessageBox_Show(Action<MessageBoxResult> resultAction, string messageBoxText,
            string caption = "", MessageBoxButton button = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultResult = MessageBoxResult.None, MessageBoxOptions options = MessageBoxOptions.None)
        {
            MessageBoxRequest?.Invoke(this,
                new MessageBoxEventArgs(resultAction, messageBoxText, caption,
                    button, icon, defaultResult, options));
        }

        protected void OnPropertyChange(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void Dispose()
        {
        }
    }
}
