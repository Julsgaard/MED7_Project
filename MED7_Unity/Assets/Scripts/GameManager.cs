using System;
using System.Collections.Generic;
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
    
    [Header("PostIt Spawn Layout")]
    [SerializeField] private GameObject postItParent;
    [SerializeField] private GameObject postItNotePrefab;
    [SerializeField] private float sameApplicantOffset = .03f;
    [SerializeField] private float diffApplicantOffset = .05f;
    
    
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
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected");
    }
    
    public void SpawnPostItNote()
    {
        List<GameObject> postItNotes = new List<GameObject>();
        
        float currentBaseOffsetX = 0;
        float totalOffsetX = 0;
        
        float postItWidth = postItNotePrefab.transform.localScale.x;
        float sameApplicantNoteOffset = postItWidth + (postItWidth * sameApplicantOffset);
        
        float applicantNum = 0;
        float noteNum = 0;
        
        foreach (var applicant in applicantNotes.applicants)
        {
            float currApplicantNumNotes = applicant.notes.Count;
            float currApplicantNumCols = (float)Math.Round(Math.Sqrt(currApplicantNumNotes));

            float currNoteX = 0;
            float currNoteY = 0;
            float currNoteYLayer = 0;
            
            foreach (var noteText in applicant.notes)
            {
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

    private GameObject SetupNoteForNetwork()
    {
        GameObject postItNoteObj = Instantiate(postItNotePrefab);
        NetworkObject networkObject = postItNoteObj.GetComponent<NetworkObject>();
        networkObject.Spawn();
        return postItNoteObj;
    }

}
