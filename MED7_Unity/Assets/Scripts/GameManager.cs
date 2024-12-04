using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private string defaultIpAddress; //TODO: Does not work for some reason - It connects to NetworkManager IP
    [SerializeField] private TMP_InputField ipAddressInputField;
    //[SerializeField] private TMP_InputField portInputField;
    [SerializeField] private GameObject connectUIObject, introUIObject, createApplicantUIObject, blackBackgroundUI, arSettingsUI;
    [SerializeField] private Button nextButton, connectToServerButton, connectToServerButtonOptions, serverButton, moveAllNotesUpButton;
    [SerializeField] private ApplicantNotes applicantNotes;
    [SerializeField] private GameObject postItNotePrefab;
    private bool _notesSentToServer = false;
    
    // Set the default IP address for the UI input field
    private void Awake()
    {
        AddListenersToUI();
        
        // Set the input field text to the default IP address
        ipAddressInputField.text = defaultIpAddress;
        
        //TODO: Disable the Camera. Right now the blackBackgroundUI is used to hide the camera view
        
        ShowIntroUI();
    }
    
    private void ShowIntroUI()
    {
        introUIObject.SetActive(true);
        blackBackgroundUI.SetActive(true);
        createApplicantUIObject.SetActive(false);
        arSettingsUI.SetActive(false);
        connectUIObject.SetActive(false);
    }
    
    private void NextButtonIntro()
    {
        // Disable the intro UI
        introUIObject.SetActive(false);
        
        // Enable the create applicant UI
        createApplicantUIObject.SetActive(true);
    }
    
    // Method for showing the connect to server UI
    public void ShowConnectUI()
    {
        // Enable the connect to server UI
        if (connectUIObject)
        {
            connectUIObject.SetActive(true);
            //Debug.Log("Connect UI enabled");
        }
    }
    
    private void AddListenersToUI()
    {
        // Add listeners to the buttons
        connectToServerButton.onClick.AddListener(ConnectToServer);
        connectToServerButtonOptions.onClick.AddListener(ConnectToServer);
        nextButton.onClick.AddListener(NextButtonIntro);
        
        serverButton.onClick.AddListener(StartServer);
        moveAllNotesUpButton.onClick.AddListener(MoveAllNotesUp);
    }
    
    private void MoveAllNotesUp()
    {
        NoteManager.Instance.MoveAllNotes();
        //NoteManager.Instance.SetNoteData();
    }
    
    // Method for connecting to the server
    private void ConnectToServer()
    {
        SetIPAddress(); 
        
        // Connect to the server
        NetworkManager.Singleton.StartClient();
        
        // Subscribe to the client connected and disconnected events
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }
    
    private void SetIPAddress()
    {
        // Get the transport
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        
        // Set the IP address (Overwrites the default IP address in the network manager)
        transport.ConnectionData.Address = ipAddressInputField.text;
    }
    
    // Method for starting the server
    private void StartServer()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        
        // Start the server
        NetworkManager.Singleton.StartServer();
        Debug.Log("Server started");
        
        // Disable the connect UI
        connectUIObject.SetActive(false);
        blackBackgroundUI.SetActive(false);
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsClient && NetworkManager.Singleton.IsConnectedClient)
        {
            Debug.Log($"Client {clientId} connected to the server");
            
            // Disable the connect UI
            connectUIObject.SetActive(false);
            blackBackgroundUI.SetActive(false);
            
            // Enable AR settings UI 
            arSettingsUI.SetActive(true);

            if (!_notesSentToServer)
            {
                // Send all applicant notes to the server
                SendAllNotesToServer();
            }
            
            // Unsubscribe from the callback to prevent multiple subscriptions
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }

        if (IsServer)
        {
            Debug.Log($"Client {clientId} connected to the server");
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsClient && !NetworkManager.Singleton.IsConnectedClient)
        {
            Debug.Log($"Client {clientId} disconnected from the server");

            //TODO: Handle UI updates or reconnection if the client disconnects
            
            // Unsubscribe from the callback
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        if (IsServer)
        {
            Debug.Log($"Client {clientId} disconnected from the server");
        }
    }

    private void SendAllNotesToServer()
    {
        foreach (var applicant in applicantNotes.applicants)
        {
            foreach (var noteText in applicant.notes)
            {
                if (noteText != "" && noteText != null)
                {
                    CreateNoteServerRpc(noteText, applicant.applicantColour, applicant.applicantNumber);
                    Debug.Log($"Note sent to server: {noteText}");
                }
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void CreateNoteServerRpc(string text, Color color, int applicantNumber, ServerRpcParams rpcParams = default)
    {
        GameObject postItNoteObject = Instantiate(postItNotePrefab);
        
        postItNoteObject.transform.position = GetNotePosition(applicantNumber);
        
        PostItNoteNetwork postItNoteNetwork = postItNoteObject.GetComponent<PostItNoteNetwork>();
        
        if (postItNoteNetwork != null)
        {
            // Set the note data directly on the server
            postItNoteNetwork.noteText.Value = new FixedString512Bytes(text);
            postItNoteNetwork.noteColor.Value = color;
        }
        else
        {
            Debug.LogError("PostItNoteNetwork component is missing on the postItNotePrefab.");
        }
        
        NetworkObject networkObject = postItNoteObject.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            networkObject.Spawn();
        }
        else
        {
            Debug.LogError("NetworkObject component is missing on the postItNotePrefab.");
        }
    }

    // TODO: LORD LINUS!
    private Vector3 GetNotePosition(int applicantNumber)
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        Debug.Log($"Client ID: {clientId}");
        
        return new Vector3(0, 0, 0);
    }
    
}
