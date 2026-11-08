using UnityEngine;
using UnityEngine.UI;
public class HUD : MonoBehaviour
{
    public enum InfoType
    {
        Level,
        Exp,
        Kill,
        Time,
        Health
    }
    public InfoType type;

    Text myText;
    Slider mySlider;

    void Awake()
    {
        myText = GetComponent<Text>();
        mySlider = GetComponent<Slider>();
    }

    void LateUpdate()
    {
        switch(type){
            case InfoType.Level:
                myText.text = GameManager.instance.level.ToString();
                break;
            case InfoType.Exp:
                float curExp = GameManager.instance.exp;
                float maxExp = GameManager.instance.nextExp[GameManager.instance.level];
                mySlider.value = curExp / maxExp;
                break;
            case InfoType.Kill:
                myText.text = GameManager.instance.kill.ToString();
                break;
            case InfoType.Time:
                break;
            case InfoType.Health:
                break;
        }
    }
}
