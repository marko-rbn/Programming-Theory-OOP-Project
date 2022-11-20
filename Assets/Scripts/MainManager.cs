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

    // Update is called once per frame
    void Update()
    {
        //TODO: handle click on Entity to select as target for info display panel
        //TODO: and follow Entity with camera
    }

    private void SpawnAllEntitiesOfType(GameObject entityPrefab)
    {
        int entityCount = (int)DataManager.Instance.settings.GetPropertyForTag(entityPrefab.tag, "PopulationSize");
        for (int i = 0; i < entityCount; i++)
        {
            SpawnOne(entityPrefab);
        }
    }

    public void SpawnOne(GameObject entityPrefab, bool randomLoc = true)
    {
        int x = (int)entityPrefab.gameObject.transform.position.x;
        int z = (int)entityPrefab.gameObject.transform.position.z;
        if (randomLoc)
        {
            //random location for all initial spawns
            int rng = DataManager.Instance.initialSpawnRange;
            x = Random.Range(-rng, rng);
            z = Random.Range(-rng, rng);
        }
        GameObject newEntity = Instantiate(entityPrefab, entityContainer.transform);
        newEntity.transform.position = new Vector3(x, 15, z);
    }

    public void OnEntityDeath(Entity corpse)
    {
        //spawn Fungus
        SpawnOne(fungusPrefab);
    }
}
