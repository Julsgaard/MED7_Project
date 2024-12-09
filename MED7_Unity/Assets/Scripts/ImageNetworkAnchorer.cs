using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ImageNetworkAnchorer : NetworkBehaviour
{

    [SerializeField]
    private ARTrackedImageManager imageManager;

    [SerializeField]
    private GameObject postItParentPrefab;

    [SerializeField] private TextMeshPro debugText;

    private NetworkObject postItParentNetwork;
    
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
            GameObject newParent = Instantiate(postItParentPrefab,trackedImage.transform);
            
            if(postItParent == null)
            {
                postItParent = FindAnyObjectByType<PostItParentNetwork>();
                
                if(postItParent == null)
                {
                    postItParentNetwork = newParent.GetComponent<NetworkObject>();
                    if (IsServer && !postItParentNetwork.IsSpawned)
                        postItParentNetwork.Spawn();
                    
                    postItParent = FindAnyObjectByType<PostItParentNetwork>();

                    debugText = newParent.GetComponentInChildren<TextMeshPro>();
                }
            }
            
            if (postItParent.gameObject.GetComponent<ARAnchor>() == null)
            {
                postItParent.gameObject.AddComponent<ARAnchor>();
            }
            
        }

        foreach (var trackedImage in args.updated)
        {
            HandleTrackedImageUpdate(trackedImage.transform);
        }
    }
    private void HandleTrackedImageUpdate(Transform markerTransform)
    {
        var position = markerTransform.position;
        var rotation = markerTransform.rotation;
        rotation = Quaternion.Euler(90, rotation.eulerAngles.y, 0);
        
        AnchorContentServerRpc(position, rotation, NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AnchorContentServerRpc(Vector3 position, Quaternion rotation, ulong requesterClientId)
    {
        UpdateClientPositionClientRpc(position, rotation, requesterClientId);
    }

    [ClientRpc]
    private void UpdateClientPositionClientRpc(Vector3 position, Quaternion rotation, ulong requesterId)
    {
        if (requesterId == NetworkManager.Singleton.LocalClientId)
        {
            postItParent.planePos.Value = position;
            postItParent.planeRot.Value = rotation;

            // debugText HAS to be set on every run, otherwise it refers to the one on the server
            debugText = postItParent.gameObject.GetComponentInChildren<TextMeshPro>();
            debugText.text = "Client: " + requesterId +
                             "\nPosition: " + position;
        }
    }
}
