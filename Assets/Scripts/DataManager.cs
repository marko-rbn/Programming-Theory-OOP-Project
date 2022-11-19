using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataManager : MonoBehaviour
{

    public static DataManager Instance;

    public Settings settings { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void InitializeSettings(Settings newSettings)
    {
        settings = newSettings;
    }
}


[System.Serializable]
public class Settings
{
    public int Predator_PopulationSize { get; set; }
    public float Predator_ProliferationRate { get; set; }
    public int Predator_MaxLifespan { get; set; }
    public int Prey_PopulationSize { get; set; }
    public float Prey_ProliferationRate { get; set; }
    public int Prey_MaxLifespan { get; set; }
    public int Plant_PopulationSize { get; set; }
    public float Plant_ProliferationRate { get; set; }
    public int Plant_MaxLifespan { get; set; }
    public int Fungus_PopulationSize { get; set; }
    public float Fungus_ProliferationRate { get; set; }
    public int Fungus_MaxLifespan { get; set; }

    //initialized to label prefixes
    public string Predator_PopulationSize_Desc { get; set; }
    public string Predator_ProliferationRate_Desc { get; set; }
    public string Predator_MaxLifespan_Desc { get; set; }
    public string Prey_PopulationSize_Desc { get; set; }
    public string Prey_ProliferationRate_Desc { get; set; }
    public string Prey_MaxLifespan_Desc { get; set; }
    public string Plant_PopulationSize_Desc { get; set; }
    public string Plant_ProliferationRate_Desc { get; set; }
    public string Plant_MaxLifespan_Desc { get; set; }
    public string Fungus_PopulationSize_Desc { get; set; }
    public string Fungus_ProliferationRate_Desc { get; set; }
    public string Fungus_MaxLifespan_Desc { get; set; }


    private string PropNameFromSlider(Slider slider)
    {
        string[] nameParts = slider.gameObject.name.Split(" ");
        string propName = nameParts[0] + "_" + nameParts[1];
        return propName;
    }

    //store slider value in corresponding setting
    public void SaveSliderValue(Slider slider)
    {
        string propName = PropNameFromSlider(slider);
        PropertyInfo prop = this.GetType().GetProperty(propName);
        if (prop != null)
        {
            prop.SetValue(this, System.Convert.ChangeType(slider.value, prop.PropertyType));
            return;
        }

        //something went wrong
        Debug.Log("Settings property " + propName + " not found!");
    }

    //store slider label text prefix in corresponding setting
    //to be used as text prefix for slider UI updates
    //run once through at the start of Scene
    public void SaveSliderDescription(Slider slider)
    {
        string propName = PropNameFromSlider(slider) + "_Desc";
        PropertyInfo prop = this.GetType().GetProperty(propName);
        if (prop != null)
        {
            var labelObject = slider.gameObject.transform.Find("Label").gameObject;
            prop.SetValue(this, labelObject.GetComponent<TextMeshProUGUI>().text);
            return;
        }

        //something went wrong
        Debug.Log("Settings property " + propName + " not found!");
    }

    public float GetSliderValue(Slider slider)
    {
        string propName = PropNameFromSlider(slider);
        PropertyInfo prop = this.GetType().GetProperty(propName);
        if (prop != null)
        {
            return (float)prop.GetValue(this);
        }

        //something went wrong
        Debug.Log("Settings property " + propName + " not found!");
        return 0f;
    }

    public string GetSliderDescription(Slider slider)
    {
        string propName = PropNameFromSlider(slider) + "_Desc";
        PropertyInfo prop = this.GetType().GetProperty(propName);
        if (prop != null)
        {
            string descriptionPrefix = prop.GetValue(this).ToString();
            return descriptionPrefix;
        }

        //something went wrong
        Debug.Log("Settings property " + propName + " not found!");
        return null;
    }

    public float GetPropertyForTag(string tag, string settingName)
    {
        string propName = tag + "_" + settingName;
        PropertyInfo prop = this.GetType().GetProperty(propName);
        if (prop != null)
        {
            return System.Convert.ToSingle(prop.GetValue(this));
        }

        //something went wrong
        Debug.Log("Settings property " + propName + " not found!");
        return 0f;
    }


}
