using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class FingerCollider : MonoBehaviour
{
    public static Postit postIt = null;
    private Vector3 oldPos;
    Material thisMaterial;
    // Start is called before the first frame update
    void Start()
    {
        thisMaterial = this.gameObject.GetComponent<Renderer>().material;
        GameObject.Find("Post-it Note");
    }

    // Update is called once per frame
    void Update()
    {
        movePostIt(postIt);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Post-it")
        {
            thisMaterial.color = Color.green;
        }

    }

    private void OnTriggerStay(Collider other)
    {

    }

    private void movePostIt(GameObject currentPostIt)
    {
        Vector3 movePos = oldPos - gameObject.transform.position;
        oldPos = gameObject.transform.position;
        movePos = new Vector3(movePos.x, 0, movePos.z);
        currentPostIt.updatePosition(movePos);
        
    }
}
