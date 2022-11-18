using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
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

        settings = new Settings();

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public void SliderValueChanged(Slider slider)
    {
        string[] nameParts = slider.gameObject.name.Split(" ");
        string propName = nameParts[0] + "_" + nameParts[1];
        //Debug.Log("Slider " + propName + " set to " + slider.value);
        PropertyInfo prop = settings.GetType().GetProperty(propName);
        if (prop == null)
        {
            Debug.Log("Settings property " + propName + " not found!");
        } else
        {
            prop.SetValue(settings, System.Convert.ChangeType(slider.value, prop.PropertyType));
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("Setting changed... " + settings.Predator_PopulationSize);
        }
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
}
