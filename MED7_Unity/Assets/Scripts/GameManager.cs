using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    [Header("Network Manager IP")]
    [SerializeField] private string defaultIpAddress;
    
    [Header("UI Objects")]
    [SerializeField] private TMP_InputField ipAddressInputField;
    [SerializeField] private GameObject connectUIObject, introUIObject, createApplicantUIObject, blackBackgroundUI, arSettingsUI;
    [SerializeField] private Button nextButton, connectToServerButton, connectToServerButtonOptions, serverButton, moveAllNotesUpButton;

    [Header("Guide user to find marker")] 
    [SerializeField] private GameObject findMarkerUI;
    [SerializeField] private Button placeNotesButton;
    [SerializeField] private TextMeshProUGUI markerInstruction;
    private bool _isFindingMarker, _isNotesButtonClicked;

    [Header("Android Specific GameObjects")]
    [SerializeField] private GameObject xrOrigin;
    [SerializeField] private GameObject arSession, windowsCamera, manomotionManager, gizmoCanvas, skeletonManager;
    
    [Header("PostIt Spawn Layout")]
    [SerializeField] private GameObject postItParentLocal;
    [FormerlySerializedAs("postItNotePrefab")] [SerializeField] private GameObject networkNotePrefab;
    private bool _notesSentToServer, _localNotesImported;
    [SerializeField] private float sameApplicantOffset = .03f;
    [SerializeField] private float diffApplicantOffset = .05f;
    
    [Header("Script References")]
    [SerializeField] private ApplicantNotes applicantNotes;
    
    
    public Dictionary<ulong, PlaneTransformData> markerTransformData = new Dictionary<ulong, PlaneTransformData>();

    
    private void Awake()
    {
        AddListenersToUI();
        
        // Set the input field text to the default IP address
        ipAddressInputField.text = defaultIpAddress;
        
        ShowIntroUI();
    }
    
    private void ShowIntroUI()
    {
        introUIObject.SetActive(true);
        blackBackgroundUI.SetActive(true);
        createApplicantUIObject.SetActive(false);
        arSettingsUI.SetActive(false);
        connectUIObject.SetActive(false);
        findMarkerUI.SetActive(false);
        
        
        // Changes based on platform
#if UNITY_ANDROID && !UNITY_EDITOR
        // Android specific changes not in editor
        serverButton.gameObject.SetActive(false);
        xrOrigin.SetActive(true);
        arSession.SetActive(true);
        windowsCamera.SetActive(false);
#else
        serverButton.gameObject.SetActive(true);
        xrOrigin.SetActive(false);
        arSession.SetActive(false);
        windowsCamera.SetActive(true);
        manomotionManager.SetActive(false);
        gizmoCanvas.SetActive(false);
        skeletonManager.SetActive(false);
#endif
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
        //NoteManager.Instance.MoveAllNotes();
    }
    
    // Method for connecting to the server
    private void ConnectToServer()
    {
        SetIPAddress();

        // Check if the client is already connected to prevent multiple connections from the same client when pressing the connect button quickly
        if (!NetworkManager.Singleton.IsClient)
        {
            // Connect to the server
            NetworkManager.Singleton.StartClient();
        
            // Subscribe to the client connected and disconnected events
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
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
        
        // Start the server and log it
        NetworkManager.Singleton.StartServer();
        DataLogger.instance.LogServerStarted();
        
        connectUIObject.SetActive(false);
        blackBackgroundUI.SetActive(false);
        introUIObject.SetActive(false);
        arSettingsUI.SetActive(true);
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsClient && NetworkManager.Singleton.IsConnectedClient)
        {
            markerTransformData.Add(clientId, new PlaneTransformData(Vector3.zero, Quaternion.identity));
            
            // Disable the connect UI
            connectUIObject.SetActive(false);
            blackBackgroundUI.SetActive(false);
            
            // Enable AR settings UI 
            arSettingsUI.SetActive(true);

            if (!_notesSentToServer)
            {
                // Send all applicant notes to the server / but first, find marker
                StartCoroutine(UserToFindMarkerBeforeSpawnNotes());
            }
            
            // Unsubscribe from the callback to prevent multiple subscriptions
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
        if (IsServer)
        {
            // Log client connected
            DataLogger.instance.LogClientConnected(clientId);
        }
    }

    private IEnumerator UserToFindMarkerBeforeSpawnNotes()
    {
        if (_isFindingMarker) yield break;
        
        _isFindingMarker = true;
        
        TabletopMarkerAnchorer anchor = FindObjectOfType<TabletopMarkerAnchorer>();
        findMarkerUI.SetActive(true);
        CanvasGroup buttonCg = placeNotesButton.GetComponent<CanvasGroup>();
        buttonCg.alpha = 0;
        
        markerInstruction.text = "Find the marker and place it within view of the camera view";
        yield return new WaitUntil(() => anchor.isMarkerFound);
        
        buttonCg.alpha = 1;
        
        placeNotesButton.onClick.AddListener(SendAllNotesToServer);
        
        markerInstruction.text = "Position yourself around the table. When you're ready, place your notes.";
        yield return new WaitUntil(() => _isNotesButtonClicked);
        
        buttonCg.alpha = 0;
        
        markerInstruction.text = "Creating your notes...";
        yield return new WaitUntil(() => _notesSentToServer);
        
        markerInstruction.text = "Sent to server!";
        yield return new WaitForSeconds(1f);

        _localNotesImported = false;
        
        markerInstruction.text = "Setting  up locally ...";
        RequestImportNotesToLocalServerRpc();
        
        yield return new WaitUntil(() => _localNotesImported);
        
        markerInstruction.text = "Success";
        yield return new WaitForSeconds(1f);
        
        markerInstruction.text = "";
        findMarkerUI.SetActive(false);
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
            
            StopAllCoroutines();
        }
        if (IsServer)
        {
            // Log client disconnected
            DataLogger.instance.LogClientDisconnected(clientId);
        }
    }

    private void SendAllNotesToServer()
    {
        _isNotesButtonClicked = true;
        
        float currentBaseOffsetX = 0; // offset starts at 0
        
        float postItWidth = networkNotePrefab.transform.localScale.x; // store the size we've given post its
        float sameApplicantNoteOffset = postItWidth + (postItWidth * sameApplicantOffset); // calc offset based on size
        
        float applicantNum = 0; // numbers for indenting the offset between new applicant notes
        
        float totalCols = 0;
        foreach (var applicant in applicantNotes.applicants) 
            totalCols += (float)Math.Round(Math.Sqrt(applicant.notes.Count));
        float totalNotesWidth = totalCols * sameApplicantNoteOffset - (sameApplicantOffset * applicantNum) + diffApplicantOffset * (applicantNum-1);
        float centerOffsetX = (totalNotesWidth / 2);
        
        // loop through all applicants and their notes
        foreach (var applicant in applicantNotes.applicants)
        {
            float currApplicantNumNotes = applicant.notes.Count; // the number of total notes for this applicant
            float currApplicantNumCols = (float)Math.Round(Math.Sqrt(currApplicantNumNotes)); // sqrt for square layout

            float currNoteX = 0; // keep track of current x pos
            float currNoteY = 0; // same for y
            
            foreach (var noteText in applicant.notes)
            {
                // Check if the note text is note empty or null
                if (!string.IsNullOrEmpty(noteText))
                {
                    // calculate positions
                    float posX = currentBaseOffsetX + currNoteX * sameApplicantNoteOffset - centerOffsetX;
                    float posZ = currNoteY * sameApplicantNoteOffset;
                    Vector3 newPos = new Vector3(posX, 0, posZ);

                    CreateNoteServerRpc(newPos, noteText, applicant.applicantColour, applicant.applicantNumber);
                    
                    // increment positions
                    currNoteX++;
                    if (currNoteX >= currApplicantNumCols) 
                    {
                        currNoteX = 0;
                        currNoteY++;
                    }
                }
            }
            // if we want to add for another applicant, we set the new corner for where to begin by adding the offset
            applicantNum++;
            currentBaseOffsetX += (sameApplicantNoteOffset * currApplicantNumCols) + (diffApplicantOffset-sameApplicantOffset);
        }
        
        _notesSentToServer = true;
    }
    
    // Server RPC method for creating the note on the server. RequireOnwership is set to false, it allows the client to create the note on the server
    [ServerRpc(RequireOwnership = false)]
    public void CreateNoteServerRpc(Vector3 newPos, string text, Color color, int applicantNumber, ServerRpcParams rpcParams = default)
    {
        // Creating the note GameObject
        GameObject postItNoteObject = Instantiate(networkNotePrefab);
        
        // Get NetworkObject and spawn it
        NetworkObject networkObject = postItNoteObject.GetComponent<NetworkObject>();
        networkObject.Spawn();
        
        // Get the PostItNoteNetwork component from the PostItNoteObject
        PostItNoteNetwork postItNoteNetwork = postItNoteObject.GetComponent<PostItNoteNetwork>();
        
        // Set the note data directly on the server
        postItNoteNetwork.noteText.Value = new FixedString512Bytes(text);
        postItNoteNetwork.noteColor.Value = color;
        postItNoteNetwork.notePosition.Value = newPos;
        
        postItNoteNetwork.newClient(NetworkManager.Singleton.LocalClientId);
        
        // Log the note creation
        DataLogger.instance.LogPostItNoteCreated(newPos, text, color, rpcParams.Receive.SenderClientId); //noTODO: needs the correct client id
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestImportNotesToLocalServerRpc()
    {
        ImportNotesToLocalClientRpc();
    }
    
    [ClientRpc(RequireOwnership = false)]
    private void ImportNotesToLocalClientRpc()
    {
        LocalNoteManager localNoteManager = FindObjectOfType<LocalNoteManager>();
        localNoteManager.RegisterAndSpawnNotesLocally();
        
        _localNotesImported = true;
    }
}
