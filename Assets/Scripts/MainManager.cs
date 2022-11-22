using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    public GameObject predatorPrefab;
    public GameObject preyPrefab;
    public GameObject plantPrefab;
    public GameObject fungusPrefab;

    public int entityCountLimit = 500;

    public Entity selectedEntity { get; private set; }
    public GameObject infoPanel;
    public List<TextMeshProUGUI> infoPanelText;

    public GameObject markerObject;
    private MarkerFollowSelected marker;

    private GameObject entityContainer;  //parent object for containing all spawns
    public Dictionary<string, int> entityCounts = new();

    private void Awake()
    {
        entityContainer = GameObject.Find("Entity Container");
        marker = markerObject.GetComponent<MarkerFollowSelected>();
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
        entityCounts.Add(fungusPrefab.tag, 0);
        SpawnAllEntitiesOfType(fungusPrefab);

        entityCounts.Add(plantPrefab.tag, 0);
        SpawnAllEntitiesOfType(plantPrefab);

        entityCounts.Add(preyPrefab.tag, 0);
        SpawnAllEntitiesOfType(preyPrefab);

        entityCounts.Add(predatorPrefab.tag, 0);
        SpawnAllEntitiesOfType(predatorPrefab);
    }

    void Update()
    {
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
        marker.target = (target == null) ? null : target.gameObject;
        markerObject.SetActive(target != null);
        selectedEntity = target;
        infoPanel.SetActive((target != null));
    }

    private void UpdateInfoDisplay()
    {
        infoPanelText[0].SetText(selectedEntity.tag + (selectedEntity.isDead ? " corpse" : ""));  //entity
        infoPanelText[1].SetText(selectedEntity.storedEnergy.ToString("0.#"));  //energy
        infoPanelText[2].SetText(selectedEntity.timeToLiveRemaining.ToString());  //time remaining
        infoPanelText[3].SetText(selectedEntity.actionMode);  //action mode
        //TODO: add total entity type count to panel
    }

    private void SpawnAllEntitiesOfType(GameObject entityPrefab)
    {
        int entityCount = (int)DataManager.Instance.settings.GetPropertyForTag(entityPrefab.tag, "PopulationSize");
        for (int i = 0; i < entityCount; i++)
        {
            SpawnOne(entityPrefab);
        }
    }

    public bool SpawnOne(GameObject entityPrefab, bool randomLoc = true)
    {
        //refuse if too many
        if (entityCounts[entityPrefab.tag] >= entityCountLimit) {
            return false;
        }

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

        entityCounts[newEntity.tag]++;
        return true;
    }

    public void OnEntityDeath(Entity corpse)
    {
        //spawn Fungus
        SpawnOne(fungusPrefab);
    }
}
