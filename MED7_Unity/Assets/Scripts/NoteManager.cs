using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance { get; private set; }

    [SerializeField] private List<PostItNoteNetwork> notes = new List<PostItNoteNetwork>();
    
    private void Awake()
    {
        Instance = this;
    }

    public void RegisterNote(PostItNoteNetwork note)
    {
        if (!notes.Contains(note))
        {
            notes.Add(note);
        }
    }

    public void MoveAllNotes()
    {
        foreach (var note in notes)
        {
            note.RequestMoveNote();
        }
    }
    
    // public void SetNoteData()
    // {
    //     foreach (var note in notes)
    //     {
    //         note.SetNoteData("Test", Color.white);
    //     }
    // }
}