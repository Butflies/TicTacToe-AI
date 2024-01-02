using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("相机平滑因子")]
    public float CameraScrollSmoothFactor = 1;
    [Tooltip("正交相机缩放因子")]
    public float OrthographicScaleFactor = 1;
    [Tooltip("透视相机缩放因子")]
    public float PerspectiveScaleFactor = 3;

    [HideInInspector]
    public Camera Camera;

    float targetValue;
    bool lastCamProjection;
    Config config;

    // Start is called before the first frame update
    void Awake()
    {
        Camera = Camera.main;
    }

    private void Start()
    {
        config = GetComponent<Config>();
        float camHeight = Mathf.Max(config.BoardSize.x, config.BoardSize.y) * config.GridSize * (1 + config.GridGap);
        Camera.transform.position = new Vector3(config.BoardSize.x * config.GridSize / 2 - config.GridSize / 2 + (int)(config.BoardSize.x / 2) * config.GridGap, camHeight, config.BoardSize.y * config.GridSize / 2 - config.GridSize / 2 + (int)(config.BoardSize.y / 2) * config.GridGap);
        Camera.transform.forward = Vector3.down;
    }

    // Update is called once per frame
    void Update()
    {
        CameraControl();
    }


    void CameraControl()
    {
        if (Camera.orthographic != lastCamProjection)
        {
            lastCamProjection = Camera.orthographic;
            targetValue = 0;
        }
        if (Camera.orthographic)
        {

            if (Input.mouseScrollDelta.y != 0)
            {
                targetValue = Camera.orthographicSize - Input.mouseScrollDelta.y * OrthographicScaleFactor;
            }
            if (targetValue > 0 && Camera.orthographicSize != targetValue)
            {
                Camera.orthographicSize = Mathf.Lerp(Camera.orthographicSize, targetValue, CameraScrollSmoothFactor);
            }

        }
        else
        {
            if (Input.mouseScrollDelta.y != 0)
            {
                targetValue = Camera.fieldOfView - Input.mouseScrollDelta.y * PerspectiveScaleFactor;
            }
            if (targetValue > 0 && Camera.fieldOfView != targetValue)
            {
                Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, targetValue, CameraScrollSmoothFactor);
            }
        }
    }
}
