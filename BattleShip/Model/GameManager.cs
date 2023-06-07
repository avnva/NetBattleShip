using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Wpf.Toolkit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BattleShip.Model;
public enum HitState
{
    Kill,
    Hit,
    Miss
}
public class GameManager
{
    private List<Cell> _playerCells;
    private List<Cell> _enemyCells;
    private List<Ship> _availableShips;
    private Cell _cell;
    private Player _player;
    private RequestParser _requestParser;
    private string _lastRequest;
    private bool _requestSuccess;
    public int PlayerScore;
    public int OpponentScore;
    public int MaxNumberOfPoints;

    public event Action LastRequestChanged;
    public event Action RequestSuccessedChanged;
    public event Action PlayerCellsChanged;
    public event Action EnemyCellsChanged;
    public event Action AvailableShipsChanged;
    public delegate void CellUpdatedEventHandler(Cell cell);
    public event CellUpdatedEventHandler CellUpdated;
    public delegate void EnemyCellUpdatedEventHandler(Cell cell);
    public event EnemyCellUpdatedEventHandler EnemyCellUpdated;
    public delegate void AvailableShipsUpdatedEventHandler(Ship ship);

    private string _hitsCellRequest = "Hits cell:";
    private string _waitCellForCheckRequest = "Wait opponent...";
    private string _cellStateRequest = "Cell state:";

    public GameManager(Player player)
    {
        // Инициализация списка клеток игрока
        _requestParser = new RequestParser();
        _player = player;
        PlayerCells = GenerateCells();
        AvailableShips = GenerateShips();
        MaxNumberOfPoints = AvailableShips.Count;
    }

    private List<Ship> GenerateShips()
    {
        var AvailableShips = new List<Ship>
        {
            new Ship(3),
            new Ship(1)
            //new Ship(1),
            //new Ship(1),
            //new Ship(1),
            //new Ship(2),
            //new Ship(2),
            //new Ship(2),
            //new Ship(3),
            //new Ship(3),
            //new Ship(4)
        };
        return AvailableShips;
    }
    public void GenerateEnemyField()
    {
        EnemyCells = GenerateCells();
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
    public List<Ship> AvailableShips
    {
        get { return _availableShips; }
        set
        {
            _availableShips = value;
            OnAvailableShipsChanged();
        }
    }
    public string LastRequest
    {
        get { return _lastRequest; }
        set
        {
            _lastRequest = value;
            OnLastRequestChanged();
        }
    }
    public bool RequestSuccess
    {
        get { return _requestSuccess; }
        set
        {
            _requestSuccess = value;
            OnRequestSuccessedChanged();
        }
    }

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
    protected virtual void OnLastRequestChanged()
    {
        LastRequestChanged?.Invoke();
    }
    protected virtual void OnRequestSuccessedChanged()
    {
        RequestSuccessedChanged?.Invoke();
    }
    protected virtual void OnPlayerCellsChanged()
    {
        PlayerCellsChanged?.Invoke();
    }
    protected virtual void OnEnemyCellsChanged()
    {
        EnemyCellsChanged?.Invoke();
    }
    protected virtual void OnAvailableShipsChanged()
    {
        AvailableShipsChanged?.Invoke();
    }

    private void UpdateCell(Cell cell)
    {
        CellUpdated?.Invoke(cell);
    }
    private void UpdateEnemyCell(Cell cell)
    {
        EnemyCellUpdated?.Invoke(cell);
    }

    public void AddShipToCells(int row, int column, Ship ship, ShipDirection direction)
    {
        Cell cell = _playerCells.FirstOrDefault(c => c.Row == row && c.Column == column);
        int shipSize = ship.Size;

        if (!CanPlaceShip(cell, shipSize, direction))
            throw new Exception("Здесь нельзя разместить корабль данного типа!");

        PlaceShip(cell, shipSize, direction);
        AvailableShips.Remove(ship);
        OnAvailableShipsChanged();
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
                    Cell cell = FindCell(row, column + i, "Player");
                    if (!CheckCell(cell, CellState.Empty) || !CheckLowerAndUpperCell(cell, CellState.Empty, "Player"))
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
                    Cell cell = FindCell(row + i, column, "Player");
                    if (!CheckCell(cell, CellState.Empty) || !CheckRightAndLeftCell(cell, CellState.Empty, "Player"))
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

        Cell leftCell = FindCell(row, column - 1, "Player");
        if (!CheckCell(leftCell, CellState.Empty))
            return false;
        else if (!CheckLowerAndUpperCell(leftCell, CellState.Empty, "Player"))
            return false;


        Cell rightCell = FindCell(row, column + shipSize, "Player");
        if (!CheckCell(leftCell, CellState.Empty))
            return false;
        else if (!CheckLowerAndUpperCell(rightCell, CellState.Empty, "Player"))
            return false;
        return true;
    }

    private bool CheckUpperAndLowerBorders(int row, int column, int shipSize)
    {
        if (row + shipSize > 10)
            return false;

        Cell lowerCell = FindCell(row - 1, column, "Player");
        if (!CheckCell(lowerCell, CellState.Empty))
            return false;
        else if (!CheckRightAndLeftCell(lowerCell, CellState.Empty, "Player"))
            return false;

        Cell rightCell = FindCell(row + shipSize, column, "Player");
        if (!CheckCell(rightCell, CellState.Empty))
            return false;
        else if (!CheckRightAndLeftCell(rightCell, CellState.Empty, "Player"))
            return false;
        return true;
    }
    private bool CheckLowerAndUpperCell(Cell cell, CellState state, string field)
    {
        if(state == CellState.Empty)
        {
            if (cell == null)
                return true;
            if (CheckCell(FindCell(cell.Row + 1, cell.Column, field), state) &&
                CheckCell(FindCell(cell.Row - 1, cell.Column, field), state))
                return true;
        }
        else if (state == CellState.Ship || state == CellState.Hit)
        {
            if (CheckCell(FindCell(cell.Row + 1, cell.Column, field), state) ||
            CheckCell(FindCell(cell.Row - 1, cell.Column, field), state))
                return true;  
        }
        return false;

    }
    private bool CheckRightAndLeftCell(Cell cell, CellState state, string field)
    {
        if(state == CellState.Empty)
        {
            if (cell == null)
                return true;
            if (CheckCell(FindCell(cell.Row, cell.Column + 1, field), state) &&
                CheckCell(FindCell(cell.Row, cell.Column - 1, field), state))
                return true;
        }
        else if (state == CellState.Ship || state == CellState.Hit)
        {
            if (CheckCell(FindCell(cell.Row, cell.Column + 1, field), state) ||
            CheckCell(FindCell(cell.Row, cell.Column - 1, field), state))
                return true;
        }
        return false;
    }

    private bool CheckCell(Cell cell, CellState state)
    {
        if (state == CellState.Empty)
        {
            if (cell == null || cell.State == CellState.Empty)
                return true;
        }
        else if (state == CellState.Ship)
        {
            if (cell != null && cell.State == CellState.Ship)
                return true;
        }
        else if (state == CellState.Hit)
        {
            if (cell != null && (cell.State == CellState.Ship || cell.State == CellState.Hit))
                return true;
        }
        else if (state == CellState.Miss)
        {
            if (cell != null)
                return true;
        }
        return false;
        
    }
    private Cell FindCell(int row, int col, string field)
    {
        Cell cell;
        bool cellExists = true;
        if (row < 0 || col < 0)
            cellExists = false;
        if (cellExists == false)
            return null;
        if(field == "Player")
            cell = PlayerCells.FirstOrDefault(c => c.Row == row && c.Column == col);
        else
            cell = EnemyCells.FirstOrDefault(c => c.Row == row && c.Column == col);
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

    public void DeleteShipFromCells(int row, int column)
    {
        Cell cell = _playerCells.FirstOrDefault(c => c.Row == row && c.Column == column);
        ShipDirection shipDirection = DeterminingDirection(cell, CellState.Ship, "Player");
        Cell firstCell = FindFirstCell(cell, shipDirection, CellState.Ship, "Player");
        int shipSize = DeterminingShipSize(firstCell, shipDirection, CellState.Ship);
        Ship ship = new Ship(shipSize);
        // Разместить корабль на игровом поле
        DeleteShip(firstCell, shipSize, shipDirection);
        AvailableShips.Add(ship);
        OnAvailableShipsChanged();
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

    private Cell FindFirstCell(Cell cell, ShipDirection shipDirection, CellState state, string field)
    {
        Cell firstCell = new Cell(cell.Row, cell.Column);
        firstCell.State = state;
        if (shipDirection == ShipDirection.Horizontal)
        {
            for (int i = 1; i < 4; i++)
            {
                if (CheckCell(FindCell(cell.Row, cell.Column - i, field), state))
                    firstCell = FindCell(cell.Row, cell.Column - i, field);
                else break;
            }
        }
        else
        {
            for (int i = 1; i < 4; i++)
            {
                if (CheckCell(FindCell(cell.Row - i, cell.Column, field),  state))
                    firstCell = FindCell(cell.Row - i, cell.Column, field);
                else break;
            }
        }
        return firstCell;
    }

    private int DeterminingShipSize(Cell startCell, ShipDirection direction, CellState state)
    {
        int shipSize = 0;
        string field = "Player";
        if (direction == ShipDirection.Horizontal)
        {
            for (int i = 0; i < 4; i++)
            {
                if (CheckCell(FindCell(startCell.Row, startCell.Column + i, field), state))
                {
                    shipSize++;
                    if (state == CellState.Hit)
                        if(FindCell(startCell.Row, startCell.Column + i, field).State == CellState.Hit)
                            shipSize--;
                }
                    
                else break;
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                if (CheckCell(FindCell(startCell.Row + i, startCell.Column, field), state))
                {
                    shipSize++;
                    if (state == CellState.Hit)
                        if (FindCell(startCell.Row + i, startCell.Column, field).State == CellState.Hit)
                            shipSize--;
                }
                else break;
            }
        }
        return shipSize;
    }

    private ShipDirection DeterminingDirection(Cell startCell, CellState state, string field)
    {
        if (!CheckLowerAndUpperCell(startCell, state, field))
            if (CheckRightAndLeftCell(startCell, state, field))
                return ShipDirection.Horizontal;
        return ShipDirection.Vertical;
    }

    private int DeterminingKilledShipSize(Cell cell, ShipDirection direction)
    {
        int firstRow = cell.Row;
        int firstColumn = cell.Column;
        Cell newCell = cell;
        int shipSize = 1;
        bool flag = true;
        if (direction == ShipDirection.Horizontal)
        {
            int j = firstColumn;
            while (flag)
            {
                newCell = FindCell(firstRow, j + 1, "Enemy");
                if (CheckCell(newCell, CellState.Hit))
                {
                    shipSize++;
                    j++;
                }
                else
                    flag = false;
            }
        }
        else if(direction == ShipDirection.Vertical)
        {
            int j = firstRow;
            while (flag)
            {
                newCell = FindCell(j + 1, firstColumn, "Enemy");
                if (CheckCell(newCell, CellState.Hit))
                {
                    shipSize++;
                    j++;
                }
                else
                    flag = false;
            }
        }
        return shipSize;
    }
    private List<Cell> cellsAround = new List<Cell>();
    private bool IsUpperBorderExist = true;
    private bool IsLowerBorderExist = true;
    private bool IsRightBorderExist = true;
    private bool IsLeftBorderExist = true;

    private void CheckLeftAndRightKilledShipBorders(Cell cell, int shipSize)
    {
        int firstRow = cell.Row;
        int firstColumn = cell.Column;
        Cell leftCell = FindCell(firstRow, firstColumn - 1, "Enemy");
        Cell rightCell = FindCell(firstRow, firstColumn + shipSize, "Enemy");
        if (CheckCell(leftCell, CellState.Miss))
        {
            cellsAround.Add(leftCell);
            CheckIsUpperBorderExist(leftCell);
            CheckIsLowerBorderExist(leftCell);
            if (CheckCell(rightCell, CellState.Miss))
            {
                cellsAround.Add(rightCell);
                CheckIsUpperBorderExist(rightCell);
                CheckIsLowerBorderExist(rightCell);
            }
        }
        else
        {
            cellsAround.Add(rightCell);
            CheckIsUpperBorderExist(rightCell);
            CheckIsLowerBorderExist(rightCell);
        }
    }
    private void CheckUpperAndLowerBorders(Cell cell, int shipSize)
    {
        int firstRow = cell.Row;
        int firstColumn = cell.Column;
        Cell upperCell = FindCell(firstRow - 1, firstColumn, "Enemy");
        Cell lowerCell = FindCell(firstRow + shipSize, firstColumn, "Enemy");
        if (CheckCell(upperCell, CellState.Miss))
        {
            cellsAround.Add(upperCell);
            CheckIsRightBorderExist(upperCell);
            CheckIsLeftBorderExist(upperCell);
            if (CheckCell(lowerCell, CellState.Miss))
            {
                cellsAround.Add(lowerCell);
                CheckIsRightBorderExist(lowerCell);
                CheckIsLeftBorderExist(lowerCell);
            }
        }
        else
        {
            cellsAround.Add(lowerCell);
            CheckIsRightBorderExist(lowerCell);
            CheckIsLeftBorderExist(lowerCell);
        }
    }
    private void CheckIsRightBorderExist(Cell cell)
    {
        int firstRow = cell.Row;
        int firstColumn = cell.Column;
        Cell newCell = FindCell(firstRow, firstColumn + 1, "Enemy");
        IsRightBorderExist = CheckCell(newCell, CellState.Miss);
        if (IsRightBorderExist)
            cellsAround.Add(newCell);
    }
    private void CheckIsLeftBorderExist(Cell cell)
    {
        int firstRow = cell.Row;
        int firstColumn = cell.Column;
        Cell newCell = FindCell(firstRow, firstColumn - 1, "Enemy");
        IsLeftBorderExist = CheckCell(newCell, CellState.Miss);
        if (IsLeftBorderExist)
            cellsAround.Add(newCell);
    }
    private void CheckIsUpperBorderExist(Cell cell)
    {
        int firstRow = cell.Row;
        int firstColumn = cell.Column;
        Cell newCell = FindCell(firstRow - 1, firstColumn, "Enemy");
        IsUpperBorderExist = CheckCell(newCell, CellState.Miss);
        if (IsUpperBorderExist)
            cellsAround.Add(newCell);
    }
    private void CheckIsLowerBorderExist(Cell cell)
    {
        int firstRow = cell.Row;
        int firstColumn = cell.Column;
        Cell newCell = FindCell(firstRow + 1, firstColumn, "Enemy");
        IsLowerBorderExist = CheckCell(newCell, CellState.Miss);
        if (IsLowerBorderExist)
            cellsAround.Add(newCell);
    }
    private void DeterminingLowerBorderCells(int firstRow, int firstColumn, int shipSize)
    {
        for (int i = 0; i < shipSize; i++)
        {
            Cell newCell = FindCell(firstRow + 1, firstColumn + i, "Enemy");
            cellsAround.Add(newCell);
        }
    }
    private void DeterminingUpperBorderCells(int firstRow, int firstColumn, int shipSize)
    {
        for (int i = 0; i < shipSize; i++)
        {
            Cell newCell = FindCell(firstRow - 1, firstColumn + i, "Enemy");
            cellsAround.Add(newCell);
        }
    }
    private void DeterminingLeftBorderCells(int firstRow, int firstColumn, int shipSize)
    {
        for (int i = 0; i < shipSize; i++)
        {
            Cell newCell = FindCell(firstRow + i, firstColumn - 1, "Enemy");
            cellsAround.Add(newCell);
        }
    }
    private void DeterminingRightBorderCells(int firstRow, int firstColumn, int shipSize)
    {
        for (int i = 0; i < shipSize; i++)
        {
            Cell newCell = FindCell(firstRow + i, firstColumn + 1, "Enemy");
            cellsAround.Add(newCell);
        }
    }
    private void MakeAroundCellsStatesMiss(Cell firstCell, ShipDirection direction)
    {
        int firstRow = firstCell.Row;
        int firstColumn = firstCell.Column;

        int shipSize = DeterminingKilledShipSize(firstCell, direction);
        if (shipSize == 1)
        {
            CheckLeftAndRightKilledShipBorders(firstCell, shipSize);
            CheckUpperAndLowerBorders(firstCell, shipSize);
        }
        else if (direction == ShipDirection.Horizontal)
        {
            CheckLeftAndRightKilledShipBorders(firstCell, shipSize);
            if (IsUpperBorderExist == false)
                DeterminingLowerBorderCells(firstRow, firstColumn, shipSize);
            else if (IsLowerBorderExist == false)
                DeterminingUpperBorderCells(firstRow, firstColumn, shipSize);
            else if (IsUpperBorderExist == true && IsLowerBorderExist == true)
            {
                DeterminingLowerBorderCells(firstRow, firstColumn, shipSize);
                DeterminingUpperBorderCells(firstRow, firstColumn, shipSize);
            }
        }
        else if (direction == ShipDirection.Vertical)
        {
            CheckUpperAndLowerBorders(firstCell, shipSize);
            if (IsRightBorderExist == false)
                DeterminingLeftBorderCells(firstRow, firstColumn, shipSize);
            else if (IsLeftBorderExist == false)
                DeterminingRightBorderCells(firstRow, firstColumn, shipSize);
            else if (IsRightBorderExist == true && IsLeftBorderExist == true)
            {
                DeterminingLeftBorderCells(firstRow, firstColumn, shipSize);
                DeterminingRightBorderCells(firstRow, firstColumn, shipSize);
            }
        }
        foreach(Cell cell in cellsAround)
        {
            cell.State = CellState.Miss;
            UpdateEnemyCell(cell);
        }
    }

    public async Task<HitState> HitCell(int row, int column)
    {
        string BadResponse = "Try again";
        Cell cell = _enemyCells.FirstOrDefault(c => c.Row == row && c.Column == column);
        LastRequest = _hitsCellRequest + $"row:{row} " + $"column:{column}";
        RequestSuccess = false;
        Response response = await _player.SendRequestWithResponseAsync(_requestParser.Parse(_hitsCellRequest + $"row:{row} " + $"column:{column}"));
        RequestSuccess = true;
        BadResponse = response.Contents;
        while (BadResponse.Contains("Try again"))
        {
            LastRequest = _hitsCellRequest + $"row:{row} " + $"column:{column}";
            RequestSuccess = false;
            response = await _player.SendRequestWithResponseAsync(_requestParser.Parse(_hitsCellRequest + $"row:{row} " + $"column:{column}"));
            BadResponse = response.Contents;
            RequestSuccess = true;
        }
        HitState state = GetCellStateFromResponse(response.Contents);
        //_player.StartCheckingOpponent();
        if (state == HitState.Kill)
        {
            cell.State = CellState.Hit;
            PlayerScore++;
            ShipDirection shipDirection = DeterminingDirection(cell, CellState.Hit, "Enemy");
            Cell firstCell = FindFirstCell(cell, shipDirection, CellState.Hit, "Enemy");

            MakeAroundCellsStatesMiss(firstCell, shipDirection);
        }
        else if (state == HitState.Hit)
            cell.State = CellState.Hit;
        else if (state == HitState.Miss)
            cell.State = CellState.Miss;
        UpdateEnemyCell(cell);
        return state;
    }

    public async Task<bool> IsOpponentMove()
    {
        LastRequest = _waitCellForCheckRequest;
        RequestSuccess = false;
        Response response = await _player.SendRequestWithResponseAsync(_requestParser.Parse(_waitCellForCheckRequest));
        RequestSuccess = true;
        if (GetCellFromResponse(response.Contents) != null)
        {
            Cell checkedCell = GetCellFromResponse(response.Contents);
            if (checkedCell.State == CellState.Ship)
            {
                checkedCell.State = CellState.Hit;
                ShipDirection shipDirection = DeterminingDirection(checkedCell, CellState.Hit, "Player");
                Cell firstCell = FindFirstCell(checkedCell, shipDirection, CellState.Hit, "Player");
                int shipSize = DeterminingShipSize(firstCell, shipDirection, CellState.Hit);
                if (shipSize == 0)
                {
                    LastRequest = _cellStateRequest + HitState.Kill.ToString();
                    RequestSuccess = false;
                    await _player.SendRequestAsync(_requestParser.Parse(_cellStateRequest + HitState.Kill.ToString()));
                    RequestSuccess = true;
                    OpponentScore++;
                    if(OpponentScore == MaxNumberOfPoints)
                    {
                        UpdateCell(checkedCell);
                        return true;
                    }
                }
                else
                {
                    LastRequest = _cellStateRequest + HitState.Hit.ToString();
                    RequestSuccess = false;
                    await _player.SendRequestAsync(_requestParser.Parse(_cellStateRequest + HitState.Hit.ToString()));
                    RequestSuccess = true;
                }
                    
            }
            else if (checkedCell.State == CellState.Empty)
            {
                checkedCell.State = CellState.Miss;
                UpdateCell(checkedCell);
                LastRequest = _cellStateRequest + HitState.Miss.ToString();
                RequestSuccess = false;
                await _player.SendRequestAsync(_requestParser.Parse(_cellStateRequest + HitState.Miss.ToString()));
                RequestSuccess = true;
                return true;
            }
            UpdateCell(checkedCell);
        }
        return false;
    }

    private Cell GetCellFromResponse(string response)
    {
        // Паттерн регулярного выражения для извлечения значения row и column
        string pattern = @"row:\s*(\d+)\s*column:\s*(\d+)";

        // Поиск совпадений в строке response
        Match match = Regex.Match(response, pattern);

        if (match.Success)
        {
            // Получение значений row и column из совпадений
            int row = int.Parse(match.Groups[1].Value);
            int column = int.Parse(match.Groups[2].Value);
            Cell cell = _playerCells.FirstOrDefault(c => c.Row == row && c.Column == column);
            return cell;
        }
        else
        {
            return null;
        }
    }

    private HitState GetCellStateFromResponse(string response)
    {
        // Проверка наличия подстроки "Cell state: " в ответе
        if (response.Contains("Cell state:"))
        {
            // Получение значения CellState
            string cellStateString = response.Substring(response.IndexOf("Cell state: ") + 12).Trim();

            // Преобразование значения CellState в соответствующий перечислитель
            switch (cellStateString.ToLower())
            {
                case "kill":
                    return HitState.Kill;
                case "hit":
                    return HitState.Hit;
                case "miss":
                    return HitState.Miss;
                default:
                    throw new ArgumentException("Invalid cell state");
            }
        }
        else
        {
            throw new ArgumentException("Invalid response format");
        }
    }
}
