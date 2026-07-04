using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class ARSessionManager : MonoBehaviour
{
    [Header("AR Foundation")]
    public ARSession        arSession;
    public ARRaycastManager arRaycastManager;
    public ARPlaneManager   arPlaneManager;
    public GameObject       xrOrigin;

    [Header("Prefabs")]
    public GameObject currentElectricityPrefab;
    public GameObject emInductionPrefab;
    public GameObject wavesIIPrefab;

    [Header("UI")]
    public TMP_Text   hintText;
    public GameObject placementRing;
    public GameObject cameraCoverPanel;

    private static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();
    private GameObject _model;
    private bool       _placed;
    private ARExperienceScreenController _arScreen;

    void Awake()
    {
        Debug.Log("[AR] Awake called");
        
        if (xrOrigin != null)
        {
            xrOrigin.SetActive(false);
            Debug.Log("[AR] XR Origin deactivated in Awake");
        }
        else
        {
            Debug.LogError("[AR] xrOrigin is NULL in Awake!");
        }
        
        if (cameraCoverPanel != null)
        {
            cameraCoverPanel.SetActive(true);
            Debug.Log("[AR] cameraCoverPanel set active in Awake");
        }
        else
        {
            Debug.LogError("[AR] cameraCoverPanel is NULL in Awake!");
        }
    }

    public void StartAR()
    {
        Debug.Log("========================================");
        Debug.Log("[AR] StartAR called");
        Debug.Log("[AR] CurrentTopicID: " + AppState.CurrentTopicID);
        Debug.Log("========================================");

        // Check all references first
        Debug.Log("[AR] Checking references...");
        Debug.Log("[AR] arSession is " + (arSession != null ? "assigned" : "NULL"));
        Debug.Log("[AR] arRaycastManager is " + (arRaycastManager != null ? "assigned" : "NULL"));
        Debug.Log("[AR] arPlaneManager is " + (arPlaneManager != null ? "assigned" : "NULL"));
        Debug.Log("[AR] xrOrigin is " + (xrOrigin != null ? "assigned" : "NULL"));
        Debug.Log("[AR] cameraCoverPanel is " + (cameraCoverPanel != null ? "assigned" : "NULL"));

        // Show AR screen first
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_AR);
        Debug.Log("[AR] AR screen shown");

        // Hide cover → camera visible
        if (cameraCoverPanel != null)
        {
            cameraCoverPanel.SetActive(false);
            Debug.Log("[AR] cameraCoverPanel hidden (set to false)");
        }
        else
        {
            Debug.LogError("[AR] cameraCoverPanel is NULL - cannot hide!");
        }

        // Start AR systems
        if (xrOrigin != null)
        {
            xrOrigin.SetActive(true);
            Debug.Log("[AR] xrOrigin activated (set to true)");
        }
        else
        {
            Debug.LogError("[AR] xrOrigin is NULL - cannot activate!");
        }

        if (arSession != null)
        {
            arSession.enabled = true;
            Debug.Log("[AR] arSession enabled (set to true)");
        }
        else
        {
            Debug.LogError("[AR] arSession is NULL - cannot enable!");
        }

        if (arPlaneManager != null)
        {
            arPlaneManager.enabled = true;
            Debug.Log("[AR] arPlaneManager enabled (set to true)");
        }
        else
        {
            Debug.LogError("[AR] arPlaneManager is NULL - cannot enable!");
        }

        _placed = false;
        if (_model != null)
        {
            Destroy(_model);
            Debug.Log("[AR] Old model destroyed");
        }

        ShowRing(false);
        SetHint("Move phone slowly over a flat surface");
        Debug.Log("[AR] Hint set to: 'Move phone slowly over a flat surface'");

        _arScreen = FindObjectOfType<ARExperienceScreenController>();
        if (_arScreen != null)
        {
            _arScreen.OnARStarted(AppState.CurrentTopicID);
            Debug.Log("[AR] ARExperienceScreenController found and notified");
        }
        else
        {
            Debug.LogWarning("[AR] ARExperienceScreenController not found");
        }

        Debug.Log("[AR] StartAR completed");
        Debug.Log("========================================");
    }

    public void StopAR()
    {
        Debug.Log("[AR] StopAR called");

        if (_model != null)
        {
            Destroy(_model);
            Debug.Log("[AR] Model destroyed");
        }
        _model = null;
        _placed = false;

        if (xrOrigin != null)
        {
            xrOrigin.SetActive(false);
            Debug.Log("[AR] xrOrigin deactivated");
        }

        if (arSession != null)
        {
            arSession.enabled = false;
            Debug.Log("[AR] arSession disabled");
        }

        if (arPlaneManager != null)
        {
            arPlaneManager.enabled = false;
            foreach (var p in arPlaneManager.trackables)
                p.gameObject.SetActive(false);
            Debug.Log("[AR] arPlaneManager disabled");
        }

        if (cameraCoverPanel != null)
        {
            cameraCoverPanel.SetActive(true);
            Debug.Log("[AR] cameraCoverPanel shown (set to true)");
        }

        ShowRing(false);
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_LEARN);
        Debug.Log("[AR] StopAR completed");
    }

    public void ResetPlacement()
    {
        if (_model != null) Destroy(_model);
        _model = null;
        _placed = false;
        if (arPlaneManager != null)
        {
            arPlaneManager.enabled = true;
            foreach (var p in arPlaneManager.trackables)
                p.gameObject.SetActive(true);
        }
        ShowRing(false);
        SetHint("Move phone slowly over a flat surface");
        _arScreen?.OnModelRemoved();
    }

    void Update()
    {
        if (xrOrigin == null || !xrOrigin.activeSelf) return;
        if (_placed) return;

        var c = new Vector2(Screen.width * .5f, Screen.height * .5f);
        if (arRaycastManager.Raycast(c, Hits, TrackableType.PlaneWithinPolygon))
        { ShowRing(true); SetHint("Surface detected — tap to place"); }
        else
        { ShowRing(false); SetHint("Move phone slowly over a flat surface"); }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es != null && es.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                return;
            if (arRaycastManager.Raycast(Input.GetTouch(0).position,
                    Hits, TrackableType.PlaneWithinPolygon))
                Place(Hits[0].pose);
        }
    }

    void Place(Pose pose)
    {
        if (_model != null) Destroy(_model);
        var prefab = GetPrefab();
        if (prefab == null)
        { Debug.LogError("[AR] No prefab for: " + AppState.CurrentTopicID); return; }

        _model = Instantiate(prefab, pose.position, pose.rotation);
        _placed = true;
        ShowRing(false);
        SetHint("Pinch to scale  ·  Drag to rotate");

        if (arPlaneManager != null)
        {
            arPlaneManager.enabled = false;
            foreach (var p in arPlaneManager.trackables)
                p.gameObject.SetActive(false);
        }

        if (_arScreen == null)
            _arScreen = FindObjectOfType<ARExperienceScreenController>();
        _arScreen?.OnModelPlaced(AppState.CurrentTopicID);
        Debug.Log("[AR] Placed: " + AppState.CurrentTopicID);
    }

    void ShowRing(bool s) { if (placementRing != null) placementRing.SetActive(s); }
    void SetHint(string t) { if (hintText != null) hintText.text = t; }

    GameObject GetPrefab()
    {
        switch (AppState.CurrentTopicID)
        {
            case "current_electricity": return currentElectricityPrefab;
            case "em_induction": return emInductionPrefab;
            case "waves_ii": return wavesIIPrefab;
            default:
                Debug.LogWarning("[AR] Unknown topic: " + AppState.CurrentTopicID);
                return emInductionPrefab;
        }
    }
}