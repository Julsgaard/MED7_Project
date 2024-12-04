using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PostItNoteNetwork : NetworkBehaviour //TODO: DOES NOT WORK, IT JUST SPAWNS THE NOTE INFO FROM THE SERVER
{
    [SerializeField] private NetworkVariable<Vector3> notePosition = new NetworkVariable<Vector3>();
    [SerializeField] private NetworkVariable<FixedString512Bytes> noteText = new NetworkVariable<FixedString512Bytes>();
    [SerializeField] private NetworkVariable<Color> noteColor = new NetworkVariable<Color>();

    public override void OnNetworkSpawn()
    {
        notePosition.OnValueChanged += OnPositionChanged;
        OnPositionChanged(Vector3.zero, notePosition.Value);
        noteText.OnValueChanged += OnTextChanged;
        OnTextChanged(new FixedString512Bytes(), noteText.Value);
        noteColor.OnValueChanged += OnColorChanged;
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
        
        Debug.Log("Set note color: " + newColor);
    }
    
    public void SetNoteData(string textData, Color colorData)
    {
        if (IsClient)
        {
            // noteText.Value = new FixedString512Bytes(textData);
            
            // Clients request the server to set the note data
            SetNoteDataServerRpc(textData, colorData);
            Debug.Log("CLIENT SET NOTE DATA" + textData);
        }

        if (IsServer)
        {
            // Set the note text and colour
            // Convert string to FixedString512Bytes
            // FixedString512Bytes fixedString = new FixedString512Bytes();
            // fixedString.CopyFrom(textData);
            //
            // // Set the note text
            // noteText.Value = fixedString;
            //
            // // Set the note colour
            // noteColor.Value = colorData;
            //
            // Debug.Log("SERVER SET NOTE DATA");
        }
    }
    // private string ConvertFixedString512BytesToString(FixedString512Bytes fixedString)
    // {
    //     return fixedString.ToString();
    // }

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
        Debug.Log($"Server moved note to {newPosition} for client {rpcParams.Receive.SenderClientId}");
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetNoteDataServerRpc(string newText, Color newColor, ServerRpcParams rpcParams = default)
    {
        // Set the note text and colour
        Debug.Log($"Server set note text to {newText} and colour to {newColor} for client {rpcParams.Receive.SenderClientId}");
        
        // Convert string to FixedString512Bytes
        FixedString512Bytes fixedString = new FixedString512Bytes();
        fixedString.CopyFrom(newText);
        
        // Set the note text
        noteText.Value = fixedString;
        
        // Set the note colour
        noteColor.Value = newColor;
    }

    private Vector3 GenerateRandomPosition()
    {
        return new Vector3(Random.Range(-0.2f, 0.2f), 0.1f, Random.Range(-0.2f, 0.2f));
    }
}