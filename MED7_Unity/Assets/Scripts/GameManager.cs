using System.Net;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class GameManager : MonoBehaviour
{
    //[SerializeField] private GameObject arSessionObject;
    [SerializeField] private ARSession arSession;
    [SerializeField] private string defaultIpAddress = "192.168.50.141";
    [SerializeField] private TMP_InputField ipAddressInputField;
    //[SerializeField] private TMP_InputField portInputField;
    [SerializeField] private GameObject connectUIObject, introUIObject, createApplicantUIObject, blackBackgroundUI;
    [SerializeField] private Button nextButton, connectToServerButton;
    
    // Set the default IP address for the UI input field
    private void Awake()
    {
        AddListenersToUI();
        
        // Set the input field text to the default IP address
        ipAddressInputField.text = defaultIpAddress;
        
        //TODO: Disable the Camera?
        //arSession.enabled = false;
        
        createApplicantUIObject.SetActive(false);
        
        ShowIntroUI();
    }
    
    private void ShowIntroUI()
    {
        introUIObject.SetActive(true);
    }
    
    public void NextButtonIntro()
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
            Debug.Log("Connect UI enabled");
        }
    }
    
    private void AddListenersToUI()
    {
        // Add listeners to the buttons
        connectToServerButton.onClick.AddListener(ConnectToServer);
        nextButton.onClick.AddListener(NextButtonIntro);
    }
    
    // Method for connecting to the server. Used in the NetworkManagerUI script
    public void ConnectToServer()
    {
        SetIPAddress();
        
        // Connect to the server
        NetworkManager.Singleton.StartClient();
        
        // Check if the client is connected to the server
        if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("Connected to server");
        }
        
        // Disable the connect to server UI
        connectUIObject.SetActive(false);
        blackBackgroundUI.SetActive(false);
        
        //arSession.enabled = true;
        //arSession.Reset(); 
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
}
