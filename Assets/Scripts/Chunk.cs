using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    int vertexIndex = 0;
    GameObject chunkObject;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    public byte[,,] voxelMap = new byte[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    World world;
    bool _isActive = false;

    public ChunkCoord coord;
    public bool isVoxelMapPopulated = false;
    public GameObject gameObject
    {
        get { return chunkObject; }
    }
    public Chunk(ChunkCoord _coord, World _world, bool generateOnLoad=false)
    {
        world = _world;
        coord = _coord;
        _isActive = true;
        if(generateOnLoad)
            Init();
    }

    public void Init()
    {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.chunkWidth, 0f, coord.z * VoxelData.chunkWidth);
        chunkObject.name = "Chunk" + coord.ToString();

        PopulateVoxelMap();
        UpdateChunk();
    }

    private void PopulateVoxelMap()
    {
        for (int x = 0; x < VoxelData.chunkWidth; x++)
        {
            for (int y = 0; y < VoxelData.chunkHeight; y++)
            {
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + position);
                }
            }
        }
        isVoxelMapPopulated = true;
    }
    private void UpdateMeshData(Vector3 pos)
    {
        for (int p = 0; p < 6; p++)
        {
            if (!CheckVoxel(pos + VoxelData.faceCheck[p]))
            {
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

                AddTexture(world.blockTypes[blockID].getTextureID(p));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);

                vertexIndex += 4;
            }
        }
    }
    private void UpdateChunk()
    {
        ClearChunkData();
        for (int x = 0; x < VoxelData.chunkWidth; x++)
        {
            for (int y = 0; y < VoxelData.chunkHeight; y++)
            {
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    if(world.blockTypes[voxelMap[x,y,z]].isSolid)
                    {
                        UpdateMeshData(new Vector3(x, y, z));
                    }
                }
            }
        }
        CreateMesh();
    }
    private void ClearChunkData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }
    private void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
    public bool isActive
    {
        get
        {
            return _isActive;
        }
        set
        {
            _isActive = value;
            if (chunkObject != null)
                chunkObject.SetActive(value);
        }
    }
    public Vector3 position
    {
        get
        {
            if (chunkObject != null)
                return chunkObject.transform.position;
            else
                return Vector3.zero;
        }
    }
    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.chunkWidth - 1 ||
           y < 0 || y > VoxelData.chunkHeight - 1 ||
           z < 0 || z > VoxelData.chunkWidth - 1)
            return false;
        else
            return true;
    }
    public void EditVoxel(Vector3 pos, byte newID)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        voxelMap[xCheck, yCheck, zCheck] = newID;
        UpdateSurroundingVoxel(xCheck, yCheck, zCheck);
        UpdateChunk();
    }
    private void UpdateSurroundingVoxel(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);
        for (int p = 0; p < 6; p++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceCheck[p];
            Vector3Int currentVoxelInt = Vector3Int.FloorToInt(currentVoxel);
            if(!IsVoxelInChunk(currentVoxelInt.x, currentVoxelInt.y, currentVoxelInt.z))
            {
                world.GetChunkFromVector3(currentVoxel + position).UpdateChunk();
            }
        }
    }
    private bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        if (!IsVoxelInChunk(x, y, z))
            return world.CheckForVoxel(pos + position);
        return world.blockTypes[voxelMap[x, y, z]].isSolid;
    }
    public byte GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        return voxelMap[xCheck, yCheck, zCheck];
    }
    private void AddTexture(int textureID)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }
}

public class ChunkCoord
{
    public int x;
    public int z;
    public ChunkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }
    public ChunkCoord()
    {
        x = 0;
        z = 0;
    }
    public ChunkCoord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VoxelData.chunkWidth;
        z = zCheck / VoxelData.chunkWidth;
    }
    public override string ToString()
    {
        return x + "," + z;
    }
    public bool Equals(ChunkCoord other)
    {
        if (other == null)
        {
            return false;
        }

        else if (other.x == x && other.z == z)
        {
            return true;
        }
        return false;
    }
}