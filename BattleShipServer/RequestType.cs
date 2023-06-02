namespace BattleShipServer;

public enum RequestType : byte
{
    CreateNewGame = 0,
    JoinToGame = 1,
    WaitingOpponent = 2,
    Port = 3,
    Online = 4,
    Ping = 5,
    Disconnect = 6,
    Exception = 7


}

