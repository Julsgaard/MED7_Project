using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance { get; private set; }

    private PostItNoteNetwork myNote;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Optional: Uncomment if you need this object to persist across scenes
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterNote(PostItNoteNetwork note)
    {
        myNote = note;
    }

    public void MoveNote()
    {
        if (myNote != null)
        {
            myNote.MoveNote();
        }
    }
}