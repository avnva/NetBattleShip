namespace BattleShipServer;

public enum RequestType : byte
{
    CreateNewGame = 0,
    JoinToGame = 1,
    Port = 2,
    Name = 3,
    Disconnect = 4,
    Exception = 5


}

