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

    private string _chooseShipText = "Выберите корабль:";
    private string _selectedShipText = "Выбран корабль: ";
    private string _chooseDirectionText = "Выберите положение:";
    private string _shipsAreOverText = "Все корабли расставлены!";

    private Visibility _visibilityComboBox;
    private Visibility _visibilityCancellationButton;
    private Visibility _visibilityReadyButton;
    private Visibility _visibilityChooseDirectionPanel;
    public GameViewModel()
    {
        PlayerCells = GenerateCells();
        EnemyCells = GenerateCells();
        ChoiseShipText = _chooseShipText;
        ChangeVisibilityComboBox = Visibility.Visible;
        ChangeVisibilityCancellationButton = Visibility.Collapsed;
        ChangeVisibilityReadyButton = Visibility.Collapsed;
        ChangeVisibilityChooseDirectionPanel = Visibility.Collapsed;

        AvailableShips = new ObservableCollection<Ship>
        {
            new Ship("Торпедный катер", 1),
            new Ship("Торпедный катер", 1),
            new Ship("Торпедный катер", 1),
            new Ship("Торпедный катер", 1),
            new Ship("Эсминец", 2),
            new Ship("Эсминец", 2),
            new Ship("Эсминец", 2),
            new Ship("Крейсер", 3),
            new Ship("Крейсер", 3),
            new Ship("Линкор", 4)
        };
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
            ChoiseShipText = _selectedShipText + Environment.NewLine + SelectedShip.Name + Environment.NewLine + _chooseDirectionText;
            ChangeVisibilityComboBox = Visibility.Collapsed;
            ChangeVisibilityCancellationButton = Visibility.Collapsed;
            ChangeVisibilityChooseDirectionPanel = Visibility.Visible;
            ChangeVisibilityReadyButton = Visibility.Collapsed;
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
    public Visibility ChangeVisibilityChooseDirectionPanel
    {
        get { return _visibilityChooseDirectionPanel; }
        set
        {
            _visibilityChooseDirectionPanel = value;
            OnPropertyChange(nameof(ChangeVisibilityChooseDirectionPanel));
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

    // CancellationChoiseCommand
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

    //в класс Game
    private ObservableCollection<CellViewModel> GenerateCells()
    {
        var cells = new ObservableCollection<CellViewModel>();

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                var cell = new CellViewModel(i, j);
                cells.Add(cell);
            }
        }

        return cells;
    }

    private void AddShip(CellViewModel cell)
    {
        try
        {
            if (SelectedShip == null)
                throw new Exception("Выберите тип корабля!");
            int shipSize = (int)SelectedShip.Size;
            ShipDirection direction = CurrentDirection;

            // Проверить, есть ли достаточно свободных клеток для размещения корабля в указанном направлении
            if (!CanPlaceShip(cell, shipSize, direction))
                throw new Exception("Здесь нельзя разместить корабль данного типа!");
            // Разместить корабль на игровом поле
            PlaceShip(cell, shipSize, direction);
            AvailableShips.Remove(SelectedShip);
            ChoiseShipText = _chooseShipText;
            ChangeVisibilityComboBox = Visibility.Visible;
            ChangeVisibilityCancellationButton = Visibility.Collapsed;
            ChangeVisibilityChooseDirectionPanel = Visibility.Collapsed;
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
        }
    }

    //в класс Game
    private bool CanPlaceShip(CellViewModel startCell, int shipSize, ShipDirection direction)
    {
        int row = startCell.Row;
        int column = startCell.Column;

        if(shipSize == 1)
        {
            if(!CheckRightAndLeftBorders(row, column, shipSize) || 
                !CheckUpperAndLowerBorders(row, column, shipSize))
                return false;
        }
        else if (direction == ShipDirection.Horizontal)
        {
            if (CheckRightAndLeftBorders(row, column, shipSize))
                for (int i = 0; i < shipSize; i++)
                {
                    CellViewModel cell = FindCell(row, column + i);
                    if (!CheckCell(cell) || !CheckLowerAndUpperCell(cell))
                        return false;
                }
            else
                return false;
        }
        else if (direction == ShipDirection.Vertical)
        {
            if (CheckUpperAndLowerBorders(row, column, shipSize))
                for (int i = 0; i < shipSize; i++)
                {
                    CellViewModel cell = FindCell(row + i, column);
                    if (!CheckCell(cell) || !CheckRightAndLeftCell(cell))
                        return false;
                }
            else
                return false;
        }
        return true;
    }

    private bool CheckRightAndLeftBorders(int row, int column, int shipSize)
    {
        if (column + shipSize > 10)
            return false;

        CellViewModel leftCell = FindCell(row, column - 1);
        if (!CheckCell(leftCell))
            return false;
        else if (!CheckLowerAndUpperCell(leftCell))
            return false;


        CellViewModel rightCell = FindCell(row, column + shipSize);
        if (!CheckCell(leftCell))
            return false;
        else if (!CheckLowerAndUpperCell(rightCell))
            return false;
        return true;
    }

    private bool CheckUpperAndLowerBorders(int row, int column, int shipSize)
    {
        if (row + shipSize > 10)
            return false;

        CellViewModel lowerCell = FindCell(row - 1, column);
        if (!CheckCell(lowerCell))
            return false;
        else if (!CheckRightAndLeftCell(lowerCell))
            return false;

        CellViewModel rightCell = FindCell(row + shipSize, column);
        if (!CheckCell(rightCell))
            return false;
        else if (!CheckRightAndLeftCell(rightCell))
            return false;
        return true;
    }
    private bool CheckLowerAndUpperCell(CellViewModel cell)
    {
        if (cell == null)
            return true;
        if (!CheckCell(FindCell(cell.Row + 1, cell.Column)) || 
            !CheckCell(FindCell(cell.Row - 1, cell.Column)))
            return false;
        else
            return true;
    }

    private bool CheckRightAndLeftCell(CellViewModel cell)
    {
        if (cell == null)
            return true;
        if (!CheckCell(FindCell(cell.Row, cell.Column + 1)) ||
            !CheckCell(FindCell(cell.Row, cell.Column - 1)))
            return false;
        else
            return true;
    }

    private bool CheckCell(CellViewModel cell)
    {
        if (cell == null || cell.State == CellState.Empty)
            return true;
        else
            return false;
    }

    private CellViewModel FindCell(int row, int col)
    {
        bool cellExists = true;
        if (row < 0 || col < 0)
            cellExists = false;
        if (cellExists == false)
            return null;

        CellViewModel cell = PlayerCells.FirstOrDefault(c => c.Row == row && c.Column == col);
        return cell;
    }
    private void PlaceShip(CellViewModel startCell, int shipSize, ShipDirection direction)
    {
        int row = startCell.Row;
        int column = startCell.Column;

        if (direction == ShipDirection.Horizontal)
        {
            for (int i = 0; i < shipSize; i++)
            {
                var cell = PlayerCells.FirstOrDefault(c => c.Row == row && c.Column == column + i);
                if (cell != null)
                    cell.State = CellState.Ship;
            }
        }
        else // ShipDirection.Vertical
        {
            for (int i = 0; i < shipSize; i++)
            {
                var cell = PlayerCells.FirstOrDefault(c => c.Row == row + i && c.Column == column);
                if (cell != null)
                    cell.State = CellState.Ship;
            }
        }
    }
}
