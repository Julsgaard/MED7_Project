using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PostItNoteNetwork : NetworkBehaviour
{
    public  NetworkVariable<Vector3> notePosition = new NetworkVariable<Vector3>();
    public  NetworkVariable<FixedString512Bytes> noteText = new NetworkVariable<FixedString512Bytes>();
    public  NetworkVariable<Color> noteColor = new NetworkVariable<Color>();

    public override void OnNetworkSpawn()
    {
        // Subscribe to value changes
        notePosition.OnValueChanged += OnPositionChanged;
        noteText.OnValueChanged += OnTextChanged;
        noteColor.OnValueChanged += OnColorChanged;
        
        // Initialize with current values
        OnPositionChanged(Vector3.zero, notePosition.Value);
        OnTextChanged(new FixedString512Bytes(), noteText.Value);
        OnColorChanged(Color.magenta, noteColor.Value);
        
        NoteManager.Instance.RegisterNote(this);
    }

    private void OnPositionChanged(Vector3 oldPosition, Vector3 newPosition)
    {
        transform.position = newPosition;
    }
    private void OnTextChanged(FixedString512Bytes oldText, FixedString512Bytes newText)
    {
        // Set the text for the note
        TextMeshPro textMeshPro = GetComponentInChildren<TextMeshPro>();
        textMeshPro.text = newText.ToString();
        
        Debug.Log("Set note text: " + newText);
    }
    private void OnColorChanged(Color oldColor, Color newColor)
    {
        // Set the color for the note
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.color = newColor;
        
        //Debug.Log("Set note color: " + newColor);
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
    public void RequestMoveServerRpc(Vector3 newPosition, ServerRpcParams rpcParams = default)
    {
        // Server updates the note position
        notePosition.Value = newPosition;
        Debug.Log($"Server moved note to {newPosition} for client {rpcParams.Receive.SenderClientId}");
    }

    private Vector3 GenerateRandomPosition()
    {
        return new Vector3(Random.Range(-0.2f, 0.2f), 0.1f, Random.Range(-0.2f, 0.2f));
    }
}