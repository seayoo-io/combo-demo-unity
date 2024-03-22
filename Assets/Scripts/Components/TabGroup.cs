using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    [HideInInspector]
    public List<TabButton> tabButtons = new List<TabButton>();
    public List<GameObject> tabPages = new List<GameObject>();

    public Sprite tabIdleSprite;
    public Sprite tabHoverSprite;
    public Sprite tabSelectedSprite;
    private TabButton selectedTab;

    public void Start()
    {
        foreach (TabButton tabButton in tabButtons)
        {
            if (tabButton.transform.GetSiblingIndex() == 0)
            {
                OnTabSelected(tabButton);
                tabButton.background.sprite = tabSelectedSprite;
            }
        }
    }

    public void Subscribe(TabButton tabButton)
    {
        tabButtons.Add(tabButton);

        tabButtons.Sort((x, y) => x.transform.GetSiblingIndex().CompareTo(y.transform.GetSiblingIndex()));
    }

    public void OnTabEnter(TabButton tabButton)
    {
        ResetTabs();
        if ((selectedTab == null) || (tabButton != selectedTab))
        {
            tabButton.background.sprite = tabHoverSprite;
        }
    }

    public void OnTabExit(TabButton tabButton)
    {
        ResetTabs();
    }

    public void OnTabSelected(TabButton tabButton)
    {
        if (selectedTab != null)
        {
            selectedTab.Deselect();
        }

        selectedTab = tabButton;

        selectedTab.Select();

        ResetTabs();
        tabButton.background.sprite = tabSelectedSprite;
        int index = tabButton.transform.GetSiblingIndex();
        for (int i = 0; i < tabPages.Count; i++)
        {
            if (i == index)
            {
                tabPages[i].SetActive(true);
            }
            else
            {
                tabPages[i].SetActive(false);
            }
        }
    }

    public void ResetTabs()
    {
        foreach (TabButton tabButton in tabButtons)
        {
            if ((selectedTab != null) && (tabButton == selectedTab))
            {
                continue;
            }

            tabButton.background.sprite = tabIdleSprite;
        }
    }

    public void NextTab()
    {
        int currentIndex = selectedTab.transform.GetSiblingIndex();
        int nextIndex = currentIndex < tabButtons.Count - 1 ? currentIndex + 1 : tabButtons.Count - 1;
        OnTabSelected(tabButtons[nextIndex]);
    }

    public void PreviousTab()
    {
        int currentIndex = selectedTab.transform.GetSiblingIndex();
        int previousIndex = currentIndex > 0 ? currentIndex - 1 : 0;
        OnTabSelected(tabButtons[previousIndex]);
    }
}
