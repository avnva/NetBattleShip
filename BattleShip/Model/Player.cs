using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;


namespace BattleShip;

public class Player
{
    public IPEndPoint IpEndPoint;
    private TcpClient _playerSocket;
    private Thread ping;
    private NetworkStream _networkStream;
    private int lastPingTime;
    private int pingTime = 5;
    private string _checkOnline = "Check online";
    public bool IsOpponentConnected = false;
    public event EventHandler CheckOpponentOnlineEvent;
    private RequestParser _requestParser;

    public Player()
    {
        lastPingTime = DateTime.Now.Second;
        _requestParser = new RequestParser();
    }

    public async Task ConnectAsync(string endPoint)
    {
        IpEndPoint = IPEndPoint.Parse(endPoint);
        _playerSocket = new TcpClient();

        await _playerSocket.ConnectAsync(IpEndPoint);
        _networkStream = _playerSocket.GetStream();
        //ping = new Thread(WaitForPing);
        //ping.Start();
        //await GetPortAsync(IpEndPoint);
        //_playerSocket.Close();

        //_playerSocket = new TcpClient();
        //await _playerSocket.ConnectAsync(IpEndPoint);
        //_networkStream = _playerSocket.GetStream();
        //ping = new Thread(WaitForPing);
        //ping.Start();
    }
    public int GetPort(IPEndPoint ip)
    {
        return ip.Port;
    }

    public async Task ConnectToNewPort(IPEndPoint ip, Response response)
    {
        int.TryParse(response.Contents, out var port);
        ip.Port = port;
        _playerSocket.Close();

        _playerSocket = new TcpClient();
        await _playerSocket.ConnectAsync(IpEndPoint);
        _networkStream = _playerSocket.GetStream();
        //ping = new Thread(WaitForPing);
        //ping.Start();
        StartCheckingOpponent();
    }

    private System.Timers.Timer _timer;

    public void StartCheckingOpponent()
    {
        // Создаем таймер, который будет вызывать метод CheckOpponent каждые 5 секунд
        _timer = new System.Timers.Timer();
        _timer.Interval = 5000;
        _timer.Elapsed += CheckOnline;
        _timer.AutoReset = true;
        _timer.Enabled = true;
    }

    public void StopCheckingOpponent()
    {
        // Останавливаем таймер
        _timer?.Dispose();
    }
    public void CheckOnline(object sender, EventArgs e)
    {
        CheckOpponentOnline();
        OnOpponentOnline();
    }
    private void OnOpponentOnline()
    {
        CheckOpponentOnlineEvent?.Invoke(null, EventArgs.Empty);
    }
    public async Task<bool> CheckOpponentOnline()
    {
        string request = _requestParser.Parse(_checkOnline);
        Response response = await SendRequestAsync(request);
        return response.Flag;
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
            if (Math.Abs(DateTime.Now.Second - lastPingTime) >= pingTime)
            {
                lastPingTime = DateTime.Now.Second;
                if (!await PingAsync())
                    break;
            }
        }
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
    public async Task<Response> SendRequestAsync(string message)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        await _networkStream.WriteAsync(messageBytes, 0, messageBytes.Length);
        await _networkStream.FlushAsync();

        return await GetResponseAsync(message);
    }


    private async Task<Response> GetResponseAsync(string message)
    {
        byte[] buffer = new byte[1024 * 8];
        int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);;
        switch ((RequestType)buffer[0])
        {
            //case RequestType.CreateNewGame:

            //case RequestType.JoinToGame:
            case RequestType.Online:
                return new Response(RequestType.Online, 
                    await GetBoolResponseAsync(buffer));
 
            case RequestType.Port:
                return new Response(RequestType.Port,
                    await GetStringResponseAsync(buffer, bytesRead));
            default:
                throw new ApplicationException("Invalid signature!");
        }
    }
    private async Task<bool> GetBoolResponseAsync(byte[] buffer)
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
        await _networkStream.WriteAsync(new[] { (byte)RequestType.Disconnect }, 0, 1);
        await _networkStream.FlushAsync();
        StopCheckingOpponent();
        CloseSocket();
    }

    private void CloseSocket()
    {
        _playerSocket.Close();
        Disconnected?.Invoke();
        StopCheckingOpponent();
    }

    public Action Disconnected { get; set; }
}
