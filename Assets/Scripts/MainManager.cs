using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    public GameObject predatorPrefab;
    public GameObject preyPrefab;
    public GameObject plantPrefab;
    public GameObject fungusPrefab;

    private GameObject entityContainer;

    // Start is called before the first frame update
    void Start()
    {
        if (DataManager.Instance == null)
        {
            //means we're starting in the wrong scene
            SceneManager.LoadScene(0);
            return;
        }

        //spawn everything according to population size settings
        entityContainer = GameObject.Find("Entity Container");
        int predatorCount = 10; // DataManager.Instance.settings.Predator_PopulationSize;
        for (int i = 0; i < predatorCount; i++)
        {
            //random location
            int x = Random.Range(-490, 490);
            int y = Random.Range(-490, 490);
            Instantiate(predatorPrefab, entityContainer.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDeath(Entity corpse)
    {
        //spawn Fungus and remove corpse
        Debug.Log("TODO: clean up the dead.");
    }
}
