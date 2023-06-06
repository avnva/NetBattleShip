using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BattleShip;

public enum FormState
{
    ServerConnect, // Состояние после нажатия на кнопку
    CreateNewGame, //Создание новой игры
    EnjoyToGame, // Подключение к существующей игре
    WaitingStartGame
}
public class LogInViewModel:ViewModelBase
{
    private Player _player;
    private bool _connected;
    private string _enterEndPoint;
    private IPEndPoint _endPoint;
    private string _name;
    private string _gameRoomNumber;
    private RequestParser _requestParser;
    private int _startPort;
    private string _lastRequest;
    private bool _requestSuccess;

    private Visibility _visibilityConnectControls;
    private Visibility _visibilityChoiseControls;
    private Visibility _visibilityNameControls;
    private Visibility _visibilityConnectButton;

    private bool _isEnabledMaskedTextBox;
    private bool _isEnabledConnectButton;

    private string _createNewGameRequest = "Create new game";
    private string _waitingOpponentRequest = "Waiting opponent";
    private string _connectToExistingGameRoomRequest = "Connect to existing game room: ";

    private string _startEndPoint = "127.0.0.01:8888";
    private string _connectToServerText = "Подключиться";
    private string _startGameText = "Начать игру";
    private string _waitingText = "Ожидаем противника...";
    private string _opponentConnectedText = "Противник найден. Можно начинать игру!";
    private string _manual = "Перед началом игры необходимо расставить корабли.\n" +
        "Всего есть несколько типов кораблей с определенным размером. Ознакомиться с ними можно будет в предложенном списке.\n\n" +
        "Обратите внимание!\n" +
        "При выборе горизонтального положения корабль будет размещаться от нажатой клетки вправо на указанный размер.\n" +
        "При выборе вертикального - вниз.";

    public LogInViewModel()
    {
        _player = new Player();
        _requestParser = new RequestParser();
        _connected = false;

        HideControls();
        ChangeVisibilityConnectControls = Visibility.Visible;
        ChangeVisibilityNameControls = Visibility.Visible;
        ChangeVisibilityConnectButton = Visibility.Visible;

        IsEnabledTb = true;
        IsEnabledConnectButton = true;

        CurrentCommandConnectButton = ConnectToServerCommand;
        CurrentLabelText = "Введите IP адрес сервера:";
        CurrentContentConnectButton = _connectToServerText;

        EndPoint = _startEndPoint;
        CurrentBindingTextBox = EndPoint;
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
        get { return _gameRoomNumber; }
        set
        {
            _gameRoomNumber = value;
            OnPropertyChange(nameof(GameRoomNumber));
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

    //команда для кнопки подключения к серверу
    private RelayCommand _connect;
    public RelayCommand ConnectToServerCommand
    {
        get
        {
            return _connect ?? new RelayCommand(
                _execute => {
                    Connect();
                    
                },
                _canExecute => true
            );
        }
    }
    private RelayCommand _createNewGame;
    public RelayCommand CreateNewGameCommand
    {
        get
        {
            return _createNewGame ?? new RelayCommand(
                _execute => {
                    ConnectToNewGameRoom();
                },
                _canExecute => true
            );
        }
    }
    private RelayCommand _connectToGameRoom;
    public RelayCommand ConnectToExistingGameRoomCommand
    {
        get
        {
            return _connectToGameRoom ?? new RelayCommand(
                _execute => {
                    ChangeContent(FormState.EnjoyToGame);
                },
                _canExecute => true
            );
        }
    }
    private RelayCommand _startGame;
    public RelayCommand StartGameCommand
    {
        get
        {
            return _startGame ?? (_startGame = new RelayCommand(
                _execute => {
                    StartGame();
                },
                _canExecute => true
            ));
        }
    }
    private RelayCommand _getReadyToGame;
    public RelayCommand GetReadyToGameCommand
    {
        get
        {
            return _getReadyToGame ?? new RelayCommand(
                _execute => {
                    GetReadyToGame();
                },
                _canExecute => true
            );
        }
    }
    private async void Connect()
    {
        OpponentConnectTextChange = "Ожидаем ответ от сервера...";
        IsEnabledConnectButton = false;
        EndPoint = CurrentBindingTextBox;
        var parts = EndPoint.Split(':');
        var ipAddressString = parts[0];
        var portString = parts[1];

        if (!IPAddress.TryParse(ipAddressString, out IPAddress ipAddress))
            throw new Exception("Неверный формат IP адреса");
        int port = int.Parse(portString);
            
        if(await _player.ConnectAsync(ipAddress, port))
        {
            _player.Name = Name;
        _endPoint = _player.IpEndPoint;
        _startPort = port;
        _player.ServerDisconnected += ServerDisconnected;
        ChangeContent(FormState.ServerConnect);
        }
        else
        {
            MessageBox_Show(null, "Неверный IP адрес", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            OpponentConnectTextChange = "";
            IsEnabledConnectButton = true;
        }
        
    }
    private bool connectFlag = false;
    private async Task HandleFirstMessageBoxResult(MessageBoxResult result)
    {
        string _reconnectRequest = "Reconnect to port: ";
        if (result == MessageBoxResult.Yes)
        {
            int port = _player.Port;
            bool connect = false;
            while (connectFlag != true)
            {
                connectFlag = await _player.ConnectAsync(_player.IpEndPoint.Address, _startPort);
                if (connectFlag == false)
                {
                     MessageBox_Show(HandleSecondMessageBoxResult, "Ошибка подключения. Попробовать снова?", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                }
                else
                    connect = true;
            }
            if (connect)
            {
                if(port != _startPort)
                    await SendRequest(_reconnectRequest + port);
            }
            else
            {
                Environment.Exit(0);
            }
        }
        else if (result == MessageBoxResult.No)
        {
            Environment.Exit(0);
        }
    }
    private void HandleSecondMessageBoxResult(MessageBoxResult result)
    {
        if (result == MessageBoxResult.Yes)
            connectFlag = false;
        else
        {
            Environment.Exit(0);
        }
            
    }
    private async Task ServerDisconnected()
    {
        _player.StopCheckingOpponent();
        MessageBox_ShowAsync(HandleFirstMessageBoxResult, "Сервер отключился. Хотите переподключиться?", "Предупреждение", MessageBoxButton.YesNo);
    }
    private async Task ConnectToNewGameRoom()
    {
        try
        {
            _lastRequest = "CreateNewGame";
            //_requestSuccess = false;
            await _player.CreateNewGame();
            //_requestSuccess = true;
            _endPoint = _player.IpEndPoint;
            _gameRoomNumber = _player.GetPort(_endPoint).ToString();
            _player.CheckOpponentOnlineEvent += ChangeStatusOpponentOnline;
            ChangeContent(FormState.CreateNewGame);
        }
        catch (Exception ex)
        {
            MessageBox_Show(null, ex.Message, "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private void ChangeStatusOpponentOnline(object sender, EventArgs e)
    {
        if (_player.IsOpponentConnected)
        {
            OpponentConnectTextChange = _opponentConnectedText;
            IsEnabledConnectButton = true;
        }
        else
        {
            OpponentConnectTextChange = _waitingText;
            IsEnabledConnectButton = false;
        }
    }
    private void StartGame()
    {
        try
        {
            _player.StopCheckingOpponent();
            _lastRequest = _createNewGameRequest;
            _requestSuccess = false;
            SendRequest(_createNewGameRequest);
        }
        catch (Exception ex)
        {
            MessageBox_Show(null, ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private async Task GetReadyToGame()
    {
        GameRoomNumber = CurrentBindingTextBox;
        _lastRequest = _connectToExistingGameRoomRequest + GameRoomNumber.ToString();
        _requestSuccess = false;
        await SendRequest(_connectToExistingGameRoomRequest + GameRoomNumber.ToString());
        //await SendRequest(_waitingOpponentRequest);
    }
    private async Task SendRequest(string request)
    {
        try
        {
            request = _requestParser.Parse(request);
            Response response = await _player.SendRequestWithResponseAsync(request);
            
            switch (response.Type)
            {
                case RequestType.CreateNewGame:
                    if (response.Flag)
                    {
                        _requestSuccess = true;
                        Hide?.Invoke();
                        OpenNewView(new GameView(new GameViewModel(_player)));
                        MessageBox_Show(null, _manual, "Начало игры", MessageBoxButton.OK, MessageBoxImage.Information);
                        Close?.Invoke();
                    }
                    else
                    {
                        _requestSuccess = true;
                        throw new Exception("Ошибка подключения");
                    }
                    break;
                case RequestType.JoinToGame:
                    //_player.ConnectToNewPort(EndPoint, response);
                    if (response.Flag)
                    {
                        _requestSuccess = true;
                        ChangeContent(FormState.WaitingStartGame);
                        _lastRequest = _waitingOpponentRequest;
                        _requestSuccess = false;
                        await SendRequest(_waitingOpponentRequest);
                    }
                    else
                    {
                        _requestSuccess = true;
                        CurrentBindingTextBox = null;
                        MessageBox_Show(null, "Такой комнаты не существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
                case RequestType.WaitingOpponent:
                    if (response.Flag)
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            _requestSuccess = true;
                            OpenNewView(new GameView(new GameViewModel(_player)));
                            Hide?.Invoke();
                            MessageBox_Show(null, _manual, "Начало игры", MessageBoxButton.OK, MessageBoxImage.Information);
                            Close?.Invoke();
                        });
                    }
                    break;
                case RequestType.Reconnect:
                    if (response.Flag)
                    {
                        MessageBox_Show(null, "Успешно!", "Переподключение", MessageBoxButton.OK, MessageBoxImage.Information);
                        if (_requestSuccess == false)
                        {
                            if (_lastRequest != null && _lastRequest != "CreateNewGame")
                                await SendRequest(_lastRequest);
                            else if (_lastRequest != null && _lastRequest == "CreateNewGame")
                                _player.StartCheckingOpponent();
                        }
                    }
                    else
                    {
                        await ServerDisconnected();
                    }
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            MessageBox_Show(null, ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private void ChangeContent(FormState state)
    {
        switch (state)
        {
            case FormState.ServerConnect:
                if (CheckConnection())
                    ServerConnectUpdateContent();
                break;
            case FormState.CreateNewGame:
                CreateNewGameContentUpdate();
                break;
            case FormState.EnjoyToGame:
                EnjoyToGameContentUpdate();
                break;
            case FormState.WaitingStartGame:
                WaitingStartContentUpdate();
                break;
            default:
                break;
        }
    }
    private bool CheckConnection()
    {
        return _connected = _player.CheckConnection();
    }
    private void ServerConnectUpdateContent()
    {
        // Обновление содержимого формы для состояния после нажатия на кнопку подключения к серверу
        HideControls();
        ChangeVisibilityChoiseControls = Visibility.Visible;
        OpponentConnectTextChange = "";
        IsEnabledConnectButton = true;
    }
    private void CreateNewGameContentUpdate()
    {
        // Обновление содержимого формы для состояния создания новой игры
        HideControls();
        ChangeVisibilityConnectControls = Visibility.Visible;
        ChangeVisibilityConnectButton = Visibility.Visible;

        IsEnabledTb = false;
        IsEnabledConnectButton = false;
        CurrentCommandConnectButton = StartGameCommand;
        CurrentContentConnectButton = _startGameText;
        CurrentBindingTextBox = GameRoomNumber;

        CurrentLabelText = "Номер вашей комнаты:";
        OpponentConnectTextChange = _waitingText;
    }
    private void EnjoyToGameContentUpdate()
    {
        // Обновление содержимого формы для состояния подключения к существующей игре
        HideControls();
        ChangeVisibilityConnectControls = Visibility.Visible;
        ChangeVisibilityConnectButton = Visibility.Visible;
        IsEnabledTb = true;

        CurrentCommandConnectButton = GetReadyToGameCommand;
        CurrentBindingTextBox = GameRoomNumber;
        CurrentLabelText = "Введите номер комнаты:";
        OpponentConnectTextChange = "";
    }
    private void WaitingStartContentUpdate()
    {
        IsEnabledTb = false;
        ChangeVisibilityConnectButton = Visibility.Collapsed;
        OpponentConnectTextChange = "Ожидание начала игры...";
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
    private string _currentBindingTb;
    public string CurrentBindingTextBox
    {
        get { return _currentBindingTb; }
        set
        {
            _currentBindingTb = value;
            OnPropertyChange(nameof(CurrentBindingTextBox));
        }
    }
    private string _currentContentConnectButton;
    public string CurrentContentConnectButton
    {
        get { return _currentContentConnectButton; }
        set
        {
            _currentContentConnectButton = value;
            OnPropertyChange(nameof(CurrentContentConnectButton));
        }
    }
    private string _opponentConnectText;
    public string OpponentConnectTextChange
    {
        get { return _opponentConnectText; }
        set
        {
            _opponentConnectText = value;
            OnPropertyChange(nameof(OpponentConnectTextChange));
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
    public Visibility ChangeVisibilityConnectButton
    {
        get { return _visibilityConnectButton; }
        set
        {
            _visibilityConnectButton = value;
            OnPropertyChange(nameof(ChangeVisibilityConnectButton));
        }

    }
    public bool IsEnabledTb
    {
        get { return _isEnabledMaskedTextBox; }
        set
        {
            _isEnabledMaskedTextBox = value;
            OnPropertyChange(nameof(IsEnabledTb));
        }
    }
    public bool IsEnabledConnectButton
    {
        get { return _isEnabledConnectButton; }
        set
        {
            _isEnabledConnectButton = value;
            OnPropertyChange(nameof(IsEnabledConnectButton));
        }
    }
    private void HideControls()
    {
        ChangeVisibilityChoiseControls = Visibility.Collapsed;
        ChangeVisibilityConnectButton = Visibility.Collapsed;
        ChangeVisibilityConnectControls = Visibility.Collapsed;
        ChangeVisibilityNameControls = Visibility.Collapsed;
    }

}
