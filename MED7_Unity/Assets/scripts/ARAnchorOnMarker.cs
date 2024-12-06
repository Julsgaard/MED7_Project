using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARAnchorOnMarker : MonoBehaviour
{
    [SerializeField] private GameObject planeInstance; // This is the local plane object
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    public bool isMarkerFound;

    // private GameObject markerCoordinateSystem;
    // private NetworkObject networkPlane;
    // private PostItParentNetwork planePositionNetwork;

    
    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }
    
    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
    
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // CALLED ONLY ONCE: WHEN MARKER IS FIRST DETECTED
        foreach (var trackedImage in args.added) 
        {
            // AnchorContent(trackedImage.transform);
            isMarkerFound = true;
            
            // Anchor locally
            AlignLocalAnchorToMarker(trackedImage.transform);
            
            // networkPlane.Spawn(); // Spawn the plane on the network
        }
        
        //TODO: MAYBE UNCOMMENT THIS AGAIN LATER IF THERE ARE PROBLEMS WITH THE ANCHOR
        // // CALLED EVERY FRAME AFTERWARD: WHEN MARKER IS BEING TRACKED AS IT MOVES
        // foreach (var updatedImage in args.updated)
        // {
        //     /* TODO: SOME LOGIC TO MOVE THE PLANE INSTANCE TO THE UPDATED IMAGE POSITION
        //      - either locally or on the network. doesn't work yet
        //      - tried both but cant get it to work */
        //     
        //     Debug.Log($"Setting planeInstance (name: {planeInstance.name}) position (from: {planeInstance.transform.position}) to updatedImage position ({updatedImage.transform.position}).");
        //     AlignToMarker(updatedImage.transform);
        // }
    }
    
    private void AlignLocalAnchorToMarker(Transform markerTransform)
    {
        // Align the local planeInstance to the markerTransform
        planeInstance.transform.position = markerTransform.position;
        planeInstance.transform.rotation = markerTransform.rotation;
    }
    
    public Vector3 GetMarkerWorldPosition()
    {
        return planeInstance.transform.position;
    }

    public Quaternion GetMarkerWorldRotation()
    {
        return planeInstance.transform.rotation;
    }
    
    
    // public GameObject GetMarkerCoordinateSystem()
    // {
    //     if (planeInstance != null)
    //         return planeInstance;
    //
    //     Debug.Log("Plane instance was null!");
    //     return null;
    // }

    // private void Start()
    // {
    //     anchorManager = FindObjectOfType<ARAnchorManager>();
    //     
    //     // planePositionNetwork = planeInstance.GetComponent<PostItParentNetwork>();
    //     
    //     // networkPlane = planeInstance.GetComponent<NetworkObject>();
    //     
    //     markerCoordinateSystem = new GameObject("Marker Coordinate System");
    //     markerCoordinateSystem.transform.SetPositionAndRotation(planeInstance.transform.position, planeInstance.transform.rotation);
    // }
    
    
    // private void HandleTrackedImageUpdate(ARTrackedImage trackedImage)
    // {
    //     //if (!IsServer) return;
    //
    //     // Broadcast tracked image position and rotation to clients
    //     Vector3 position = trackedImage.transform.position;
    //     Quaternion rotation = trackedImage.transform.rotation;
    //
    //     UpdateTrackedImageClientRpc(trackedImage.referenceImage.name, position, rotation);
    // }

    // [ClientRpc]
    // private void UpdateTrackedImageClientRpc(string imageName, Vector3 position, Quaternion rotation)
    // {
    //     // Update the position and rotation on client devices
    //     var trackedObject = FindObjectOfType<YourTrackedObjectHandler>().GetObjectForImage(imageName);
    //     if (trackedObject != null)
    //     {
    //         trackedObject.transform.position = position;
    //         trackedObject.transform.rotation = rotation;
    //     }
    // }
    
    // [ServerRpc(RequireOwnership = false)]
    // public void AlignToMarker(Transform markerTransform)
    // {
    //     Debug.Log($"Is planeInstance null? {planeInstance == null}");
    //     Debug.Log($"Is planePositionNetwork null? {planePositionNetwork == null}");
    //     
    //     RequestMoveNoteServerRpc(markerTransform.position);
    //     
    //     //planePositionNetwork.planePos.Value = markerTransform.position;
    //     //planePositionNetwork.planeRot.Value = markerTransform.rotation;
    //     
    //     // planeInstance.transform.position = markerTransform.position;
    //     // planeInstance.transform.rotation = markerTransform.rotation;
    //     //
    //     // networkPlane.transform.position = markerTransform.position;
    //     // networkPlane.transform.rotation = markerTransform.rotation;
    //     // XROrigin xrOrigin = FindObjectOfType<XROrigin>();
    //     //
    //     // // Apply the offset directly to the XROrigin's transform
    //     // xrOrigin.transform.position += markerTransform.position - xrOrigin.CameraFloorOffsetObject.transform.position;
    //     //
    //     // newAnchor.transform.position = markerTransform.position;
    // }
    
    // [ServerRpc(RequireOwnership = false)]
    // private void RequestMoveNoteServerRpc(Vector3 position)
    // {
    //     planePositionNetwork.RequestMoveNote(position);
    // }
    
    // [ServerRpc(RequireOwnership = false)]
    // private void AnchorContent(Transform markerTransform)
    // {
    //     planePositionNetwork.planePos.Value = markerTransform.position;
    //     planePositionNetwork.planeRot.Value = markerTransform.rotation;
    //     
    //     if (planeInstance.GetComponent<ARAnchor>() == null)
    //     {
    //         planeInstance.AddComponent<ARAnchor>();
    //     }
    // }
}
