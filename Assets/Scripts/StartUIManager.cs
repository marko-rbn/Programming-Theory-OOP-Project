using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Dynamic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartUIManager : MonoBehaviour
{

    public GameObject predatorSection;
    public GameObject grazerSection;
    public GameObject plantSection;
    public GameObject fungusSection;

    private Settings settings;

    private void Start()
    {
        if (DataManager.Instance.settings == null)
        {
            //first time through, initialize settings based on current state of Sliders
            settings = new Settings();
            InitializeFromEntitySection(predatorSection);
            InitializeFromEntitySection(grazerSection);
            InitializeFromEntitySection(plantSection);
            InitializeFromEntitySection(fungusSection);
        }
        else
        {
            //TODO: reinitialize slider values to those stored in DataManager settings
            InitializeEntitySectionFromSettings(predatorSection);
            InitializeEntitySectionFromSettings(grazerSection);
            InitializeEntitySectionFromSettings(plantSection);
            InitializeEntitySectionFromSettings(fungusSection);
        }
    }

    private void InitializeFromEntitySection(GameObject entitySection)
    {
        Slider slider;
        foreach (Transform child in entitySection.gameObject.transform)
        {
            if ((slider = child.gameObject.GetComponent<Slider>()) != null)
            {
                settings.SaveSliderDescription(slider);
                settings.SaveSliderValue(slider);
                UpdateSliderDescription(slider, slider.value);
            }
        }
    }

    private void InitializeEntitySectionFromSettings(GameObject entitySection)
    {
        Slider slider;
        foreach (Transform child in entitySection.gameObject.transform)
        {
            if ((slider = child.gameObject.GetComponent<Slider>()) != null)
            {
                float value = DataManager.Instance.settings.GetSliderValue(slider);
                slider.value = value;
                UpdateSliderDescription(slider, value);
            }
        }
    }

    //value can be int or float
    private void UpdateSliderDescription(Slider slider, float value)
    {
        string prefix = settings.GetSliderDescription(slider);
        var labelObject = slider.gameObject.transform.Find("Label").gameObject;
        string stringValue = value.ToString("0.##");
        labelObject.GetComponent<TextMeshProUGUI>().SetText(prefix + ": " + stringValue);
    }

    public void SliderValueChanged(Slider slider)
    {
        settings.SaveSliderValue(slider);
        UpdateSliderDescription(slider, slider.value);
    }

    public void StartSimulation()
    {
        //copy settings to DataManager
        DataManager.Instance.InitializeSettings(settings);
        //load main simulation scene
        SceneManager.LoadScene(1);
    }
}
