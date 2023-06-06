

namespace BattleShipServer;
public class FileLogger : ILogger, IDisposable
{
    private string _path;
    private FileStream _fileStream;
    private StreamWriter _writer;

    public FileLogger(string path)
    {
        if (String.IsNullOrEmpty(path))
            throw new ArgumentNullException(nameof(path));
        _path = path;
        _fileStream = new FileStream(_path, FileMode.OpenOrCreate);
        _writer = new StreamWriter(_fileStream);
    }
    public void Log(string message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));
        _writer.WriteLine(message);
        _writer.Flush();
    }

    public void Dispose()
    {
        _fileStream.Close();
        _writer.Close();
    }
}