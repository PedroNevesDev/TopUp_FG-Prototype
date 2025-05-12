using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatModifierCard : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;

    public Image statIcon;

    public Stat myStat;

    public void Setup(Stat stat)
    {
        myStat = stat;
        title.text = stat.GetFormatted();
        description.text = stat.smallDescription;
        statIcon.sprite = stat.statIcon;
    }
}
