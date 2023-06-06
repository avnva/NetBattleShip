using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BattleShip;

public class Player
{
    public IPEndPoint IpEndPoint;
    public int Port;
    private TcpClient _playerSocket;
    private Thread _ping;
    private NetworkStream _networkStream;
    private int _lastPingTime;
    private int _pingTime = 10;
    private string _checkOnline = "Check online";
    private string _getGameRoom = "Get game room";
    private RequestParser _requestParser;

    public bool IsOpponentReady;
    public bool IsOpponentConnected = false;
    public event EventHandler CheckOpponentOnlineEvent;
    public event Func<Task> ServerDisconnected;
    public Player()
    {
        _lastPingTime = DateTime.Now.Second;
        _requestParser = new RequestParser();
        Application.Current.Exit += Application_Exit;
    }

    public string Name { get; set; }
    public async Task<bool> ConnectAsync(IPAddress ip, int port)
    {
        try
        {
            IpEndPoint = new IPEndPoint(ip, port);
            _playerSocket = new TcpClient();

            await _playerSocket.ConnectAsync(IpEndPoint);
            _networkStream = _playerSocket.GetStream();
            _ping = new Thread(WaitForPing);
            _ping.Start();
            Port = port;
            return true;
        }
        catch
        {
            return false;
        }
    }
    public int GetPort(IPEndPoint ip)
    {
        return ip.Port;
    }
    public async Task CreateNewGame()
    {
        string request = _requestParser.Parse(_getGameRoom);
        Response newPort = await SendRequestWithResponseAsync(request);

        await ConnectToNewPort(IpEndPoint, newPort);
        StartCheckingOpponent();
    }

    public async Task<bool> ConnectToNewPort(IPEndPoint ip, Response response)
    {
        if (int.TryParse(response.Contents, out var port))
        {
            ip.Port = port;
            _playerSocket.Close();

            _playerSocket = new TcpClient();
            await _playerSocket.ConnectAsync(IpEndPoint);
            _networkStream = _playerSocket.GetStream();
            Port = port;
            //_ping = new Thread(WaitForPing);
            //_ping.Start();
            return true;
        }
        else
            return false;
    }

    private System.Timers.Timer _timer;
    public void StartCheckingOpponent()
    {
        // Таймер, который будет проверять каждые 5 секунд онлайн ли оппонент 
        _timer = new System.Timers.Timer();
        _timer.Interval = 5000;
        _timer.Elapsed += CheckOnline;
        _timer.AutoReset = true;
        _timer.Enabled = true;
    }
    private void CheckOnline(object sender, EventArgs e)
    {
        CheckOpponentOnline();
        OnOpponentOnline();
    }
    private async Task CheckOpponentOnline()
    {
        string request = _requestParser.Parse(_checkOnline);
        Response response = await SendRequestWithResponseAsync(request);
        IsOpponentConnected = response.Flag;
    }
    private void OnOpponentOnline()
    {
        CheckOpponentOnlineEvent?.Invoke(null, EventArgs.Empty);
    }
    public void StopCheckingOpponent()
    {
        _timer?.Dispose();
    }
    public bool CheckConnection()
    {
        if (_playerSocket != null)
            return _playerSocket.Connected;
        else
            return false;
    }
    private async void WaitForPing()
    {
        while (true)
        {
            if (Math.Abs(DateTime.Now.Second - _lastPingTime) >= _pingTime)
            {
                _lastPingTime = DateTime.Now.Second;
                if (!await PingAsync())
                {
                    _ping.Interrupt();
                    break;
                }   
            }
        }
        await OnServerDisconnected();
    }
    private async Task<bool> PingAsync()
    {
        try
        {
            byte[] request = new[] { (byte)RequestType.Ping };
            await _networkStream.WriteAsync(request);
            return true;
        }
        catch
        {
            CloseSocket();
            return false;
        }
    }
    private async Task OnServerDisconnected()
    {
        ServerDisconnected?.Invoke();
    }
    public async Task SendRequestAsync(string message)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        await _networkStream.WriteAsync(messageBytes, 0, messageBytes.Length);
        await _networkStream.FlushAsync();
    }
    public async Task<Response> SendRequestWithResponseAsync(string message)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        await _networkStream.WriteAsync(messageBytes, 0, messageBytes.Length);
        await _networkStream.FlushAsync();

        return await GetResponseAsync();
    }
    public async Task<Response> GetResponseAsync()
    {
        byte[] buffer = new byte[1024 * 8];
        int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);
        switch ((RequestType)buffer[0])
        {
            case RequestType.CreateNewGame:
                return new Response(RequestType.CreateNewGame,
                    GetBoolResponseAsync(buffer));
            case RequestType.JoinToGame:
                if(await ConnectToNewPort(IpEndPoint, new Response(RequestType.Port,
                   await GetStringResponseAsync(buffer, bytesRead))))
                return new Response(RequestType.JoinToGame,
                    true);
                else
                    return new Response(RequestType.JoinToGame,
                    false);
            case RequestType.WaitingOpponent:
                return new Response(RequestType.WaitingOpponent,
                    GetBoolResponseAsync(buffer));
            case RequestType.Online:
                return new Response(RequestType.Online,
                    GetBoolResponseAsync(buffer));
            case RequestType.Port:
                return new Response(RequestType.Port,
                    await GetStringResponseAsync(buffer, bytesRead));
            case RequestType.StartGame:
                return new Response(RequestType.StartGame,
                    GetBoolResponseAsync(buffer));
            case RequestType.CheckOpponentCell:
                return new Response(RequestType.CheckOpponentCell,
                    await GetStringResponseAsync(buffer, bytesRead));
            case RequestType.OpponentMove:
                return new Response(RequestType.OpponentMove,
                    await GetStringResponseAsync(buffer, bytesRead));
            case RequestType.Reconnect:
                if (await ConnectToNewPort(IpEndPoint, new Response(RequestType.Port,
                   await GetStringResponseAsync(buffer, bytesRead))))
                    return new Response(RequestType.Reconnect,
                        true);
                else
                    return new Response(RequestType.Reconnect,
                    false);
            default:
                throw new ApplicationException("Invalid signature!");
        }
    }

    private bool GetBoolResponseAsync(byte[] buffer)
    {
        buffer = buffer.Skip(1).ToArray();
        bool response = BitConverter.ToBoolean(buffer, 0);
        return response;
    }

    private async Task<string> GetStringResponseAsync(byte[] buffer, int bytesRead)
    {
        StringBuilder response = new StringBuilder();
        buffer = buffer.Skip(1).ToArray();
        response.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead - 1));

        while (bytesRead > 0)
        {
            if (_networkStream.DataAvailable == false)
                break;

            bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);
            response.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
        }

        return response.ToString();
    }

    public async Task DisconnectAsync()
    {
        if(_playerSocket != null && _networkStream != null)
        {
            if(_ping != null && _timer != null)
            {
                _ping.Interrupt();
                StopCheckingOpponent();
            }
            await _networkStream.WriteAsync(new[] { (byte)RequestType.Disconnect }, 0, 1);
            await _networkStream.FlushAsync();
            CloseSocket();
        }
    }
    private void CloseSocket()
    {
        _playerSocket.Close();
        //Disconnected?.Invoke();
    }
    private void Application_Exit(object sender, ExitEventArgs e)
    {
        DisconnectAsync().Wait(); // Завершения отключения после закрытия программы
    }

    //public Action Disconnected { get; set; }
}
