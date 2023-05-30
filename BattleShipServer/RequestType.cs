namespace BattleShipServer;

public enum RequestType : byte
{
    CreateNewGame = 0,
    JoinToGame = 1,
    Port = 2,
    Online = 3,
    Ping = 4,
    Disconnect = 5,
    Exception = 6


}

