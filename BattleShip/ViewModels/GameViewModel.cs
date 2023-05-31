using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleShip.ViewModels;

namespace BattleShip;

public class GameViewModel : ViewModelBase
{
    private ObservableCollection<CellViewModel> _playerCells;
    private ObservableCollection<CellViewModel> _enemyCells;

    public ObservableCollection<CellViewModel> PlayerCells
    {
        get { return _playerCells; }
        set
        {
            _playerCells = value;
            OnPropertyChange();
        }
    }

    public ObservableCollection<CellViewModel> EnemyCells
    {
        get { return _enemyCells; }
        set
        {
            _enemyCells = value;
            OnPropertyChange();
        }
    }

    public GameViewModel()
    {
        PlayerCells = GenerateCells();
        EnemyCells = GenerateCells();
    }

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
}
