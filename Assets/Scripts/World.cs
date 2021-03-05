using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] int seed;
    [SerializeField] BiomeAttributes biome;
    [SerializeField] Sprite[] icons;

    public Vector3 spawnPosittion;
    public Material material;
    public Material transparentMaterial;
    public BlockType[] blockTypes;
    Chunk[,] chunks = new Chunk[VoxelData.worldSizeInChunks, VoxelData.worldSizeInChunks];
    List<ChunkCoord> activeChunks = new List<ChunkCoord>();

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;
    public ChunkCoord PlayerChunkCoord
    {
        get
        {
            return playerChunkCoord;
        }
    }
    bool isCreatingChunks;
    private void Start()
    {
        Random.InitState(seed);
        spawnPosittion = new Vector3(VoxelData.worldSizeInChunks * VoxelData.chunkWidth / 2f, VoxelData.chunkHeight,
                             VoxelData.worldSizeInChunks * VoxelData.chunkWidth / 2f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }
    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
        }
        if(chunksToCreate.Count>0&&!isCreatingChunks)
        {
            StartCoroutine("CreateChunks");
        }
    }
    void GenerateWorld()
    {
        int startSpawnCoord = ((VoxelData.worldSizeInChunks / 2) - VoxelData.viewDistanceInChunks);
        int endSpawnCoord = ((VoxelData.worldSizeInChunks / 2) + VoxelData.viewDistanceInChunks);
        for (int x = startSpawnCoord; x < endSpawnCoord; x++)
        {
            for (int z = startSpawnCoord; z < endSpawnCoord; z++)
            {
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this,true);
                activeChunks.Add(new ChunkCoord(x, z));
            }
        }
        player.position = spawnPosittion;
    }
    IEnumerator CreateChunks()
    {
        isCreatingChunks = true;
        while(chunksToCreate.Count>0)
        {
            chunks[chunksToCreate[0].x, chunksToCreate[0].z].Init();
            chunksToCreate.RemoveAt(0);
            yield return new WaitForSeconds(0.1f);
        }
        isCreatingChunks = false;
    }
    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.chunkWidth);
        return new ChunkCoord(x, z);
    }
    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.chunkWidth);
        return chunks[x, z];
    }
    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;
        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        for (int x = coord.x - VoxelData.viewDistanceInChunks; x < coord.x + VoxelData.viewDistanceInChunks; x++)
        {
            for (int z = coord.z - VoxelData.viewDistanceInChunks; z < coord.z + VoxelData.viewDistanceInChunks; z++)
            {
                if (IsChunkInWorld(new ChunkCoord(x, z)))
                {
                    if (chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                        chunksToCreate.Add(new ChunkCoord(x, z));
                    }
                    if (!chunks[x, z].isActive)
                    {
                        chunks[x, z].isActive = true;
                    }
                    activeChunks.Add(new ChunkCoord(x, z));
                }
                // Check if this chunk was already in the active chunks list.
                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {

                    //if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                    if (previouslyActiveChunks[i].x == x && previouslyActiveChunks[i].z == z)
                        previouslyActiveChunks.RemoveAt(i);

                }

            }
        }
        foreach (ChunkCoord _coord in previouslyActiveChunks)
        {
            chunks[_coord.x, _coord.z].isActive = false;
        }
    }
    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);
        /*PASS*/
        //if outside world
        if (!IsVoxelInWorld(pos))
        {
            return 0;
        }
        if(yPos==0)
        {
            return 1;
        }
        //End of the world
        if (pos.y < 1)
        {
            return 1;
        }
        /*BASIC TERRAIN PASS*/
        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
        byte voxelValue = 0;
        if (yPos==terrainHeight)
        {
            voxelValue = 3;
        }
        else if(yPos<terrainHeight&&yPos>terrainHeight-4)
        {
            voxelValue = 5;
        }
        else if(yPos > terrainHeight)
        {
            voxelValue = 0;
        }
        else
        {
            voxelValue = 2;
        }
        /*Second pass*/
        if(voxelValue==2)
        {
            foreach(var lode in biome.lodes)
            {
                if(yPos>lode.minHeight&&yPos<lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }
        return voxelValue;
    }

    bool IsChunkInWorld(ChunkCoord coord)
    {
        if (coord.x > 0 && coord.x < VoxelData.worldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.worldSizeInChunks - 1)
            return true;
        else
            return false;
    }
    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.worldSizeInVoxel
            && pos.y >= 0 && pos.y < VoxelData.chunkHeight
            && pos.z >= 0 && pos.z < VoxelData.worldSizeInVoxel)
            return true;
        else
            return false;
    }
    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);
        if(!IsVoxelInWorld(pos) || pos.y < 0 || pos.y > VoxelData.chunkHeight)
        {
            return false;
        }
        if(chunks[thisChunk.x,thisChunk.z]!=null && chunks[thisChunk.x, thisChunk.z].isVoxelMapPopulated)
        {
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isSolid;
        }
        return blockTypes[GetVoxel(pos)].isSolid;
    }
    public bool CheckIfVoxelTransparent(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);
        if (!IsVoxelInWorld(pos) || pos.y < 0 || pos.y > VoxelData.chunkHeight)
        {
            return false;
        }
        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isVoxelMapPopulated)
        {
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isTransparent;
        }
        return blockTypes[GetVoxel(pos)].isTransparent;
    }
}
[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;
    public bool isTransparent;
    public Sprite icon;
    [Header("Texture Value")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;
    public int getTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error faceIndex");
                return 0;
        }
    }
}

