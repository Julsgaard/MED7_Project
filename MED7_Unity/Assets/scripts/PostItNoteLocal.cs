using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostItNoteLocal : MonoBehaviour
{
    public  Vector3 notePosition = new Vector3();
    public  string noteText = "";
    public  Color noteColor = new Color();

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
}
