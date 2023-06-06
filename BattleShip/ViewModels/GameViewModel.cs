using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleShip.ViewModels;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;
using System.Net;
using System.Windows.Controls;
using System.Windows;
using System.Data.Common;
using BattleShip.Model;

namespace BattleShip;
public enum ShipDirection
{
    Horizontal,
    Vertical
}

public class GameViewModel : ViewModelBase
{
    private ObservableCollection<CellViewModel> _playerCells;
    private ObservableCollection<CellViewModel> _enemyCells;
    private ObservableCollection<Ship> _availableShips;
    private Ship _selectedShip;
    private GameManager _gameManager;
    private Player _player;
    private RequestParser _requestParser;

    private string _chooseShipText = "Выберите корабль:";
    private string _selectedShipText = "Выбран корабль: ";
    private string _chooseDirectionText = "Выберите положение:";
    private string _shipsAreOverText = "Все корабли расставлены!";
    private string _opponentOnlineText = "Противник онлайн";
    private string _opponentOfflineText = "Противник офлайн...";
    private string _yourTurnText = "Ваш ход!\nВыберите клетку\nна поле соперника";
    private string _opponentTurnText = "Ход соперника...";
    private string _manual = "Суть игры заключается в том, что необходимо уничтожить все корабли соперника, прежде чем он уничтожит Ваши.\n" +
        "Игроки ходят по очереди. При попадании в корабль соперника дается дополнительный ход.\n\n" +
        "Успехов!\n";

    private string _startGameRequest = "Start game";
    private string _endOfTurnRequest = "End of turn";


    private Visibility _visibilityComboBox;
    private Visibility _visibilityCancellationButton;
    private Visibility _visibilityReadyButton;
    private Visibility _visibilityChooseDirectionPanel;
    private Visibility _visibilityDeleteButton;
    private Visibility _visibilityWaitOpponentText;
    public GameViewModel(Player player)
    {
        _player = player;
        _gameManager = new GameManager(_player);
        _requestParser = new RequestParser();
        _gameManager.PlayerCellsChanged += OnPlayerCellsChanged;
        _gameManager.EnemyCellsChanged += OnEnemyCellsChanged;
        _player.CheckOpponentOnlineEvent += ChangeStatusOpponentOnline;
        _gameManager.CellUpdated += CellUpdated;
        _gameManager.EnemyCellUpdated += EnemyCellUpdated;
        _gameManager.AvailableShipsChanged += OnAvailableShipsChanged;
        PlayerCells = new ObservableCollection<CellViewModel>(ConvertToCellViewModels(_gameManager.PlayerCells));
        EnemyCells = new ObservableCollection<CellViewModel>();
        AvailableShips = new ObservableCollection<Ship>(_gameManager.AvailableShips);

        CurrentTextStateLabel = _chooseShipText;
        HideControls();
        ChangeVisibilityComboBox = Visibility.Visible;
        CurrentCommandCellButton = AddShipCommand;
        CurrentCommandEnemyCellButton = HitShipCommand;
        

    }
    private void ChangeStatusOpponentOnline(object sender, EventArgs e)
    {
        if (_player.IsOpponentConnected)
        {
            OpponentConnectionText = _opponentOnlineText;
        }
        else
        {
            OpponentConnectionText = _opponentOfflineText;
        }
    }

    private void OnPlayerCellsChanged()
    {
        // Обновление ObservableCollection при изменении списка клеток в GameManager
        PlayerCells.Clear();
        foreach (var cell in _gameManager.PlayerCells)
        {
            PlayerCells.Add(ConvertToCellViewModel(cell));
        }
    }
    private void OnEnemyCellsChanged()
    {
        // Обновление ObservableCollection при изменении списка клеток в GameManager
        EnemyCells.Clear();
        foreach (var cell in _gameManager.EnemyCells)
        {
            EnemyCells.Add(ConvertToCellViewModel(cell));
        }
    }
    private void OnAvailableShipsChanged()
    {
        AvailableShips.Clear();
        foreach (var ship in _gameManager.AvailableShips)
        {
            AvailableShips.Add(ship);
        }
    }

    private void CellUpdated(Cell cell)
    {
        // Найти соответствующую клетку в ObservableCollection<CellViewModel>
        var cellViewModel = PlayerCells.FirstOrDefault(c => c.Row == cell.Row && c.Column == cell.Column);
        if (cellViewModel != null)
            cellViewModel.State = cell.State;
    }
    private void EnemyCellUpdated(Cell cell)
    {
        var cellViewModel = EnemyCells.FirstOrDefault(c => c.Row == cell.Row && c.Column == cell.Column);
        if (cellViewModel != null)
            cellViewModel.State = cell.State;
    }

    private CellViewModel ConvertToCellViewModel(Cell cellModel)
    {
        CellViewModel cellViewModel = new CellViewModel(cellModel.Row, cellModel.Column, cellModel.State);
        return cellViewModel;
    }

    private IEnumerable<CellViewModel> ConvertToCellViewModels(IEnumerable<Cell> cellModels)
    {
        var cellViewModels = new List<CellViewModel>();

        foreach (var cellModel in cellModels)
        {
            var cellViewModel = ConvertToCellViewModel(cellModel);
            cellViewModels.Add(cellViewModel);
        }

        return cellViewModels;
    }


    public ObservableCollection<Ship> AvailableShips
    {
        get { return _availableShips; }
        set
        {
            _availableShips = value;
            OnPropertyChange(nameof(AvailableShips));
            
        }
    }

    public Ship SelectedShip
    {
        get { return _selectedShip; }
        set
        {
            _selectedShip = value;
            OnPropertyChange(nameof(SelectedShip));
            CheckShipSize();
        }
    }
    private void CheckShipSize()
    {
        if (SelectedShip != null && SelectedShip.Size != 1)
        {
            CurrentTextStateLabel = _selectedShipText + Environment.NewLine + SelectedShip.Name + Environment.NewLine + _chooseDirectionText;
            HideControls();
            ChangeVisibilityChooseDirectionPanel = Visibility.Visible;
        }
        else if(SelectedShip != null && SelectedShip.Size == 1)
        {
            CurrentTextStateLabel = _selectedShipText + Environment.NewLine + SelectedShip.Name;
            HideControls();
            ChangeVisibilityCancellationButton = Visibility.Visible;
        }
    }

    public Visibility ChangeVisibilityComboBox
    {
        get { return _visibilityComboBox; }
        set
        {
            _visibilityComboBox = value;
            OnPropertyChange(nameof(ChangeVisibilityComboBox));
        }

    }
    public Visibility ChangeVisibilityCancellationButton
    {
        get { return _visibilityCancellationButton; }
        set
        {
            _visibilityCancellationButton = value;
            OnPropertyChange(nameof(ChangeVisibilityCancellationButton));
        }

    }

    public Visibility ChangeVisibilityReadyButton
    {
        get { return _visibilityReadyButton; }
        set
        {
            _visibilityReadyButton = value;
            OnPropertyChange(nameof(ChangeVisibilityReadyButton));
        }

    }

    public Visibility ChangeVisibilityDeleteButton
    {
        get { return _visibilityDeleteButton; }
        set
        {
            _visibilityDeleteButton = value;
            OnPropertyChange(nameof(ChangeVisibilityDeleteButton));
        }

    }
    public Visibility ChangeVisibilityChooseDirectionPanel
    {
        get { return _visibilityChooseDirectionPanel; }
        set
        {
            _visibilityChooseDirectionPanel = value;
            OnPropertyChange(nameof(ChangeVisibilityChooseDirectionPanel));
        }

    }

    public Visibility ChangeVisibilityWaitOpponentText
    {
        get { return _visibilityWaitOpponentText; }
        set
        {
            _visibilityWaitOpponentText = value;
            OnPropertyChange(nameof(ChangeVisibilityWaitOpponentText));
        }
    }

    private ICommand _currentCommandCellButton;
    public ICommand CurrentCommandCellButton
    {
        get { return _currentCommandCellButton; }
        set
        {
            _currentCommandCellButton = value;
            OnPropertyChange(nameof(CurrentCommandCellButton));
        }
    }

    private ICommand _currentCommandEnemyCellButton;
    public ICommand CurrentCommandEnemyCellButton
    {
        get { return _currentCommandEnemyCellButton; }
        set
        {
            _currentCommandEnemyCellButton = value;
            OnPropertyChange(nameof(CurrentCommandEnemyCellButton));
        }
    }

    private ShipDirection _currentDirection;
    public ShipDirection CurrentDirection
    {
        get { return _currentDirection; }
        set
        {
            _currentDirection = value;
            OnPropertyChange(nameof(CurrentDirection));
            CurrentTextStateLabel = _selectedShipText + Environment.NewLine + SelectedShip.Name;
            HideControls();
            ChangeVisibilityCancellationButton = Visibility.Visible;
        }
    }



    private ICommand _setDirectionCommand;
    public ICommand SetDirectionCommand => _setDirectionCommand ??= new RelayCommand(SetDirection);

    private void SetDirection(object direction)
    {
        if (direction is ShipDirection shipDirection)
        {
            CurrentDirection = shipDirection;
            CurrentTextStateLabel += Environment.NewLine + "Положение: ";
            if (shipDirection == ShipDirection.Vertical)
                CurrentTextStateLabel += "вертикально";
            else
                CurrentTextStateLabel += "горизонтально";
        }
    }

    private string _opponentConnectText;
    public string OpponentConnectionText
    {
        get { return _opponentConnectText; }
        set
        {
            _opponentConnectText = value;
            OnPropertyChange(nameof(OpponentConnectionText));
        }
    }

    private string _currentStateText;
    public string CurrentTextStateLabel
    {
        get { return _currentStateText; }
        set
        {
            _currentStateText = value;
            OnPropertyChange(nameof(CurrentTextStateLabel));
        }
    }

    private RelayCommand _cancelleration;
    public RelayCommand CancellationChoiseCommand
    {
        get
        {
            return _cancelleration ?? new RelayCommand(
                _execute => {
                    CancelChoise();
                },
                _canExecute => true
            );
        }
    }

    private void CancelChoise()
    {
        HideControls();
        CurrentTextStateLabel = _chooseShipText;
        ChangeVisibilityComboBox = Visibility.Visible;
        if (AvailableShips.Count != 10)
            ChangeVisibilityDeleteButton = Visibility.Visible;

    }
    public ObservableCollection<CellViewModel> PlayerCells
    {
        get { return _playerCells; }
        set
        {
            _playerCells = value;
            OnPropertyChange(nameof(PlayerCells));
        }
    }

    public ObservableCollection<CellViewModel> EnemyCells
    {
        get { return _enemyCells; }
        set
        {
            _enemyCells = value;
            OnPropertyChange(nameof(EnemyCells));
        }
    }

    private RelayCommand _addShip;
    public RelayCommand AddShipCommand
    {
        get
        {
            return _addShip ?? (_addShip = new RelayCommand(
                _execute => {
                    AddShip(CellClicked((string)_execute));
                    CheckShipsCollection();
                },
                _canExecute => true
            ));
        }
    }

    private RelayCommand _deleteShip;
    public RelayCommand DeleteShipCommand
    {
        get
        {
            return _deleteShip ?? (_deleteShip = new RelayCommand(
                _execute => {
                    CurrentCommandCellButton = DeleteShipFromCellCommand;
                    CurrentTextStateLabel = _chooseShipText;
                    HideControls();
                },
                _canExecute => true
            ));
        }
    }
    private RelayCommand _deleteShipFromCell;
    public RelayCommand DeleteShipFromCellCommand
    {
        get
        {
            return _deleteShipFromCell ?? (_deleteShipFromCell = new RelayCommand(
                _execute => {
                    DeleteShip(CellClicked((string)_execute));
                    CurrentCommandCellButton = AddShipCommand;
                },
                _canExecute => true
            ));
        }
    }

    private RelayCommand _hitShip;
    public RelayCommand HitShipCommand
    {
        get
        {
            return _hitShip ?? (_hitShip = new RelayCommand(
                _execute => {
                    HitCell(EnemyCellClicked((string)_execute));
                    //CheckShipsCollection();
                },
                _canExecute => true
            ));
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
        HideControls();
        ChangeVisibilityWaitOpponentText = Visibility.Visible;
        CurrentCommandCellButton = null;
        SendRequest(_startGameRequest);
    }
    private async Task SendRequest(string request)
    {
        try
        {
            request = _requestParser.Parse(request);
            Response response = await _player.SendRequestWithResponseAsync(request);

            switch (response.Type)
            {
                case RequestType.StartGame:
                    if (response.Flag)
                    {
                        _gameManager.GenerateEnemyField();
                        EnemyCells = new ObservableCollection<CellViewModel>(ConvertToCellViewModels(_gameManager.EnemyCells));
                        HideControls();
                        CurrentTextStateLabel = _yourTurnText;
                        MessageBox_Show(null, _manual, "Старт игры", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        HideControls();
                        _gameManager.GenerateEnemyField();
                        MessageBox_Show(null, _manual, "Старт игры", MessageBoxButton.OK, MessageBoxImage.Information);
                        await OpponentMove();
                        
                    }
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

    private CellViewModel CellClicked(object parameter)
    {
        string cellPosition = (string)parameter;

        // Разбиваем строку позиции клетки для получения строки и столбца
        string[] parts = cellPosition.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        int row = int.Parse(parts[0].Trim().Substring(4));
        int col = int.Parse(parts[1].Trim().Substring(7));

        // Получение соответствующей клетки из массива клеток
        CellViewModel cell = PlayerCells.Single(c => c.Row == row && c.Column == col);
        return cell;
    }
    private CellViewModel EnemyCellClicked(object parameter)
    {
        string cellPosition = (string)parameter;

        // Разбиваем строку позиции клетки для получения строки и столбца
        string[] parts = cellPosition.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        int row = int.Parse(parts[0].Trim().Substring(4));
        int col = int.Parse(parts[1].Trim().Substring(7));

        // Получение соответствующей клетки из массива клеток
        CellViewModel cell = EnemyCells.Single(c => c.Row == row && c.Column == col);
        return cell;
    }
    
    private async Task OpponentMove()
    {
        bool flag = false;
        CurrentCommandEnemyCellButton = null;
        CurrentTextStateLabel = _opponentTurnText;
        while(flag == false)
        {
           flag = await _gameManager.IsOpponentMove();
        }
        if (_gameManager.OpponentScore != _gameManager.MaxNumberOfPoints)
        {
            CurrentTextStateLabel = _yourTurnText;
            CurrentCommandEnemyCellButton = HitShipCommand;
        }
        else if (_gameManager.OpponentScore == _gameManager.MaxNumberOfPoints)
        {
            CurrentCommandEnemyCellButton = null;
            CurrentTextStateLabel = "Вы проиграли!";
            MessageBox_Show(null, "You lose!", "", MessageBoxButton.OK, MessageBoxImage.Information);
            
        }
    }
    private void WaitingRespone()
    {
        CurrentCommandEnemyCellButton = null;
        CurrentTextStateLabel = "Ждем ответ...";
    }
    private void ResponseRecieved()
    {
        CurrentTextStateLabel = _yourTurnText;
        CurrentCommandEnemyCellButton = HitShipCommand;
    }
    private async Task HitCell(CellViewModel cell)
    {
        if (cell.State == CellState.Empty)
        {
            WaitingRespone();
            HitState hitState = await _gameManager.HitCell(cell.Row, cell.Column);
            ResponseRecieved();
            if (_gameManager.PlayerScore == _gameManager.MaxNumberOfPoints)
            {
                CurrentCommandEnemyCellButton = null;
                CurrentTextStateLabel = "Вы победили!";
                MessageBox_Show(null, "You win!", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            if (hitState == HitState.Miss)
            {
                //await _player.SendRequestAsync(_requestParser.Parse(_endOfTurnRequest));
                await OpponentMove();
            }
        }
        else
        {
            MessageBox_Show(null, "Вы уже делали встрел по этой клетке!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
    private void AddShip(CellViewModel cell)
    {
        try
        {
            if (SelectedShip == null)
                throw new Exception("Выберите тип корабля!");
            ShipDirection direction = CurrentDirection;
            _gameManager.AddShipToCells(cell.Row, cell.Column, SelectedShip, direction);

            CurrentTextStateLabel = _chooseShipText;

            HideControls();
            ChangeVisibilityComboBox = Visibility.Visible;

            if(AvailableShips.Count != 10)
                ChangeVisibilityDeleteButton = Visibility.Visible;
        }
        catch (Exception ex)
        {
            MessageBox_Show(null, ex.Message, "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DeleteShip(CellViewModel cell)
    {
        try
        {
            if (cell.State == CellState.Empty)
                throw new Exception("Выберите корабль!");

            ShipDirection direction = CurrentDirection;
            _gameManager.DeleteShipFromCells(cell.Row, cell.Column);

            CurrentTextStateLabel = _chooseShipText;

            HideControls();
            ChangeVisibilityComboBox = Visibility.Visible;

            if (AvailableShips.Count != 10)
                ChangeVisibilityDeleteButton = Visibility.Visible;
        }
        catch (Exception ex)
        {
            MessageBox_Show(null, ex.Message, "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CheckShipsCollection()
    {
        if (AvailableShips.Count == 0)
        {
            CurrentTextStateLabel = _shipsAreOverText;
            HideControls();
            ChangeVisibilityReadyButton = Visibility.Visible;
            ChangeVisibilityDeleteButton = Visibility.Visible;
        }
    }

    private void HideControls()
    {
        ChangeVisibilityCancellationButton = Visibility.Collapsed;
        ChangeVisibilityChooseDirectionPanel = Visibility.Collapsed;
        ChangeVisibilityComboBox = Visibility.Collapsed;
        ChangeVisibilityDeleteButton = Visibility.Collapsed;
        ChangeVisibilityReadyButton = Visibility.Collapsed;
        ChangeVisibilityWaitOpponentText = Visibility.Collapsed;
    }

}
