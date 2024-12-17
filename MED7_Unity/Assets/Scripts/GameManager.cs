using System;
using System.Collections;
using System.Collections.Generic;
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
    private bool _isFindingMarker, _isNotesButtonClicked;

    [Header("Android Specific GameObjects")]
    [SerializeField] private GameObject xrOrigin;
    [SerializeField] private GameObject arSession, windowsCamera, manomotionManager, gizmoCanvas, skeletonManager;
    
    [Header("PostIt Spawn Layout")]
    // [SerializeField] private GameObject postItParentLocal;
    [SerializeField] private GameObject postItNotePrefab;
    private bool _notesSentToServer;
    [SerializeField] private float sameApplicantOffset = .03f;
    [SerializeField] private float diffApplicantOffset = .05f;
    [SerializeField] private ARAnchorOnMarker arAnchorOnMarker;
    
    [Header("Script References")]
    [SerializeField] private ApplicantNotes applicantNotes;

    private readonly List<ulong> _clients = new List<ulong>();

    private void Awake()
    {
        AddListenersToUI();
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
        introUIObject.SetActive(false);
        createApplicantUIObject.SetActive(true);
    }
    
    public void ShowConnectUI()
    {
        if (connectUIObject)
            connectUIObject.SetActive(true);
    }
    
    private void AddListenersToUI()
    {
        connectToServerButton.onClick.AddListener(ConnectToServer);
        connectToServerButtonOptions.onClick.AddListener(ConnectToServer);
        nextButton.onClick.AddListener(NextButtonIntro);
        serverButton.onClick.AddListener(StartServer);
    }
    
    private void ConnectToServer()
    {
        SetIPAddress();

        if (NetworkManager.Singleton.IsClient) return;
        
        NetworkManager.Singleton.StartClient();
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }
    
    private void SetIPAddress()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ipAddressInputField.text; // override default networkmanager IP
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
            // Disable the connect UI
            connectUIObject.SetActive(false);
            blackBackgroundUI.SetActive(false);
            
            // Enable AR settings UI 
            arSettingsUI.SetActive(true);

            if (!_notesSentToServer)
            {
                StartCoroutine(UserToFindMarkerBeforeSpawnNotes());
            }
            
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
        
        findMarkerUI.SetActive(true);
        CanvasGroup buttonCg = placeNotesButton.GetComponent<CanvasGroup>();
        buttonCg.alpha = 0;
        
        markerInstruction.text = "Find the marker and place it within view of the camera view";
        yield return new WaitUntil(() => arAnchorOnMarker.isMarkerFound);

        placeNotesButton.onClick.AddListener(SendAllNotesToServer);
        buttonCg.alpha = 1;
        
        markerInstruction.text = "Position yourself around the table. When you're ready, place your notes.";
        yield return new WaitUntil(() => _isNotesButtonClicked);
        
        buttonCg.alpha = 0;
        
        markerInstruction.text = "Creating your notes...";
        yield return new WaitUntil(() => _notesSentToServer);
        
        markerInstruction.text = "Success!";
        yield return new WaitForSeconds(2f);
        
        markerInstruction.text = "You can move your notes around using a pinch gesture!";
        yield return new WaitForSeconds(8f);
        
        findMarkerUI.SetActive(false);
        
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsClient)
        {
            Debug.Log($"Client {clientId} disconnected from the server");
    
            connectUIObject.SetActive(true); 
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            
            StopAllCoroutines();
        }
        if (IsServer)
        {
            DataLogger.instance.LogClientDisconnected(clientId);
        }
    }

    private void SendAllNotesToServer()
    {
        _isNotesButtonClicked = true;
        
        float currentBaseOffsetX = 0; // offset starts at 0
        float postItWidth = postItNotePrefab.transform.localScale.x; // store the size we've given post its
        float sameApplicantNoteOffset = postItWidth + (postItWidth * sameApplicantOffset); // calc offset based on size
        
        float applicantNum = 0; // numbers for indenting the offset between new applicant notes
        float totalCols = 0;
        
        foreach (var applicant in applicantNotes.applicants)
            totalCols += (float)Math.Round(Math.Sqrt(applicant.notes.Count));
        float totalNotesWidth = totalCols * sameApplicantNoteOffset - (sameApplicantOffset * applicantNum) + diffApplicantOffset * (applicantNum-1);
        float centerOffsetX = (totalNotesWidth / 2);
        
        foreach (var applicant in applicantNotes.applicants)
        {
            float currApplicantNumNotes = applicant.notes.Count; // the number of total notes for this applicant
            float currApplicantNumCols = (float)Math.Round(Math.Sqrt(currApplicantNumNotes)); // sqrt for square layout
            float currNoteX = 0; // keep track of current x pos
            float currNoteY = 0; // same for y
            
            foreach (var noteText in applicant.notes)
            {
                if (!string.IsNullOrEmpty(noteText))
                {
                    float posX = currentBaseOffsetX + currNoteX * sameApplicantNoteOffset - centerOffsetX;
                    float posZ = currNoteY * sameApplicantNoteOffset;
                    
                    Vector3 newPos = new Vector3(posX, 0, posZ);
                    Quaternion newRot = Quaternion.identity; // TODO: Maybe remove this if we don't want the notes to rotate
                    CreateNoteServerRpc(newPos, newRot, noteText, applicant.applicantColour, applicant.applicantNumber);
                    
                    // increment positions
                    currNoteX++;
                    if (currNoteX >= currApplicantNumCols) 
                    {
                        currNoteX = 0;
                        currNoteY++;
                    }
                }
            }
            
            applicantNum++;
            currentBaseOffsetX += (sameApplicantNoteOffset * currApplicantNumCols) + (diffApplicantOffset-sameApplicantOffset);
        }
        _notesSentToServer = true;
        UpdateOldNotesServerRpc();
    }
    
    // Server RPC method for creating the note on the server. RequireOnwership is set to false, it allows the client to create the note on the server
    [ServerRpc(RequireOwnership = false)]
    public void CreateNoteServerRpc(Vector3 newPos, Quaternion newRot, string text, Color color, int applicantNumber, ServerRpcParams rpcParams = default)
    {
        // Instantiate the note on the server using the post-it note prefab
        GameObject postItNoteObject = Instantiate(postItNotePrefab);
        
        // Get the network object and spawn it
        NetworkObject networkObject = postItNoteObject.GetComponent<NetworkObject>();
        networkObject.Spawn(); // Synchronizes the object across the network
        
        // Get the postItNoteNetwork script from the object and set the note's properties
        PostItNoteNetwork postItNoteNetwork = postItNoteObject.GetComponent<PostItNoteNetwork>();
        postItNoteNetwork.StartNote(color, new FixedString512Bytes(text), newPos, newRot);
        
        // Get the sender's client ID for logging
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        
        _clients.Add(senderClientId); //TODO: LocalClientId is always 0 - maybe use senderClientId instead?
        postItNoteNetwork.AddClient(_clients);
        postItNoteNetwork.ShowObjectToSpecificClients();
        
        // Log the note creation
        DataLogger.instance.LogPostItNoteCreated(newPos, text, color, senderClientId);
    }

    [ServerRpc(RequireOwnership =false)]
    private void UpdateOldNotesServerRpc() 
    {
        foreach (PostItNoteNetwork note in NoteManager.instance.notes)
        {
            note.UpdateNote();
        }
    }
}
