using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class PostItParentNetwork : NetworkBehaviour
{
    public NetworkVariable<Vector3> planePos = new NetworkVariable<Vector3>();
    public NetworkVariable<Quaternion> planeRot = new NetworkVariable<Quaternion>();
        
    public  NetworkVariable<Vector3> notePosition = new NetworkVariable<Vector3>();
    public  NetworkVariable<FixedString512Bytes> noteText = new NetworkVariable<FixedString512Bytes>();
    public  NetworkVariable<bool> isBeingMoved = new NetworkVariable<bool>();
    public  NetworkVariable<ulong> movingCLient = new NetworkVariable<ulong>();

    private Vector3 serverPosition;
    private Quaternion serverRotation;
    public override void OnNetworkSpawn()
    {
        // Subscribe to value changes
        planePos.OnValueChanged += OnPositionChanged;
        planeRot.OnValueChanged += OnRotationChanged;
        
        // Initialize with current values
        OnPositionChanged(Vector3.zero, planePos.Value);
        OnRotationChanged(Quaternion.identity, planeRot.Value);
    }

    private void OnRotationChanged(Quaternion previousvalue, Quaternion newvalue)
    {
        RequestServerPositionClientRpc();
        gameObject.transform.localRotation = newvalue * serverRotation;
    }

    private void OnPositionChanged(Vector3 oldPosition, Vector3 newPosition)
    {
        RequestServerPositionClientRpc();
        gameObject.transform.localPosition = newPosition + serverPosition;
    }
    
    public void RequestMoveNote(Vector3 movement)
    {
        if (isBeingMoved.Value)
        {
            if (movingCLient.Value != NetworkManager.Singleton.LocalClientId)
            {
                return;
            }
            else
            {
                Vector3 newPosition = gameObject.transform.localPosition + movement;
                RequestMoveServerRpc(newPosition);
            }
            return;
        }
        else
        {
            isBeingMoved.Value = true;
            movingCLient.Value = NetworkManager.Singleton.LocalClientId;
            Vector3 newPosition = gameObject.transform.localPosition + movement;
            RequestMoveServerRpc(newPosition);

        }
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestMoveServerRpc(Vector3 newPosition, ServerRpcParams rpcParams = default)
    {
        // Server updates the note position
        notePosition.Value = newPosition;
        Debug.Log($"Server moved note to {newPosition} for client {rpcParams.Receive.SenderClientId}");
    }
    [ClientRpc]
    public void RequestServerPositionClientRpc()
    {
        if (IsServer)
        {
            SendServerPositionToClientRpc(transform.position, transform.rotation);
        }
    }

    [ClientRpc]
    private void SendServerPositionToClientRpc(Vector3 position, Quaternion rotation)
    {
        serverPosition = position;
        serverRotation = rotation;
    }

}
