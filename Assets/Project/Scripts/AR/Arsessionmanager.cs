// ARSessionManager.cs
// Folder:  Assets/_Project/Scripts/AR/
// Attach:  ARSessionManager GameObject in Main.unity (NOT inside any screen panel)
//
// Manages the AR camera lifecycle and model placement.
// All in one scene — no additive loading needed.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class ARSessionManager : MonoBehaviour
{
    [Header("AR Foundation — drag XR Origin into both fields")]
    public ARSession        arSession;
    public ARRaycastManager arRaycastManager;
    public ARPlaneManager   arPlaneManager;

    [Header("Topic prefabs from Assets/_Project/Prefabs/AR/")]
    public GameObject currentElectricityPrefab;
    public GameObject emInductionPrefab;
    public GameObject wavesIIPrefab;

    [Header("UI elements from Screen_ARExperience — drag directly")]
    public TMP_Text   hintText;
    public GameObject placementRing;

    private static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();
    private GameObject _placedModel;
    private bool       _placed;
    private ARExperienceScreenController _arScreen;

    // ── Called by ExploreARButton on Screen_Learn ─────────────────────
    public void StartAR()
    {
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_AR);

        if (arSession != null)    arSession.enabled    = true;
        if (arPlaneManager != null) arPlaneManager.enabled = true;

        _placed = false;
        if (_placedModel != null) Destroy(_placedModel);

        ShowRing(false);
        SetHint("Move phone slowly over a flat surface");

        _arScreen = FindObjectOfType<ARExperienceScreenController>();
        _arScreen?.OnARStarted(AppState.CurrentTopicID);

        Debug.Log("[AR] StartAR — topic: " + AppState.CurrentTopicID);
    }

    // ── Called by Back button via ARExperienceScreenController ─────────
    public void StopAR()
    {
        if (_placedModel != null) Destroy(_placedModel);
        _placedModel = null;
        _placed      = false;

        if (arPlaneManager != null) arPlaneManager.enabled = false;
        if (arSession != null)      arSession.enabled      = false;

        if (arPlaneManager != null)
            foreach (var plane in arPlaneManager.trackables)
                plane.gameObject.SetActive(false);

        ShowRing(false);
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_LEARN);
        Debug.Log("[AR] StopAR.");
    }

    // ── Called by Reset button via ARExperienceScreenController ────────
    public void ResetPlacement()
    {
        if (_placedModel != null) Destroy(_placedModel);
        _placedModel = null;
        _placed      = false;

        if (arPlaneManager != null)
        {
            arPlaneManager.enabled = true;
            foreach (var plane in arPlaneManager.trackables)
                plane.gameObject.SetActive(true);
        }

        ShowRing(false);
        SetHint("Move phone slowly over a flat surface");

        if (_arScreen == null)
            _arScreen = FindObjectOfType<ARExperienceScreenController>();
        _arScreen?.OnModelRemoved();
    }

    // ── Unity Update ──────────────────────────────────────────────────
    void Update()
    {
        if (arSession == null || !arSession.enabled) return;
        if (_placed) return;

        // Raycast from screen centre for placement ring
        Vector2 centre = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        if (arRaycastManager.Raycast(centre, Hits, TrackableType.PlaneWithinPolygon))
        {
            ShowRing(true);
            SetHint("Surface detected — tap to place");
        }
        else
        {
            ShowRing(false);
            SetHint("Move phone slowly over a flat surface");
        }

        // Tap to place
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es != null && es.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                return;

            if (arRaycastManager.Raycast(Input.GetTouch(0).position,
                    Hits, TrackableType.PlaneWithinPolygon))
                PlaceModel(Hits[0].pose);
        }
    }

    // ── Model placement ───────────────────────────────────────────────
    void PlaceModel(Pose pose)
    {
        if (_placedModel != null) Destroy(_placedModel);

        GameObject prefab = GetPrefab();
        if (prefab == null)
        {
            Debug.LogError("[AR] No prefab assigned for: " + AppState.CurrentTopicID);
            return;
        }

        _placedModel = Instantiate(prefab, pose.position, pose.rotation);
        _placed      = true;

        ShowRing(false);
        SetHint("Pinch to scale  ·  Drag to rotate");

        if (arPlaneManager != null)
        {
            arPlaneManager.enabled = false;
            foreach (var plane in arPlaneManager.trackables)
                plane.gameObject.SetActive(false);
        }

        if (_arScreen == null)
            _arScreen = FindObjectOfType<ARExperienceScreenController>();
        _arScreen?.OnModelPlaced(AppState.CurrentTopicID);

        Debug.Log("[AR] Placed: " + AppState.CurrentTopicID);
    }

    // ── Helpers ───────────────────────────────────────────────────────
    void ShowRing(bool show) { if (placementRing != null) placementRing.SetActive(show); }
    void SetHint(string t)   { if (hintText      != null) hintText.text = t; }

    GameObject GetPrefab() =>
        AppState.CurrentTopicID switch
        {
            "current_electricity" => currentElectricityPrefab,
            "em_induction"        => emInductionPrefab,
            "waves_ii"            => wavesIIPrefab,
            _                     => emInductionPrefab
        };
}