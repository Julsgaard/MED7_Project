using Unity.Collections;
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
        if (IsClient) return;

        CreateLogFile();
    }

    private void CreateLogFile()
    {
        string timeStamp = GetTimeStamp();
        _savePath = Application.persistentDataPath + $"/log_{timeStamp}.csv";
        
        // Create a new .csv file and write the header
        string header = "Timestamp;EventType;Position;Text;Color;ClientId";
        System.IO.File.WriteAllText(_savePath, header + "\n");
        
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
        
        // Make sure that the text does not contain any newlines or semicolons, because it will break the .csv file
        text = text.Replace("\n", "\\n").Replace("\r", "\\r").Replace(";", "\\;");
        
        // Combine the data into a single string
        string log = $"{timeStamp};{eventType};{position};{text};{color};{clientId}";
        
        // Append the log to the file
        System.IO.File.AppendAllText(_savePath, log + "\n");
    }

    public void LogPostItNoteCreated(Vector3 position, string text, Color color, ulong clientId)
    {
        if (IsClient) return;

        string positionStr = position.ToString();
        string colorStr = ColorUtility.ToHtmlStringRGBA(color);
        LogData("PostItNoteCreated", positionStr, text, colorStr, clientId.ToString());
    }

    public void LogPostItNoteMove(Vector3 position,FixedString512Bytes text, Color color, ulong clientId)
    {
        if (IsClient) return;

        string positionStr = position.ToString();
        string textStr = text.ToString();
        string colorStr = ColorUtility.ToHtmlStringRGBA(color);
        LogData("PostItNoteMoved", positionStr, textStr, colorStr, clientId.ToString());
    }

    public void LogClientConnected(ulong clientId)
    {
        if (IsClient) return;

        LogData("ClientConnected", "", "", "", clientId.ToString());
    }

    public void LogClientDisconnected(ulong clientId)
    {
        if (IsClient) return;

        LogData("ClientDisconnected", "", "", "", clientId.ToString());
    }
    
    public void LogServerStarted()
    {
        if (IsClient) return;
        
        LogData("ServerStarted", "", "", "", "");
    }
}
