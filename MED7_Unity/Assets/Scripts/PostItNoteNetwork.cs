using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PostItNoteNetwork : NetworkBehaviour
{
    public enum ClientColor
    {
        YELLOW,
        VERMILLION,
        REDDISHPURPLE,
    }

    public NetworkVariable<Vector3> notePosition = new NetworkVariable<Vector3>();
    public NetworkVariable<Quaternion> noteRotation = new NetworkVariable<Quaternion>();
    public NetworkVariable<FixedString512Bytes> noteText = new NetworkVariable<FixedString512Bytes>();
    public NetworkVariable<Color> noteColor = new NetworkVariable<Color>();
    public NetworkVariable<Color> outLineColor = new NetworkVariable<Color>();
    public NetworkVariable<bool> isBeingMoved = new NetworkVariable<bool>();
    public NetworkVariable<ulong> movingClient = new NetworkVariable<ulong>();

    private List<ulong> clients = new List<ulong>();
    private Dictionary<ulong, ClientColor> clientColours;

    string outlineColourVariable = "_OutlineColour";
    string baseColor = "_BaseColour";
    private Renderer unityRenderer;
    
    private Dictionary<ClientColor, Color> clientColorMap = new Dictionary<ClientColor, Color>
    {
        {ClientColor.YELLOW, new Color(240,228,66)},
        {ClientColor.VERMILLION, new Color(213,90,0)},
        {ClientColor.REDDISHPURPLE, new Color(204,121,167)}
    };
    
    private Coroutine _lerpCoroutine;
    
    public override void OnNetworkSpawn()
    {
        // Get the renderer component and set the outline color
        unityRenderer = GetComponent<Renderer>();
        unityRenderer.material.SetColor(outlineColourVariable, noteColor.Value);
        
        // Initialize the clientColours dictionary
        clientColours = new Dictionary<ulong, ClientColor>();
        
        // Subscribe to value changes
        notePosition.OnValueChanged += OnPositionChanged;
        noteText.OnValueChanged += OnTextChanged;
        noteColor.OnValueChanged += OnColorChanged;
        noteRotation.OnValueChanged += OnRotationChanger;
        outLineColor.OnValueChanged += OnOutlineColorChanged;

        // Initialize with current values
        OnPositionChanged(Vector3.zero, notePosition.Value);
        OnTextChanged(new FixedString512Bytes(), noteText.Value);
        OnColorChanged(Color.magenta, noteColor.Value);
        NoteManager.instance.RegisterNote(this);
    }
    
    private void OnRotationChanger(Quaternion oldrotation, Quaternion newRotation)
    {
        if (IsServer) { return; }
        if (ARAnchorOnMarker.instance.GetLocalPostItParent() == null) { return; }
        
        transform.localRotation = ARAnchorOnMarker.instance.GetLocalPostItParent().transform.rotation * newRotation;
    }
    
    private void OnPositionChanged(Vector3 oldPosition, Vector3 newPosition)
    {
        if (IsServer) { return; }

        if (_lerpCoroutine != null)
            StopCoroutine(_lerpCoroutine);
        
        if (ARAnchorOnMarker.instance.GetLocalPostItParent() == null) { return; }
        
        var finalPosition = new Vector3(newPosition.x,
            ARAnchorOnMarker.instance.GetLocalPostItParent().transform.position.y + 0.01f,
            newPosition.z);
        _lerpCoroutine = StartCoroutine(LerpToPosition(oldPosition, finalPosition));
    }

    private IEnumerator LerpToPosition(Vector3 startPosition, Vector3 targetPosition, float lerpTime = 0.1f)
    {
        float distance = Vector3.Distance(startPosition, targetPosition);
        float speed = distance / lerpTime;

        while (Vector3.Distance(transform.localPosition, targetPosition) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        transform.localPosition = targetPosition; // Ensure the final position is set exactly to the target
    }
    
    private void OnTextChanged(FixedString512Bytes oldText, FixedString512Bytes newText)
    {
        // Set the text for the note
        TextMeshPro textMeshPro = GetComponentInChildren<TextMeshPro>();
        textMeshPro.text = newText.ToString();
    }
    
    private void OnColorChanged(Color oldColor, Color newColor)
    {
        unityRenderer.material.SetColor(baseColor, newColor);
    }
    
    private void OnOutlineColorChanged(Color oldColor, Color newColor)
    {
        unityRenderer.material.SetColor(outlineColourVariable, newColor);
    }
    
    public void StartNote(Color startColor, FixedString512Bytes startString, Vector3 startPosition, Quaternion startRotaion)
    {
        unityRenderer = GetComponent<Renderer>();
        noteRotation.Value = startRotaion;
        notePosition.Value = startPosition;
        noteText.Value = startString;
        noteColor.Value = startColor;
        outLineColor.Value = startColor;
    }
    
    public void UpdateNote()
    {
        UpdateNotesClientRpc();
    }
    
    [ClientRpc]
    private void UpdateNotesClientRpc()
    {
        unityRenderer = GetComponent<Renderer>();
        OnPositionChanged(Vector3.zero, notePosition.Value);
        OnRotationChanger(Quaternion.identity, noteRotation.Value);
        OnTextChanged("", noteText.Value);
        OnColorChanged(Color.red, noteColor.Value);
        OnOutlineColorChanged(Color.red, outLineColor.Value);
    }
    
    // TODO: Update note position when we find and update marker
    // TODO: Fix outline color from the dictionary index error
    [ServerRpc(RequireOwnership = false)]
    public void RequestMoveNoteServerRpc(Vector3 movement, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        if (!CanMove(senderClientId))
        {
            Debug.LogWarning($"Move request denied: Note is being moved by {movingClient.Value}, but {senderClientId} tried to move it.");
            return;
        }
        
        // Update the position of the note
        Vector3 newPosition = gameObject.transform.localPosition + movement;
        notePosition.Value = newPosition;
        
        isBeingMoved.Value = true;
        movingClient.Value = senderClientId;
        
        //Log the movement of the note
        DataLogger.instance.LogPostItNoteMove(notePosition.Value, noteText.Value, noteColor.Value, senderClientId);

        // Reset
        isBeingMoved.Value = false;
        movingClient.Value = 0;
    }

    private bool CanMove(ulong senderClientId)
    {
        // Allow movement if no one is moving the note or the sender is the current mover
        return !isBeingMoved.Value || movingClient.Value == senderClientId;
    }
    
    public void ShowObjectToSpecificClients()
    {
        if (IsServer)
        {
            foreach (var client in clients)
            {
                if (client == NetworkManager.Singleton.LocalClientId)
                {
                    ShowObjectToSpecificClientRpc();
                }
            }
        }
    }
    
    //TODO: Show object to specific client this one is currently not working as intended
    [ClientRpc]
    private void ShowObjectToSpecificClientRpc()
    {
        unityRenderer.enabled = true;
        Renderer[] childRenderers = unityRenderer.gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in childRenderers)
        {
            renderer.enabled = true;
        }
    }
    
    public void AddClient(List<ulong> clientIDs)
    {
        if (IsServer)
        {
            foreach (ulong clientID in clientIDs)
            {
                AddClientClientRpc(clientID);
            }
        }
    }
    
    [ClientRpc]
    private void AddClientClientRpc(ulong clientID)
    {
        if (clients.Contains(clientID))
        {
            return;
        }
        clients.Add(clientID);
        NewClient(clientID);
    }
    
    private void NewClient(ulong clientID)
    {
        if (!clientColours.ContainsKey(clientID))
        {
            clientColours[clientID] = (ClientColor)clients.IndexOf(clientID);
        }
    }
}