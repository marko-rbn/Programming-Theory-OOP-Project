using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    public GameObject predatorPrefab;
    public GameObject preyPrefab;
    public GameObject plantPrefab;
    public GameObject fungusPrefab;

    public GameObject infoPanel;
    public List<TextMeshProUGUI> infoPanelText;

    private GameObject entityContainer;
    public Entity selectedEntity { get; private set; }

    private void Awake()
    {
        entityContainer = GameObject.Find("Entity Container");
        infoPanel.SetActive(false);
    }

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

    void Update()
    {
        //TODO: follow selected Entity with camera?
        if (selectedEntity != null)
        {
            UpdateInfoDisplay();
        }
    }

    public void SelectEntity(Entity target)
    {
        if (selectedEntity != null)
        {
            selectedEntity.Deselect();
        }
        selectedEntity = target;
        infoPanel.SetActive((target != null));
    }

    private void UpdateInfoDisplay()
    {
        infoPanelText[0].SetText(selectedEntity.tag + (selectedEntity.isDead ? " corpse" : ""));  //entity
        infoPanelText[1].SetText(selectedEntity.storedEnergy.ToString("0.#"));  //energy
        infoPanelText[2].SetText(selectedEntity.timeToLiveRemaining.ToString());  //time remaining
        infoPanelText[3].SetText(selectedEntity.actionMode);  //action mode
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
