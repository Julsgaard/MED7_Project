using System.Net;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject arSessionObject;
    [SerializeField] private string defaultIpAddress = "192.168.50.141";
    [SerializeField] private TMP_InputField ipAddressInputField;
    [SerializeField] private TMP_InputField portInputField;
    [SerializeField] private GameObject connectUIObject;
    [SerializeField] private GameObject introUIObject;
    [SerializeField] private GameObject createApplicantUIObject;
    
    // Set the default IP address for the UI input field
    private void Awake()
    {
        // Set the input field text to the default IP address
        ipAddressInputField.text = defaultIpAddress;
        
        // Disable the AR session object for the beginning
        arSessionObject.SetActive(false);
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
        if (connectUIObject)
        {
            connectUIObject.SetActive(false);
            Debug.Log("Connect UI disabled");
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
}
