using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/ItemData")]
public class ItemData : ScriptableObject
{
    public enum ItemType
    {
        Melee, Ranged, Glove, Shoe, Heal, FireBreath, LightningStrike
    }
    //아이템 데이터 관리
    [Header("# Main Info")]
    public ItemType itemType;
    public int itemId;
    public string itemName;
    [TextArea]
    public string itemDesc;
    public Sprite itemIcon;
    [Header("# Level Data")]
    public float baseDamage;
    public int baseCount;
    public float[] damages;
    public int[] counts;
    [Header("# Weapon")]
    public GameObject projectile;
    public Sprite hand;
    [Header("# Area Weapon")]
    public float attackInterval;
    public float attackRange;
    public float areaRadius;
    public float effectScale = 1f;

}
