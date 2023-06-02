using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit;

namespace BattleShip;

public class GameManager
{
    private List<Cell> _playerCells;
    private List<Cell> _enemyCells;
    private Cell _cell;

    public event Action PlayerCellsChanged;
    public event Action EnemyCellsChanged;
    public delegate void CellUpdatedEventHandler(Cell cell);
    public event CellUpdatedEventHandler CellUpdated;

    public List<Cell> PlayerCells
    {
        get { return _playerCells; }
        set
        {
            _playerCells = value;
            OnPlayerCellsChanged();
        }
    }
    public List<Cell> EnemyCells
    {
        get { return _enemyCells; }
        set
        {
            _enemyCells = value;
            OnEnemyCellsChanged();
        }
    }
    public Cell Cell
    {
        get { return _cell; }
        set
        {
            _cell = value;
            UpdateCell(_cell);
        }
    }

    public GameManager()
    {
        // Инициализация списка клеток
        PlayerCells = GenerateCells();
    }

    private List<Cell> GenerateCells()
    {
        var cells = new List<Cell>();

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                var cell = new Cell(i, j);
                cells.Add(cell);
            }
        }

        return cells;
    }

    protected virtual void OnPlayerCellsChanged()
    {
        PlayerCellsChanged?.Invoke();
    }
    protected virtual void OnEnemyCellsChanged()
    {
        EnemyCellsChanged?.Invoke();
    }


    private void UpdateCell(Cell cell)
    {
        CellUpdated?.Invoke(cell);
    }

    public void AddShipToCells(int row, int column, Ship ship, ShipDirection direction)
    {
        Cell cell = _playerCells.FirstOrDefault(c => c.Row == row && c.Column == column);
        int shipSize = (int)ship.Size;
        if (!CanPlaceShip(cell, shipSize, direction))
            throw new Exception("Здесь нельзя разместить корабль данного типа!");
        // Разместить корабль на игровом поле
        PlaceShip(cell, shipSize, direction);
    }

    private bool CanPlaceShip(Cell startCell, int shipSize, ShipDirection direction)
    {
        int row = startCell.Row;
        int column = startCell.Column;

        if (shipSize == 1)
        {
            if (!CheckRightAndLeftBorders(row, column, shipSize) ||
                !CheckUpperAndLowerBorders(row, column, shipSize))
                return false;
        }
        else if (direction == ShipDirection.Horizontal)
        {
            if (CheckRightAndLeftBorders(row, column, shipSize))
                for (int i = 0; i < shipSize; i++)
                {
                    Cell cell = FindCell(row, column + i);
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
                    Cell cell = FindCell(row + i, column);
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

        Cell leftCell = FindCell(row, column - 1);
        if (!CheckCell(leftCell))
            return false;
        else if (!CheckLowerAndUpperCell(leftCell))
            return false;


        Cell rightCell = FindCell(row, column + shipSize);
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

        Cell lowerCell = FindCell(row - 1, column);
        if (!CheckCell(lowerCell))
            return false;
        else if (!CheckRightAndLeftCell(lowerCell))
            return false;

        Cell rightCell = FindCell(row + shipSize, column);
        if (!CheckCell(rightCell))
            return false;
        else if (!CheckRightAndLeftCell(rightCell))
            return false;
        return true;
    }
    private bool CheckLowerAndUpperCell(Cell cell)
    {
        if (cell == null)
            return true;
        if (!CheckCell(FindCell(cell.Row + 1, cell.Column)) ||
            !CheckCell(FindCell(cell.Row - 1, cell.Column)))
            return false;
        else
            return true;
    }

    private bool CheckRightAndLeftCell(Cell cell)
    {
        if (cell == null)
            return true;
        if (!CheckCell(FindCell(cell.Row, cell.Column + 1)) ||
            !CheckCell(FindCell(cell.Row, cell.Column - 1)))
            return false;
        else
            return true;
    }

    private bool CheckCell(Cell cell)
    {
        if (cell == null || cell.State == CellState.Empty)
            return true;
        else
            return false;
    }

    private Cell FindCell(int row, int col)
    {
        bool cellExists = true;
        if (row < 0 || col < 0)
            cellExists = false;
        if (cellExists == false)
            return null;

        Cell cell = PlayerCells.FirstOrDefault(c => c.Row == row && c.Column == col);
        return cell;
    }
    private void PlaceShip(Cell startCell, int shipSize, ShipDirection direction)
    {
        int row = startCell.Row;
        int column = startCell.Column;

        if (direction == ShipDirection.Horizontal)
        {
            for (int i = 0; i < shipSize; i++)
            {
                var cell = PlayerCells.FirstOrDefault(c => c.Row == row && c.Column == column + i);
                cell.State = CellState.Ship;
                UpdateCell(cell);
            }
        }
        else
        {
            for (int i = 0; i < shipSize; i++)
            {
                var cell = PlayerCells.FirstOrDefault(c => c.Row == row + i && c.Column == column);
                cell.State = CellState.Ship;
                UpdateCell(cell);
            }
        }
    }

    public Ship DeleteShipFromCells(int row, int column)
    {
        Cell cell = _playerCells.FirstOrDefault(c => c.Row == row && c.Column == column);
        ShipDirection shipDirection = DeterminingDirection(cell);
        Cell firstCell = FindFirstCell(cell, shipDirection);
        int shipSize = DeterminingShipSize(firstCell, shipDirection);
        Ship ship = new Ship(shipSize);
        // Разместить корабль на игровом поле
        DeleteShip(firstCell, shipSize, shipDirection);
        return ship;
    }

    private void DeleteShip(Cell startCell, int shipSize, ShipDirection direction)
    {
        int row = startCell.Row;
        int column = startCell.Column;

        if (direction == ShipDirection.Horizontal)
        {
            for (int i = 0; i < shipSize; i++)
            {
                var cell = PlayerCells.FirstOrDefault(c => c.Row == row && c.Column == column + i);
                cell.State = CellState.Empty;
                UpdateCell(cell);
            }
        }
        else
        {
            for (int i = 0; i < shipSize; i++)
            {
                var cell = PlayerCells.FirstOrDefault(c => c.Row == row + i && c.Column == column);
                cell.State = CellState.Empty;
                UpdateCell(cell);
            }
        }
    }

    private Cell FindFirstCell(Cell cell, ShipDirection shipDirection)
    {
        Cell firstCell = new Cell(cell.Row,cell.Column);
        if (shipDirection == ShipDirection.Horizontal)
        {
            for (int i = 1; i < 4; i++)
            {
                if (CheckShipCell(FindCell(cell.Row, cell.Column - i)))
                    firstCell = FindCell(cell.Row, cell.Column - i);
                else break;
            }
        }
        else
        {
            for (int i = 1; i < 4; i++)
            {
                if (CheckShipCell(FindCell(cell.Row - i, cell.Column)))
                    firstCell = FindCell(cell.Row - i, cell.Column);
                else break;
            }
        }
        return firstCell;
    }

    private int DeterminingShipSize(Cell startCell, ShipDirection direction)
    {
        int shipSize = 1;
        if (direction == ShipDirection.Horizontal)
        {
            for (int i = 1; i < 4; i++)
            {
                if (CheckShipCell(FindCell(startCell.Row, startCell.Column + i)))
                    shipSize++;
                else break;
            }
        }
        else
        {
            for (int i = 1; i < 4; i++)
            {
                if (CheckShipCell(FindCell(startCell.Row + i, startCell.Column)))
                    shipSize++;
                else break;
            }
        }
        return shipSize;
    }

    private ShipDirection DeterminingDirection(Cell startCell)
    {
        if (!CheckLowerAndUpperShipCell(startCell))
            if (CheckRightAndLeftShipCell(startCell))
                return ShipDirection.Horizontal;
        return ShipDirection.Vertical;
    }
    private bool CheckShipCell(Cell cell)
    {
        if (cell != null && cell.State == CellState.Ship)
            return true;
        else
            return false;
    }
    private bool CheckLowerAndUpperShipCell(Cell cell)
    {
        if (CheckShipCell(FindCell(cell.Row + 1, cell.Column)) ||
            CheckShipCell(FindCell(cell.Row - 1, cell.Column)))
            return true;
        else
            return false;
    }
    private bool CheckRightAndLeftShipCell(Cell cell)
    {
        if (CheckShipCell(FindCell(cell.Row, cell.Column + 1)) ||
            CheckShipCell(FindCell(cell.Row, cell.Column - 1)))
            return true;
        else
            return false;
    }
}
