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
    private string _name;
    private string _gameRoomNumber;

    private RequestParser _requestParser;

    private Visibility _visibilityConnectControls;
    private Visibility _visibilityChoiseControls;
    private Visibility _visibilityNameControls;

    private bool _isEnabledMaskedTextBox;

    
    private string _IPandPortMask = "000/000/000/000:00000";
    private string _roomNumberMask = "00000";
    private string _createNewGameRequest = "Create new game";
    private string _getNewPort = "Get new port";


    public LogInViewModel()
    { 
        socket = new Player();
        _requestParser = new RequestParser();
        _connected = false;

        ChangeVisibilityChoiseControls = Visibility.Collapsed;
        ChangeVisibilityConnectControls = Visibility.Visible;
        ChangeVisibilityNameControls = Visibility.Visible;

        _isEnabledMaskedTextBox = true;

        CurrentCommandConnectButton = ConnectCommand;
        CurrentMaskFormat = _IPandPortMask;
        CurrentBindingMaskedTextBox = EndPoint;
        CurrentLabelText = "Введите IP адрес сервера:";

        EndPoint = "127.000.000.001:8888";
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

    public string GameRoomNumber
    {
        get => _gameRoomNumber;
        set
        {
            _gameRoomNumber = value;
            OnPropertyChange(nameof(GameRoomNumber));
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
                    ConnectToNewPort();
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

    //поменять
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

    private string _currentlabelText;
    public string CurrentLabelText
    {
        get { return _currentlabelText; }
        set
        {
            _currentlabelText = value;
            OnPropertyChange(nameof(CurrentLabelText));
        }
    }

    private string _currentBinding;

    public string CurrentBindingMaskedTextBox
    {
        get { return _currentBinding; }
        set
        {
            _currentBinding = value;
            OnPropertyChange(nameof(CurrentBindingMaskedTextBox));
        }
    }

    private async Task ConnectToNewPort()
    {
        try
        {
            string request = _requestParser.Parse(_getNewPort);
            Response newPort = await socket.SendRequestAsync(request);
            await socket.ConnectToNewPort(_endPoint, newPort);
            _endPoint = socket.IpEndPoint;
            _gameRoomNumber = GetGameRoomNumber();
            ChangeContent(FormState.CreateNewGame);
        }
        catch (Exception ex)
        {
            MessageBox_Show(null, ex.Message, "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task SendRequest(string request)
    {
        try
        {
            request = _requestParser.Parse(request);
            Response response = await socket.SendRequestAsync(request);

            switch (response.Type)
            {
                case RequestType.CreateNewGame:
                    
                    break;
                //case RequestType.Disks:
                //    ClientLog += $"Client received: {response.Contents}\n";
                //    UpdateServerDirectoryContents(response.Contents.Split('|'));
                //    break;
                //case RequestType.FileContents:
                //    ClientLog += $"Client received: {response.Contents}\n";
                //    break;
                default:
                    
                    break;
            }
        }
        catch (Exception ex)
        {
            MessageBox_Show(null, ex.Message, "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
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
    private string GetGameRoomNumber()
    {
        return socket.GetPort(_endPoint).ToString();
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
    }

    private void CreateNewGameContent()
    {
        // Создание содержимого формы для состояния создания новой игры
        ChangeVisibilityChoiseControls = Visibility.Collapsed;
        ChangeVisibilityConnectControls = Visibility.Visible;
        ChangeVisibilityNameControls = Visibility.Collapsed;
        IsEnabledMaskedTbControls = false;

        CurrentCommandConnectButton = CreateNewGame;
        CurrentMaskFormat = _roomNumberMask;
        CurrentBindingMaskedTextBox = GameRoomNumber;
        CurrentLabelText = "Номер вашей комнаты:";
    }

    private void EnjoyToGameContent()
    {
        // Создание содержимого формы для состояния подключения к существующей игре
        ChangeVisibilityChoiseControls = Visibility.Collapsed;
        ChangeVisibilityConnectControls = Visibility.Visible;
        //ChangeEnabledChoiseControls = false;
        //ChangeEnabledConnectControls = true;
        CurrentCommandConnectButton = StartCommand;
        CurrentMaskFormat = _roomNumberMask;
        CurrentLabelText = "Введите номер комнаты:";

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

    public string EndPoint
    {
        get { return _enterEndPoint; }
        set
        {
            _enterEndPoint = value;
            OnPropertyChange(nameof(EndPoint));
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

    public bool IsEnabledMaskedTbControls
    {
        get { return _isEnabledMaskedTextBox; }
        set
        {
            _isEnabledMaskedTextBox = value;
            OnPropertyChange(nameof(IsEnabledMaskedTbControls));
        }
    }

}
