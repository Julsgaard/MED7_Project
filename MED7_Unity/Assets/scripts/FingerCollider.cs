using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;

public class FingerCollider : MonoBehaviour
{
    public static PostItNote postIt = null;
    private Vector3 oldPos;
    Material thisMaterial;
    private LineRenderer lineRenderer;

    //ManomotionManager manomotionManager;
    // Start is called before the first frame update
    void Start()
    {
        thisMaterial = gameObject.GetComponent<Renderer>().material;
        //manomotionManager = ManomotionManager.Instance;
        thisMaterial.color = Color.blue;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f; // Width at the start of the line
        lineRenderer.endWidth = 0.01f;   // Width at the end of the line

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
        if (postIt != null)
        {
            thisMaterial.color = Color.cyan;
            movePostIt(postIt);
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
        Ray cameraRay = Camera.main.ScreenPointToRay(screenPoint);
        RaycastHit[] hits = Physics.RaycastAll(cameraRay, 1000,3);
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, cameraRay.origin); // Start point of the ray
        lineRenderer.SetPosition(1, cameraRay.origin + cameraRay.direction * 10000);
        foreach (RaycastHit hit in hits) {
            GetComponentInChildren<TextMeshPro>().text = hit.collider.gameObject.tag;
            
            if (hit.collider.gameObject.tag == "Post-it")
            {
                postIt = hit.collider.gameObject.GetComponent<PostItNote>();
                if (postIt != null)
                {
                    GetComponentInChildren<TextMeshPro>().text = postIt.gameObject.name;
                }
                break;
            }
        }
        
    }

    private void movePostIt(PostItNote currentPostIt)
    {
        Vector3 movePos = oldPos - gameObject.transform.position;
        movePos = new Vector3(movePos.x, 0, movePos.z);
        currentPostIt.updatePosition(movePos);
        oldPos = gameObject.transform.position;
    }
}
