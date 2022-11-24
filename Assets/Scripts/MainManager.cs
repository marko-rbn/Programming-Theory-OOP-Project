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
    public TextMeshProUGUI topText;

    public GameObject marker1Object;
    private MarkerFollowSelected marker1;
    public GameObject marker2Object;
    protected MarkerFollowSelected marker2;

    private GameObject entityContainer;  //parent object for containing all spawns
    public Dictionary<string, int> entityCounts = new();  //count updated only in SpawnOne() and AfterEntityDecay()

    //TODO: add Restart and BackToOptions buttons
    private void Awake()
    {
        entityContainer = GameObject.Find("Entity Container");
        marker1 = marker1Object.GetComponent<MarkerFollowSelected>();
        marker2 = marker2Object.GetComponent<MarkerFollowSelected>();
        infoPanel.SetActive(false);
    }

    //spawn initial populations
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

    //text updates
    void Update()
    {
        List<string> ss = new();
        foreach (var kv in entityCounts)
        {
            ss.Add(kv.Key + ": " + kv.Value);
        }
        topText.SetText(string.Join("\t\t", ss));

        if (selectedEntity != null)
        {
            UpdateInfoDisplay();
        }
    }

    private void UpdateInfoDisplay()
    {
        infoPanelText[0].SetText(selectedEntity.tag + (selectedEntity.isDead ? " corpse" : ""));  //entity
        infoPanelText[1].SetText(selectedEntity.storedEnergy.ToString("0.#"));  //energy
        infoPanelText[2].SetText(selectedEntity.timeToLiveRemaining.ToString());  //time remaining
        infoPanelText[3].SetText(selectedEntity.actionMode + " (" + Mathf.FloorToInt(Time.realtimeSinceStartup - selectedEntity.actionModeStarted) + ")");  //action mode
        infoPanelText[4].SetText("reserved");
    }

    public void SelectEntity(Entity target)
    {
        if (selectedEntity != null)
        {
            selectedEntity.Deselect();
        }
        selectedEntity = target;

        marker1.target = (target == null) ? null : target.gameObject;
        marker1Object.SetActive(target != null);
        if (target != null)
        {
            //scale marker1 to target
            marker1Object.transform.localScale = target.transform.localScale * 0.15f;
            //reset marker2
            MarkTargetsTarget(target.actionTarget);
        }

        infoPanel.SetActive((target != null));
    }

    public void MarkTargetsTarget(GameObject target)
    {
        marker2.target = target;
        marker2Object.SetActive(target != null);
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

    public void AfterEntityDeath(Entity corpse)
    {
        //spawn a Fungus
        SpawnOne(fungusPrefab);
    }

    public void AfterEntityDecay(string entityTag)
    {
        entityCounts[entityTag]--;
    }
}
