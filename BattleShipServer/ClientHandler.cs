using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace BattleShipServer;

public class ClientHandler : IDisposable
{
    private TcpClient client;
    private ILogger logger;
    public Port port;
    private NetworkStream networkStream;
    Thread thread;
    private bool _isActiv;
    public GameRoom GameRoom { get; private set; }

    public event Func<TcpClient, int, Task> GetExistingPortRequested;
    public event Func<TcpClient, Task> GetNewPortRequested;
    public event Func<Port, TcpClient, Task> StartGameRequested;
    public event Func<Port, TcpClient, Task> CheckOnline;
    public event Func<Port, TcpClient, Task> CreateNewGameRequested;
    public event Action<Port, TcpClient> DisconnectRequest;
    public event Func<Port, TcpClient, string, Task> SendCoordinateToOpponentRequested;
    public event Func<Port, TcpClient, string, Task> SendCellStateToOpponentRequested;
    public event Action<Port> SetReadinessRequested;


    public ClientHandler(TcpClient client, ILogger logger, Port _port)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));
        port = _port;
        this.client = client;
        networkStream = this.client.GetStream();
        this.logger = logger;
    }

    public void Start()
    {
        _isActiv = true;
        thread = new Thread(Listen);
        thread.Start();
    }

    private async void Listen()
    {
        try
        {
            while (_isActiv)
            {
                byte[] buffer = new byte[1024 * 8];
                StringBuilder request = new StringBuilder();

                await GetRequest(buffer, request);
                await SendResponse(request.ToString());

                logger.Log($"Socket server received message: \"{request}\"");
            }
        }
        catch (Exception ex)
        {
            Disconnect();
        }
    }


    private async Task GetRequest(byte[] buffer, StringBuilder request)
    {
        CheckConneciton();
        int bytesRead = await networkStream.ReadAsync(buffer);

        while (bytesRead > 0)
        {
            request.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

            if (networkStream.DataAvailable == false)
                break;

            bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
        }
    }

    private async Task SendResponse(string request)
    {
        byte[] type = Encoding.UTF8.GetBytes(request.Substring(0, 1));
        request = request.Substring(1, request.Length - 1);
        try
        {
            switch ((RequestType)type[0])
            {
                case RequestType.CreateNewGame:
                    await CreateNewGame();
                    break;
                case RequestType.JoinToGame:
                    //проверить создана ли игровая комната в переданном порту и количество человек в ней
                    _isActiv = false;
                    await JoinToExistingGameRoom(GetPortFromRequest(request));
                    break;
                case RequestType.Port:
                    //отправить номер порта
                    await CreateNewGameRoom();
                    _isActiv = false;
                    break;
                case RequestType.Online:
                    await CheckOpponentOnline();
                    break;
                case RequestType.WaitingOpponent:
                    break;
                case RequestType.Ping:
                    break;
                case RequestType.Disconnect:
                    _isActiv = false;
                    Disconnect();
                    break;
                case RequestType.StartGame:
                    await StartGame();
                    break;
                case RequestType.CheckOpponentCell:
                    await SendCoordinateMessage(request);
                    break;
                case RequestType.CellState:
                    await SendStateMessage(request);
                    break;
                case RequestType.OpponentMove:
                    SetReadiness();
                    break;
                default:
                    throw new ApplicationException("Wrong signature!");
            }
        }
        catch (Exception ex)
        {
            await SendStringAsync(ex.Message, RequestType.Exception);
        }
    }
    private void SetReadiness()
    {
        if (SetReadinessRequested != null)
            SetReadinessRequested.Invoke(port);
        else
            throw new ArgumentNullException(nameof(SetReadinessRequested));
    }
    private async Task SendStateMessage(string request)
    {
        if (SendCellStateToOpponentRequested != null)
            await SendCellStateToOpponentRequested.Invoke(port, client, request);
        else
            throw new ArgumentNullException(nameof(SendCellStateToOpponentRequested));
    }
    private async Task SendCoordinateMessage(string request)
    {
        if (SendCoordinateToOpponentRequested != null)
            await SendCoordinateToOpponentRequested.Invoke(port, client, request);

        else
            throw new ArgumentNullException(nameof(SendCoordinateToOpponentRequested));
    }

    private async Task StartGame()
    {
        if (StartGameRequested != null)
            await StartGameRequested.Invoke(port, client);
        else
            throw new ArgumentNullException(nameof(StartGameRequested));
    }
    private async Task CreateNewGame()
    {
        if (CreateNewGameRequested != null)
            await CreateNewGameRequested.Invoke(port, client);
        else
            throw new ArgumentNullException(nameof(CreateNewGameRequested));
    }

    private async Task CheckOpponentOnline()
    {
        if (CheckOnline != null)
             await CheckOnline.Invoke(port, client);
        else
            throw new ArgumentNullException(nameof(CheckOnline));
    }

    private string GetPortFromRequest(string request) 
    {
        string result = Regex.Replace(request, @"[^\d]", "");
        return result;
    }
    private void Disconnect()
    {
        if (DisconnectRequest != null)
            DisconnectRequest.Invoke(port, client);
        else
            throw new ArgumentNullException(nameof(DisconnectRequest));
    }


    private string GetPort()
    {
        int value = port.PortValue;
        string strPort = value.ToString();
        return strPort;
    }

    private async Task CreateNewGameRoom()
    {
        if (GetNewPortRequested != null)
            await GetNewPortRequested.Invoke(client);
        else
            throw new ArgumentNullException(nameof(GetNewPortRequested));
    }

    private async Task JoinToExistingGameRoom(string PortValue)
    {
        int ConnectionPortValue = 0;
        if (int.TryParse(PortValue, out int number))
            ConnectionPortValue = number;
        else
            throw new ArgumentException(nameof(PortValue));

        if (ConnectionPortValue != 0)
        {
            if (GetExistingPortRequested != null)
                await GetExistingPortRequested.Invoke(client, ConnectionPortValue);
            else
                throw new ArgumentNullException(nameof(GetExistingPortRequested));
        }
        else
            throw new ArgumentException();
    }

    private async Task SendStringAsync(string response, RequestType type)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(response);
        byte[] responseBytes = new byte[bytes.Length + 1];
        logger.Log($"Server sent: {response}");
        responseBytes[0] = (byte)type;
        Array.Copy(bytes, 0, responseBytes, 1, bytes.Length);

        await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
        await networkStream.FlushAsync();
    }


    private void CheckConneciton()
    {
        if (client.Connected == false)
            Disconnect();
    }

    //private void Disconnect()
    //{
    //    //port.Occupied = false;
    //    client.Close();
    //    client.Dispose();
    //    logger.Log(" >> Client disconnected");
    //}

    public void Dispose()
    {
        Disconnect();
    }
}
