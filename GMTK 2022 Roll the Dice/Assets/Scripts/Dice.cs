using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Dice : MonoBehaviour
{
    public float rollForce;
    public int landedSide;
    public int score;
    public int highscore;
    public bool rolled;
    public bool frozen = false;
    public float waitTime = .5f;

    public List<Tile> tiles;

    [Header("Stats")]
    public int health;
    public int energy;
    public int maxHealth = 100;
    public int maxEnergy = 50;
    public int freeze;
    public SliderManager healthSlider, energySlider;
    public Text freezeText;
    public Text scoreText;

    [Header("Swipe")]
    public Vector2 startSwipe;
    public Vector2 endSwipe;
    public Vector3 swipeDir;

    [Header("Side")]
    public Vector3Int directionValues;
    private Vector3Int opposingDirectionValues;
    readonly List<int> faceRepresent = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };

    [Header("Trail")]
    public Transform trail;
    private TrailRenderer trailR;

    [Header("Effect log")]
    public List<string> effects;
    public GameObject logEffect;
    public Transform logParent;

    [Header("Other")]
    public Text landedSideText;
    public GameObject statsPanel;
    public Vector3 lastPosition;
    public Quaternion lastRotation;

    private Rigidbody rb;
    private CameraManager cameraManager;
    private Camera cam;
    private bool resetPos;
    private Tutorial tutorial;
    private int scoreToAdd;
    private AudioSource source;
    private AudioSource managerSource;

    void Start()
    {
        opposingDirectionValues = 7 * Vector3Int.one - directionValues;

        managerSource = FindObjectOfType<GameManager>().GetComponent<AudioSource>();
        source = GetComponent<AudioSource>();
        trailR = trail.GetComponent<TrailRenderer>();
        tutorial = FindObjectOfType<Tutorial>();
        rb = GetComponent<Rigidbody>();
        cameraManager = FindObjectOfType<CameraManager>();
        cam = Camera.main;
        healthSlider.UpdateSlider(health, maxHealth);
        energySlider.UpdateSlider(energy, maxEnergy);
    }

    void Update()
    {
        if (transform.position.y < -20) ResetPosition();

        if (tutorial != null && tutorial.chatBox.activeInHierarchy) return;

        if (!rolled)
        {
            if (energy <= 0) StartCoroutine(EndGame());

            //Swipe
            if (Input.GetMouseButtonDown(0))
            {
                tutorial.tutorialTrail.gameObject.SetActive(false);
                startSwipe = cam.ScreenToViewportPoint(Input.mousePosition);
                trail.transform.position = ScreenToWorld(Input.mousePosition);
                trailR.emitting = true;
                trailR.Clear();
                trail.gameObject.SetActive(true);
            }
            if (Input.GetMouseButton(0))
            {
                if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) trail.transform.position = ScreenToWorld(Input.mousePosition);
            }
            if (Input.GetMouseButtonUp(0))
            {
                endSwipe = cam.ScreenToViewportPoint(Input.mousePosition);
                if (Mathf.Abs(Mathf.Abs(endSwipe.magnitude) - Mathf.Abs(startSwipe.magnitude)) < .05f) return;
                energy -= 3;
                energySlider.UpdateSlider(energy, maxEnergy);
                swipeDir = Swipe();
                managerSource.Play();
                Roll(Swipe(), rollForce);
                trailR.emitting = false;
                trail.gameObject.SetActive(false);
            }
        }
        else
        {
            if (Input.GetButtonDown("Jump") && !frozen && freeze > 0)
                StartCoroutine(Freeze());

            if (frozen) return;

            if (resetPos)
            {
                rolled = false;
                resetPos = false;
                return;
            }

            if (rb.velocity.magnitude < .1f)
            {
                if (waitTime > 0) waitTime -= Time.deltaTime;
                else
                {
                    if (tutorial != null && tutorial.enabled)
                    {
                        StartCoroutine(TutorialRoll());
                    }
                    else
                    {
                        DetectSide();
                        if (landedSideText.gameObject.activeInHierarchy) { StopAllCoroutines(); landedSideText.gameObject.SetActive(false); }
                        StartCoroutine(LandText());
                        ProcessEffects();
                        lastPosition = transform.position;
                        lastRotation = transform.rotation;
                        rolled = false;
                    }
                }
            }
        }
        
    }

    public void Roll(Vector3 swipeDirection, float force)
    {
        waitTime = .5f;
        rb.AddForce(swipeDirection * force, ForceMode.Impulse);
        rb.AddTorque(Vector3.one * (swipeDirection.x + swipeDirection.y) * force * 8);
        rolled = true;
    }

    public Vector3 Swipe()
    {
        return new Vector3(endSwipe.x - startSwipe.x, Mathf.Abs((endSwipe.magnitude - startSwipe.magnitude) / 2), endSwipe.y - startSwipe.y);
    }

    public IEnumerator LandText()
    {
        landedSideText.gameObject.SetActive(true);

        bool scaledUp = false;

        while(!scaledUp)
        {
            if (landedSideText.transform.localScale.x >= 1.5f) break;

            landedSideText.transform.localScale += Vector3.one * .075f;
            landedSideText.text = Random.Range(1, 7).ToString();
            yield return null;
        }

        landedSideText.text = landedSide.ToString();
        yield return new WaitForSeconds(1);

        while (scaledUp)
        {
            if (landedSideText.transform.localScale.x <= .1f) break;

            landedSideText.transform.localScale -= Vector3.one * .075f;
            yield return null;
        }

        landedSideText.transform.localScale = Vector3.zero;
        landedSideText.gameObject.SetActive(false);
    }


    public void DetectSide()
    {
        if (Vector3.Cross(Vector3.up, transform.right).magnitude < 0.5f) //x axis a.b.sin theta <45; if ((int) Vector3.Cross(Vector3.up, transform.right).magnitude == 0) //Previously
        {
            if (Vector3.Dot(Vector3.up, transform.right) > 0) landedSide = faceRepresent[directionValues.x];
            else landedSide = faceRepresent[opposingDirectionValues.x];
        }
        else if (Vector3.Cross(Vector3.up, transform.up).magnitude < 0.5f) //y axis
        {
            if (Vector3.Dot(Vector3.up, transform.up) > 0) landedSide = faceRepresent[directionValues.y];
            else landedSide = faceRepresent[opposingDirectionValues.y];
        }
        else if (Vector3.Cross(Vector3.up, transform.forward).magnitude < 0.5f) //z axis
        {
            if (Vector3.Dot(Vector3.up, transform.forward) > 0) landedSide = faceRepresent[directionValues.z];
            else landedSide = faceRepresent[opposingDirectionValues.z];
        }
    }

    public void ResetPosition()
    {
        foreach (Tile tile in tiles)
            tile.gotEffect = false;

        tiles.Clear();

        health -= 5;
        healthSlider.UpdateSlider(health, maxHealth);
        if (health <= 0) StartCoroutine(EndGame());

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = lastRotation;
        transform.position = lastPosition + Vector3.up * .2f;
        cameraManager.transform.position = transform.position + cameraManager.offset * 1.5f;
        resetPos = true;
    }

    public Vector3 ScreenToWorld(Vector3 position)
    {
        position.z = cam.nearClipPlane + 1;
        return cam.ScreenToWorldPoint(position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!rolled) return;

        if (collision.collider.TryGetComponent(out Tile tile))
        {
            if (!tile.gotEffect && !tiles.Contains(tile))
            {
                tile.gotEffect = true;
                if (tile.type == Tile.Type.broken) return;
                source.Play();
                tiles.Add(tile);
            }
        }
    }

    public void ProcessEffects()
    {
        foreach (Tile tile in tiles)
            TileEffect(tile);

        for (int i = 0; i < effects.Count; i++)
        {
            GameObject log = Instantiate(logEffect, logParent);
            log.transform.localPosition = Vector3.zero;
            log.GetComponent<EffectLog>().UpdateEffect(effects[i], Vector3.up * 80 * (i + 1));

        }

        effects.Clear();

        StartCoroutine(UpdateScore());

        healthSlider.UpdateSlider(health, maxHealth);
        if (health <= 0) StartCoroutine(EndGame());

        energySlider.UpdateSlider(energy, maxEnergy);

        tiles.Clear();
    }

    public void TileEffect(Tile tile)
    {
        scoreToAdd += landedSide;

        if (tile.type == Tile.Type.health) // Heal
        {
            if (health + landedSide > maxHealth) health = maxHealth;
            else health += landedSide;

            effects.Add("+" + landedSide + " Health");
        }
        else if (tile.type == Tile.Type.energy) // Energy
        {
            if (energy + landedSide > maxEnergy) energy = maxEnergy;
            else energy += landedSide;

            effects.Add("+" + landedSide + " Energy");
        }
        else if (tile.type == Tile.Type.maxHealth)
        {
            maxHealth += landedSide;
            health += landedSide;

            effects.Add("+" + landedSide + " Max health");
        }
        else if (tile.type == Tile.Type.maxEnergy)
        {
            maxEnergy += landedSide;
            energy += landedSide;

            effects.Add("+" + landedSide + " Max energy");
        }
        else if (tile.type == Tile.Type.damage)
        {
            health -= landedSide;

            effects.Add("-" + landedSide + " Health");
        }
        else if (tile.type == Tile.Type.freeze)
        {
            freeze += landedSide;
            freezeText.text = "x" + freeze;

            effects.Add("+" + landedSide + "x Freeze");
        }
    }

    public IEnumerator Freeze()
    {
        freeze--;
        freezeText.text = "x" + freeze;
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).GetComponent<AudioSource>().Play();
        frozen = true;
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        yield return new WaitForSecondsRealtime(1);

        transform.GetChild(0).gameObject.SetActive(false);
        rb.isKinematic = false;
        frozen = false;
    }

    IEnumerator TutorialRoll()
    {
        tutorial.DisplayText();
        while(tutorial.chatBox.activeInHierarchy)
            yield return null;

        DetectSide();
        ProcessEffects();
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        rolled = false;
        PlayerPrefs.SetInt("Tutorial", 0);
        tutorial.enabled = false;
    }

    IEnumerator EndGame()
    {
        print("Ended game");
        highscore = PlayerPrefs.GetInt("Highscore");
        if (score > highscore)
        {
            highscore = score;
            PlayerPrefs.SetInt("Highscore", highscore);
        }

        statsPanel.transform.GetChild(1).GetComponent<Text>().text = "Score: " + score + "\nHighscore: " + highscore;
        statsPanel.SetActive(true);
        statsPanel.transform.localPosition = Vector3.up * -1000;

        while (statsPanel.transform.localPosition.y < 0)
        {
            statsPanel.transform.localPosition += Vector3.up * 15;
            yield return null;
        }

        statsPanel.transform.localPosition = Vector3.zero;
    }

    IEnumerator UpdateScore()
    {
        int desiredScore = score + scoreToAdd;
        while(score < desiredScore)
        {
            score++;
            scoreText.text = score.ToString();
            yield return null;
        }

        score = desiredScore;
        scoreText.text = score.ToString();
        scoreToAdd = 0;
    }
}