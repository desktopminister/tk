using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TKStartingWeaponPicker : MonoBehaviour
{
    [Serializable]
    class Card
    {
        public Button button;
        public Image icon;
        public Text title;
        public Text description;
    }

    [SerializeField] RectTransform content;
    [SerializeField] Card[] cards;
    Action<Item> onChosen;
    Button focus;

    public void Show(List<Item> weapons, Action<Item> choose)
    {
        onChosen = choose;
        for (int i = 0; i < cards.Length; i++)
        {
            var card = cards[i];
            card.button.gameObject.SetActive(i < weapons.Count);
            if (i >= weapons.Count) continue;

            var item = weapons[i];
            card.icon.sprite = item.data.itemIcon;
            card.title.text = item.data.itemName;
            card.description.text = item.data.introDesc;
            card.button.onClick.AddListener(() => Choose(item));
            var nav = new Navigation { mode = Navigation.Mode.Explicit };
            nav.selectOnUp = cards[(i + weapons.Count - 1) % weapons.Count].button;
            nav.selectOnDown = cards[(i + 1) % weapons.Count].button;
            card.button.navigation = nav;
        }
        Fit();
        focus = cards[0].button;
        focus.Select();
    }

    void Choose(Item item)
    {
        if (onChosen == null) return;
        var choose = onChosen;
        onChosen = null;
        gameObject.SetActive(false);
        Destroy(gameObject);
        choose(item);
    }

    void OnRectTransformDimensionsChange() => Fit();

    void Fit()
    {
        if (!content) return;
        Rect bounds = ((RectTransform)transform).rect;
        float scale = Mathf.Min(bounds.width * 0.87f / content.rect.width, bounds.height / 540f);
        if (scale > 0f) content.localScale = Vector3.one * scale;
    }

    void LateUpdate()
    {
        var events = EventSystem.current;
        if (!events || !focus) return;
        var selected = events.currentSelectedGameObject;
        if (!selected || !selected.transform.IsChildOf(content))
            focus.Select();
        else if (selected.TryGetComponent<Button>(out var button))
            focus = button;
    }
}
