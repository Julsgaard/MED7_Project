using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.ARFoundation;

public class TabletopMarkerAnchorer : NetworkBehaviour
{
    [SerializeField] private ARTrackedImageManager imageManager;
    [SerializeField] private GameObject localTabletopPrefab;
    [SerializeField] private TextMeshPro debugText;

    private GameManager _gameManager;
    private GameObject localTabletopGO;
    
    public bool isMarkerFound;
    
    private void Awake()
    {
        if (imageManager != null)
            imageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }
    
    public GameObject GetTabletopObject() => localTabletopGO;

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
        {
            isMarkerFound = true; 
            localTabletopGO = Instantiate(localTabletopPrefab, trackedImage.transform.position, trackedImage.transform.rotation);
        }
        
        foreach (var trackedImage in args.updated)
            UpdateTabletopPositionAndRotation(trackedImage);
    }

    private void UpdateTabletopPositionAndRotation(ARTrackedImage trackedImage)
    {
        var position = trackedImage.transform.position;
        var rotation = trackedImage.transform.rotation;
        rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        localTabletopGO.transform.SetPositionAndRotation(position, rotation);
    }
}
