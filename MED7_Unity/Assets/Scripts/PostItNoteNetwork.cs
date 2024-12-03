using Unity.Netcode;
using UnityEngine;

public class PostItNoteNetwork : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<Vector3> notePosition = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        notePosition.OnValueChanged += OnPositionChanged;
        OnPositionChanged(Vector3.zero, notePosition.Value);
        NoteManager.Instance.RegisterNote(this);
    }

    private void OnPositionChanged(Vector3 oldPosition, Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    public void RequestMoveNote()
    {
        // Clients request the server to move the note
        if (IsClient)
        {
            Vector3 newPosition = GenerateRandomPosition();
            RequestMoveServerRpc(newPosition);
        }

        if (IsServer)
        {
            Vector3 newPosition = GenerateRandomPosition();
            notePosition.Value = newPosition;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestMoveServerRpc(Vector3 newPosition, ServerRpcParams rpcParams = default)
    {
        // Server updates the note position
        notePosition.Value = newPosition;
        Debug.Log($"Server moved note to {newPosition} on behalf of client {rpcParams.Receive.SenderClientId}");
    }

    private Vector3 GenerateRandomPosition()
    {
        return new Vector3(Random.Range(0.1f, 0.2f), 0.1f, Random.Range(0.1f, 0.2f));
    }
}