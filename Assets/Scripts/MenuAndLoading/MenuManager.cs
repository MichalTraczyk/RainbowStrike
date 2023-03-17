using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    [SerializeField]
    Menu[] menus;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
    public void OpenMenu(string menuToOpen)
    {
        for(int i = 0;i <menus.Length;i++)
        {
            if(menus[i].menuName == menuToOpen)
            {
                menus[i].Open();
            }
            else if(menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
    }
    public void OpenMenu(Menu menu)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }


        menu.Open();
    }
    void CloseMenu(Menu menu)
    {
        menu.Close();
    }
    public void CloseAllMenus()
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
    }
    public void ExitGame()
    {
        Application.Quit(0);
    }
}
