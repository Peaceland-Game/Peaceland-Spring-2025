using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
public class ResolutionControl : MonoBehaviour
{
    Resolution[] resolutions;
    List<Resolution> filteredResolutions;
    public TMP_Dropdown resDropdown;
    bool fullscreen;
    private int currentIndex;
    void Start()
    {
        //get the resolutions list from the screen
        resolutions = Screen.resolutions;
        resDropdown.ClearOptions();
        fullscreen = Screen.fullScreen;

        filteredResolutions = new List<Resolution>();
        Resolution recentRes = resolutions[0];
        //filter through the available resolutions
        for (int i = 0; i < resolutions.Length; i++)
        {
            //only include the first resolution and subsequent resolutions that aren't duplicates
            if ((i == 0 
                || (resolutions[i].width != recentRes.width && resolutions[i].height != recentRes.height))
                //also filter for wide enough screen resolutions
                && ((double)resolutions[i].width / (double)resolutions[i].height >= 1.76)
                && ((double)resolutions[i].width / (double)resolutions[i].height <= 1.78))
            {
                
                //add the resolution to the filtered list as well as change the resolution we're checking duplicates against
                filteredResolutions.Add(resolutions[i]);
                recentRes = resolutions[i];
            }
        }

        //convert the resolutions to a string list
        List<string> resStrings = new List<string>();
        for(int i = 0; i < filteredResolutions.Count; i++)
        {
            string option = filteredResolutions[i].width + " x " + filteredResolutions[i].height;
            if( i == filteredResolutions.Count - 1)
            {
                option += " (Fullscreen)";
            }
            resStrings.Add(option);

            //test if the resolution matches the screen resolution
            if (filteredResolutions[i].width == Screen.currentResolution.width && filteredResolutions[i].height == Screen.currentResolution.height)
            {
                currentIndex = i;
            }
        }
        //add the options to the dropdown and set it at the current resolution
        resDropdown.AddOptions(resStrings);
        resDropdown.value = currentIndex;
        SetResolution(currentIndex);
    }

    public void SetResolution(int index)
    {
        Resolution res;
        //lock to fullscreen if maximum resolution
        if (index == filteredResolutions.Count - 1)
        {
            currentIndex = filteredResolutions.Count - 1;
            res = filteredResolutions[currentIndex];
            fullscreen = true;
        }
        else
        {
            fullscreen = false;
        }
        //update the current index and set the screen's new resolution
        res = filteredResolutions[index];
        currentIndex = index;
        Screen.SetResolution(res.width, res.height, fullscreen);
    }
}
