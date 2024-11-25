using System.Net;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private string defaultIpAddress = "192.168.50.141";
    [SerializeField] private TMP_InputField ipAddressInputField;
    [SerializeField] private bool connectUI = false;
    
    // Set the default IP address for the UI input field
    private void Start()
    {
        ipAddressInputField.text = defaultIpAddress;
    }
    
    
    // Method for connecting to the server used in the NetworkManagerUI script
    public void ConnectToServer()
    {
        // Get the IP address from the input field
        string ipAddress = ipAddressInputField.text;
            
        // Check if the IP address is valid (NOT SURE IF WORKING!)
        if (!IPAddress.TryParse(ipAddress, out _))
        {
            Debug.LogError("Invalid IP address");
            return;
        }
            
        // Convert IP address to bytes
        byte[] ipAddressBytes = IPAddress.Parse(ipAddress).GetAddressBytes();
            
        // Set the IP address
        NetworkManager.Singleton.NetworkConfig.ConnectionData = ipAddressBytes;
            
        // Connect to the server
        NetworkManager.Singleton.StartClient();
    }
    
}
