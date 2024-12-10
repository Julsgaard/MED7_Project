using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalNoteManager : MonoBehaviour
{
    private List<PostItNoteLocal> _localNotes = new List<PostItNoteLocal>();
    private Dictionary<ulong, PostItNoteLocal> _noteMap = new Dictionary<ulong, PostItNoteLocal>();
    private NetworkNoteManager _networkNoteManager;

    private void Awake()
    {
        _networkNoteManager = FindObjectOfType<NetworkNoteManager>();
    }

    public void RegisterAndSpawnNotesLocally()
    {
        var serverNotes = _networkNoteManager.notes;

        foreach (var note in serverNotes)
        {
            if (!_noteMap.ContainsKey(note.NetworkObjectId))
            {
                var newNote = new GameObject();
                var newLocalNote = newNote.AddComponent<PostItNoteLocal>();

                newLocalNote.notePosition = note.notePosition.Value;
                newLocalNote.noteText = note.noteText.Value.ToString();
                newLocalNote.noteColor = note.noteColor.Value;

                _localNotes.Add(newLocalNote);
                _noteMap[note.NetworkObjectId] = newLocalNote;
            }
            else
            {
                var existingNote = _noteMap[note.NetworkObjectId];
                existingNote.notePosition = note.notePosition.Value;
                existingNote.noteText = note.noteText.Value.ToString();
                existingNote.noteColor = note.noteColor.Value;
            }
        }
    }
    
    public void UpdateAllLocalNotes()
    {
        var serverNotes = _networkNoteManager.notes;
        
        foreach (var note in serverNotes)
        {
            if (_noteMap.ContainsKey(note.NetworkObjectId))
            {
                var existingNote = _noteMap[note.NetworkObjectId];
                existingNote.notePosition = note.notePosition.Value;
                existingNote.noteText = note.noteText.Value.ToString();
                existingNote.noteColor = note.noteColor.Value;
            }
        }
    }
}
