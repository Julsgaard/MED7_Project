using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance { get; private set; }

    private List<PostItNoteNetwork> notes = new List<PostItNoteNetwork>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Optionally, use DontDestroyOnLoad if needed
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterNote(PostItNoteNetwork note)
    {
        if (!notes.Contains(note))
        {
            notes.Add(note);
        }
    }

    public void UnregisterNote(PostItNoteNetwork note)
    {
        if (notes.Contains(note))
        {
            notes.Remove(note);
        }
    }

    public void MoveAllNotes()
    {
        foreach (var note in notes)
        {
            note.RequestMoveNote();
        }
    }
}