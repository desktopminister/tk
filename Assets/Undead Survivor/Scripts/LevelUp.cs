using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class LevelUp : MonoBehaviour
{
    RectTransform rect;
    Item[] items;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        items = GetComponentsInChildren<Item>(true);
    }

    public void Show()
    {
        TKGameAudio.Play(TKGameAudio.Cue.LevelUp);
        Next();
        rect.localScale = Vector3.one;
        GameManager.instance.Stop();
    }
    public void Hide()
    {
        rect.localScale = Vector3.zero;
        if (!GameManager.instance.ShowPendingLevelUp())
            GameManager.instance.Resume();
    }

    public void Select(int index)
    {
        items[index].OnClick();
    }

    void Next()
    {
        //1. 모든 아이템 비활성화
        foreach (Item item in items)
        {
            item.gameObject.SetActive(false);
        }

        List<Item> available = new List<Item>();
        Item heal = null;
        foreach (Item item in items)
        {
            if (item.data.itemType == ItemData.ItemType.Heal)
                heal = item;
            if (item.level < item.data.damages.Length)
                available.Add(item);
        }
        if (available.Count == 0 && heal != null)
        {
            heal.level = 0;
            heal.GetComponent<Button>().interactable = true;
            available.Add(heal);
        }

        for (int i = 0; i < 3 && available.Count > 0; i++)
        {
            int index = Random.Range(0, available.Count);
            available[index].gameObject.SetActive(true);
            available.RemoveAt(index);
        }
    }
}
