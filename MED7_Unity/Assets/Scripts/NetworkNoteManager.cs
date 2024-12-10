using System.Collections.Generic;
using UnityEngine;

public class NetworkNoteManager : MonoBehaviour
{
    public static NetworkNoteManager instance { get; private set; }

    [SerializeField] public List<PostItNoteNetwork> notes = new List<PostItNoteNetwork>();
    
    private void Awake()
    {
        // Singleton
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
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