using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class MarkerDetection : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager imageManager;
    private GameManager _gameManager;
    
    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }
    
    private void OnEnable()
    {
        // Subscribe to the event
        imageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe from the event
        imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
    
    // Called when a marker is detected
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        if (_gameManager.markerFound) return;
        
        foreach (var trackedImage in eventArgs.added)
        {
            Debug.Log($"Image detected: {trackedImage.referenceImage.name}");
            
            _gameManager.markerFound = true;
        }
    }
}
