using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARAnchorOnMarker : MonoBehaviour
{
    [SerializeField]
    private ARAnchorManager anchorManager;
    [SerializeField]
    private GameObject prefabToPlace;
    [SerializeField]
    private ARTrackedImageManager trackedImageManager;

    private GameObject instance;
    void Awake()
    {

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
                AnchorContent(trackedImage.transform.position, prefabToPlace);
        }
        foreach (var updatedImage in args.updated)
        {
            instance.transform.position = updatedImage.transform.position;
        }

    }
    private void AnchorContent(Vector3 position, GameObject prefab)
    {
        instance = Instantiate(prefab, position, Quaternion.identity);

        if (instance.GetComponent<ARAnchor>() == null)
        {
            instance.AddComponent<ARAnchor>();
        }
    }
    void OnDisable()
    {
        // Unsubscribe from image tracking events
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
    
}
