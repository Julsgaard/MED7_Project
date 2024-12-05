using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PostItNoteNetwork : NetworkBehaviour
{
    public enum clientColor{
        yellow,
        vermillion,
        reddishPurple,
    }

    public  NetworkVariable<Vector3> notePosition = new NetworkVariable<Vector3>();
    public  NetworkVariable<FixedString512Bytes> noteText = new NetworkVariable<FixedString512Bytes>();
    public  NetworkVariable<Color> noteColor = new NetworkVariable<Color>();
    public  NetworkVariable<bool> isBeingMoved = new NetworkVariable<bool>();
    public  NetworkVariable<ulong> movingCLient = new NetworkVariable<ulong>();

    private Dictionary<ulong, clientColor> clientColours = new Dictionary<ulong, clientColor>();

    string outlineColor = "OutlineColour";
    string baseColor = "BaseColour";
    Renderer unityRenderer;

    private Dictionary<clientColor,Color> clientColorMap = new Dictionary<clientColor, Color>
    {
        {clientColor.yellow, new Color(240,228,66)},
        {clientColor.vermillion, new Color(213,90,0)},
        {clientColor.reddishPurple, new Color(204,121,167)}
    };
    public override void OnNetworkSpawn()
    {
        unityRenderer = GetComponent<Renderer>(); // fucking shit ass fuck
        
        // Subscribe to value changes
        notePosition.OnValueChanged += OnPositionChanged;
        noteText.OnValueChanged += OnTextChanged;
        noteColor.OnValueChanged += OnColorChanged;

        // Initialize with current values
        OnPositionChanged(Vector3.zero, notePosition.Value);
        OnTextChanged(new FixedString512Bytes(), noteText.Value);
        OnColorChanged(Color.magenta, noteColor.Value);

        NoteManager.Instance.RegisterNote(this);
        
        //Renderer renderer = GetComponent<Renderer>(); // what the shit
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
        Debug.Log($"unityRenderer");
        // Set the color for the note
        unityRenderer.material.SetColor(baseColor, newColor);
        //Debug.Log("Set note color: " + newColor);
    }

    public void newClient(ulong clientID)
    {
        clientColor lastClientColour = clientColours.LastOrDefault().Value;
        lastClientColour++;
        clientColours.Add(clientID, lastClientColour);
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
            unityRenderer.material.SetColor(outlineColor, clientColorMap[clientColours[NetworkManager.Singleton.LocalClientId]]);
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

    public void RequestStopMove()
    {
        isBeingMoved.Value = false;
        unityRenderer.material.SetColor(outlineColor, noteColor.Value);
    }

}