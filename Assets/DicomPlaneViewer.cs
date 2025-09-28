using UnityEngine;
using UnityEngine.UI;

public class DicomPlaneViewer : MonoBehaviour
{
    public enum PlaneType { Axial, Coronal, Sagittal }
    public PlaneType plane = PlaneType.Axial;

    public Text sliceNumberText;
    [Header("Navigation Keys")]
    public KeyCode sliceUpKey = KeyCode.UpArrow;
    public KeyCode sliceDownKey = KeyCode.DownArrow;
    private int currentSlice = 0;
    private Texture2D tex;

    public Transform crosshair; // drag child object here

    private int CurrentSlice
    {
        get
        {
            switch (plane)
            {
                case PlaneType.Axial: return DicomSliceManager.AxialSlice;
                case PlaneType.Coronal: return DicomSliceManager.CoronalSlice;
                case PlaneType.Sagittal: return DicomSliceManager.SagittalSlice;
            }
            return 0;
        }
        set
        {
            switch (plane)
            {
                case PlaneType.Axial: DicomSliceManager.AxialSlice = value; break;
                case PlaneType.Coronal: DicomSliceManager.CoronalSlice = value; break;
                case PlaneType.Sagittal: DicomSliceManager.SagittalSlice = value; break;
            }
        }
    }


    void OnEnable()
    {
        DicomVolumeLoader.OnVolumeLoaded += ReloadSlice;
    }

    void OnDisable()
    {
        DicomVolumeLoader.OnVolumeLoaded -= ReloadSlice;
    }
    public void SetInitialSlice()
    {
        switch (plane)
        {
            case PlaneType.Axial: currentSlice = DicomVolumeLoader.Depth / 2; break;
            case PlaneType.Coronal: currentSlice = DicomVolumeLoader.Height / 2; break;
            case PlaneType.Sagittal: currentSlice = DicomVolumeLoader.Width / 2; break;
        }
        ShowSlice(currentSlice);
    }
    void Start()
    {
        if (DicomVolumeLoader.VolumeData == null)
        {
            Debug.LogError("Volume not loaded! Add DicomVolumeLoader to scene.");
            return;
        }

        SetInitialSlice();
    }


    void Update()
    {
        if (DicomVolumeLoader.VolumeData == null) return;

        int maxSlice = 0;
        switch (plane)
        {
            case PlaneType.Axial: maxSlice = DicomVolumeLoader.Depth - 1; break;
            case PlaneType.Coronal: maxSlice = DicomVolumeLoader.Height - 1; break;
            case PlaneType.Sagittal: maxSlice = DicomVolumeLoader.Width - 1; break;
        }

        bool changed = false;

        if (Input.GetKeyDown(sliceUpKey))
        {
            CurrentSlice = Mathf.Clamp(CurrentSlice + 1, 0, maxSlice);
            changed = true;
        }

        if (Input.GetKeyDown(sliceDownKey))
        {
            CurrentSlice = Mathf.Clamp(CurrentSlice - 1, 0, maxSlice);
            changed = true;
        }

        if (changed)
        {
            // Broadcast reload to all viewers
            DicomPlaneViewer[] viewers = FindObjectsOfType<DicomPlaneViewer>();
            foreach (var viewer in viewers)
            {
                viewer.ReloadSlice();
            }
        }
    }

    public void ReloadSlice()
    {
        if (DicomVolumeLoader.VolumeData == null) return;

        ShowSlice(CurrentSlice);

        if (crosshair != null)
        {
            Vector3 pos = Vector3.zero;

            // Map slice positions from other planes
            switch (plane)
            {
                case PlaneType.Axial:
                    // crosshair shows sagittal/coronal positions
                    pos.x = DicomSliceManager.SagittalSlice / (float)DicomVolumeLoader.Width;
                    pos.y = DicomSliceManager.CoronalSlice / (float)DicomVolumeLoader.Height;
                    break;
                case PlaneType.Coronal:
                    pos.x = DicomSliceManager.SagittalSlice / (float)DicomVolumeLoader.Width;
                    pos.y = DicomSliceManager.AxialSlice / (float)DicomVolumeLoader.Depth;
                    break;
                case PlaneType.Sagittal:
                    pos.x = DicomSliceManager.CoronalSlice / (float)DicomVolumeLoader.Height;
                    pos.y = DicomSliceManager.AxialSlice / (float)DicomVolumeLoader.Depth;
                    break;
            }

            // Convert normalized position to local Quad coordinates (-0.5 to 0.5)
            pos.x -= 0.5f;
            pos.y -= 0.5f;
            crosshair.localPosition = pos;
        }
    }


    public void ShowSlice(int index)
    {
        var volume = DicomVolumeLoader.VolumeData;
        int w = DicomVolumeLoader.Width;
        int h = DicomVolumeLoader.Height;
        int d = DicomVolumeLoader.Depth;

        Color32[] colors = null;
        int width = 0, height = 0;

        if (plane == PlaneType.Axial)
        {
            width = w; height = h;
            colors = new Color32[width * height];
            index = Mathf.Clamp(index, 0, d - 1);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    byte val = volume[x, y, index];
                    colors[y * w + x] = new Color32(val, val, val, 255);
                }
            }
        }
        else if (plane == PlaneType.Coronal)
        {
            width = w; height = d;
            colors = new Color32[width * height];
            index = Mathf.Clamp(index, 0, h - 1);

            for (int z = 0; z < d; z++)
            {
                for (int x = 0; x < w; x++)
                {
                    byte val = volume[x, index, z];
                    colors[z * w + x] = new Color32(val, val, val, 255);
                }
            }
        }
        else if (plane == PlaneType.Sagittal)
        {
            width = h; height = d;
            colors = new Color32[width * height];
            index = Mathf.Clamp(index, 0, w - 1);

            for (int z = 0; z < d; z++)
            {
                for (int y = 0; y < h; y++)
                {
                    byte val = volume[index, y, z];
                    colors[z * h + y] = new Color32(val, val, val, 255);
                }
            }
        }

        if (tex == null || tex.width != width || tex.height != height)
            tex = new Texture2D(width, height, TextureFormat.ARGB32, false);

        tex.SetPixels32(colors);
        tex.Apply();

        GetComponent<Renderer>().material.mainTexture = tex;

        if (sliceNumberText)
            sliceNumberText.text = $"{plane} Slice: {index}";
    }
}
