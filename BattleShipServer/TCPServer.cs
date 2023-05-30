using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
            clientHandler.Start();
        }
    }

    private async Task CreateNewGame(TcpClient client)
    {
        Port newPort = FindFreePort();
        _logger.Log($" >> Found new port: {newPort.PortValue}");
        
        await SendNewPort(client, newPort);
        _logger.Log($" >> Waiting for connection");

        TcpClient cl = await WaitForConnection(newPort);
        _logger.Log($" >> Client connected on port {newPort.PortValue}");

        ClientHandler clientHandler = new ClientHandler(cl, _logger, newPort);
        clientHandler.CheckOnline += CheckOnline;
        clientHandler.Start();

        AddClientToNewGameRoom(cl, newPort);
        _logger.Log($" >> Client connected to new game room");
    }

    private async Task HandleCreateNewGameRoom(TcpClient client)
    {
        await CreateNewGame(client);
    }

    private bool CheckOnline(Port port) 
    { 
        return roomManager.CheckPlayersConnection(port);
    }
    private async Task HandleJoinToExistingGameRoom(TcpClient client, int portValue)
    {
        Port ConnectionPort = null;
        foreach (Port port in _ports)
            if (port.PortValue == portValue || port.Occupied == true)
                ConnectionPort = port;
        if (ConnectionPort == null)
            throw new ArgumentNullException("Такой порт не существует");
        else
            roomManager.AddPlayerToExistsRoom(client, ConnectionPort);
    }

    private void AddClientToNewGameRoom(TcpClient client, Port port)
    {
        roomManager.AddPlayerToNewRoom(client, port);
    }

    private async Task SendNewPort(TcpClient client, Port port)
    {
        NetworkStream networkStream = client.GetStream();
        byte[] bytes = Encoding.UTF8.GetBytes($"{port.PortValue}"), responseBytes = new byte[bytes.Length + 1];

        responseBytes[0] = (byte)RequestType.Port;
        Array.Copy(bytes, 0, responseBytes, 1, bytes.Length);

        await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
        await networkStream.FlushAsync();

        Disconnect(client);
    }

    private async Task<TcpClient> WaitForConnection(Port port)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, port.PortValue);
        listener.Start();

        TcpClient client = await listener.AcceptTcpClientAsync();
        listener.Stop();

        _logger.Log($" >> Listener on port {port.PortValue} started!");
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

    private void Disconnect(TcpClient client)
    {
        client.Close();
        client.Dispose();
    }
}
