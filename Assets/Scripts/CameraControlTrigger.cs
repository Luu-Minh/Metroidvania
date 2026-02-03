using UnityEngine;
using Unity.Cinemachine;
using UnityEditor;

public class CameraControlTrigger : MonoBehaviour
{
    public CustomInspectorObject customInspectorObject;

    Collider2D coll;

    void Start()
    {
        coll = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (customInspectorObject.panCameraOnContact)
            {
                CameraManager.instance.PanCameraOnContact(customInspectorObject.panDistance, customInspectorObject.panTime, customInspectorObject.panDirection, false);
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Vector2 exitDirection = (collision.transform.position = coll.bounds.center).normalized;

            if (customInspectorObject.swapCameras && customInspectorObject.cameraOnLeft != null && customInspectorObject.cameraOnRight != null)
            {
                //swap cameras
                CameraManager.instance.SwapCameras(customInspectorObject.cameraOnLeft, customInspectorObject.cameraOnRight, exitDirection);
            }

            if (customInspectorObject.panCameraOnContact)
            {
                CameraManager.instance.PanCameraOnContact(customInspectorObject.panDistance, customInspectorObject.panTime, customInspectorObject.panDirection, true);
            }
        }
    }
}

[System.Serializable]
public class CustomInspectorObject
{
    public bool swapCameras = false;
    public bool panCameraOnContact = false;

    [HideInInspector] public CinemachineCamera cameraOnLeft;
    [HideInInspector] public CinemachineCamera cameraOnRight;

    [HideInInspector] public PanDirection panDirection;
    [HideInInspector] public float panDistance = 3f;
    [HideInInspector] public float panTime = 0.35f;
}

public enum PanDirection
{
    Up,
    Down,
    Left,
    Right
}

[CustomEditor(typeof(CustomInspectorObject))]
#if UNITY_EDITOR
public class MyScriptEditor : Editor
{
    CameraControlTrigger cameraControlTrigger;

    void OnEnable()
    {
        cameraControlTrigger = (CameraControlTrigger)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (cameraControlTrigger.customInspectorObject.swapCameras)
        {
            cameraControlTrigger.customInspectorObject.cameraOnLeft = EditorGUILayout.ObjectField("Camera On Left", cameraControlTrigger.customInspectorObject.cameraOnLeft, typeof(CinemachineCamera), true) as CinemachineCamera;
            cameraControlTrigger.customInspectorObject.cameraOnRight = EditorGUILayout.ObjectField("Camera On Right", cameraControlTrigger.customInspectorObject.cameraOnRight, typeof(CinemachineCamera), true) as CinemachineCamera;
        }

        if (cameraControlTrigger.customInspectorObject.panCameraOnContact)
        {
            cameraControlTrigger.customInspectorObject.panDirection = (PanDirection)EditorGUILayout.EnumPopup("Pan Direction", cameraControlTrigger.customInspectorObject.panDirection);
            cameraControlTrigger.customInspectorObject.panDistance = EditorGUILayout.FloatField("Pan Distance", cameraControlTrigger.customInspectorObject.panDistance);
            cameraControlTrigger.customInspectorObject.panTime = EditorGUILayout.FloatField("Pan Time", cameraControlTrigger.customInspectorObject.panTime);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(cameraControlTrigger);
        }
    }
}
#endif