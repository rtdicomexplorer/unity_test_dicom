using UnityEngine;
using UnityEngine.UI; // for Text and Slider
using FellowOakDicom;
using FellowOakDicom.Imaging;
using System.IO;
using System.Linq;
using Color32 = UnityEngine.Color32;
using TMPro;
public class DicomSeriesViewer : MonoBehaviour
{
    [Header("DICOM Settings")]
    public string dicomFolderPath;

    [Header("UI Elements")]
    public TMP_Text sliceNumberText;
    public Slider windowSlider;
    public Slider levelSlider;

    private Texture2D[] slices;       // Textures displayed on the Quad
    private byte[][] originalPixels;  // Original pixel intensities for each slice
    private int currentSlice = 0;

    private float window = 4096f;
    private float level = 2048f;

    void Start()
    {
        LoadDicomSeries(dicomFolderPath);
        ShowSlice(currentSlice);

        // Initialize sliders
        if (windowSlider)
        {
            windowSlider.minValue = 1f;
            windowSlider.maxValue = 4096f;
            windowSlider.value = window;
            windowSlider.onValueChanged.AddListener(OnWindowChanged);
        }
        if (levelSlider)
        {
            levelSlider.minValue = 0f;
            levelSlider.maxValue = 4096f;
            levelSlider.value = level;
            levelSlider.onValueChanged.AddListener(OnLevelChanged);
        }
    }


    void Update()
    {

        // Scroll through slices with mouse wheel
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f && slices != null && slices.Length > 0)
            {
                currentSlice = Mathf.Clamp(currentSlice + (scroll > 0 ? 1 : -1), 0, slices.Length - 1);
                ShowSlice(currentSlice);
            }
        }
    }

    void LoadDicomSeries(string folderPath)
    {
        string[] files = Directory.GetFiles(folderPath, "*.dcm");
        if (files.Length == 0)
        {
            Debug.LogError("No DICOM files found in folder: " + folderPath);
            return;
        }

        // Sort files by InstanceNumber (slice order)
        files = files.OrderBy(f =>
        {
            var dicomFile = DicomFile.Open(f);
            return dicomFile.Dataset.GetSingleValueOrDefault(DicomTag.InstanceNumber, 0);
        }).ToArray();

        slices = new Texture2D[files.Length];
        originalPixels = new byte[files.Length][];

        for (int i = 0; i < files.Length; i++)
        {
            var dicomFile = DicomFile.Open(files[i]);
            var dicomImage = new DicomImage(dicomFile.Dataset);
            var rendered = dicomImage.RenderImage();
            var pixelData = rendered.Pixels;

            int width = rendered.Width;
            int height = rendered.Height;

            Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            byte[] original = new byte[width * height];
            Color32[] colors = new Color32[width * height];

            for (int p = 0; p < width * height; p++)
            {
                int packed = pixelData[p];
                byte intensity = (byte)((packed >> 16) & 0xFF); // red channel for grayscale
                original[p] = intensity;

                colors[p] = new Color32(intensity, intensity, intensity, 255);
            }

            tex.SetPixels32(colors);
            tex.Apply();

            slices[i] = tex;
            originalPixels[i] = original;
        }

        Debug.Log("Loaded " + slices.Length + " DICOM slices.");
    }

    void ShowSlice(int index)
    {
        if (slices == null || slices.Length == 0) return;

        UpdateSliceTexture(index);

   
            sliceNumberText.text = $"Slice: {currentSlice + 1}/{slices.Length}";

    }

    void UpdateSliceTexture(int index)
    {
        byte[] original = originalPixels[index];
        Color32[] colors = new Color32[original.Length];

        float min = level - window / 2f;
        float max = level + window / 2f;

        for (int i = 0; i < original.Length; i++)
        {
            float v = Mathf.Clamp(original[i], min, max);
            float normalized = (v - min) / (max - min);
            byte finalByte = (byte)(normalized * 255f);
            colors[i] = new Color32(finalByte, finalByte, finalByte, 255);
        }

        slices[index].SetPixels32(colors);
        slices[index].Apply();

        GetComponent<Renderer>().material.mainTexture = slices[index];
    }

    void OnWindowChanged(float value)
    {
        window = value;
        ShowSlice(currentSlice);
    }

    void OnLevelChanged(float value)
    {
        level = value;
        ShowSlice(currentSlice);
    }


}

