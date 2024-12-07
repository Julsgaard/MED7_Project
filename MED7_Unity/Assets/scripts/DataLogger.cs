using Unity.Netcode;
using UnityEngine;

// DataLogger for saving data to a .csv file for later analysis
public class DataLogger : NetworkBehaviour
{
    public static DataLogger instance { get; private set; }
    private string _savePath;

    private void Awake()
    {
        // Singleton
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        CreateLogFile();
    }

    private void CreateLogFile()
    {
        string timeStamp = GetTimeStamp();
        _savePath = Application.persistentDataPath + $"/log_{timeStamp}.csv";
        
        // Create a new .csv file and write the header
        string header = "Timestamp;EventType;Position;Text;Color;ClientId";
        System.IO.File.WriteAllText(_savePath, header + "\n"); //TODO: If the note text contains a ; or a newline, this will break
        
        Debug.Log("Log file created at: " + _savePath);
    }

    private string GetTimeStamp()
    {
        return System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    }

    // Log data to a .csv file in multiple columns
    private void LogData(string eventType, string position, string text, string color, string clientId)
    {
        string timeStamp = GetTimeStamp();
        string log = $"{timeStamp};{eventType};{position};{text};{color};{clientId}";
        System.IO.File.AppendAllText(_savePath, log + "\n");
        
        Debug.Log("Logged: " + log);
    }

    public void LogPostItNoteCreated(Vector3 position, string text, Color color, ulong clientId)
    {
        string positionStr = position.ToString();
        string colorStr = ColorUtility.ToHtmlStringRGBA(color);
        LogData("PostItNoteCreated", positionStr, text, colorStr, clientId.ToString());
    }

    public void LogPostItNoteMove(Vector3 position, ulong clientId)
    {
        string positionStr = position.ToString();
        LogData("PostItNoteMoved", positionStr, "", "", clientId.ToString());
    }

    public void LogClientConnected(ulong clientId)
    {
        LogData("ClientConnected", "", "", "", clientId.ToString());
    }

    public void LogClientDisconnected(ulong clientId)
    {
        LogData("ClientDisconnected", "", "", "", clientId.ToString());
    }
    
    public void LogServerStarted()
    {
        LogData("ServerStarted", "", "", "", "");
    }
}
