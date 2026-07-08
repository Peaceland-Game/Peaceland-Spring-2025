using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
public class ResolutionControl : MonoBehaviour
{
    Resolution[] resolutions;
    public TMP_Dropdown resDropdown;
    bool fullscreen;
    [SerializeField] private int currentIndex;
    void Start()
    {
        //get the resolutions list from the screen
        resolutions = Screen.resolutions;
        resDropdown.ClearOptions();
        fullscreen = Screen.fullScreen;

        //convert the resolutions to a string list
        List<string> resStrings = new List<string>();
        for(int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            resStrings.Add(option);

            //test if the resolution matches the screen resolution
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentIndex = i;
            }
        }

        resDropdown.AddOptions(resStrings);
        resDropdown.value = currentIndex;
    }

    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        currentIndex = index;
        Screen.SetResolution(res.width, res.height, fullscreen);
    }

    public void SetFullscreen (bool isFullscreen)
    {
        fullscreen = isFullscreen;
        SetResolution(currentIndex);
    }
}
