using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http;
using System.Windows.Input;
using System.Windows.Controls;

namespace BattleShip;

public enum FormState
{
    Initial, // Начальное состояние формы
    ServerConnect, // Состояние после нажатия на кнопку
    CreateNewGame, //Создание новой игры
    EnjoyToGame // Подключение к существующей игре
}
public class LogInViewModel:ViewModelBase
{
    private Player socket;
    private bool _connected;
    private string _enterEndPoint;
    private string _endPoint;
    private string clientLog;
    private Visibility _visibilityConnectToServerControls;
    private Visibility _visibilityChoiseControls;
    private bool _isEnabledConnectButton;
    private bool _isEnabledChoiseControls;

    public LogInViewModel()
    {
        EndPoint = "127.0.0.1:8888";
        socket = new Player();
        ChangeVisibilityChoiseControls = Visibility.Collapsed;
        ChangeVisibilityConnectToServerControls = Visibility.Visible;
        ChangeEnabledConnectToServerControls = true;
        ChangeEnabledChoiseControls = false;
        _connected = false;
    }


    private RelayCommand _connect;

    public RelayCommand ConnectCommand
    {
        get
        {
            return _connect ?? new RelayCommand(
                _execute => {
                    Connect();
                    ChangeContent(FormState.ServerConnect);
                },
                _canExecute => !_connected
            );
        }
    }
    public Visibility ChangeVisibilityChoiseControls
    {
        get { return _visibilityChoiseControls; }
        set
        {
            _visibilityChoiseControls = value;
            OnPropertyChange(nameof(ChangeVisibilityChoiseControls));
        }
    }

    public Visibility ChangeVisibilityConnectToServerControls
    {
        get { return _visibilityConnectToServerControls; }
        set
        {
            _visibilityConnectToServerControls = value;
            OnPropertyChange(nameof(ChangeVisibilityConnectToServerControls));
        }
    }
    public bool ChangeEnabledChoiseControls
    {
        get { return _isEnabledChoiseControls; }
        set
        {
            _isEnabledChoiseControls = value;
            OnPropertyChange(nameof(ChangeEnabledChoiseControls));
        }
    }

    public bool ChangeEnabledConnectToServerControls
    {
        get { return _isEnabledConnectButton; }
        set
        {
            _isEnabledConnectButton = value;
            OnPropertyChange(nameof(ChangeEnabledConnectToServerControls));
        }
    }

    private void ChangeContent(FormState state)
    {
        if (CheckConnection())
        {
            switch (state)
            {
                //case FormState.Initial:
                //    CurrentContent = CreateInitialContent();
                //    break;
                case FormState.ServerConnect:
                    CreateServerConnectContent();
                    break;
                case FormState.CreateNewGame:
                    CreateNewGameContent();
                    break;
                case FormState.EnjoyToGame:
                    CreateEnjoyToGameContent();
                    break;
                default:
                    //FormState.Initial;
                    break;
            }
        }
    }
    private bool CheckConnection()
    {
        return _connected = socket.CheckConnection();
    }

    private void CreateServerConnectContent()
    {
        // Создание содержимого формы для состояния после нажатия на кнопку подключения к серверу
        ChangeVisibilityChoiseControls = Visibility.Visible;
        ChangeVisibilityConnectToServerControls = Visibility.Collapsed;
        ChangeEnabledChoiseControls = true;
        ChangeEnabledConnectToServerControls = false;
    }

    private void CreateNewGameContent()
    {
        // Создание содержимого формы для состояния создания новой игры
;
    }

    private void CreateEnjoyToGameContent()
    {
        // Создание содержимого формы для состояния подключения к существующей игре

    }

    private async void Connect()
    {
        try
        {
             await socket.ConnectAsync(EndPoint);
            _endPoint = socket.IpEndPoint.ToString();
        }
        catch (Exception ex)
        {
            MessageBox_Show(null, ex.Message, "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public string ClientLog
    {
        get { return clientLog; }
        set
        {
            clientLog = value;
            OnPropertyChange(nameof(clientLog));
        }
    }

    public string EndPoint
    {
        get { return _enterEndPoint; }
        set
        {
            _enterEndPoint = value;
            OnPropertyChange(nameof(EndPoint));
        }
    }

}
