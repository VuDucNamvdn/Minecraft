using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    [SerializeField] Text text;
    [SerializeField] World world;
    [SerializeField] Transform player;
    float frameRate;
    float timer;

    int halfWorldSizeInVoxels;
    int halfWorldSizeInChunks;

    void Start()
    {

        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<Text>();

        halfWorldSizeInVoxels = VoxelData.worldSizeInChunks / 2;
        halfWorldSizeInChunks = VoxelData.worldSizeInChunks / 2;

    }

    void Update()
    {

        string debugText = "Nam Tập làm game";
        debugText += "\n";
        debugText += frameRate + " fps";
        debugText += "\n\n";
        debugText += "XYZ: " + (Mathf.FloorToInt(player.transform.position.x) - halfWorldSizeInVoxels) + " / " + Mathf.FloorToInt(player.transform.position.y) + " / " + (Mathf.FloorToInt(player.transform.position.z) - halfWorldSizeInVoxels);
        debugText += "\n";
        if(world.PlayerChunkCoord!=null)
            debugText += "Chunk: " + (world.PlayerChunkCoord.x - halfWorldSizeInChunks) + " / " + (world.PlayerChunkCoord.z - halfWorldSizeInChunks);



        text.text = debugText;

        if (timer > 1f)
        {

            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;

        }
        else
            timer += Time.deltaTime;

    }
}
