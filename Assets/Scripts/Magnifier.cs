// malos presents.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class Magnifier : MonoBehaviour
{
    // ---------- Editor

    public bool enableZoom = true;

    public float panSensitivity = 0.1f;

    public float scrollWheelSensitivity = 0.2f;

    public float maxZoom = 20f;

    public GameObject zoomUIRoot;

    public Image zoomCanvasImage;

    public float zoomCanvasWidth = 100f;

    public Image zoomIndicatorImage;

    public Text zoomText;

    public bool debug = false;

    // ---------- Public UI


    // ---------- Public 


    public void Magnify()
    {
        isMagnifying = true;
        zoomUIRoot.SetActive(true);
    }

    public void Restore()
    {
        camera.ResetProjectionMatrix();
        isMagnifying = false;
        zoomUIRoot.SetActive(false);
    }

    public void SetMagnifyRegion(int xmin, int xmax, int ymin, int ymax)
    {
        enableZoom = false;

        this.xmin = xmin;
        this.xmax = xmax;
        this.ymin = ymin;
        this.ymax = ymax;
        SetMagnifyRegion_Internal();
    }

    // ---------- Private

    Camera camera;

    bool isMagnifying = false;

    bool hideUI = false;

    float zoomLevel = 1f; // like on a zoom camera.

    Vector2 viewCenter = Vector2.zero;

    float xmin, ymin, xmax, ymax;


    // range from (-1, -1) to (1, 1) inclusive, with (-1, -1) being screen's lower-left, and (1, 1) being screen's upper-right
    void SetMagnifyRegion_Internal()
    {

        // Retrive camera properties
        float n = camera.nearClipPlane;
        float f = camera.farClipPlane;
        float fovy = camera.fieldOfView; // in degrees, uses verticle fov (fovy)
        float aspect = camera.aspect; // width / height

        // Calculate l, r, b, t
        float ogT = Mathf.Tan(fovy / 2f * Mathf.Deg2Rad) * n;
        float ogR = ogT * aspect;

        float l = ogR * xmin;
        float r = ogR * xmax;
        float b = ogT * ymin;
        float t = ogT * ymax;

        Matrix4x4 persp = Matrix4x4.Frustum(l, r, b, t, n, f);
        camera.projectionMatrix = persp;
        camera.cullingMatrix = persp * camera.worldToCameraMatrix;

    }

    void UpdateZoom()
    {
        float scrollDelta = Input.mouseScrollDelta.y;
        bool mouseButtonDown = Input.GetMouseButton(0);
        float mouseX = mouseButtonDown ? -Input.GetAxisRaw("Mouse X") : 0; // negate to perform "natural scrolling" like in macOS
        float mouseY = mouseButtonDown ? -Input.GetAxisRaw("Mouse Y") : 0;

        if (scrollDelta != 0 || mouseX != 0 || mouseY != 0)
        {

            // update values
            zoomLevel += scrollDelta * scrollWheelSensitivity * zoomLevel; // multiplies a extra zoomLevel to linearize zoom
            zoomLevel = Mathf.Clamp(zoomLevel, 1, maxZoom);

            if (zoomLevel == 1f)
            {
                Restore();
            }
            else
            {
                Magnify();

                float ratio = 1 / zoomLevel; // zoomLevel -> region

                viewCenter += new Vector2(mouseX, mouseY) * panSensitivity * ratio;
                viewCenter.x = Mathf.Clamp(viewCenter.x, ratio - 1, 1 - ratio);
                viewCenter.y = Mathf.Clamp(viewCenter.y, ratio - 1, 1 - ratio);

                // calculate magnifying region
                xmin = viewCenter.x - ratio;
                ymin = viewCenter.y - ratio;
                xmax = viewCenter.x + ratio;
                ymax = viewCenter.y + ratio;

                SetMagnifyRegion_Internal();

                UpdateUI();

                if (debug)
                {
                    print(string.Format("ZoomLevel: {0}, ViewCenter: {1}, xmin = {2}, ymin = {3}, xmax = {4}, ymax = {5}", zoomLevel, viewCenter, xmin, ymin, xmax, ymax));
                }
            }
        }
    }

    void UpdateUI()
    {

        if (!enableZoom || !isMagnifying || hideUI)
        {
            zoomUIRoot.SetActive(false);
            return;
        }

        zoomUIRoot.SetActive(true);

        float zoomCanvasHeight = zoomCanvasWidth / camera.aspect;
        zoomCanvasImage.rectTransform.sizeDelta = new Vector2(zoomCanvasWidth, zoomCanvasHeight);

        zoomIndicatorImage.rectTransform.anchoredPosition = new Vector3((xmin + xmax) * zoomCanvasWidth * 0.25f, (ymin + ymax) * zoomCanvasHeight * 0.25f, 0f);
        zoomIndicatorImage.rectTransform.sizeDelta = new Vector2((xmax - xmin) * 0.5f * zoomCanvasWidth, (ymax - ymin) * 0.5f * zoomCanvasHeight);

        zoomText.text = zoomLevel.ToString("f1") + "x";
    }


    // ---------- Private Unity Event

    void Start()
    {
        camera = GetComponent<Camera>();
        UpdateUI();
        print("Scroll to zoom, drag to pan the view, press [H] to toggle UI on/off.");
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.H))
        {
            hideUI = !hideUI;
            UpdateUI();
        }

        if (enableZoom)
        {
            UpdateZoom();
        }
    }

}

