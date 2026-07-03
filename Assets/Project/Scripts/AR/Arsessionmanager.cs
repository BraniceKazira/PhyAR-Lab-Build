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

    [Header("XR Origin — drag the whole XR Origin GameObject here")]
    public GameObject xrOrigin;

    [Header("Topic prefabs")]
    public GameObject currentElectricityPrefab;
    public GameObject emInductionPrefab;
    public GameObject wavesIIPrefab;

    [Header("UI")]
    public TMP_Text   hintText;
    public GameObject placementRing;

    private static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();
    private GameObject _placedModel;
    private bool       _placed;
    private ARExperienceScreenController _arScreen;

    void Awake()
    {
        // Hide XR Origin on startup — shown only during AR
        if (xrOrigin != null) xrOrigin.SetActive(false);
    }

    public void StartAR()
    {
        // Activate entire XR Origin (camera + plane detection)
        if (xrOrigin != null) xrOrigin.SetActive(true);
        if (arSession != null) arSession.enabled = true;
        if (arPlaneManager != null) arPlaneManager.enabled = true;

        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_AR);

        _placed = false;
        if (_placedModel != null) Destroy(_placedModel);

        ShowRing(false);
        SetHint("Move phone slowly over a flat surface");

        _arScreen = FindObjectOfType<ARExperienceScreenController>();
        _arScreen?.OnARStarted(AppState.CurrentTopicID);
    }

    public void StopAR()
    {
        if (_placedModel != null) Destroy(_placedModel);
        _placedModel = null;
        _placed      = false;

        // Deactivate entire XR Origin — camera feed stops
        if (xrOrigin != null) xrOrigin.SetActive(false);
        if (arSession != null) arSession.enabled = false;

        ShowRing(false);
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_LEARN);
    }

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

    void Update()
    {
        if (xrOrigin == null || !xrOrigin.activeSelf) return;
        if (_placed) return;

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

    void PlaceModel(Pose pose)
    {
        if (_placedModel != null) Destroy(_placedModel);
        GameObject prefab = GetPrefab();
        if (prefab == null) return;

        _placedModel = Instantiate(prefab, pose.position, pose.rotation);
        _placed = true;

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
    }

    void ShowRing(bool show) { if (placementRing != null) placementRing.SetActive(show); }
    void SetHint(string t)   { if (hintText != null) hintText.text = t; }

    GameObject GetPrefab() =>
        AppState.CurrentTopicID switch
        {
            "current_electricity" => currentElectricityPrefab,
            "em_induction"        => emInductionPrefab,
            "waves_ii"            => wavesIIPrefab,
            _ => emInductionPrefab
        };
}