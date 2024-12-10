using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LocalNoteManager : MonoBehaviour
{
    private List<PostItNoteLocal> _localNotes = new List<PostItNoteLocal>();
    private Dictionary<ulong, PostItNoteLocal> _noteMap = new Dictionary<ulong, PostItNoteLocal>();
    private NetworkNoteManager _networkNoteManager;

    [SerializeField] private GameObject localPostItPrefab;
    
    private static int _noteCount = 0;
    

    private void Awake()
    {
        _networkNoteManager = FindObjectOfType<NetworkNoteManager>();
    }

    public void RegisterAndSpawnNotesLocally()
    {
        Debug.Log($"{_noteCount}: Registering and spawning notes locally.");

        foreach (var note in _networkNoteManager.notes)
        {
            Debug.Log($"{_noteCount}: Working on note: '{note.noteText}'");
            
            if (!_noteMap.ContainsKey(note.NetworkObjectId))
                SetupNewNote(note);
            else
                UpdateSingleLocalNote(note);
        }
    }

    /* TODO:
     *  Fix the position of the notes when they are spawned. Right now
     *  they are spawned offset from the plane, probably based on the
     *  XROrigin. They need to be spawned on the plane, and towards
     *  the user clicking the button.
     */
    
    /* TODO: (Follow up from above) Make sure the notes are correctly
     *  positioned when other users join the session. They should be
     *  positioned in the same way for all users, so they appeat to be
     *  in the same place in the real world.
     */
    private void SetupNewNote(PostItNoteNetwork note)
    {
        Debug.Log($"{_noteCount}: Setting up relations.");
        
        var tabletopObject = FindObjectOfType<TabletopMarkerAnchorer>().GetTabletopObject();
        
        var newNote = Instantiate(localPostItPrefab, note.notePosition.Value, Quaternion.identity, tabletopObject.transform);
        var newLocalNote = newNote.GetComponent<PostItNoteLocal>();
        var textMeshPro = newNote.GetComponentInChildren<TextMeshPro>();
        var colorRenderer = newNote.GetComponent<Renderer>();
        
        Debug.Log($"{_noteCount}: Setting note values.");
        
        newLocalNote.transform.position = note.notePosition.Value;
        textMeshPro.text = note.noteText.Value.ToString();
        colorRenderer.material.SetColor("BaseColor", note.noteColor.Value);
        newLocalNote.networkedPartnerId = note.NetworkObjectId;
        
        var rescaleFactorX = 1 / tabletopObject.transform.localScale.x;
        var rescaleFactorZ = 1 / tabletopObject.transform.localScale.z;
        newLocalNote.transform.localScale = Vector3.Scale(newLocalNote.transform.localScale, new Vector3(rescaleFactorX, 1, rescaleFactorZ));
        
        Debug.Log($"{_noteCount}: Adding note to local notes list.");
        
        _localNotes.Add(newLocalNote);
        _noteMap[note.NetworkObjectId] = newLocalNote;
        
        _noteCount++;
    }
    
    public void UpdateAllLocalNotes()
    {
        foreach (var note in _networkNoteManager.notes.Where(note => _noteMap.ContainsKey(note.NetworkObjectId)))
            UpdateSingleLocalNote(note);
    }
    
    public void UpdateSingleLocalNote(PostItNoteNetwork note)
    {
            var noteToUpdate = _noteMap[note.NetworkObjectId];
            noteToUpdate.notePosition = note.notePosition.Value;
            noteToUpdate.noteText = note.noteText.Value.ToString();
            noteToUpdate.noteColor = note.noteColor.Value;
    }
}
