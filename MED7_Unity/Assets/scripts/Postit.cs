using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Postit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void updatePosition(Vector3 movement)
    {
        GetComponent<Renderer>().material.color = Color.green;
        gameObject.transform.position += movement;
    }
}
