using System;
using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARAnchorOnMarker : MonoBehaviour
{
    [SerializeField] private ARAnchorManager anchorManager;
    [SerializeField] private GameObject planeInstance; //prefabToPlace
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    
    private GameObject markerCoordinateSystem;
    private NetworkObject networkPlane;
    private PostItParentNetwork planePositionNetwork;

    public bool isMarkerFound;
    
    public GameObject GetMarkerCoordinateSystem()
    {
        if (planeInstance != null)
            return planeInstance;

        Debug.Log("Plane instance was null!");
        return null;
    }

    private void Start()
    {
        anchorManager = FindObjectOfType<ARAnchorManager>();
        
        planePositionNetwork = planeInstance.GetComponent<PostItParentNetwork>();
        
        networkPlane = planeInstance.GetComponent<NetworkObject>();
        networkPlane.Spawn();
        
        markerCoordinateSystem = new GameObject("Marker Coordinate System");
        markerCoordinateSystem.transform.SetPositionAndRotation(planeInstance.transform.position, planeInstance.transform.rotation);
    }

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }
    
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // CALLED ONLY ONCE: WHEN MARKER IS FIRST DETECTED
        foreach (var trackedImage in args.added) 
        {
            AnchorContent(trackedImage.transform);
            isMarkerFound = true;
        }
        
        // CALLED EVERY FRAME AFTERWARD: WHEN MARKER IS BEING TRACKED AS IT MOVES
        foreach (var updatedImage in args.updated)
        {
            /* TODO: SOME LOGIC TO MOVE THE PLANE INSTANCE TO THE UPDATED IMAGE POSITION
             - either locally or on the network. doesn't work yet
             - tried both but cant get it to work */
            
            Debug.Log($"Setting planeInstance (name: {planeInstance.name}) position (from: {planeInstance.transform.position}) to updatedImage position ({updatedImage.transform.position}).");
            AlignToMarker(updatedImage.transform);
        }
    }
    
    private void HandleTrackedImageUpdate(ARTrackedImage trackedImage)
    {
        //if (!IsServer) return;

        // Broadcast tracked image position and rotation to clients
        Vector3 position = trackedImage.transform.position;
        Quaternion rotation = trackedImage.transform.rotation;

        UpdateTrackedImageClientRpc(trackedImage.referenceImage.name, position, rotation);
    }

    [ClientRpc]
    private void UpdateTrackedImageClientRpc(string imageName, Vector3 position, Quaternion rotation)
    {
        // // Update the position and rotation on client devices
        // var trackedObject = FindObjectOfType<YourTrackedObjectHandler>().GetObjectForImage(imageName);
        // if (trackedObject != null)
        // {
        //     trackedObject.transform.position = position;
        //     trackedObject.transform.rotation = rotation;
        // }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void AlignToMarker(Transform markerTransform)
    {
        Debug.Log($"Is planeInstance null? {planeInstance == null}");
        Debug.Log($"Is planePositionNetwork null? {planePositionNetwork == null}");
        
        RequestMoveNoteServerRpc(markerTransform.position);
        
        //planePositionNetwork.planePos.Value = markerTransform.position;
        //planePositionNetwork.planeRot.Value = markerTransform.rotation;
        
        // planeInstance.transform.position = markerTransform.position;
        // planeInstance.transform.rotation = markerTransform.rotation;
        //
        // networkPlane.transform.position = markerTransform.position;
        // networkPlane.transform.rotation = markerTransform.rotation;
        // XROrigin xrOrigin = FindObjectOfType<XROrigin>();
        //
        // // Apply the offset directly to the XROrigin's transform
        // xrOrigin.transform.position += markerTransform.position - xrOrigin.CameraFloorOffsetObject.transform.position;
        //
        // newAnchor.transform.position = markerTransform.position;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestMoveNoteServerRpc(Vector3 position)
    {
        planePositionNetwork.RequestMoveNote(position);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void AnchorContent(Transform markerTransform)
    {
        planePositionNetwork.planePos.Value = markerTransform.position;
        planePositionNetwork.planeRot.Value = markerTransform.rotation;
        
        if (planeInstance.GetComponent<ARAnchor>() == null)
        {
            planeInstance.AddComponent<ARAnchor>();
        }
    }
    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
    
}
