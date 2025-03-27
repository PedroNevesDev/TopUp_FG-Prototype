using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public List<GameObject> menus = new List<GameObject>();

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(menus.Count>0 && menus[0].activeSelf)
            {
                HideAllMenus();
            }
            else
            {
                OpenMenu(0);
            }
        }

    }

    public void OpenMenu(Menus menuIndex)
    {
        HideAllMenus();
        if((int)menuIndex<menus.Count)
        {
            menus[(int)menuIndex].SetActive(true);
        }
    }

    public void HideAllMenus()
    {
        menus.ForEach(m=>m.SetActive(false));
    }

}

public enum Menus
{
    CardMenu,
    SkillTree
}
