using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatModifierCard : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;

    public Image statIcon;

    public void Setup(Stat stat)
    {
        title.text = stat.GetFormatted();
        description.text = stat.smallDescription;
        statIcon.sprite = stat.statIcon;
    }
}
