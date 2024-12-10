using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Serialization;
using UnityEngine.UI;
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
            CreateTabletopObject(trackedImage);
        
        foreach (var trackedImage in args.updated)
            UpdateTabletopPositionAndRotation(trackedImage);
    }

    private void UpdateTabletopPositionAndRotation(ARTrackedImage trackedImage)
    {
        var position = trackedImage.transform.position;
        var rotation = trackedImage.transform.rotation;
        rotation = Quaternion.Euler(90, rotation.eulerAngles.y, 0);
            
        localTabletopGO.transform.SetPositionAndRotation(position, rotation);
        isMarkerFound = true; 
    }

    private void CreateTabletopObject(ARTrackedImage trackedImage)
    {
        var position = trackedImage.transform.position;
        var rotation = trackedImage.transform.rotation;
        rotation = Quaternion.Euler(90, rotation.eulerAngles.y, 0);
            
        localTabletopGO = Instantiate(localTabletopPrefab, position, rotation);
    }
}
