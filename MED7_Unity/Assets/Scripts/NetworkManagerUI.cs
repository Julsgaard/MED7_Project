using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button connectToServerButton;
    [SerializeField] private TMP_InputField ipAddressInputField;
    //[SerializeField] private Button hostButton;
    //[SerializeField] private Button serverButton;
    
    [SerializeField] private GameObject _gameManagerObject;
    [SerializeField] private GameManager _gameManagerScript;
    
    private void Awake()
    {
        // Find GameManager object in the scene
        _gameManagerObject = GameObject.Find("GameManager");
        
        // Get the GameManager script from the GameManager object
        _gameManagerScript = _gameManagerObject.GetComponent<GameManager>();
        
        // Add listener to the connect to server button
        connectToServerButton.onClick.AddListener(() =>
        {
            _gameManagerScript.ConnectToServer();
        });
        
        
        
        // Add listeners to the buttons
        /*serverButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });
        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });*/
    }
}
