using UnityEngine;
using UnityEngine.UI;
public class Item : MonoBehaviour
{
    public ItemData data;
    public int level;
    public Weapon weapon;
    public Gear gear;

    Image icon;
    Text textLevel;
    Text textName;
    Text textDesc;

    void Awake()
    {
        icon = GetComponentsInChildren<Image>()[1];
        icon.sprite = data.itemIcon;

        Text[] texts = GetComponentsInChildren<Text>();
        textLevel = texts[0];
        textName = texts[1];
        textDesc = texts[2];
        textName.text = data.itemName;
    }

    void OnEnable()
    {
        textLevel.text = "Lv. " + (level + 1);

        switch (data.itemType)
        {
            case ItemData.ItemType.Melee:
            case ItemData.ItemType.Ranged:
                textDesc.text = string.Format(data.itemDesc, data.damages[level] * 100, data.counts[level]);
                break;
            case ItemData.ItemType.FireBreath:
            case ItemData.ItemType.LightningStrike:
                textDesc.text = level == 0
                    ? (data.itemType == ItemData.ItemType.FireBreath
                        ? "가장 가까운 적 방향으로 불을 뿜습니다."
                        : "범위 내 무작위 적에게 번개를 떨어뜨립니다.")
                    : string.Format(data.itemDesc, data.damages[level] * 100, data.counts[level]);
                break;
            case ItemData.ItemType.Glove:
            case ItemData.ItemType.Shoe:
                textDesc.text = string.Format(data.itemDesc, data.damages[level] * 100);
                break;
            case ItemData.ItemType.Heal:
                textDesc.text = string.Format(data.itemDesc);
                break;
        }

    }



    public void OnClick()
    {
        // Implement the logic for when the item is clicked
        switch (data.itemType)
        {
            case ItemData.ItemType.Melee:
            case ItemData.ItemType.Ranged:
            case ItemData.ItemType.FireBreath:
            case ItemData.ItemType.LightningStrike:
                // Implement ranged weapon click logic
                if (level == 0)
                {
                    GameObject newWeapon = new GameObject();
                    weapon = newWeapon.AddComponent<Weapon>();
                    weapon.Init(data);
                }
                else
                {
                    float nextDamage = data.damages[level];
                    int nextCount = 0;
                    if (data.itemType == ItemData.ItemType.FireBreath || data.itemType == ItemData.ItemType.LightningStrike)
                        nextDamage = data.baseDamage * (1f + data.damages[level]);
                    else
                        nextDamage += data.baseDamage * data.damages[level];
                    nextCount += data.counts[level];
                    weapon.LevelUp(nextDamage, nextCount);
                }
                level++;
                break;
            case ItemData.ItemType.Glove:
            case ItemData.ItemType.Shoe:
                if (level == 0)
                {
                    GameObject newGear = new GameObject();
                    gear = newGear.AddComponent<Gear>();
                    gear.Init(data);
                }
                else
                {
                    float nextRate = data.damages[level];
                    gear.LevelUp(nextRate);
                }
                level++;
                break;
            case ItemData.ItemType.Heal:
                GameManager.instance.health = GameManager.instance.maxHealth;
                level++;
                break;
            default:
                // Implement default click logic
                break;
        }

        if (level == data.damages.Length)
        {
            GetComponent<Button>().interactable = false;
        }
    }
}
