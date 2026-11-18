using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class LevelUp : MonoBehaviour
{
    RectTransform rect;
    Item[] items;
    [SerializeField] Item extraCardTemplate;
    [SerializeField] ItemData[] extraRewards;
    [SerializeField] TKStartingWeaponPicker startingWeaponPicker;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (extraCardTemplate && extraRewards != null)
        {
            // The scene supplies an inactive card. Its Item reads the assigned data when first shown.
            foreach (var reward in extraRewards)
            {
                var card = Instantiate(extraCardTemplate, extraCardTemplate.transform.parent);
                card.name = reward.itemName;
                card.data = reward;
            }
        }
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

    public void ChooseStartingWeapon(System.Action<Item> onChosen)
    {
        var pool = new List<Item>();
        foreach (var item in items)
            if (item.data.startingWeapon)
                pool.Add(item);

        // Partial shuffle without replacement, keeping the inventory's Item instances.
        int count = Mathf.Min(3, pool.Count);
        for (int i = 0; i < count; i++)
        {
            int other = Random.Range(i, pool.Count);
            (pool[i], pool[other]) = (pool[other], pool[i]);
        }
        var picker = Instantiate(startingWeaponPicker, rect.parent, false);
        picker.Show(pool.GetRange(0, count), onChosen);
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
