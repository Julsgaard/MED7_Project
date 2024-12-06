using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.ARFoundation;
using System.Xml.Serialization;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;

public class ImageNetworkAnchorer : NetworkBehaviour
{

    [SerializeField]
    private ARTrackedImageManager imageManager;

    [SerializeField]
    private GameObject postItParentPrefab;

    [SerializeField]
    private XROrigin XROrigin; 

    private PostItParentNetwork postItParent;

    public NetworkVariable<ulong> PostItParentID = new NetworkVariable<ulong>();

    public PostItParentNetwork GetPostItParent()
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(PostItParentID.Value, out NetworkObject obj))
        {
            return obj.GetComponent<PostItParentNetwork>();
        }
        return null;
    }

    public void SetPostItParentID(NetworkObject obj)
    {
        if (IsServer)
        {
            PostItParentID.Value = obj.NetworkObjectId;
        }
    }

    private void Awake()
    {
            if (IsServer) { return; }
            imageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
        {
            if(postItParent == null)
            {
                postItParent = GetPostItParent();
                if(postItParent == null)
                {
                    SpawnParentServerRpc(trackedImage.transform.position, trackedImage.transform.rotation);
                    postItParent = GetPostItParent();
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
        tableUpdatePositionServerRpc(markerTransform.position, markerTransform.rotation);
    }
    [ServerRpc(RequireOwnership = false)]
    private void tableUpdatePositionServerRpc(Vector3 position, Quaternion rotation)
    {
        GetPostItParent().planePos.Value = position;
        GetPostItParent().planeRot.Value = rotation;

        if (GetPostItParent().gameObject.GetComponent<ARAnchor>() == null)
        {
            GetPostItParent().gameObject.AddComponent<ARAnchor>();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SpawnParentServerRpc(Vector3 position, Quaternion roation)
    {
        GameObject newParent = Instantiate(postItParentPrefab, position, roation);
        NetworkObject postItParentNetwork = newParent.GetComponent<NetworkObject>();
        postItParentNetwork.Spawn();
        SetPostItParentID(postItParentNetwork);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetWorldOriginServerRpc(Vector3 position, Quaternion rotation)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            SetWorldOriginClientRpc(position, rotation, client.ClientId);
        }
    }

    [ClientRpc]
    private void SetWorldOriginClientRpc(Vector3 originPosition, Quaternion originRotation, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            if (IsServer) { return; }
            XROrigin.transform.position = originPosition;
            XROrigin.transform.rotation = originRotation;
        }
    }


}
