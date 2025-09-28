using UnityEngine;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using System.IO;
using System.Linq;

public class DicomVolumeLoader : MonoBehaviour
{

    public static event System.Action OnVolumeLoaded;

    public string dicomFolderPath;

    public static byte[,,] VolumeData; // [x, y, z]
    public static int Width, Height, Depth;

    void Awake()
    {
        if (string.IsNullOrEmpty(dicomFolderPath))
        { 
            return;
        }
        LoadDicomSeries(dicomFolderPath);
    }

    void LoadDicomSeries(string folderPath)
    {
        string[] files = Directory.GetFiles(folderPath, "*.dcm");
        if (files.Length == 0)
        {
            Debug.LogError("No DICOM files found in folder: " + folderPath);
            return;
        }

        files = files.OrderBy(f =>
        {
            var dicomFile = DicomFile.Open(f);
            return dicomFile.Dataset.GetSingleValueOrDefault(DicomTag.InstanceNumber, 0);
        }).ToArray();

        // read first slice to get dimensions
        var firstFile = DicomFile.Open(files[0]);
        var firstImage = new DicomImage(firstFile.Dataset);
        var firstRendered = firstImage.RenderImage();

        Width = firstRendered.Width;
        Height = firstRendered.Height;
        Depth = files.Length;

        VolumeData = new byte[Width, Height, Depth];

        for (int z = 0; z < Depth; z++)
        {
            var dicomFile = DicomFile.Open(files[z]);
            var dicomImage = new DicomImage(dicomFile.Dataset);
            var rendered = dicomImage.RenderImage();
            var pixelData = rendered.Pixels;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int idx = y * Width + x;
                    byte intensity = (byte)((pixelData[idx] >> 16) & 0xFF); // grayscale
                    VolumeData[x, y, z] = intensity;
                }
            }
        }

        Debug.Log($"Volume loaded: {Width}x{Height}x{Depth}");

        OnVolumeLoaded?.Invoke();
    }
}
