using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown fpsDropdown;
    public Toggle fullscreenToggle;
    public Toggle showFPSToggle;
    public TextMeshProUGUI fpsDisplay;

    private Resolution[] resolutions;
    private float deltaTime = 0.0f;

    void Start()
    {
        // 1. Initialize Resolution List
        SetupResolution();

        // 2. Load and Apply saved settings from disk
        LoadSettings();
    }

    void Update()
    {
        // FPS Counter Logic
        if (showFPSToggle.isOn)
        {
            if (!fpsDisplay.gameObject.activeSelf) fpsDisplay.gameObject.SetActive(true);

            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            fpsDisplay.text = string.Format("{0:0.} FPS", fps);
        }
        else
        {
            if (fpsDisplay.gameObject.activeSelf) fpsDisplay.gameObject.SetActive(false);
        }
    }

    void SetupResolution()
    {
        // Get all resolutions supported by the monitor
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            // Build a string like "1920x1080 @60Hz"
            string option = resolutions[i].width + "x" + resolutions[i].height + " @" + resolutions[i].refreshRateRatio.value.ToString("0") + "Hz";
            options.Add(option);

            // Check if this is the resolution the player is currently using
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        // If we haven't saved a resolution yet, default to current monitor res
        int savedRes = PlayerPrefs.GetInt("ResIndex", currentResIndex);
        resolutionDropdown.value = savedRes;
        resolutionDropdown.RefreshShownValue();
    }

    public void ApplyAndSaveSettings()
    {
        // A. Apply Fullscreen & Resolution
        // NOTE: Screen.SetResolution only works in a built .exe, not the Unity Editor!
        bool isFull = fullscreenToggle.isOn;
        Resolution res = resolutions[resolutionDropdown.value];
        Screen.SetResolution(res.width, res.height, isFull);

        // B. Apply Frame Rate & V-Sync
        SetFrameRateLogic(fpsDropdown.value);

        // C. Save settings to PlayerPrefs
        PlayerPrefs.SetInt("ResIndex", resolutionDropdown.value);
        PlayerPrefs.SetInt("Fullscreen", isFull ? 1 : 0);
        PlayerPrefs.SetInt("FPSCapIndex", fpsDropdown.value);
        PlayerPrefs.SetInt("ShowFPS", showFPSToggle.isOn ? 1 : 0);

        PlayerPrefs.Save();
        Debug.Log("Settings Applied and Saved to Disk.");
    }

    private void LoadSettings()
    {
        // Load Fullscreen (default to 1/True if not found)
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        // Load FPS Dropdown index (default to 1/60FPS if not found)
        fpsDropdown.value = PlayerPrefs.GetInt("FPSCapIndex", 1);

        // Load Show FPS toggle
        showFPSToggle.isOn = PlayerPrefs.GetInt("ShowFPS", 0) == 1;

        // Apply them immediately on startup
        ApplyAndSaveSettings();
    }

    private void SetFrameRateLogic(int index)
    {
        // IMPORTANT: V-Sync MUST be 0 (Disabled) for Application.targetFrameRate to work.
        QualitySettings.vSyncCount = 0;

        switch (index)
        {
            case 0: Application.targetFrameRate = 30; break;
            case 1: Application.targetFrameRate = 60; break;
            case 2: Application.targetFrameRate = 144; break;
            case 3: Application.targetFrameRate = -1; break; // Uncapped
        }
    }
}
