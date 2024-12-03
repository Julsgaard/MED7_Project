using System.Net;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //[SerializeField] private GameObject arSessionObject;
    //[SerializeField] private ARSession arSession;
    [SerializeField] private string defaultIpAddress = "192.168.50.141";
    [SerializeField] private TMP_InputField ipAddressInputField;
    //[SerializeField] private TMP_InputField portInputField;
    [SerializeField] private GameObject connectUIObject, introUIObject, createApplicantUIObject, blackBackgroundUI, arSettingsUI;
    [SerializeField] private Button nextButton, connectToServerButton, connectToServerButtonOptions, serverButton, moveAllNotesUpButton;
    [SerializeField] private ApplicantNotes applicantNotes;
    //[SerializeField] private SharedNetworking sharedNetworking;
    [SerializeField] private GameObject postItNotePrefab;
    
    // Set the default IP address for the UI input field
    private void Awake()
    {
        AddListenersToUI();
        
        // Set the input field text to the default IP address
        ipAddressInputField.text = defaultIpAddress;
        
        //TODO: Disable the Camera. Right now the blackBackgroundUI is used to hide the camera view
        //arSession.enabled = false;
        
        //ShowIntroUI();
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
    }
    
    // Method for connecting to the server. Used in the NetworkManagerUI script
    private void ConnectToServer()
    {
        SetIPAddress();
        
        // Connect to the server
        NetworkManager.Singleton.StartClient();
        
        // Check if the client is connected to the server
        if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("Connected to server");
            
            // Disable the connect to server UI
            connectUIObject.SetActive(false);
            blackBackgroundUI.SetActive(false);
            
            // Enable the AR settings UI
            arSettingsUI.SetActive(true);
            
            //arSession.enabled = true;
            //arSession.Reset(); 
            
            //applicantNotes.SendToServer();
            // sharedNetworking.ClientToServerRpc();
        }
        else
        {
            Debug.LogWarning("Failed to connect to server");
        }
    }
    
    private void SetIPAddress()
    {
        // Get the IP address from the input field
        string ipAddress = ipAddressInputField.text;
        
        //TODO: DOES NOT WORK YET
        // Check if the IP address is valid (NOT SURE IF WORKING!)
        if (!IPAddress.TryParse(ipAddress, out _))
        {
            Debug.LogWarning("Invalid IP address");
            return;
        }
            
        // Convert IP address to bytes
        byte[] ipAddressBytes = IPAddress.Parse(ipAddress).GetAddressBytes();
        
        // Set the IP address
        NetworkManager.Singleton.NetworkConfig.ConnectionData = ipAddressBytes;
    }
    
    private void StartServer()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        
        // Start the server
        NetworkManager.Singleton.StartServer();
        Debug.Log("Server started");
    }

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
        foreach (var applicant in applicantNotes.applicants)
        {
            foreach (var noteText in applicant.notes)
            {
                GameObject postItNoteObj = Instantiate(postItNotePrefab);
                NetworkObject networkObject = postItNoteObj.GetComponent<NetworkObject>();
                networkObject.Spawn();
                postItNoteObj.transform.position = new Vector3(0, 0, 0);

                // Set the text for the note
                // TextMeshProUGUI textMeshPro = postItNoteObj.GetComponentInChildren<TextMeshProUGUI>();
                // textMeshPro.text = noteText;
                //
                // // Set the color for the note
                // Renderer renderer = postItNoteObj.GetComponent<Renderer>();
                // renderer.material.color = applicant.applicantColour;
            }
        }
    }

}
