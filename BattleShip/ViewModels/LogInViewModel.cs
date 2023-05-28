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
using System.Windows.Data;
using Xceed.Wpf.Toolkit;

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
    private IPEndPoint _endPoint;
    private string clientLog;
    private Visibility _visibilityConnectControls;
    private Visibility _visibilityChoiseControls;
    private Visibility _visibilityNameControls;
    private bool _isEnabledConnectControls;
    private bool _isEnabledChoiseControls;
    private bool _isEnabledNameControls;
    private string _name;
    private string _IPandPortMask = "000/000/000/000:00000";
    private string _roomNumberMask = "00000";


    public LogInViewModel()
    {
        EndPoint = "127.000.000.001:8888";
        socket = new Player();
        ChangeVisibilityChoiseControls = Visibility.Collapsed;
        ChangeVisibilityConnectControls = Visibility.Visible;
        ChangeEnabledConnectControls = true;
        ChangeEnabledChoiseControls = false;
        ChangeEnabledNameControls = true;
        CurrentCommandConnectButton = ConnectCommand;
        CurrentMaskFormat = _IPandPortMask;
        _connected = false;
        LabelText = "Введите IP адрес сервера:";
    }
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChange(nameof(Name));
        }
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
                _canExecute => true
            );
        }
    }

    private RelayCommand _createNewGame;
    public RelayCommand CreateNewGame
    {
        get
        {
            return _createNewGame ?? new RelayCommand(
                _execute => {
                    ChangeContent(FormState.CreateNewGame);
                },
                _canExecute => true
            );
        }
    }

    private RelayCommand _enjoyToGame;
    public RelayCommand EnjoyToGame
    {
        get
        {
            return _enjoyToGame ?? new RelayCommand(
                _execute => {
                    ChangeContent(FormState.EnjoyToGame);
                },
                _canExecute => true
            );
        }
    }

    private RelayCommand _start;

    public RelayCommand StartCommand
    {
        get
        {
            return _connect ?? new RelayCommand(
                _execute => {
                    //поменять!
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

    public Visibility ChangeVisibilityConnectControls
    {
        get { return _visibilityConnectControls; }
        set
        {
            _visibilityConnectControls = value;
            OnPropertyChange(nameof(ChangeVisibilityConnectControls));
        }
    }


    public Visibility ChangeVisibilityNameControls
    {
        get { return _visibilityNameControls; }
        set
        {
            _visibilityNameControls = value;
            OnPropertyChange(nameof(ChangeVisibilityNameControls));
        }
    }
    //public Visibility ChangeVisibilityEnjoyToGameControl
    //{
    //    get { return _visibilityEnjoyToGameControls; }
    //    set
    //    {
    //        _visibilityEnjoyToGameControls = value;
    //        OnPropertyChange(nameof(ChangeVisibilityEnjoyToGameControl));
    //    }
    //}

    public bool ChangeEnabledChoiseControls
    {
        get { return _isEnabledChoiseControls; }
        set
        {
            _isEnabledChoiseControls = value;
            OnPropertyChange(nameof(ChangeEnabledChoiseControls));
        }
    }

    public bool ChangeEnabledConnectControls
    {
        get { return _isEnabledConnectControls; }
        set
        {
            _isEnabledConnectControls = value;
            OnPropertyChange(nameof(ChangeEnabledConnectControls));
        }
    }
    public bool ChangeEnabledNameControls
    {
        get { return _isEnabledNameControls; }
        set
        {
            _isEnabledNameControls = value;
            OnPropertyChange(nameof(ChangeEnabledNameControls));
        }
    }

    private ICommand _currentCommandConnectButton;
    public ICommand CurrentCommandConnectButton
    {
        get { return _currentCommandConnectButton; }
        set
        {
            _currentCommandConnectButton = value;
            OnPropertyChange(nameof(CurrentCommandConnectButton));
        }
    }

    private string _currentMaskFormat;
    public string CurrentMaskFormat
    {
        get { return _currentMaskFormat; }
        set
        {
            _currentMaskFormat = value;
            OnPropertyChange(nameof(CurrentMaskFormat));
        }
    }

    private string _labelText;
    public string LabelText
    {
        get { return _labelText; }
        set
        {
            _labelText = value;
            OnPropertyChange(nameof(LabelText));
        }
    }
    private void ChangeContent(FormState state)
    {
        if (!CheckConnection())
        {
            switch (state)
            {
                //case FormState.Initial:
                //    CurrentContent = CreateInitialContent();
                //    break;
                case FormState.ServerConnect:
                    UpdateContentAfterServerConnect();
                    break;
                case FormState.CreateNewGame:
                    CreateNewGameContent();
                    break;
                case FormState.EnjoyToGame:
                    EnjoyToGameContent();
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

    private void UpdateContentAfterServerConnect()
    {
        // Создание содержимого формы для состояния после нажатия на кнопку подключения к серверу
        ChangeVisibilityChoiseControls = Visibility.Visible;
        ChangeVisibilityConnectControls = Visibility.Collapsed;
        ChangeVisibilityNameControls = Visibility.Collapsed;
        ChangeEnabledChoiseControls = true;
        ChangeEnabledConnectControls = false;
        ChangeEnabledNameControls = false;
    }

    private void CreateNewGameContent()
    {
        // Создание содержимого формы для состояния создания новой игры
        ChangeVisibilityChoiseControls = Visibility.Collapsed;
        ChangeVisibilityConnectControls = Visibility.Visible;
        ChangeEnabledChoiseControls = false;
        ChangeEnabledConnectControls = true;
        CurrentCommandConnectButton = StartCommand;
        CurrentMaskFormat = _roomNumberMask;
        LabelText = "Номер вашей комнаты:";
    }

    private void EnjoyToGameContent()
    {
        // Создание содержимого формы для состояния подключения к существующей игре
        ChangeVisibilityChoiseControls = Visibility.Collapsed;
        ChangeVisibilityConnectControls = Visibility.Visible;
        ChangeEnabledChoiseControls = false;
        ChangeEnabledConnectControls = true;
        CurrentCommandConnectButton = StartCommand;
        CurrentMaskFormat = _roomNumberMask;
        LabelText = "Введите номер комнаты:";

    }
    private void StartGame()
    {

    }


    private async void Connect()
    {
        try
        {
             await socket.ConnectAsync(EndPoint);
            _endPoint = socket.IpEndPoint;
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
