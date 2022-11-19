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

    private void Awake()
    {
        entityContainer = GameObject.Find("Entity Container");     
    }

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
        SpawnAllEntitiesOfType(fungusPrefab);
        SpawnAllEntitiesOfType(plantPrefab);
        SpawnAllEntitiesOfType(preyPrefab);
        SpawnAllEntitiesOfType(predatorPrefab);
    }

    private void SpawnAllEntitiesOfType(GameObject entityPrefab)
    {
        int entityCount = (int)DataManager.Instance.settings.GetPropertyForTag(entityPrefab.tag, "PopulationSize");
        for (int i = 0; i < entityCount; i++)
        {
            //random location
            int x = Random.Range(-490, 490);
            int z = Random.Range(-490, 490);
            var newEntity = Instantiate(entityPrefab, entityContainer.transform);
            newEntity.transform.Translate(new Vector3(x, 10, z));
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
