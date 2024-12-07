using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.ARFoundation;

public class ARAnchorOnMarker : MonoBehaviour
{
    [SerializeField] private GameObject arPlane; // This is the local plane object
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    public bool isMarkerFound;
    
    void Awake()
    {
        arPlane.SetActive(false);
    }
    
    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
    
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // CALLED ONLY ONCE: WHEN MARKER IS FIRST DETECTED
        foreach (var trackedImage in args.added) 
        {
            isMarkerFound = true;
            
            // Anchor locally
            AlignLocalAnchorToMarker(trackedImage.transform);
            
            // Enable the plane object
            arPlane.SetActive(true);
            
            Debug.Log("Found marker: " + trackedImage.transform.position + ", " + trackedImage.transform.rotation);
        }
        
        
        
        // TODO: MAYBE ADD SOME CODE TO RECALIBRATE THE ANCHOR IF THE MARKER OR ANCHOR IS LOST
 
    }
    
    private void AlignLocalAnchorToMarker(Transform markerTransform)
    {
        // Set the position and rotation of the plane object to match the marker
        arPlane.transform.position = markerTransform.position;
        arPlane.transform.rotation = markerTransform.rotation;
        
        Debug.Log("Setting placeInstance position and rotation to match marker: " + markerTransform.position + ", " + markerTransform.rotation);
        
        // Add ARAnchor component to the plane
        if (arPlane.GetComponent<ARAnchor>() == null)
        {
            var arAnchor = arPlane.AddComponent<ARAnchor>();
            Debug.Log("Added ARAnchor component to plane");
            
            Debug.Log("AR Anchor Enabled: " + arAnchor.enabled);
        }
    }
    
    public Vector3 GetMarkerWorldPosition()
    {
        return arPlane.transform.position;
    }

    public Quaternion GetMarkerWorldRotation()
    {
        return arPlane.transform.rotation;
    }
    
}
