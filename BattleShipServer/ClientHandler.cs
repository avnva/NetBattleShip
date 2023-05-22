using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BattleShipServer;

public class ClientHandler : IDisposable
{
    private TcpClient client;
    private ILogger logger;
    private Port port;
    private NetworkStream networkStream;
    public GameRoom GameRoom { get; private set; }

    public event Func<TcpClient, int, Task> GetExistingPortRequested;
    public event Func<TcpClient, Task> GetNewPortRequested;
    public event EventHandler Check;

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
        Thread thread = new Thread(Listen);
        thread.Start();
    }

    private async void Listen()
    {
        try
        {
            while (true)
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
                    //выдать игровую комнату и новый порт
                    await CreateNewGameRoom();
                    break;
                case RequestType.JoinToGame:
                    //проверить создана ли игровая комната в переданном порту и количество человек в ней
                    await JoinToExistingGameRoom(request);
                    break;
                case RequestType.Port:
                    //отправить номер порта
                    await SendStringAsync(GetPort(), RequestType.Port);
                    break;
                case RequestType.Ping:
                    break;
                case RequestType.Disconnect:
                    Disconnect();
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

    private void Disconnect()
    {
        //port.Occupied = false;
        client.Close();
        client.Dispose();
        logger.Log(" >> Client disconnected");
    }

    public void Dispose()
    {
        Disconnect();
    }
}
