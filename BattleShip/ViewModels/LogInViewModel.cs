using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http;

namespace BattleShip;

public class LogInViewModel:ViewModelBase
{
    private Player socket;
    private bool _connected;
    private string _enterEndPoint;
    private string _endPoint;
    private string clientLog;

    public LogInViewModel()
    {
        EndPoint = "127.0.0.1:8888";
        socket = new Player();
    }

    private RelayCommand _connect;

    public RelayCommand ConnectCommand
    {
        get
        {
            return _connect ?? new RelayCommand(
                _execute => Connect(),
                _canExecute => !_connected
            );
        }
    }

    private async void Connect()
    {
        try
        {
            await socket.ConnectAsync(EndPoint);
            _endPoint = socket.IpEndPoint.ToString();
            _connected = true;
            ClientLog += $"Client connected to: {_endPoint}\n";
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
