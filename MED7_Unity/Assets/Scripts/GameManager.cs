using System;
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
    
    [Header("PostIt Spawn Layout")]
    [SerializeField] private GameObject postItParent;
    [SerializeField] private GameObject postItNotePrefab;
    private bool _notesSentToServer = false;
    [SerializeField] private float sameApplicantOffset = .03f;
    [SerializeField] private float diffApplicantOffset = .05f;
    
    
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
        
        connectUIObject.SetActive(false);
        blackBackgroundUI.SetActive(false);
        introUIObject.SetActive(false);
        arSettingsUI.SetActive(true);
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
        if (IsClient)
        {
            Debug.Log($"Client {clientId} disconnected from the server");

            // Enable the connect UI
            connectUIObject.SetActive(true); 
            
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
        // loop through all applicants and their notes
        foreach (var applicant in applicantNotes.applicants)
        {
            float currApplicantNumNotes = applicant.notes.Count;
            float currApplicantNumCols = (float)Math.Round(Math.Sqrt(currApplicantNumNotes));

            float currNoteX = 0;
            float currNoteY = 0;
            float currNoteYLayer = 0;
            
            foreach (var noteText in applicant.notes)
            {
                // Check if the note text is note empty or null
                if (noteText != "" && noteText != null)
                {
                    // Send the note to the server
                    CreateNoteServerRpc(noteText, applicant.applicantColour, applicant.applicantNumber);
                    Debug.Log($"Note sent to server: {noteText}");
                }
            }
        }
    }
    
    // Server RPC method for creating the note on the server, it is called by the client when connected to the server
    // RequireOnwership is set to false, it allows the client to create the note on the server
    [ServerRpc(RequireOwnership = false)]
    public void CreateNoteServerRpc(string text, Color color, int applicantNumber, ServerRpcParams rpcParams = default)
    {
        // Creating the note GameObject
        GameObject postItNoteObject = Instantiate(postItNotePrefab);
        
        // Set the position of the note
        postItNoteObject.transform.position = GetNotePosition(applicantNumber);
        
        // Get NetworkObject and spawn it
        NetworkObject networkObject = postItNoteObject.GetComponent<NetworkObject>();
        networkObject.Spawn();
        
        // Get the PostItNoteNetwork component from the PostItNoteObject
        PostItNoteNetwork postItNoteNetwork = postItNoteObject.GetComponent<PostItNoteNetwork>();

        // Set the note data directly on the server
        postItNoteNetwork.noteText.Value = new FixedString512Bytes(text);
        postItNoteNetwork.noteColor.Value = color;
    }

    // TODO: LORD LINUS!
    private Vector3 GetNotePosition(int applicantNumber)
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        Debug.Log($"Client ID: {clientId}");
        
        return new Vector3(0, 0, 0);
    }
}
