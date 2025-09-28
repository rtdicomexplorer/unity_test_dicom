#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class DicomFolderPicker : MonoBehaviour
{
    public void PickFolder()
    {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFolderPanel("Select DICOM Folder", "", "");
        if (!string.IsNullOrEmpty(path))
        {
            Debug.Log("Picked DICOM folder: " + path);

            // Update loader
            DicomVolumeLoader loader = FindFirstObjectByType<DicomVolumeLoader>();
            if (loader != null)
            {
                loader.dicomFolderPath = path;
                loader.SendMessage("LoadDicomSeries", path); // reload volume

                DicomSliceManager.ResetSlices();

                DicomPlaneViewer[] viewers = FindObjectsOfType<DicomPlaneViewer>();
                foreach (var viewer in viewers)
                {
                    viewer.ReloadSlice();
                }

            }

            //// Tell all viewers to refresh
            //DicomPlaneViewer[] viewers = FindObjectsOfType<DicomPlaneViewer>();
            //foreach (var viewer in viewers)
            //{
            //    viewer.ReloadSlice();
            //}
        }
#endif
    }
}
