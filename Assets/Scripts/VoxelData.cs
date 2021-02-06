using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public static readonly int chunkWidth = 16;
    public static readonly int chunkHeight = 128;

    public static readonly int worldSizeInChunks = 100;

    public static int worldSizeInVoxel
    {
        get
        {
            return worldSizeInChunks*chunkWidth;
        }
    }

    public static readonly int viewDistanceInChunks = 5;
    public static readonly int TextureAtlasSizeInBlocks = 4;
    public static float NormalizedBlockTextureSize
    {

        get
        {
            return 1f / (float)TextureAtlasSizeInBlocks;
        }

    }

    public static Vector3[] voxelVerts = new Vector3[8]
    {
        new Vector3(0,0,0),new Vector3(1,0,0),
        new Vector3(1,1,0),new Vector3(0,1,0),
        new Vector3(0,0,1),new Vector3(1,0,1),
        new Vector3(1,1,1),new Vector3(0,1,1)
    };
    public static readonly Vector3[] faceCheck = new Vector3[6]
    {
        new Vector3(0,0,-1), //Backface
        new Vector3(0,0,1), // Front Face
        new Vector3(0,1,0), // Top Face
        new Vector3(0,-1,0), // Bottom Face
        new Vector3(-1,0,0), // Left Face
        new Vector3(1,0,0) // Right Face
    };
    public static readonly int[,] voxelTris = new int[6, 4] 
    {

        {0, 3, 1, 2}, // Back Face
		{5, 6, 4, 7}, // Front Face
		{3, 7, 2, 6}, // Top Face
		{1, 5, 0, 4}, // Bottom Face
		{4, 7, 0, 3}, // Left Face
		{1, 2, 5, 6} // Right Face

	};
    public static readonly Vector2[] voxelUvs = new Vector2[4]
    {
        new Vector2 (0.0f, 0.0f),
        new Vector2 (0.0f, 1.0f),
        new Vector2 (1.0f, 0.0f),
        new Vector2 (1.0f, 1.0f)
    };
}
