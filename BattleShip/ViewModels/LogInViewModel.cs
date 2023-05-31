using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BattleShip;

public enum FormState
{
    Initial, // Начальное состояние формы
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
    //private bool _statusOpponentOnline;

    private RequestParser _requestParser;

    private Visibility _visibilityConnectControls;
    private Visibility _visibilityChoiseControls;
    private Visibility _visibilityNameControls;
    private Visibility _visibilityConnectButton;

    private bool _isEnabledMaskedTextBox;
    private bool _isEnabledConnectButton;

    private string _IPandPortMask;
    private string _roomNumberMask = "0000";
    private string _IPAndPortMask = "000/000/000/000:00000";
    private string _createNewGameRequest = "Create new game";
    private string _getGameRoom = "Get game room";
    private string _connectToExistingGameRoom = "Connect to existing game room: ";
    private string _waitingText = "Ожидаем противника...";
    private string _opponentConnectedText = "Противник найден. Можно начинать игру!";


    public LogInViewModel()
    {
        _player = new Player();
        _requestParser = new RequestParser();
        _connected = false;

        ChangeVisibilityChoiseControls = Visibility.Collapsed;
        ChangeVisibilityConnectControls = Visibility.Visible;
        ChangeVisibilityNameControls = Visibility.Visible;
        ChangeVisibilityConnectButton = Visibility.Visible;

        IsEnabledMaskedTb = true;
        IsEnabledConnectButton = true;

        CurrentCommandConnectButton = ConnectCommand;
        CurrentMaskFormat = _IPandPortMask;
        CurrentLabelText = "Введите IP адрес сервера:";

        EndPoint = "127.000.000.001:8888";
        CurrentMaskFormat = _IPAndPortMask;
        CurrentBindingMaskedTextBox = EndPoint;
    }

    //команда для кнопки подключения к серверу
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

    private async void Connect()
    {
        try
        {
            await _player.ConnectAsync(EndPoint);
            _player.Name = Name;
            _endPoint = _player.IpEndPoint;
        }
        catch (Exception ex)
        {
            MessageBox_Show(null, ex.Message, "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // команда для кнопки создания новой игры
    private RelayCommand _createNewGame;
    public RelayCommand CreateNewGame
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
    private async Task ConnectToNewGameRoom()
    {
        try
        {
            string request = _requestParser.Parse(_getGameRoom);
            Response newPort = await _player.SendRequestAsync(request);

            await _player.ConnectToNewPort(_endPoint, newPort);

            _endPoint = _player.IpEndPoint;
            _gameRoomNumber = GetGameRoomNumber();

            _player.CheckOpponentOnlineEvent += ChangeStatusOpponentOnline;
            _player.StartCheckingOpponent();

            ChangeContent(FormState.CreateNewGame);
        }
        catch (Exception ex)
        {
            MessageBox_Show(null, ex.Message, "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private string GetGameRoomNumber()
    {
        return _player.GetPort(_endPoint).ToString();
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

    private RelayCommand _connectToGameRoom;
    public RelayCommand ConnectToExistingGameRoom
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
    private void StartGame()
    {
        //проверка флага готовности к игре у противника, переход на новую форму
        try
        {
            OpenNewView(new GameView(new GameViewModel()));
            Close?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox_Show(null, ex.Message, "Возникла ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
    private async Task GetReadyToGame()
    {
        //отправка запроса совпадает ли порт, флаг готовности к игре на true
        GameRoomNumber = _currentBindingTb;
        await SendRequest(_connectToExistingGameRoom + GameRoomNumber.ToString());
    }
    
    private async Task SendRequest(string request)
    {
        try
        {
            request = _requestParser.Parse(request);
            Response response = await _player.SendRequestAsync(request);

            switch (response.Type)
            {
                case RequestType.CreateNewGame:
                    
                    break;
                case RequestType.JoinToGame:
                    //смена контента у игрока, начинающего игру, о готовности оппонента.
                    //Блокировка элементов управления у игрока в ожидании игры
                    if (response.Flag)
                    {
                        _player.StartCheckingOpponent();
                        ChangeContent(FormState.WaitingStartGame);
                    }
                    else
                        MessageBox_Show(null, "Такой комнаты не существует!", "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
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
                    //FormState.Initial;
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
        ChangeVisibilityChoiseControls = Visibility.Visible;
        ChangeVisibilityConnectControls = Visibility.Collapsed;
        ChangeVisibilityNameControls = Visibility.Collapsed;
        ChangeVisibilityConnectButton = Visibility.Collapsed;
    }

    private void CreateNewGameContentUpdate()
    {
        // Создание содержимого формы для состояния создания новой игры
        ChangeVisibilityChoiseControls = Visibility.Collapsed;
        ChangeVisibilityConnectControls = Visibility.Visible;
        ChangeVisibilityNameControls = Visibility.Collapsed;
        ChangeVisibilityConnectButton = Visibility.Visible;

        IsEnabledMaskedTb = false;
        IsEnabledConnectButton = false;

        CurrentCommandConnectButton = StartGameCommand;
        CurrentBindingMaskedTextBox = null;
        CurrentMaskFormat = _roomNumberMask;
        CurrentBindingMaskedTextBox = GameRoomNumber;

        CurrentLabelText = "Номер вашей комнаты:";
        OpponentConnectTextChange = _waitingText;
    }

    private void EnjoyToGameContentUpdate()
    {
        // Создание содержимого формы для состояния подключения к существующей игре
        ChangeVisibilityChoiseControls = Visibility.Collapsed;
        ChangeVisibilityConnectControls = Visibility.Visible;
        ChangeVisibilityNameControls = Visibility.Collapsed;
        ChangeVisibilityConnectButton = Visibility.Visible;
        IsEnabledMaskedTb = true;

        CurrentCommandConnectButton = GetReadyToGameCommand;
        CurrentBindingMaskedTextBox = null;
        CurrentMaskFormat = _roomNumberMask;
        CurrentBindingMaskedTextBox = GameRoomNumber;
        CurrentLabelText = "Введите номер комнаты:";
        OpponentConnectTextChange = "";
    }




    private async Task Disconnect()
    {
        try
        {
            await _player.DisconnectAsync();
        }
        catch (Exception ex)
        {
            MessageBox_Show(null, ex.Message, "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }



    private void WaitingStartContentUpdate()
    {
        IsEnabledMaskedTb = false;
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

    private string _currentBindingTb;

    public string CurrentBindingMaskedTextBox
    {
        get { return _currentBindingTb; }
        set
        {
            _currentBindingTb = value;
            OnPropertyChange(nameof(CurrentBindingMaskedTextBox));
        }
    }

    private string _currentOpponentConnectText;
    public string OpponentConnectTextChange
    {
        get { return _currentOpponentConnectText; }
        set
        {
            _currentOpponentConnectText = value;
            OnPropertyChange(nameof(OpponentConnectTextChange));
        }
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

    public bool IsEnabledMaskedTb
    {
        get { return _isEnabledMaskedTextBox; }
        set
        {
            _isEnabledMaskedTextBox = value;
            OnPropertyChange(nameof(IsEnabledMaskedTb));
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

}
