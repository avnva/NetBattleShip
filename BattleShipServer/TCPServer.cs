using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Runtime.CompilerServices;

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
            clientHandler.Start();
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
        clientHandler.CheckOnline += CheckOnline;
        clientHandler.Start();
        if(type == RequestType.Port)
            clientHandler.CreateNewGameRequested += CreateNewGame;
        clientHandler.DisconnectRequest += Disconnect;
        clientHandler.StartGameRequested += HandleStartGame;
        clientHandler.SendCoordinateToOpponentRequested += SendCoordinateToOpponent;
        clientHandler.SendCellStateToOpponentRequested += SendStateToOpponent;
        return cl;
    }

    private async Task HandleCreateNewGameRoom(TcpClient client)
    {
        Port newPort = FindFreePort();
        _logger.Log($" >> Found new port: {newPort.PortValue}");
        TcpClient newClient = await RedirectingToNewPort(client, newPort, RequestType.Port);
        

        AddClientToNewGameRoom(newClient, newPort);
        _logger.Log($" >> Client connected to new game room");
        
    }
    private async Task HandleStartGame(Port port, TcpClient client)
    {
        TcpClient opponent = roomManager.GetOpponent(client, port);
        if (opponent == client)
            throw new Exception("Error");
        if (roomManager.IsFirstPlayer(client, port))
            await SendBoolMessage(opponent, false, RequestType.StartGame);
        else
            await SendBoolMessage(opponent, true, RequestType.StartGame);
    }

    private async Task CheckOnline(Port port, TcpClient client) 
    {
        bool opponentConnect = roomManager.CheckPlayersConnection(port);
        await SendBoolMessage(client, opponentConnect, RequestType.Online);
        if (opponentConnect)
             _logger.Log($" >> Server sent: opponent is found");
        else
            _logger.Log($" >> Server sent: opponent not found");

    }
    private async Task HandleJoinToExistingGameRoom(TcpClient client, int portValue)
    {
        Port ConnectionPort = null;
        foreach (Port port in _ports)
            if (port.PortValue == portValue || port.Occupied == true)
            {
                ConnectionPort = port;
                break;
            }

        if (ConnectionPort == null)
            throw new ArgumentNullException("Такой порт не существует");
        else
        {
            TcpClient newClient = await RedirectingToNewPort(client, ConnectionPort, RequestType.JoinToGame);
            roomManager.AddPlayerToExistsRoom(newClient, ConnectionPort);
            _logger.Log($" >> Client connected to existing game room");
            //await SendMessage(newClient, true, RequestType.JoinToGame);
            //_logger.Log($" >> Server sent: connection successful");
        }
    }
    private async Task SendCoordinateToOpponent(Port port, TcpClient client, string message)
    {
        TcpClient opponent = roomManager.GetOpponent(client, port);
        if (opponent == client)
            throw new Exception("Error");
        await SendStringMessage(opponent, message, RequestType.OpponentMove);
        _logger.Log($" >> Server sent coordinate");
    }
    private async Task SendStateToOpponent(Port port, TcpClient client, string message)
    {
        TcpClient opponent = roomManager.GetOpponent(client, port);
        if (opponent == client)
            throw new Exception("Error");
        await SendStringMessage(opponent, message, RequestType.CheckOpponentCell);
        _logger.Log($" >> Server sent cell state");
    }
    private async Task SendStringMessage(TcpClient client, string value, RequestType type)
    {
        NetworkStream networkStream = client.GetStream();
        byte[] bytes = Encoding.UTF8.GetBytes($"{value}"), responseBytes = new byte[bytes.Length + 1];

        responseBytes[0] = (byte)type;
        Array.Copy(bytes, 0, responseBytes, 1, bytes.Length);

        await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
        await networkStream.FlushAsync();
    }
    private async Task SendBoolMessage(TcpClient client, bool value, RequestType type)
    {
        NetworkStream networkStream = client.GetStream();
        //byte[] bytes = Encoding.UTF8.GetBytes($"{value}"), responseBytes = new byte[bytes.Length + 1];
        byte[] bytes = BitConverter.GetBytes(value);
        byte[] responseBytes = new byte[bytes.Length + 1];

        responseBytes[0] = (byte)type;
        Array.Copy(bytes, 0, responseBytes, 1, bytes.Length);

        await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
        await networkStream.FlushAsync();
    }

    private void AddClientToNewGameRoom(TcpClient client, Port port)
    {
        roomManager.AddPlayerToNewRoom(client, port);
    }

    private async Task SendNewPort(TcpClient client, Port port, RequestType type)
    {
        NetworkStream networkStream = client.GetStream();
        byte[] bytes = Encoding.UTF8.GetBytes($"{port.PortValue}"), responseBytes = new byte[bytes.Length + 1];

        responseBytes[0] = (byte)type;
        Array.Copy(bytes, 0, responseBytes, 1, bytes.Length);

        await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
        await networkStream.FlushAsync();

        Disconnect(null, client);
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
                return port;

        throw new ApplicationException("There is no free port.");
    }

    private void InitializePorts()
    {
        for (int i = 0; i < portsAmount; i++)
            _ports[i] = new Port(startPort.PortValue + 1 + i, false);
    }

    private void Disconnect(Port port, TcpClient client)
    {
        
        client.Close();
        client.Dispose();
        _logger.Log(" >> Client disconnected");
        if (port != null)
        {
            roomManager.RemovePlayerFromGameRoom(port, client);
            _logger.Log($" >> Disconnect client on port {port.PortValue}");
            TcpClient opponent = roomManager.GetOpponent(client, port);
            if (opponent != client)
            {
                SendBoolMessage(opponent, false, RequestType.Online);
                _logger.Log($" >> Server sent: opponent disconnected");
            }   
        }
    }

}
