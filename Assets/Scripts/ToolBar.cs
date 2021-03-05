using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolBar : MonoBehaviour
{
    World world;
    [SerializeField] PlayerController player;
    [SerializeField] RectTransform highlight;
    [SerializeField] ItemSlot[] itemSlots;
    int slotIndex = 0;
    private void Start()
    {
        world = FindObjectOfType<World>();
        foreach(var slot in itemSlots)
        {
            slot.icon.sprite = world.blockTypes[slot.itemID].icon;
            slot.icon.enabled = true;
        }
        player.currentBlockIdex = itemSlots[slotIndex].itemID;
    }
    private void Update()
    {
        float scroll = Input.GetAxis("MouseScrollWheel");
        if(scroll!=0)
        {
            if(scroll>0)
            {
                slotIndex++;
            }
            else if (scroll < 0)
            {
                slotIndex--;
            }
            if(slotIndex<0)
            {
                slotIndex = itemSlots.Length - 1;
            }
            else if (slotIndex > itemSlots.Length - 1)
            {
                slotIndex = 0;
            }
            highlight.pivot = new Vector2(-slotIndex, 0.5f);
            //SetPivot(highlight, new Vector2(slotIndex,0.5f));
            player.currentBlockIdex = itemSlots[slotIndex].itemID;
        }

    }
    public static void SetPivot(RectTransform rectTransform, Vector2 pivot)
    {
        if (rectTransform == null) return;

        Vector2 size = rectTransform.rect.size;
        Vector2 deltaPivot = rectTransform.pivot - pivot;
        Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
        rectTransform.pivot = pivot;
        rectTransform.localPosition -= deltaPosition;
    }
}
[System.Serializable]
public class ItemSlot
{
    public byte itemID;
    public Image icon;
}
