using System.Collections.Generic;
using UnityEngine;

public class PostItNoteLocal : MonoBehaviour
{
    /* To create this setup, I used the following scripts:
     * - TabletopMarkerAnchorer.cs (For anhoring local planes)
     * - NetworkNoteManager.cs (For networked notes, tracking changes from server)
     * - LocalNoteManager.cs (For local notes, implementing/copying changes from server notes locally)
     * - PostItNoteNetwork.cs (Unchanged. Holds values from networked notes)
     * - GameManager.cs (Works as before with two functions added in the end, creating local notes.)
     *
     * Note: some scripts were changed but not used:
     * - LocalPlaneAnchorer.cs (My first try. It was not used.)
     */
    
    public  Vector3 notePosition = new Vector3();
    public  string noteText = "";
    public  Color noteColor = new Color();
    public ulong networkedPartnerId;

    private string _outlineColor = "OutlineColour";
    private string _baseColor = "BaseColour";
    
    private Dictionary<ulong, ClientColor> _clientColours = new Dictionary<ulong, ClientColor>();
    private Dictionary<ClientColor,Color> _clientColorMap = new Dictionary<ClientColor, Color>
    {
        {ClientColor.YELLOW, new Color(240,228,66)},
        {ClientColor.VERMILLION, new Color(213,90,0)},
        {ClientColor.REDDISH_PURPLE, new Color(204,121,167)}
    };
    
    public enum ClientColor{
        YELLOW,
        VERMILLION,
        REDDISH_PURPLE,
    }
   
    private GameManager _gameManager;
    
    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }
    
    /* TODO: Maybe here or in LocalNoteManager add a listener to when
     *  network note is dragged or updated so its position and color
     *  are updated both in border color change (when selected) and
     *  in position when dragged.
     */
}
