using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Networking.Transport;
using Unity.VisualScripting;
using UnityEngine;

public class PostItNoteNetwork : NetworkBehaviour
{
    public enum clientColor
    {
        yellow,
        vermillion,
        reddishPurple,
    }

    public NetworkVariable<Vector3> notePosition = new NetworkVariable<Vector3>();
    public NetworkVariable<Quaternion> noteRotation = new NetworkVariable<Quaternion>();
    public NetworkVariable<FixedString512Bytes> noteText = new NetworkVariable<FixedString512Bytes>();
    public NetworkVariable<Color> noteColor = new NetworkVariable<Color>();
    public NetworkVariable<Color> outLineColor = new NetworkVariable<Color>();
    public NetworkVariable<bool> isBeingMoved = new NetworkVariable<bool>();
    public NetworkVariable<ulong> movingCLient = new NetworkVariable<ulong>();

    private List<ulong> clients = new List<ulong>();
    private Dictionary<ulong, clientColor> clientColours = new Dictionary<ulong, clientColor>();

    string outlineColourVariable = "_OutlineColour";
    string baseColor = "_BaseColour";
    private Renderer unityRenderer;


    private Dictionary<clientColor, Color> clientColorMap = new Dictionary<clientColor, Color>
    {
        {clientColor.yellow, new Color(240,228,66)},
        {clientColor.vermillion, new Color(213,90,0)},
        {clientColor.reddishPurple, new Color(204,121,167)}
    };
    public override void OnNetworkSpawn()
    {
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
    private void Awake()
    {
        unityRenderer = GetComponent<Renderer>();
        unityRenderer.material.SetColor(outlineColourVariable, noteColor.Value);
        unityRenderer.enabled = false;
        Renderer[] childRenderers = unityRenderer.gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in childRenderers)
        {
            renderer.enabled = false;
        }
    }
    private void OnRotationChanger(Quaternion oldrotation, Quaternion newRotation)
    {
        if (IsServer) { return; }
        transform.localRotation = ARAnchorOnMarker.instance.GetLocalPostItParent().transform.rotation * newRotation;
    }
    private void OnPositionChanged(Vector3 oldPosition, Vector3 newPosition)
    {
        if (IsServer) { return; }
        transform.localPosition = ARAnchorOnMarker.instance.GetLocalPostItParent().transform.position + newPosition;
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
        //unityRenderer = GetComponent<Renderer>();
        unityRenderer.material.SetColor(baseColor, newColor);
        //Debug.Log("Set note color: " + newColor);
    }
    private void OnOutlineColorChanged(Color oldColor, Color newColor)
    {
        Debug.Log($"unityRenderer");
        // Set the color for the note
        //unityRenderer = GetComponent<Renderer>();
        unityRenderer.material.SetColor(outlineColourVariable, newColor);
        //Debug.Log("Set note color: " + newColor);
    }

    public void StartNote(Color Startcolour, FixedString512Bytes startString, Vector3 startPosition, Quaternion startRotaion)
    {
        unityRenderer = GetComponent<Renderer>();
        noteRotation.Value = startRotaion;
        notePosition.Value = startPosition;
        noteText.Value = startString;
        noteColor.Value = Startcolour;
        outLineColor.Value = Startcolour;
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
            unityRenderer.material.SetColor(outlineColourVariable, clientColorMap[clientColours[NetworkManager.Singleton.LocalClientId]]);
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
        unityRenderer.material.SetColor(outlineColourVariable, noteColor.Value);
    }
    public void ShowObjectToSpecificClients()
    {
        if (IsServer)
        {
            Debug.Log("Showing object to specific clients");
            ShowObjectToSpecificClientRpc();
        }
    }
    //TODO: Show object to specific client this one is currently not working as intended
    [ClientRpc]
    private void ShowObjectToSpecificClientRpc()
    {
        foreach (var client in clients)
        {
            if (client == NetworkManager.Singleton.LocalClientId)
            {
                unityRenderer.enabled = true;
                Renderer[] childRenderers = unityRenderer.gameObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in childRenderers)
                {
                    renderer.enabled = true;
                }
            }
        }
    }
    public void AddClient(ulong clientID)
    {
        if (IsServer)
        {
            AddClientClientRpc(clientID);
            Debug.Log("Added client");
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
        newClient(clientID);
    }
    private void newClient(ulong clientID)
    {
        if (clientColours.ContainsKey(clientID))
        {
            return;
        }
        int index = clients.IndexOf(clientID);
        clientColours.Add(clientID, (clientColor)index);
    }
}