using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BattleShip;

public class MessageBoxEventArgs
{
    private readonly MessageBoxButton button;
    private readonly string caption;
    private readonly MessageBoxResult defaultResult;
    private readonly MessageBoxImage icon;
    private readonly string messageBoxText;
    private readonly MessageBoxOptions options;

    private readonly Func<MessageBoxResult, Task> resultAction;
    private readonly Action<MessageBoxResult> resultAct;

    public MessageBoxEventArgs(Func<MessageBoxResult, Task> resultAction, string messageBoxText,
        string caption = "", MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.None,
        MessageBoxOptions options = MessageBoxOptions.None)
    {
        this.resultAction = resultAction;
        this.messageBoxText = messageBoxText;
        this.caption = caption;
        this.button = button;
        this.icon = icon;
        this.defaultResult = defaultResult;
        this.options = options;
    }

    public MessageBoxEventArgs(Action<MessageBoxResult> resultAction, string messageBoxText,
        string caption = "", MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.None,
        MessageBoxOptions options = MessageBoxOptions.None)
    {
        resultAct = resultAction;
        this.messageBoxText = messageBoxText;
        this.caption = caption;
        this.button = button;
        this.icon = icon;
        this.defaultResult = defaultResult;
        this.options = options;
    }
    public void Show()
    {
        MessageBoxResult messageBoxResult =
            MessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options);
        if (resultAction != null)
            resultAction(messageBoxResult);
        else if (resultAct != null)
            resultAct(messageBoxResult);
    }
    //public void Show()
    //{
    //    Result = MessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options);
    //    resultAction?.Invoke(Result);
    //}
}
