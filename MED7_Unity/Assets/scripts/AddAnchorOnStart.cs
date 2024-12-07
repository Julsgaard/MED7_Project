using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class AddAnchorOnStart : MonoBehaviour
{
    private void Start() //TODO: Get this to work with AnchorManager
    {
        if (GetComponent<ARAnchor>() == null)
        {
            gameObject.AddComponent<ARAnchor>();
        }
        
        //TODO: https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.2/manual/anchor-manager.html
        
    }
    
}