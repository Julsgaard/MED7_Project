using System;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UI;

// Script for controlling the PostItNote GameObject in the scene using the NetworkTransform component

// CHATGPT MADE RIGHT NOW FOR TESTING AND LEARNING PURPOSES
public class PostItNoteNetwork : NetworkBehaviour
{
    private NetworkVariable<Color> noteColor = new NetworkVariable<Color>();
    private NetworkVariable<string> noteText = new NetworkVariable<string>();

    private Renderer noteRenderer;
    private TextMeshPro noteTextMesh;
    
    private void Awake()
    {
        noteRenderer = GetComponent<Renderer>();
        noteTextMesh = GetComponentInChildren<TextMeshPro>();
    }
    
    // Test method for moving the note
    /*private void AddListenersToButtons()
    {
        //Add listeners to the buttons
        moveNoteButton.onClick.AddListener(MoveNoteRandom);
    }*/
    
    
    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            noteColor.OnValueChanged += OnColorChanged;
            noteText.OnValueChanged += OnTextChanged;

            // Apply initial values
            OnColorChanged(Color.white, noteColor.Value);
            OnTextChanged(string.Empty, noteText.Value);
        }
    }
    
    public void Initialize(Color color, string text)
    {
        if (IsServer)
        {
            noteColor.Value = color;
            noteText.Value = text;
        }
    }
    
    private void OnColorChanged(Color previousColor, Color newColor)
    {
        noteRenderer.material.color = newColor;
    }

    private void OnTextChanged(string previousText, string newText)
    {
        noteTextMesh.text = newText;
    }

    /*// Method to request moving the note
    public void RequestMove(Vector3 newPosition)
    {
        if (IsOwner || IsServer)
        {
            RequestMoveServerRpc(newPosition);
        }
    }*/
    
    private void SetStartNotePosition()
    {
        // Set the start position of the note
        //transform.position = position;
    }
    
    private void MoveNoteRandom()
    {
        // Move the note to a new position
        Vector3 newPosition = new Vector3(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1));
        RequestMoveServerRpc(newPosition);
    }
    
    // ServerRpc for moving the note
    [ServerRpc]
    private void RequestMoveServerRpc(Vector3 newPosition, ServerRpcParams rpcParams = default)
    {
        // Server handles the movement of the note
        transform.position = newPosition;
        _networkTransform.SetState(transform.position, transform.rotation, transform.localScale);
        Debug.Log("Note moved to: " + newPosition);
    }
    
    // Send the note data to the server
    /*public void SendNoteData(string noteText, Color noteColor)
    {
        if (IsOwner || IsServer)
        {
            SendNoteDataServerRpc(noteText, noteColor);
        }
    }*/
    
    // Method to change the colour of the note
    public void SetColour(Color newColour)
    {
        applicantColour = newColour;
        
        // Change the colour of the note
        GetComponent<Renderer>().material.color = applicantColour;
    }
    
    // Method to change the text of the note
    public void SetText(string newNote)
    {
        note = newNote;
        
        // Change the text of the note
        GetComponentInChildren<TextMeshPro>().text = note;
    }
}