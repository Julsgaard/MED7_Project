using System;
using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
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

    [Header("Android Specific GameObjects")]
    [SerializeField] private GameObject xrOrigin;
    [SerializeField] private GameObject arSession, windowsCamera, manomotionManager, gizmoCanvas, skeletonManager;
    
    [Header("PostIt Spawn")]
    [SerializeField] private GameObject postItNotePrefab;
    [SerializeField] private float sameApplicantOffset = .03f;
    [SerializeField] private float diffApplicantOffset = .05f;
    private bool _notesSentToServer;
    public bool markerFound;
    
    [Header("Script References")]
    [SerializeField] private ApplicantNotes applicantNotes;

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
            Debug.Log($"db: Client {clientId} connected to the server!");
            
            // Disable the connect UI
            connectUIObject.SetActive(false);
            blackBackgroundUI.SetActive(false);
            
            // Enable AR settings UI 
            arSettingsUI.SetActive(true);

            if (!_notesSentToServer)
            {
                // Send all applicant notes to the server
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
        // Enable the find marker UI and set the instruction text
        findMarkerUI.SetActive(true);
        markerInstruction.text = "Look at the marker with the camera";
        
        // Wait until the marker is found
        yield return new WaitUntil(() => markerFound);
        SendAllNotesToServer();
        
        // Send post-it notes to the server so they can be spawned
        yield return new WaitUntil(() => _notesSentToServer);
        markerInstruction.text = "Your notes have been placed on the table!";
        
        // Wait for 5 seconds before disabling the find marker UI
        yield return new WaitForSeconds(5f);
        findMarkerUI.SetActive(false);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsClient)
        {
            Debug.Log($"db: Client {clientId} disconnected from the server");

            // Enable the connect UI
            connectUIObject.SetActive(true); 
            
            // Unsubscribe from the callback
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            
            // Stop the coroutine to prevent the UserToFindMarkerBeforeSpawnNotes() coroutine from running multiple times
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
        float currentBaseOffsetX = 0; // offset starts at 0
        //float totalOffsetX = 0; // we also calculate a total offset to later be able to center all notes
        
        float postItWidth = postItNotePrefab.transform.localScale.x; // store the size we've given post its
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
                    // Create the note and send it to the server
                    //postItNoteObject = Instantiate(postItNotePrefab);
                    //Debug.Log($"is postItNoteObject null: {networkObject == null}");
                    // GameObject postit = postItNetworkObject;

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
            //totalOffsetX += (sameApplicantNoteOffset * currApplicantNumCols) // full width of current appl. notes
            //                - sameApplicantOffset; // minus 1*offset which will be added on the end
            
            // if we want to add for another applicant, we set the new corner for where to begin by adding the offset
            applicantNum++;
            currentBaseOffsetX += (sameApplicantNoteOffset * currApplicantNumCols) + (diffApplicantOffset-sameApplicantOffset);
        }
        _notesSentToServer = true;
    }
    
    // Server RPC method for creating the note on the server. RequireOwnership is set to false, it allows the client to create the note on the server
    [ServerRpc(RequireOwnership = false)]
    private void CreateNoteServerRpc(Vector3 newPos, string text, Color color, int applicantNumber, ServerRpcParams rpcParams = default)
    {
        // Creating the note GameObject
        GameObject postItNoteObject = Instantiate(postItNotePrefab, newPos, Quaternion.identity);
        
        // Get NetworkObject and spawn it
        NetworkObject networkObject = postItNoteObject.GetComponent<NetworkObject>();
        networkObject.Spawn();
        
        // Get the PostItNoteNetwork component from the PostItNoteObject
        PostItNoteNetwork postItNoteNetwork = postItNoteObject.GetComponent<PostItNoteNetwork>();
        
        // Set the note data directly on the server
        postItNoteNetwork.noteText.Value = new FixedString512Bytes(text);
        postItNoteNetwork.noteColor.Value = color;
        postItNoteNetwork.notePosition.Value = newPos;
        
        // Log the note creation
        DataLogger.instance.LogPostItNoteCreated(newPos, text, color, 0); //TODO: needs the correct client id
    }
}
