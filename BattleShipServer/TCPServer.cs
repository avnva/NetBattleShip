using System.Net.Sockets;
using System.Net;
using System.Text;

namespace BattleShipServer;

public class TCPServer
{
    private ILogger _logger;
    private Port[] _ports;
    private Port startPort;
    private int portsAmount = 1000;
    private GameRoomManager roomManager;

    public TCPServer(ILogger logger)
    {
        _ports = new Port[1000];
        startPort = new Port(8888, false);
        _ports = new Port[1000];
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        roomManager = new GameRoomManager();
        InitializePorts();
    }
    public async Task Start()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, startPort.PortValue);
        listener.Start();

        _logger.Log(" >> Server started.");
        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();

            _logger.Log($" >> Client {client.Client?.RemoteEndPoint} connected on port {startPort.PortValue}");

            ClientHandler clientHandler = new ClientHandler(client, _logger, startPort);
            clientHandler.GetNewPortRequested += HandleCreateNewGameRoom;
            clientHandler.GetExistingPortRequested += HandleJoinToExistingGameRoom;
            clientHandler.DisconnectRequest += Disconnect;
            clientHandler.ReconnectRequest += Reconnect;
            clientHandler.Start();
        }
    }

    private async Task<bool> Reconnect(int reconnectedPort, TcpClient client)
    {
        Port ConnectionPort = null;
        
        foreach (Port port in _ports)
        {
            if (port.PortValue == reconnectedPort)
            {
                ConnectionPort = port;
                break;
            }
        }

        if (ConnectionPort == null)
        {
            //await SendBoolMessage(client, false, RequestType.Reconnect);
            _logger.Log($" >> Server sent: invalid room number");
            return true;
        }
        else
        {
            ConnectionPort.Occupied = true;
            TcpClient newClient = await RedirectingToNewPort(client, ConnectionPort, RequestType.Reconnect);
            //await SendBoolMessage(newClient, true, RequestType.Reconnect);
            if (roomManager.FindGameRoom(ConnectionPort) == null)
                roomManager.AddPlayerToNewRoom(newClient, ConnectionPort);
            else
                roomManager.AddPlayerToExistsRoom(newClient, ConnectionPort);
            //roomManager.AddPlayerToExistsRoom(newClient, ConnectionPort);
            _logger.Log($" >> Client reconnecting on port {reconnectedPort}");
            return false;
        }
    }
    private async Task CreateNewGame(Port port, TcpClient client)
    {
        if (roomManager.CheckPlayersConnection(port))
        {
            await SendBoolMessage(client, true, RequestType.CreateNewGame);
            TcpClient opponent = roomManager.GetOpponent(client, port);
            if (opponent == client)
                throw new Exception("Error");
            await SendBoolMessage(opponent, true, RequestType.WaitingOpponent);
            _logger.Log($" >> Start new game on port {port.PortValue}");
        }
        else
        {
            await SendBoolMessage(client, false, RequestType.CreateNewGame);
            _logger.Log($" >> Error");
        }
    }

    private async Task<TcpClient> RedirectingToNewPort(TcpClient client, Port newPort, RequestType type)
    {

        await SendNewPort(client, newPort, type);
        _logger.Log($" >> Waiting for connection");

        TcpClient cl = await WaitForConnection(newPort);
        _logger.Log($" >> Client connected on port {newPort.PortValue}");

        ClientHandler clientHandler = new ClientHandler(cl, _logger, newPort);
        clientHandler.Start();
        clientHandler.CheckOnline += CheckOnline;
        //if(type == RequestType.Port)
        clientHandler.CreateNewGameRequested += CreateNewGame;
        clientHandler.DisconnectRequest += Disconnect;
        clientHandler.StartGameRequested += HandleStartGame;
        clientHandler.SendCoordinateToOpponentRequested += CheckPlayerReady;
        clientHandler.SendCellStateToOpponentRequested += SendStateToOpponent;
        clientHandler.SetReadinessRequested += SetReadiness;
        return cl;
    }

    private async Task HandleCreateNewGameRoom(TcpClient client)
    {
        Port newPort = FindFreePort();
        _logger.Log($" >> Found new port: {newPort.PortValue}");
        TcpClient newClient = await RedirectingToNewPort(client, newPort, RequestType.Port);

        roomManager.AddPlayerToNewRoom(newClient, newPort);
        _logger.Log($" >> Client connected to new game room");
        
    }
    private async Task HandleStartGame(Port port, TcpClient client)
    {
        TcpClient opponent = roomManager.GetOpponent(client, port);
        if (opponent == client)
            throw new Exception("Error");
        if (!roomManager.IsFirstPlayer(client, port))
        {
            await SendBoolMessage(client, false, RequestType.StartGame);
            await SendBoolMessage(opponent, true, RequestType.StartGame);
        }
            
        _logger.Log($" >> Server sent: start game on port {port.PortValue}");
    }

    private async Task CheckOnline(Port port, TcpClient client) 
    {
        bool opponentConnect = roomManager.CheckPlayersConnection(port);
        await SendBoolMessage(client, opponentConnect, RequestType.Online);
        if (opponentConnect)
             _logger.Log($" >> Server sent: opponent on port {port.PortValue} is found");
        else
            _logger.Log($" >> Server sent: opponent on port {port.PortValue} not found");

    }
    private async Task<bool> HandleJoinToExistingGameRoom(TcpClient client, int portValue)
    {
        Port ConnectionPort = null;
        foreach (Port port in _ports)
        {
            if (port.PortValue == portValue && port.Occupied == true)
            {
                ConnectionPort = port;
                break;
            }
        }

        if (ConnectionPort == null)
        {
            await SendStringMessage(client, "Такой комнаты не существует", RequestType.JoinToGame);
            //await SendBoolMessage(client, false, RequestType.JoinToGame);
            _logger.Log($" >> Server sent: invalid room number");
            return true;
        }
        else
        {
            try
            {
                roomManager.GetConnectionRoom(ConnectionPort);
                TcpClient newClient = await RedirectingToNewPort(client, ConnectionPort, RequestType.JoinToGame);
                //await SendBoolMessage(newClient, true, RequestType.JoinToGame);
                roomManager.AddPlayerToExistsRoom(newClient, ConnectionPort);
                return false;
            }
            catch (Exception ex)
            {
                await SendStringMessage(client, ex.Message, RequestType.JoinToGame);
                //await SendBoolMessage(client, false, RequestType.JoinToGame);
                _logger.Log($" >> Server sent: invalid room number");
                return true;
            }
        }
    }
    private void SetReadiness(Port port)
    {
        roomManager.SetPlayerReady(true, port);
        _logger.Log($" >> Server sent: opponent on port {port} ready");
    }
    private async Task CheckPlayerReady(Port port, TcpClient client, string message)
    {
        bool startFlag;
        startFlag = roomManager.IsPlayerReady(port);
        if (startFlag || message.Contains("End of turn"))
        {
            await SendCoordinateToOpponent(port, client, message);
        }
        else
        {
            await SendStringMessage(client, "Try again", RequestType.CheckOpponentCell);
            _logger.Log($" >> Server sent: try again");
        }
    }
    private async Task SendCoordinateToOpponent(Port port, TcpClient client, string message)
    {
        TcpClient opponent = roomManager.GetOpponent(client, port);
        if (opponent == client)
            throw new Exception("Error");
        await SendStringMessage(opponent, message, RequestType.OpponentMove);

        _logger.Log($" >> Server sent coordinate on port {port}: {message}");
    }
    private async Task SendStateToOpponent(Port port, TcpClient client, string message)
    {
        roomManager.SetPlayerReady(false, port);
        TcpClient opponent = roomManager.GetOpponent(client, port);
        if (opponent == client)
            throw new Exception("Error");
        await SendStringMessage(opponent, message, RequestType.CheckOpponentCell);
        _logger.Log($" >> Server sent cell state on port {port}: {message}");
    }
    private async Task SendStringMessage(TcpClient client, string value, RequestType type)
    {
        byte[] bytes = Encoding.UTF8.GetBytes($"{value}"),
            responseBytes = new byte[bytes.Length + 1];
        await SendMessage(bytes, responseBytes, client, type);
    }
    private async Task SendBoolMessage(TcpClient client, bool value, RequestType type)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        byte[] responseBytes = new byte[bytes.Length + 1];
        await SendMessage(bytes, responseBytes, client, type);
    }

    private async Task SendNewPort(TcpClient client, Port port, RequestType type)
    {
        byte[] bytes = Encoding.UTF8.GetBytes($"{port.PortValue}"), 
            responseBytes = new byte[bytes.Length + 1];

        await SendMessage(bytes, responseBytes, client, type);
        await Disconnect(null, client);
    }
    private async Task SendMessage(byte[] bytes, byte[] responseBytes, TcpClient client, RequestType type)
    {
        NetworkStream networkStream = client.GetStream();
        responseBytes[0] = (byte)type;
        Array.Copy(bytes, 0, responseBytes, 1, bytes.Length);

        await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
        await networkStream.FlushAsync();
    }
    private async Task<TcpClient> WaitForConnection(Port port)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, port.PortValue);
        listener.Start();

        TcpClient client = await listener.AcceptTcpClientAsync();
        listener.Stop();

        _logger.Log($" >> Socket on port {port.PortValue} started!");
        return client;
    }
    private Port FindFreePort()
    {
        foreach (Port port in _ports)
            if (port.Occupied == false)
            {
                port.Occupied = true;
                return port;
            }
        throw new ApplicationException("There is no free port.");
    }
    private void InitializePorts()
    {
        for (int i = 0; i < portsAmount; i++)
            _ports[i] = new Port(startPort.PortValue + 1 + i, false);
    }
    private async Task Disconnect(Port? port, TcpClient client)
    {
        client.Close();
        client.Dispose();
        if (port != null)
        {
            if (roomManager.FindGameRoom(port) != null)
            {
                roomManager.RemovePlayerFromGameRoom(port, client);
                _logger.Log($" >> Disconnect client on port {port.PortValue}");
                TcpClient opponent = roomManager.GetOpponent(client, port);
                if (opponent != client)
                {
                    await SendBoolMessage(opponent, false, RequestType.Online);
                    _logger.Log($" >> Server sent: opponent on port {port.PortValue} disconnected");
                }
                else
                {
                    port.Occupied = false;
                    _logger.Log($" >> Server sent: port {port.PortValue} is released");
                }
            }
            else
            {
                _logger.Log(" >> Client disconnected");
            }
        }
    }
}
