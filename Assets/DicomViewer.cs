using UnityEngine;

using FellowOakDicom;

using FellowOakDicom.Imaging;
using Color32 = UnityEngine.Color32;
public class DicomViewer : MonoBehaviour
{
    public string dicomFilePath;

    void Start()
    {
        // Load DICOM
        var dicomFile = DicomFile.Open(dicomFilePath);
        var dicomImage = new DicomImage(dicomFile.Dataset);

        // Render image
        var rendered = dicomImage.RenderImage();
        var pixelData = rendered.Pixels;

        int width = rendered.Width;
        int height = rendered.Height;

        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);

        Color32[] colors = new Color32[width * height];

        // Copy pixels
        for (int i = 0; i < width * height; i++)
        {
            int packed = pixelData[i]; // PinnedIntArray has indexer
            byte a = (byte)((packed >> 24) & 0xFF);
            byte r = (byte)((packed >> 16) & 0xFF);
            byte g = (byte)((packed >> 8) & 0xFF);
            byte b = (byte)(packed & 0xFF);

            colors[i] = new Color32(r, g, b, a);
        }

        texture.SetPixels32(colors);
        texture.Apply();

        // Show texture
        GetComponent<Renderer>().material.mainTexture = texture;
    }

}
