using System.Collections.Generic;
using UnityEngine;
using TMPro; // Use UnityEngine.UI if using legacy Dropdown

public class ResolutionManager : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;
    private Resolution recommendedRes;

    void Start()
    {
        // 1. Get all supported resolutions and the native (recommended) one
        resolutions = Screen.resolutions;
        recommendedRes = Screen.currentResolution;

        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;

            // 2. Compare against native resolution to add the label
            if (resolutions[i].width == recommendedRes.width &&
                resolutions[i].height == recommendedRes.height)
            {
                option += " (recommended)";
            }

            options.Add(option);

            // 3. Highlight the resolution currently in use
            if (resolutions[i].width == Screen.width &&
                resolutions[i].height == Screen.height)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();
    }

    // 4. Function to be called by the Dropdown's "On Value Changed" event
    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
}
