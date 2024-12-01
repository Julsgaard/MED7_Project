using Unity.Netcode;
using UnityEngine;

// ServerManager for the server. This script is attached to an empty GameObject in the ServerScene.
// To run the server you need to build as dedicated server and run the .exe file.
public class ServerManager : MonoBehaviour
{
    [SerializeField] private GameObject postItNotePrefab;
    
    void Start()
    {
        StartServer();
    }
    
    // Method to start the server
    private void StartServer()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        
        // Start the server
        NetworkManager.Singleton.StartServer();
        Debug.Log("Server started");
    }
    
    // Debugging for when a client connects or disconnects.
    // ulong for client ID, it is needed for the OnClientConnectedCallback and OnClientDisconnectCallback.
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected");
        
        SpawnPostItNote();

    }
    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected");
    }
    
    private void SpawnPostItNote()
    {
        // Instantiate the post-it note prefab
        GameObject postItNoteObj = Instantiate(postItNotePrefab);
        postItNoteObj.GetComponent<NetworkObject>().Spawn();
        PostItNote postItNote = postItNoteObj.GetComponent<PostItNote>();
        
        // Set the position of the post-it note
        postItNoteObj.transform.position = new Vector3(0, 0, 0);
        
        // Set the colour of the post-it note
        postItNote.SetColour(Color.yellow);
        
        // Set the text of the post-it note
        postItNote.SetText("test");

    }
    
    /*[ServerRpc(RequireOwnership = false)]
    public void ReceivePostItNoteDataServerRpc(string noteText, Color noteColor, ServerRpcParams rpcParams = default)
    {
        // Instantiate the post-it note prefab with the received data
        GameObject postItNoteObj = Instantiate(postItNotePrefab);
        postItNoteObj.GetComponent<NetworkObject>().Spawn();

        PostItNote postItNote = postItNoteObj.GetComponent<PostItNote>();

        // Set the position of the post-it note (you can modify this to set different positions)
        postItNoteObj.transform.position = new Vector3(0, 0, 0);

        // Set the color and text of the post-it note
        postItNote.SetColour(noteColor);
        postItNote.SetText(noteText);

        Debug.Log($"Spawned post-it note for client {rpcParams.Receive.SenderClientId}");
    }*/

    private void OnApplicationQuit()
    {
        // Stop the server when the OnApplicationQuit is called
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
