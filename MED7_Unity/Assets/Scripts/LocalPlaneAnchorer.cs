using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.ARFoundation;

public class LocalPlaneAnchorer : NetworkBehaviour
{

    [SerializeField] private ARTrackedImageManager imageManager;
    [SerializeField] private GameObject parentGameObject;
    [SerializeField] private TextMeshPro debugText;

    private NetworkObject _postItParentNetwork;
    private PostItParentNetwork _parentNetworkObject;
    private GameManager _gameManager;
    
    private void Awake()
    {
        if (imageManager != null)
        {
            imageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }
        
        _parentNetworkObject = FindAnyObjectByType<PostItParentNetwork>();
        _gameManager = FindObjectOfType<GameManager>();
    }
    
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
        {
            _gameManager.markerTransformData[NetworkManager.Singleton.LocalClientId] = new PlaneTransformData(trackedImage.transform.position, trackedImage.transform.rotation);
            
            // parentGameObject.transform.SetPositionAndRotation(trackedImage.transform.position, trackedImage.transform.rotation);
            _parentNetworkObject = FindAnyObjectByType<PostItParentNetwork>();
            
            if(_parentNetworkObject == null)
            {
                _postItParentNetwork = parentGameObject.GetComponent<NetworkObject>();
                if (IsServer && !_postItParentNetwork.IsSpawned)
                    _postItParentNetwork.Spawn();
                
                _parentNetworkObject = FindAnyObjectByType<PostItParentNetwork>();
                debugText = parentGameObject.GetComponentInChildren<TextMeshPro>();
            }
            
            if (_parentNetworkObject.gameObject.GetComponent<ARAnchor>() == null)
                _parentNetworkObject.gameObject.AddComponent<ARAnchor>();
        }

        foreach (var trackedImage in args.updated)
        {
            _gameManager.markerTransformData[NetworkManager.Singleton.LocalClientId] = new PlaneTransformData(trackedImage.transform.position, trackedImage.transform.rotation);

            HandleTrackedImageUpdate(trackedImage.transform);
        }
    }
    
    private void HandleTrackedImageUpdate(Transform markerTransform)
    {
        var position = markerTransform.position;
        var rotation = markerTransform.rotation;
        rotation = Quaternion.Euler(90, rotation.eulerAngles.y, 0);
        
        AnchorContentServerRpc(position, rotation, NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AnchorContentServerRpc(Vector3 position, Quaternion rotation, ulong requesterClientId)
    {
        // parentNetworkObject.planePos.Value = position;
        // parentNetworkObject.planeRot.Value = rotation;
        
        UpdateClientPositionClientRpc(position, rotation, requesterClientId);
    }

    [ClientRpc(RequireOwnership =  false)]
    private void UpdateClientPositionClientRpc(Vector3 position, Quaternion rotation, ulong requesterId)
    {
        if (requesterId == NetworkManager.Singleton.LocalClientId)
        {
            _parentNetworkObject.gameObject.transform.position = position;
            _parentNetworkObject.gameObject.transform.rotation = rotation;

            // debugText HAS to be set on every run, otherwise it refers to the one on the server
            debugText = _parentNetworkObject.gameObject.GetComponentInChildren<TextMeshPro>();
            debugText.text = "Client: " + requesterId +
                                        "\n" + position;
        }
    }
}
