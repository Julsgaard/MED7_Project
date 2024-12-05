using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance { get; private set; }

    [SerializeField] public List<PostItNoteNetwork> notes = new List<PostItNoteNetwork>();
    
    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterNote(PostItNoteNetwork note)
    {
        if (!notes.Contains(note))
        {
            notes.Add(note);
            //Debug.Log("Note registered");
        }
    }
}