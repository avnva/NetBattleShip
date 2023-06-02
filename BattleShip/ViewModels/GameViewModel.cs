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

    private string _chooseShipText = "Выберите корабль:";
    private string _selectedShipText = "Выбран корабль: ";
    private string _chooseDirectionText = "Выберите положение:";
    private string _shipsAreOverText = "Все корабли расставлены!";
    

    private Visibility _visibilityComboBox;
    private Visibility _visibilityCancellationButton;
    private Visibility _visibilityReadyButton;
    private Visibility _visibilityChooseDirectionPanel;
    private Visibility _visibilityDeleteButton;
    public GameViewModel()
    {
        _gameManager = new GameManager();
        _gameManager.PlayerCellsChanged += OnPlayerCellsChanged;
        _gameManager.EnemyCellsChanged += OnEnemyCellsChanged;
        _gameManager.CellUpdated += CellUpdated;
        PlayerCells = new ObservableCollection<CellViewModel>(ConvertToCellViewModels(_gameManager.PlayerCells));

        ChoiseShipText = _chooseShipText;
        ChangeVisibilityComboBox = Visibility.Visible;
        ChangeVisibilityCancellationButton = Visibility.Collapsed;
        ChangeVisibilityReadyButton = Visibility.Collapsed;
        ChangeVisibilityChooseDirectionPanel = Visibility.Collapsed;
        ChangeVisibilityDeleteButton = Visibility.Collapsed;
        CurrentCommandCellButton = AddShipCommand;

        AvailableShips = new ObservableCollection<Ship>
        {
            new Ship(1),
            new Ship(1),
            new Ship(1),
            new Ship(1),
            new Ship(2),
            new Ship(2),
            new Ship(2),
            new Ship(3),
            new Ship(3),
            new Ship(4)
        };

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

    private void CellUpdated(Cell cell)
    {
        // Найти соответствующую клетку в ObservableCollection<CellViewModel>
        var cellViewModel = PlayerCells.FirstOrDefault(c => c.Row == cell.Row && c.Column == cell.Column);
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
            if(SelectedShip.Size != 1)
            {
                ChoiseShipText = _selectedShipText + Environment.NewLine + SelectedShip.Name + Environment.NewLine + _chooseDirectionText;
                ChangeVisibilityComboBox = Visibility.Collapsed;
                ChangeVisibilityCancellationButton = Visibility.Collapsed;
                ChangeVisibilityChooseDirectionPanel = Visibility.Visible;
                ChangeVisibilityReadyButton = Visibility.Collapsed;
                ChangeVisibilityDeleteButton = Visibility.Collapsed;
            }
            
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

    private ShipDirection _currentDirection;
    public ShipDirection CurrentDirection
    {
        get { return _currentDirection; }
        set
        {
            _currentDirection = value;
            OnPropertyChange(nameof(CurrentDirection));
            ChoiseShipText = _selectedShipText + Environment.NewLine + SelectedShip.Name;
            ChangeVisibilityComboBox = Visibility.Collapsed;
            ChangeVisibilityCancellationButton = Visibility.Visible;
            ChangeVisibilityChooseDirectionPanel = Visibility.Collapsed;
            ChangeVisibilityDeleteButton = Visibility.Collapsed;
        }
    }

    private ICommand _setDirectionCommand;
    public ICommand SetDirectionCommand => _setDirectionCommand ??= new RelayCommand(SetDirection);

    private void SetDirection(object direction)
    {
        if (direction is ShipDirection shipDirection)
        {
            CurrentDirection = shipDirection;
            ChoiseShipText += Environment.NewLine + "Положение: ";
            if (shipDirection == ShipDirection.Vertical)
                ChoiseShipText += "вертикально";
            else
                ChoiseShipText += "горизонтально";
        }
    }

    private string _choiseShipText;
    public string ChoiseShipText
    {
        get { return _choiseShipText; }
        set
        {
            _choiseShipText = value;
            OnPropertyChange(nameof(ChoiseShipText));
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
        ChangeVisibilityCancellationButton = Visibility.Collapsed;
        ChoiseShipText = _chooseShipText;
        ChangeVisibilityComboBox = Visibility.Visible;
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
                    ChoiseShipText = _chooseShipText;
                    ChangeVisibilityDeleteButton = Visibility.Collapsed;
                    ChangeVisibilityComboBox = Visibility.Collapsed;

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


    private void AddShip(CellViewModel cell)
    {
        try
        {
            if (SelectedShip == null)
                throw new Exception("Выберите тип корабля!");
            ShipDirection direction = CurrentDirection;
            _gameManager.AddShipToCells(cell.Row, cell.Column, SelectedShip, direction);

            AvailableShips.Remove(SelectedShip);
            ChoiseShipText = _chooseShipText;
            ChangeVisibilityComboBox = Visibility.Visible;
            ChangeVisibilityCancellationButton = Visibility.Collapsed;
            ChangeVisibilityChooseDirectionPanel = Visibility.Collapsed;

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
            Ship deletedShip = _gameManager.DeleteShipFromCells(cell.Row, cell.Column);

            AvailableShips.Add(deletedShip);
            ChoiseShipText = _chooseShipText;
            ChangeVisibilityComboBox = Visibility.Visible;
            ChangeVisibilityCancellationButton = Visibility.Collapsed;
            ChangeVisibilityChooseDirectionPanel = Visibility.Collapsed;

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
            ChoiseShipText = _shipsAreOverText;
            ChangeVisibilityComboBox = Visibility.Collapsed;
            ChangeVisibilityCancellationButton = Visibility.Collapsed;
            ChangeVisibilityChooseDirectionPanel = Visibility.Collapsed;
            ChangeVisibilityReadyButton = Visibility.Visible;
            ChangeVisibilityDeleteButton = Visibility.Visible;
        }
    }

}
