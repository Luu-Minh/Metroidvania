using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    [SerializeField] CinemachineCamera[] allVirtualCameras;
    [SerializeField] float fallPanAmount = 0.25f;
    [SerializeField] float fallYPanTime = 0.35f;
    public float fallSpeedDampingChangeThreshold = -15f;

    public bool IsLerpingYDamping { get; set; }
    public bool LerpedFromPlayerFalling { get; set; }
    Coroutine lerpYPanCoroutine;
    Coroutine panCameraCoroutine;

    CinemachineCamera currentCamera;
    CinemachinePositionComposer framingTransposer;

    float normYPanAmount;
    Vector3 startingTrackedObjectOffset;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        for (int i = 0; i < allVirtualCameras.Length; i++)
        {
            if (allVirtualCameras[i].enabled)
            {
             currentCamera = allVirtualCameras[i];

             framingTransposer = currentCamera.GetCinemachineComponent(CinemachineCore.Stage.Aim) as CinemachinePositionComposer;
            }
        }

        if (framingTransposer != null)
        {
            normYPanAmount = framingTransposer.Damping.y;
            startingTrackedObjectOffset = framingTransposer.Damping;
        }
        else
        {
            normYPanAmount = 0f;
            startingTrackedObjectOffset = Vector3.zero;
        }
    }

    #region LerpYDamping
    public void LerpYDamping(bool isPlayerFaliing)
    {
        lerpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFaliing));   
    }

    IEnumerator LerpYAction(bool isPlayerFalling)
    {
        IsLerpingYDamping = true;

        float startDampAmount = framingTransposer.Damping.y;
        float endDampAmount = 0f;

        if (isPlayerFalling)
        {
            endDampAmount = fallPanAmount;
            LerpedFromPlayerFalling = true;
        }
        else
        {
            endDampAmount = normYPanAmount;
        }

        float elapsedTime = 0f;
        while (elapsedTime < fallYPanTime)
        {
            elapsedTime += Time.deltaTime;
            float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, (elapsedTime / fallYPanTime));
            framingTransposer.Damping.y = lerpedPanAmount;
            yield return null;
        }

        IsLerpingYDamping = false;
    }
#endregion

    #region PanCamera
    public void PanCameraOnContact(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPosition)
    {
        panCameraCoroutine = StartCoroutine(PanCamera(panDistance, panTime, panDirection, panToStartingPosition));
    }

    IEnumerator PanCamera(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPosition)
    {
        Vector2 endPos = Vector2.zero;
        Vector2 startingPos = Vector2.zero;

        if (!panToStartingPosition){
            switch (panDirection){
                case PanDirection.Up:
                    endPos = Vector2.up;
                    break;
                case PanDirection.Down:
                    endPos = Vector2.down;
                    break;
                case PanDirection.Left:
                    endPos = Vector2.left;
                    break;
                case PanDirection.Right:
                    endPos = Vector2.right;
                    break;
                default:
                    break;               
            }
            endPos *= panDistance;

            startingPos = framingTransposer.Damping;
            endPos += startingPos;
        }
        else{
        startingPos = framingTransposer.Damping;
        endPos = startingTrackedObjectOffset;
        }

        float elapsedTime = 0f;
        while (elapsedTime < panTime)
        {
            elapsedTime += Time.deltaTime;
            Vector3 panLerp = Vector3.Lerp(startingPos, endPos, (elapsedTime / panTime));
            framingTransposer.Damping = panLerp;

            yield return null;
        }
    }
    #endregion

    #region SwapCameras

    public void SwapCameras(CinemachineCamera cameraFromLeft, CinemachineCamera cameraFromRight, Vector2 triggerExitDirection){
        if (currentCamera == cameraFromLeft && triggerExitDirection.x > 0f){
            //activate new camera
            cameraFromRight.enabled = true;

            //deactivate current camera
            cameraFromLeft.enabled = false;

            //set current camera to new camera
            currentCamera = cameraFromRight;

            //update framing transposer
            framingTransposer = currentCamera.GetCinemachineComponent(CinemachineCore.Stage.Aim) as CinemachinePositionComposer;
        }

        else if (currentCamera == cameraFromRight && triggerExitDirection.x < 0f){
            //activate new camera
            cameraFromLeft.enabled = true;

            //deactivate current camera
            cameraFromRight.enabled = false;

            //set current camera to new camera
            currentCamera = cameraFromLeft;

            //update framing transposer
            framingTransposer = currentCamera.GetCinemachineComponent(CinemachineCore.Stage.Aim) as CinemachinePositionComposer;
        }
    }
    #endregion
}


    

