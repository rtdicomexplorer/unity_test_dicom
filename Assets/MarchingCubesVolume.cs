using UnityEngine;
using System.Collections.Generic;

public class MarchingCubesVolume : MonoBehaviour
{
    public int threshold = 128; // Voxel value threshold
    public Material surfaceMaterial; // Assign a material in Inspector

    private void Start()
    {
        if (DicomVolumeLoader.VolumeData == null)
        {
            Debug.LogError("No volume loaded!");
            return;
        }

        Mesh mesh = GenerateIsoSurface(DicomVolumeLoader.VolumeData, threshold);

        GameObject isoSurface = new GameObject("IsoSurface");
        isoSurface.transform.position = Vector3.zero;
        MeshFilter mf = isoSurface.AddComponent<MeshFilter>();
        MeshRenderer mr = isoSurface.AddComponent<MeshRenderer>();
        mf.mesh = mesh;
        mr.material = surfaceMaterial != null ? surfaceMaterial : new Material(Shader.Find("Standard"));
        // Scale mesh to match tri-planar quads
        int w = DicomVolumeLoader.Width;
        int h = DicomVolumeLoader.Height;
        int d = DicomVolumeLoader.Depth;

        isoSurface.transform.localScale = new Vector3(
            1f / w,
            1f / h,
            1f / d
        );

        // Make material semi-transparent so you can see slices through it
        if (mr.material != null)
        {
            mr.material.SetFloat("_Mode", 3); // Transparent mode
            mr.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mr.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mr.material.SetInt("_ZWrite", 0);
            mr.material.DisableKeyword("_ALPHATEST_ON");
            mr.material.EnableKeyword("_ALPHABLEND_ON");
            mr.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mr.material.renderQueue = 3000;

            Color c = mr.material.color;
            c.a = 0.3f; // transparency (0=fully transparent, 1=opaque)
            mr.material.color = c;
        }
    }

    Mesh GenerateIsoSurface(byte[,,] volume, int threshold)
    {
        int w = volume.GetLength(0);
        int h = volume.GetLength(1);
        int d = volume.GetLength(2);

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // VERY SIMPLE: check each voxel and create cube if above threshold
        for (int x = 0; x < w - 1; x++)
        {
            for (int y = 0; y < h - 1; y++)
            {
                for (int z = 0; z < d - 1; z++)
                {
                    if (volume[x, y, z] >= threshold)
                    {
                        // create a cube for this voxel
                        AddCube(vertices, triangles, new Vector3(x, y, z));
                    }
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // support large meshes
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    void AddCube(List<Vector3> vertices, List<int> triangles, Vector3 pos)
    {
        // Cube vertices relative to pos
        Vector3[] cubeVerts = new Vector3[]
        {
            pos + new Vector3(0,0,0),
            pos + new Vector3(1,0,0),
            pos + new Vector3(1,1,0),
            pos + new Vector3(0,1,0),
            pos + new Vector3(0,0,1),
            pos + new Vector3(1,0,1),
            pos + new Vector3(1,1,1),
            pos + new Vector3(0,1,1)
        };

        int startIndex = vertices.Count;
        vertices.AddRange(cubeVerts);

        int[] cubeTris = new int[]
        {
            0,2,1,0,3,2, // Front
            5,6,4,6,7,4, // Back
            4,7,0,7,3,0, // Left
            1,2,5,2,6,5, // Right
            3,7,2,7,6,2, // Top
            4,0,5,0,1,5  // Bottom
        };

        for (int i = 0; i < cubeTris.Length; i++)
        {
            triangles.Add(startIndex + cubeTris[i]);
        }
    }
}
