using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button clientButton;
    //[SerializeField] private Button hostButton;
    [SerializeField] private Button serverButton;
    
    private void Awake()
    {
        // Add listeners to the buttons
        serverButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });
        /*hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });*/
        clientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
    }
}
