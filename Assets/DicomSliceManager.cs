using UnityEngine;

public static class DicomSliceManager
{
    public static int AxialSlice = 0;
    public static int CoronalSlice = 0;
    public static int SagittalSlice = 0;

    // Call this when volume is loaded to reset middle slices
    public static void ResetSlices()
    {
        if (DicomVolumeLoader.VolumeData != null)
        {
            AxialSlice = DicomVolumeLoader.Depth / 2;
            CoronalSlice = DicomVolumeLoader.Height / 2;
            SagittalSlice = DicomVolumeLoader.Width / 2;
        }
    }
}
