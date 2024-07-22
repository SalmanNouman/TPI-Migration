using Cinemachine;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using VARLab.CloudSave;
using VARLab.Navigation.PointClick;

[CloudSaved]
[JsonObject(MemberSerialization.OptIn)]
public class SerializedNavProperties : MonoBehaviour, ICloudSerialized, ICloudDeserialized
{
    [JsonProperty]
    public Vector3 rotation;

    [JsonProperty]
    public Vector3 position;

    [JsonProperty]
    public float povCamVerticalAxisValue;

    [JsonProperty]
    public float povCamHorizontalAxisValue;

    public CinemachineVirtualCamera povCam;

    private PointClickNavigation navigation;
    private Transform player;
    private CinemachinePOV povCamPOV;
    private CinemachineBrain cmBrain;

    private bool isUpdatingPosition;
    private bool isPanning;
    /// <summary>
    /// Loads and checks for player, PovCam, povCamPOV and Stores the position of the player
    /// </summary>

    private void Awake()
    {
        navigation = GetComponent<PointClickNavigation>();
        player = navigation.Player.transform;

        if (player == null)
        {
            Debug.LogError("PointClickNavigation does not have a valid player.");
            return;
        }

        cmBrain = Camera.main.GetComponent<CinemachineBrain>();

        if (povCam == null)
        {
            Debug.LogError("CinemachineVirtualCamera component not assigned. Drag and drop the CinemachineVirtualCamera component from the POV camera GameObject.");
            return;
        }

        povCamPOV = povCam.GetCinemachineComponent<CinemachinePOV>();

        if (povCamPOV == null)
        {
            Debug.LogError("CinemachinePOV component not found on assigned CinemachineVirtualCamera.");
            return;
        }
        StorePositionAndRotation(player);
    }
    /// <summary>
    /// initializes camera function calls it on start
    /// </summary>
    private void Start()
    {
        StartCoroutine(InitializeCamera());
    }
    /// <summary>
    /// Initializes pov camera vertical and horizontal positions sends a log error if not there
    /// </summary>
    private IEnumerator InitializeCamera()
    {
        yield return null;

        if (povCamPOV != null)
        {
            povCamPOV.m_VerticalAxis.Value = povCamVerticalAxisValue;
            povCamPOV.m_HorizontalAxis.Value = povCamHorizontalAxisValue;
        }
        else
        {
            Debug.LogError("CinemachinePOV component not found on assigned CinemachineVirtualCamera.");
        }
    }
    /// <summary>
    /// Saves the cameras vertical and horizontal axis then logs what those saved values are
    /// </summary>
    private void SaveCameraSettings()
    {
        if (povCamPOV != null)
        {
            povCamVerticalAxisValue = povCamPOV.m_VerticalAxis.Value;
            povCamHorizontalAxisValue = povCamPOV.m_HorizontalAxis.Value;

            Debug.Log($"Saved Camera Settings - Vertical: {povCamVerticalAxisValue}, Horizontal: {povCamHorizontalAxisValue}");
        }
    }
    /// <summary>
    /// Disables recentering so that the camera stays in it's position instead of moving
    /// </summary>
    private void DisableRecentering()
    {
        if (povCamPOV != null)
        {
            povCamPOV.m_HorizontalRecentering.m_enabled = false;
            povCamPOV.m_VerticalRecentering.m_enabled = false;
        }
    }
    /// <summary>
    /// stores the position and rotation of the player/camera
    /// </summary>
    /// <param name="t"> Tramsfprm for rotation and position</param>
    private void StorePositionAndRotation(Transform t)
    {
        rotation = t.rotation.eulerAngles;
        position = t.position;
    }
    /// <summary>
    /// stores the waypoints transform details
    /// </summary>
    /// <param name="waypoint">Waypoint information</param>
    public void GetWaypointDetails(Waypoint waypoint)
    {
        StorePositionAndRotation(waypoint.transform);
    }
    /// <summary>
    /// the function called when the load button is selected 
    /// </summary>
    public void OnDeserialize()
    {
        StartCoroutine(SetPositionAndRotation());
    }
    /// <summary>
    /// function called upon when save button is selected
    /// </summary>
    public void OnSerialize()
    {
        StorePositionAndRotation(player);
        SaveCameraSettings();
    }
    /// <summary>
    /// sets position and rotation when load is selected
    /// </summary>
    /// <returns>null</returns>
    private IEnumerator SetPositionAndRotation()
    {
        if (!player || isUpdatingPosition) yield break;

        isUpdatingPosition = true;


        yield return new WaitUntil(() => !cmBrain.IsBlending);

        player.position = position;
        player.rotation = Quaternion.Euler(rotation);


        yield return null;


        navigation.Player.Warp(position);
        navigation.LookAt(player);

        isUpdatingPosition = false;
        yield return null;
        RestoreCameraSettings();
        yield return null;
        DisableRecentering();
    }
    /// <summary>
    /// restores previous camera settings
    /// </summary>
    private void RestoreCameraSettings() //restores previous camera settings
    {
        if (!povCamPOV) { return; }
        povCamPOV.m_VerticalAxis.Value = povCamVerticalAxisValue;
        povCamPOV.m_HorizontalAxis.Value = povCamHorizontalAxisValue;

        Debug.Log($"Restored Camera Settings - Vertical: {povCamVerticalAxisValue}, Horizontal: {povCamHorizontalAxisValue}");
    }
}
