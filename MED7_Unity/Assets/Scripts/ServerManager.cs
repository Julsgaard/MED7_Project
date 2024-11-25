using Unity.Netcode;
using UnityEngine;

// ServerManager for the server. This script is attached to an empty GameObject in the ServerScene.
// To run the server you need to build as dedicated server and run the .exe file.
public class ServerManager : MonoBehaviour
{
    void Start()
    {
        StartServer();
        Debug.Log("Starting headless server");
        
    }
    
    // Method to start the server
    private void StartServer()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        
        // Start the server
        NetworkManager.Singleton.StartServer();
    }
    
    // Debugging for when a client connects or disconnects.
    // ulong for client ID, it is needed for the OnClientConnectedCallback and OnClientDisconnectCallback.
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected");
    }
    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected");
    }

    private void OnApplicationQuit()
    {
        // Stop the server when the OnApplicationQuit is called
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
