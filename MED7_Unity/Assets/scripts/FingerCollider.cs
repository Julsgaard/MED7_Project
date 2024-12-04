using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class FingerCollider : MonoBehaviour
{
    public static Postit postIt = null;
    private Vector3 oldPos;
    Material thisMaterial;
    //ManomotionManager manomotionManager;
    // Start is called before the first frame update
    void Start()
    {
        thisMaterial = gameObject.GetComponent<Renderer>().material;
        //manomotionManager = ManomotionManager.Instance;
        thisMaterial.color = Color.blue;
    }

    // Update is called once per frame
    void Update()
    {
        
        //thisMaterial.color = Color.green;
        //movePostIt(postIt);
        //GetComponentInChildren<TextMeshPro>().text = gameObject.transform.position.ToString();
        HandInfo handInfo = ManomotionManager.Instance.Hand_infos[0].hand_info;
        if (handInfo.gesture_info.mano_gesture_trigger == ManoGestureTrigger.PICK)
        {
            if(postIt == null)
            {
                thisMaterial.color = Color.green;
                catchPostIt();
                oldPos = gameObject.transform.position;
            }
            else
            {
                postIt.gameObject.GetComponent<Renderer>().material.color = Color.green;
                movePostIt(postIt);
            }
        }
        else if (handInfo.gesture_info.mano_gesture_trigger == ManoGestureTrigger.DROP)
        {
            thisMaterial.color = Color.red;

            if (postIt != null)
            {
                postIt = null;
            }
        }
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
    private void catchPostIt()
    {
        thisMaterial.color = Color.yellow;
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        Vector3 realRay = gameObject.transform.position - screenPoint;
        Debug.Log(screenPoint);
        GetComponentInChildren<TextMeshPro>().text = screenPoint.ToString();
        Ray cameraRay = Camera.main.ScreenPointToRay(screenPoint);
        RaycastHit[] hits = Physics.RaycastAll(screenPoint, realRay, 10000, 3);
        Debug.DrawLine(Camera.main.WorldToScreenPoint(gameObject.transform.position), gameObject.transform.position, Color.blue);
        if (hits.Length == 0)
        {
            return;
        }
        foreach (RaycastHit hit in hits)
        {
            
            if (hit.collider.gameObject.tag == "Post-it")
            {
                postIt = hit.collider.gameObject.GetComponent<Postit>();
                break;
            }
        }
    }

    private void movePostIt(Postit currentPostIt)
    {
        Vector3 movePos = oldPos - gameObject.transform.position;
        movePos = new Vector3(movePos.x, 0, movePos.z);
        currentPostIt.updatePosition(movePos);
        oldPos = gameObject.transform.position;
    }
}
