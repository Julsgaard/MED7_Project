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
    //private NetworkObject networkObject;
    //private PostItParentNetwork planeNetwork;

    public bool isMarkerFound;

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
        //networkObject = planeInstance.GetComponent<NetworkObject>();
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
            AnchorContent(trackedImage.transform.position, planeInstance);
            isMarkerFound = true;
        }
        foreach (var updatedImage in args.updated)
        {
            //instance.transform.position = updatedImage.transform.position;
            planeInstance.transform.position = updatedImage.transform.position;
            markerCoordinateSystem.transform.SetPositionAndRotation(planeInstance.transform.position, planeInstance.transform.rotation);
        }

    }
    private void AnchorContent(Vector3 position, GameObject plane)
    {
        //instance = Instantiate(prefab, position, Quaternion.identity);
        
        //if (instance.GetComponent<ARAnchor>() == null)
        if (plane.GetComponent<ARAnchor>() == null)
        {
            //instance.AddComponent<ARAnchor>();
            plane.AddComponent<ARAnchor>();
        }
    }
    void OnDisable()
    {
        // Unsubscribe from image tracking events
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
    
}
