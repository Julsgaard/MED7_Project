using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.ARFoundation;

public class ImageNetworkAnchorer : NetworkBehaviour
{

    [SerializeField]
    private ARTrackedImageManager imageManager;

    [SerializeField]
    private GameObject postItParentPrefab;


    private PostItParentNetwork postItParent;

    private void Awake()
    {
        if (imageManager != null)
        {
            imageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
        {
            if(postItParent == null)
            {
                postItParent = FindAnyObjectByType<PostItParentNetwork>();
                if(postItParent == null)
                {
                    GameObject newParent = Instantiate(postItParentPrefab,trackedImage.transform);
                    NetworkObject postItParentNetwork = newParent.GetComponent<NetworkObject>();
                    postItParentNetwork.Spawn();
                    postItParent = FindAnyObjectByType<PostItParentNetwork>();
                }
                
            }
            HandleTrackedImageUpdate(trackedImage.transform);
        }

        foreach (var trackedImage in args.updated)
        {
            HandleTrackedImageUpdate(trackedImage.transform);
        }
    }
    private void HandleTrackedImageUpdate(Transform markerTransform)
    {
        AnchorContentServerRpc(markerTransform.position, markerTransform.rotation);
    }
    [ServerRpc(RequireOwnership = false)]
    private void AnchorContentServerRpc(Vector3 position, Quaternion rotation)
    {
        postItParent.planePos.Value = position;
        postItParent.planeRot.Value = rotation;

        if (postItParent.gameObject.GetComponent<ARAnchor>() == null)
        {
            postItParent.gameObject.AddComponent<ARAnchor>();
        }
    }
}
