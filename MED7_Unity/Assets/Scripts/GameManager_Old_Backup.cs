<<<<<<< HEAD
using System;
using System.Collections.Generic;
using System.Net;
=======
>>>>>>> 959e8a58d800dc9724356324a1570980aa1e7c27
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
<<<<<<< HEAD
    //[SerializeField] private SharedNetworking sharedNetworking;
    
    [Header("PostIt Spawn Layout")]
    [SerializeField] private GameObject postItParent;
    [SerializeField] private GameObject postItNotePrefab;
    [SerializeField] private float sameApplicantOffset = .03f;
    [SerializeField] private float diffApplicantOffset = .05f;
    
=======
    [SerializeField] private GameObject postItNotePrefab;
    private bool _notesSentToServer = false;
>>>>>>> 959e8a58d800dc9724356324a1570980aa1e7c27
    
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
<<<<<<< HEAD
        Debug.Log($"Client {clientId} connected");
=======
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
>>>>>>> 959e8a58d800dc9724356324a1570980aa1e7c27
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
<<<<<<< HEAD
    
    public void SpawnPostItNote()
    {
        List<GameObject> postItNotes = new List<GameObject>();
        
        float currentBaseOffsetX = 0;
        float totalOffsetX = 0;
        
        float postItWidth = postItNotePrefab.transform.localScale.x;
        float sameApplicantNoteOffset = postItWidth + (postItWidth * sameApplicantOffset);
        
        float applicantNum = 0;
        float noteNum = 0;
        
=======

    private void SendAllNotesToServer()
    {
        // loop through all applicants and their notes
>>>>>>> 959e8a58d800dc9724356324a1570980aa1e7c27
        foreach (var applicant in applicantNotes.applicants)
        {
            float currApplicantNumNotes = applicant.notes.Count;
            float currApplicantNumCols = (float)Math.Round(Math.Sqrt(currApplicantNumNotes));

            float currNoteX = 0;
            float currNoteY = 0;
            float currNoteYLayer = 0;
            
            foreach (var noteText in applicant.notes)
            {
<<<<<<< HEAD
                GameObject postItNoteObj = SetupNoteForNetwork();
                
                postItNotes.Add(postItNoteObj);
                
                postItNoteObj.transform.SetParent(postItParent.transform);
                
                // Calculate positions
                float posX = currentBaseOffsetX + currNoteX * sameApplicantNoteOffset;
                float posZ = currNoteY * sameApplicantNoteOffset;

                postItNoteObj.transform.position = new Vector3(posX, 0, posZ);
            
                Debug.Log($"Note Position: ({posX}, {posZ})");

                // Increment positions
                currNoteX++;
                if (currNoteX >= currApplicantNumCols) 
                {
                    currNoteX = 0;
                    currNoteY++;
                }
                
                // float posX = currentBaseOffsetX + currNoteX * sameApplicantNoteOffset + curr;
                // float newZpos = currNoteY % currApplicantNumCols;
                // Debug.Log($"NewZpos: {newZpos}");
                // float posZ = newZpos * sameApplicantNoteOffset;
                // //float posZ = (currNoteY % currApplicantNumCols) * sameApplicantNoteOffset + currNoteYLayer * sameApplicantNoteOffset;
                // postItNoteObj.transform.position = new Vector3(posX, 0, posZ);
                //
                // //Debug.Log($"NoteX for Note {noteNum}: {currNoteX}");
                //
                // // if new line, we want x pos to be 0 again, otherwise increment
                // if (currNoteY % currApplicantNumCols == 0)
                // {
                //     currNoteX = 0;
                //     //currNoteYLayer++;
                // }
                // else
                // {
                //     currNoteX++;
                // }
                //
                // currNoteY++;

                // Set the text for the note
                TextMeshPro textMeshPro = postItNoteObj.GetComponentInChildren<TextMeshPro>();
                textMeshPro.text = noteText;
                
                // Set the color for the note
                Renderer r = postItNoteObj.GetComponent<Renderer>();
                r.material.color = applicant.applicantColour;
                
                Debug.Log($"Calculating pos for Note {noteNum}: ({posX},{posZ}) BaseOffset is {currentBaseOffsetX}) + noteX is {currNoteX}," +
                          $"noteY is {currNoteY}, currYLayer is {currNoteYLayer}." +
                          $"nulCols: {currApplicantNumCols}");

                noteNum++;
=======
                // Check if the note text is note empty or null
                if (noteText != "" && noteText != null)
                {
                    // Send the note to the server
                    CreateNoteServerRpc(noteText, applicant.applicantColour, applicant.applicantNumber);
                    Debug.Log($"Note sent to server: {noteText}");
                }
>>>>>>> 959e8a58d800dc9724356324a1570980aa1e7c27
            }

            totalOffsetX += (sameApplicantNoteOffset * currApplicantNumCols) // full width of current appl. notes
                            - sameApplicantOffset;
            
            // if we want to add for another applicant, we set the new corner for where to begin by adding the offset
            applicantNum++;
            currentBaseOffsetX = totalOffsetX + (diffApplicantOffset * postItWidth * applicantNum);
        }
        
        Vector3 offsetXVector = new Vector3(currentBaseOffsetX / 2, 0, 0);

        foreach (var note in postItNotes)
        {
            note.gameObject.transform.position -= offsetXVector;
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

<<<<<<< HEAD
    private GameObject SetupNoteForNetwork()
    {
        GameObject postItNoteObj = Instantiate(postItNotePrefab);
        NetworkObject networkObject = postItNoteObj.GetComponent<NetworkObject>();
        networkObject.Spawn();
        return postItNoteObj;
    }

=======
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
>>>>>>> 959e8a58d800dc9724356324a1570980aa1e7c27
}
