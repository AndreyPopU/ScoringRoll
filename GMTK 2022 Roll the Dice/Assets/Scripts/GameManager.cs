using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public List<GameObject> tilePrefabs;
    public int lastSpawnZ = 8;
    public int tutorial = 1;
    private string tutorialPref = "Tutorial";
    public GameObject[] badTiles;

    public List<Tile> latestTiles;
    private Dice dice;
    private int spawnedRows;

    void Start()
    {
        dice = FindObjectOfType<Dice>();
        InvokeRepeating("Bounce", 5, 5);

        if (PlayerPrefs.HasKey(tutorialPref))
            tutorial = PlayerPrefs.GetInt(tutorialPref);

        if (tutorial == 0) GetComponent<Tutorial>().enabled = false;
    }

    void Update()
    {
        if (dice.transform.position.z - lastSpawnZ + 15 > 0)
        {
            StartCoroutine(SpawnTilesSet(-10, 10, lastSpawnZ + 3, lastSpawnZ + 6));
        }
    }

    public IEnumerator SpawnTilesSet(int width1, int width2, int height1, int height2)
    {
        lastSpawnZ += 3;

        if (spawnedRows % 7 == 0) tilePrefabs.Add(badTiles[Random.Range(0, badTiles.Length)]);

        spawnedRows++;

        for (int i = width1; i < width2; i+=3)
            for (int j = height1; j < height2; j+=3)
            {
                GameObject tile = Instantiate(tilePrefabs[Random.Range(0, tilePrefabs.Count)], new Vector3(i, -5, j), Quaternion.identity);
                if (latestTiles.Count > 35) latestTiles.RemoveRange(28, 7);
                latestTiles.Add(tile.GetComponent<Tile>());
                yield return new WaitForSecondsRealtime(.1f);
            }
    }

    void Bounce()
    {
        StartCoroutine(BounceTile());
        
    }

    IEnumerator BounceTile()
    {
        for (int i = 0; i < Random.Range(3, 6); i++)
        {
            Tile tile = latestTiles[Random.Range(0, latestTiles.Count)];
            if (tile != null) tile.Bounce();
            yield return new WaitForSecondsRealtime(Random.Range(.25f, .75f));
        }
    }

    public void Play(string sceneName)
    {
        StartCoroutine(LoadLevel(sceneName));
    }

    IEnumerator LoadLevel(string sceneName)
    {
        FadePanel.instance.StartCoroutine(FadePanel.instance.FadeIn());

        while (true)
        {
            if (FadePanel.instance.panel.alpha == 1)
            {
                SceneManager.LoadScene(sceneName);
                yield break;
            }

            yield return null;
        }
    }
}
