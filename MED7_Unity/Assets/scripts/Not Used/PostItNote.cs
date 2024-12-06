using System;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

// Script for controlling the PostItNote GameObject in the scene using the NetworkTransform component

// CHATGPT MADE RIGHT NOW FOR TESTING AND LEARNING PURPOSES
public class PostItNote : NetworkBehaviour
{
    private NetworkTransform networkTransform;

    // Variables for the PostItNote GameObject
    [SerializeField] private Vector3 position;
    [SerializeField] private Color applicantColour;
    [SerializeField] private string note;
    
    private void Start()
    {
        networkTransform = GetComponent<NetworkTransform>();
    }
    private void Update()
    {
        SetText(gameObject.transform.position.ToString());
    }


    /*// Method to request moving the note
    public void RequestMove(Vector3 newPosition)
    {
        if (IsOwner || IsServer)
        {
            RequestMoveServerRpc(newPosition);
        }
    }

    [ServerRpc]
    private void RequestMoveServerRpc(Vector3 newPosition, ServerRpcParams rpcParams = default)
    {
        // Server handles the movement of the note
        transform.position = newPosition;
        networkTransform.SetState(transform.position, transform.rotation, transform.localScale);
    }*/

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