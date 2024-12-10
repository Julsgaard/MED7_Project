using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARAnchorOnMarker : MonoBehaviour
{
    [SerializeField] private ARAnchorManager anchorManager;
    [SerializeField] private GameObject planePrefab;
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [HideInInspector] public GameObject postItParent;
    [HideInInspector] public ARAnchor planeAnchor;
    [HideInInspector] public bool isMarkerFound = false;
    [HideInInspector] public static ARAnchorOnMarker instance;
    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added) 
        {
            CreatePostItParent(trackedImage.transform);
        }
        foreach (var updatedImage in args.updated)
        {
            UpdateTracker(updatedImage.transform);
        }
    }
    
    public void CreatePostItParent(Transform pose)
    {
        postItParent = Instantiate(planePrefab, pose.position, pose.rotation);
        planeAnchor = postItParent.GetComponent<ARAnchor>();
        isMarkerFound = true;
    }

    private void UpdateTracker(Transform newPose)
    {
        postItParent.transform.position = newPose.position;
        var rotation = newPose.rotation;
        rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        postItParent.transform.rotation = rotation;
        planeAnchor.transform.position = newPose.position;
        planeAnchor.transform.rotation = newPose.rotation;
    }


    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
    public GameObject GetLocalPostItParent()
    {
        return postItParent;
    }
}
