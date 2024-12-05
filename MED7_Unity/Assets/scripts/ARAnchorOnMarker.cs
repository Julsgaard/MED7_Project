using System;
using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARAnchorOnMarker : MonoBehaviour
{
    [SerializeField]
    private ARAnchorManager anchorManager;
    [SerializeField]
    private GameObject planeInstance; //prefabToPlace
    [SerializeField]
    private ARTrackedImageManager trackedImageManager;

    private GameObject markerCoordinateSystem;
    private NetworkObject networkPlane;
    //private PostItParentNetwork planeNetwork;

    public bool isMarkerFound;

    private XROrigin xrOrigin;
    private ARAnchor newAnchor;

    //private GameObject instance;
    public GameObject GetMarkerCoordinateSystem()
    {
        if (planeInstance != null)
            return planeInstance;

        Debug.Log("Plane instance was null!");
        return null;
    }

    private void Start()
    {
        xrOrigin = FindObjectOfType<XROrigin>();
        anchorManager = FindObjectOfType<ARAnchorManager>();
        
        networkPlane = planeInstance.GetComponent<NetworkObject>();
        //networkObject.Spawn();
        
        //planeNetwork = planeInstance.GetComponent<PostItParentNetwork>();
        
        markerCoordinateSystem = new GameObject("Marker Coordinate System");
        markerCoordinateSystem.transform.SetPositionAndRotation(planeInstance.transform.position, planeInstance.transform.rotation);
        
    }

    void OnEnable()
    {
        // Subscribe to image tracking events
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // When a new image is detected
        foreach (ARTrackedImage trackedImage in args.added) 
        {
            //AnchorContent(trackedImage.transform.position, prefabToPlace);
            AnchorContent(trackedImage.transform);
            isMarkerFound = true;
        }
        foreach (var updatedImage in args.updated)
        {
            if (NetworkManager.Singleton.IsHost)
                return;
            
            //instance.transform.position = updatedImage.transform.position;
            Debug.Log($"Setting planeInstance (name: {planeInstance.name}) position (from: {planeInstance.transform.position}) to updatedImage position ({updatedImage.transform.position}).");
            // planeInstance.transform.position = updatedImage.transform.position;
            planeInstance.transform.SetPositionAndRotation(updatedImage.transform.position, updatedImage.transform.rotation);
            // markerCoordinateSystem.transform.SetPositionAndRotation(planeInstance.transform.position, planeInstance.transform.rotation);
            AlignToMarker(updatedImage.transform);
        }

    }
    
    public void AlignToMarker(Transform markerTransform)
    {
        planeInstance.transform.position = markerTransform.position;
        planeInstance.transform.rotation = markerTransform.rotation;
        
        networkPlane.transform.position = markerTransform.position;
        networkPlane.transform.rotation = markerTransform.rotation;
        // XROrigin xrOrigin = FindObjectOfType<XROrigin>();
        //
        // // Apply the offset directly to the XROrigin's transform
        // xrOrigin.transform.position += markerTransform.position - xrOrigin.CameraFloorOffsetObject.transform.position;
        //
        // newAnchor.transform.position = markerTransform.position;
    }

    
    private void AnchorContent(Transform markerTransform)
    {
        //instance = Instantiate(prefab, position, Quaternion.identity);
        planeInstance.transform.position = markerTransform.position;
        planeInstance.transform.rotation = markerTransform.rotation;
        
        networkPlane.transform.position = markerTransform.position;
        networkPlane.transform.rotation = markerTransform.rotation;
        
        //if (instance.GetComponent<ARAnchor>() == null)
        if (planeInstance.GetComponent<ARAnchor>() == null)
        {
            //instance.AddComponent<ARAnchor>();
            planeInstance.AddComponent<ARAnchor>();
        }
    }
    void OnDisable()
    {
        // Unsubscribe from image tracking events
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
    
}
