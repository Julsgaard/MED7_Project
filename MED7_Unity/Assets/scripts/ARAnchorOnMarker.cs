using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARAnchorOnMarker : NetworkBehaviour
{
    [SerializeField] private ARAnchorManager anchorManager;
    [SerializeField] private GameObject planeInstance;
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    
    private GameObject _arAnchorObject;
    private NetworkObject _networkPlane;

    public bool isMarkerFound;
    
    public GameObject GetMarkerCoordinateSystem()
    {
        if (planeInstance != null)
            return planeInstance;

        return null;
    }

    public override void OnNetworkSpawn()
    {
        anchorManager = FindObjectOfType<ARAnchorManager>();
        _networkPlane = planeInstance.GetComponent<NetworkObject>();
        
        if (!_networkPlane.IsSpawned)
            _networkPlane.Spawn();
    }

    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }
    
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added) 
        {
            var position = trackedImage.transform.position;
            var rotation = trackedImage.transform.rotation;
            
            RequestAlignToMarkerServerRpc(position, rotation);
            CreateArAnchor(position, rotation);
            
            isMarkerFound = true;
        }
        
        foreach (var updatedImage in args.updated)
        {
            var position = updatedImage.transform.position;
            var rotation = updatedImage.transform.rotation;            
            
            RequestAlignToMarkerServerRpc(position, rotation);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RequestAlignToMarkerServerRpc(Vector3 position, Quaternion rotation, ServerRpcParams serverRpcParams = default)
    {
        MovePlaneToMarkerClientRpc(position, rotation, serverRpcParams.Receive.SenderClientId);
    }
    
    [ClientRpc]
    private void MovePlaneToMarkerClientRpc(Vector3 position, Quaternion rotation, ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            planeInstance.transform.position = position;
            planeInstance.transform.rotation = rotation;
        }
    }
    
    private void CreateArAnchor(Vector3 position, Quaternion rotation)
    {
        if (!IsOwner) return;
        
        _arAnchorObject = new GameObject("ARAnchorObject");
        _arAnchorObject.transform.SetPositionAndRotation(position, rotation);
        _arAnchorObject.AddComponent<ARAnchor>();
    }
    
    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
    
}
