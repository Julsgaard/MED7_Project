using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class PostItParentNetwork : NetworkBehaviour
{
    public  NetworkVariable<Vector3> planePos = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        // Subscribe to value changes
        planePos.OnValueChanged += OnPositionChanged;
        
        // Initialize with current values
        OnPositionChanged(Vector3.zero, planePos.Value);
    }

    private void OnPositionChanged(Vector3 oldPosition, Vector3 newPosition)
    {
        transform.position = newPosition;
    }
}
