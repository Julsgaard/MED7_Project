using Unity.Netcode;
using UnityEngine;

public class PostItNoteNetwork : NetworkBehaviour
{
    private NetworkVariable<Vector3> notePosition = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        notePosition.OnValueChanged += OnPositionChanged;

        // Apply the initial position
        OnPositionChanged(Vector3.zero, notePosition.Value);

        if (IsOwner)
        {
            NoteManager.Instance.RegisterNote(this);
        }
    }

    private void OnDestroy()
    {
        notePosition.OnValueChanged -= OnPositionChanged;
    }

    private void OnPositionChanged(Vector3 oldPosition, Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    public void MoveNote()
    {
        // if (IsServer)
        // {
        //     // Server moves the note directly
        //     Debug.Log("Server moving the note.");
        //     Vector3 newPosition = GenerateRandomPosition();
        //     UpdatePosition(newPosition);
        // }
        // else if (IsClient)
        // {
        //     // Client requests the server to move the note
        //     Debug.Log("Client requesting note move.");
        //     RequestMoveServerRpc(GenerateRandomPosition());
        // }


        RequestMoveServerRpc(GenerateRandomPosition());
        
    }
    

    [ServerRpc(RequireOwnership = false)]
    private void RequestMoveServerRpc(Vector3 newPosition, ServerRpcParams rpcParams = default)
    {
        // Server handles the move request
        Debug.Log($"Server received move request from client {rpcParams.Receive.SenderClientId}");
        UpdatePosition(newPosition);
    }

    private void UpdatePosition(Vector3 newPosition)
    {
        notePosition.Value = newPosition;
        Debug.Log($"Updated note position to {newPosition}");
    }

    private Vector3 GenerateRandomPosition()
    {
        return new Vector3(Random.Range(0.1f, 0.2f), 0.1f, Random.Range(0.1f, 0.2f));
    }
}