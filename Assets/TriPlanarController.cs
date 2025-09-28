using UnityEngine;

public class TriPlanarController : MonoBehaviour
{
    public DicomPlaneViewer axialViewer;
    public DicomPlaneViewer coronalViewer;
    public DicomPlaneViewer sagittalViewer;

    private int x, y, z;

    void Start()
    {
        // Start at center of volume
        x = DicomVolumeLoader.Width / 2;
        y = DicomVolumeLoader.Height / 2;
        z = DicomVolumeLoader.Depth / 2;

        UpdateViews();
    }

    void Update()
    {
        // Example controls: arrow keys move through volume
        if (Input.GetKeyDown(KeyCode.LeftArrow)) x = Mathf.Max(0, x - 1);
        if (Input.GetKeyDown(KeyCode.RightArrow)) x = Mathf.Min(DicomVolumeLoader.Width - 1, x + 1);

        if (Input.GetKeyDown(KeyCode.UpArrow)) y = Mathf.Min(DicomVolumeLoader.Height - 1, y + 1);
        if (Input.GetKeyDown(KeyCode.DownArrow)) y = Mathf.Max(0, y - 1);

        if (Input.GetKeyDown(KeyCode.PageUp)) z = Mathf.Min(DicomVolumeLoader.Depth - 1, z + 1);
        if (Input.GetKeyDown(KeyCode.PageDown)) z = Mathf.Max(0, z - 1);

        UpdateViews();
    }

    void UpdateViews()
    {
        axialViewer.ShowSlice(z);
        coronalViewer.ShowSlice(y);
        sagittalViewer.ShowSlice(x);
    }
}

