using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;

public class FingerCollider : MonoBehaviour
{
    public static PostItNoteNetwork PostIt = null;
    private Vector3 _oldPos;
    private Material _thisMaterial;
    private LineRenderer _debugLine;
    private GameObject _debugBall;
    private Camera _camera;

    private TextMeshPro _tmp;
    private int _defaultLayerMask = 1 << 0; // Layer 0 (default)

    private bool _isHoldingPostIt, _isTryingToCatchPostIt;
    
    private RaycastHit[] _raycastHits = new RaycastHit[3]; // Pre-allocated array for the nonAlloc raycast call


    void Start()
    {
        _thisMaterial = gameObject.GetComponent<Renderer>().material;
        _thisMaterial.color = Color.red;
        
        _debugLine = GetComponent<LineRenderer>();
        _debugLine.startWidth = 0.001f; // Width at the start of the line
        _debugLine.endWidth = 0.001f;   // Width at the end of the line
        
        _camera = Camera.main;
        _tmp = GetComponentInChildren<TextMeshPro>();
        _tmp.text = "Awaiting pinch gesture";
        
        StartCoroutine(FaceTextTowardsCamera());
    }

    // Update is called once per frame
    void Update()
    {
        HandInfo handInfo = ManomotionManager.Instance.Hand_infos[0].hand_info;
        
        if ((handInfo.gesture_info.mano_gesture_trigger == ManoGestureTrigger.PICK && !_isHoldingPostIt) || _isTryingToCatchPostIt)
            StartCatchingPostIt();
        
        if (handInfo.gesture_info.mano_gesture_trigger == ManoGestureTrigger.DROP)
            DropPostIt();
        
        if (_isHoldingPostIt)
            MovePostIt(PostIt);
    }


    private void StartCatchingPostIt()
    {
        _isTryingToCatchPostIt = true;
        TryCatchPostIt();
    }

    private void DropPostIt()
    {
        _thisMaterial.color = Color.red;
        _tmp.text = "Awaiting pinch gesture";
        PostIt = null;
        
        _debugLine.enabled = false;
        _isHoldingPostIt = false;
        _isTryingToCatchPostIt = false;
    }
    
    private void TryCatchPostIt()
    {
        Vector3 screenPoint = _camera.WorldToScreenPoint(gameObject.transform.position);
        Ray cameraRay = _camera.ScreenPointToRay(screenPoint);
        
        CheckRayHitsForPostIts(cameraRay);
    }
    
    private void MovePostIt(PostItNoteNetwork currentPostIt)
    {
        Vector3 screenPoint = _camera.WorldToScreenPoint(gameObject.transform.position);
        Ray cameraRay = _camera.ScreenPointToRay(screenPoint);
        RaycastHit hit;
        
        DisplayDebugLine(cameraRay, Color.green);
        
        if (Physics.Raycast(cameraRay, out hit, 10, LayerMask.GetMask("Table"), QueryTriggerInteraction.Ignore))
        {
            _tmp.text = "Moving note";
            _thisMaterial.color = Color.green;
            DisplayDebugLine(cameraRay, Color.green);
            currentPostIt.RequestMoveNoteServerRpc(hit.point);
        }
    }

    private void CheckRayHitsForPostIts(Ray cameraRay)
    {
        int hitCount = Physics.RaycastNonAlloc(cameraRay, _raycastHits, 3, _defaultLayerMask);
        
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = _raycastHits[i];
            
            if (hit.collider.gameObject.CompareTag("Post-it")) // if we hit a note
            {
                PostIt = hit.collider.gameObject.GetComponent<PostItNoteNetwork>();
                
                _isHoldingPostIt = true;
                _tmp.text = "Move within table surface";
                _isTryingToCatchPostIt = false;
                
                _thisMaterial.color = Color.gray;
                DisplayDebugLine(cameraRay, Color.gray);
                return;
            }
            
        }
        
        if (_isTryingToCatchPostIt) // if we didn't hit a note
        {
            _tmp.text = "Tracking";
            _thisMaterial.color = Color.yellow;
            DisplayDebugLine(cameraRay, Color.yellow);
        }
    }

    private void DisplayDebugLine(Ray cameraRay, Color rayColor)
    {
        _debugLine.enabled = true;
        
        _debugLine.startColor = rayColor;
        _debugLine.endColor = rayColor;
        
        _debugLine.positionCount = 2;
        _debugLine.SetPosition(0, cameraRay.origin - new Vector3(0, 0.01f, 0));
        _debugLine.SetPosition(1, cameraRay.origin + cameraRay.direction * 3);
    }

    private IEnumerator FaceTextTowardsCamera()
    {
        _tmp.gameObject.transform.LookAt(_camera.transform);
        _tmp.gameObject.transform.rotation = Quaternion.Euler(_camera.transform.forward);
        
        yield return new WaitForSeconds(0.1f);
    }
}
