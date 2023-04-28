using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum Type { health, energy, maxHealth, maxEnergy, damage, freeze, broken}

    public Type type;
    public bool gotEffect = false;
    public bool popped;
    public bool shakes;
    public bool falls;
    public bool shouldFall;

    private ParticleSystem effect;
    private MeshRenderer[] renderers;
    private float colorSaturation;

    void Start()
    {
        colorSaturation = 1;
        renderers = GetComponentsInChildren<MeshRenderer>();
        effect = GetComponentInChildren<ParticleSystem>();

        if (type == Type.broken) InvokeRepeating("Shake", Random.Range(3, 7), Random.Range(5, 15));
    }

    void Update()
    {
        if (falls && shouldFall)
        {
            StopAllCoroutines();
            transform.position = Vector3.Lerp(transform.position, transform.position - Vector3.up * 1.5f, .075f);
            if (transform.position.y < -20) Destroy(gameObject);
            return;
        }

        if (!popped)
        {
            if (transform.position.y < .5f)
            {
                transform.position = Vector3.Lerp(transform.position, transform.position + Vector3.up, .075f);
                if (transform.position.y >= .5f) popped = true;
            }
        }
        else
        {
            if (transform.position.y > 0)
            {
                transform.position = Vector3.Lerp(transform.position, transform.position - Vector3.up, .02f);
            }
        }

        if (gotEffect)
        {
            if (effect != null && !effect.isPlaying) effect.Play();

            if (colorSaturation > .4f)
            {
                colorSaturation -= .05f;
                foreach (MeshRenderer rend in renderers)
                    rend.material.color = new Color(colorSaturation, colorSaturation, colorSaturation, 1);
            }
        }
        else
        {
            if (colorSaturation < 1)
            {
                colorSaturation += .05f;
                foreach (MeshRenderer rend in renderers)
                    rend.material.color = new Color(colorSaturation, colorSaturation, colorSaturation, 1);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Dice") shouldFall = true;
    }

    public void Bounce() // Invoked
    {
        StartCoroutine(Bounce(1));
    }

    void Shake() // Invoked
    {
        StartCoroutine(Shake(1, .01f));
    }

    private IEnumerator Bounce(float force)
    {
        bool pop = false;
        while(true)
        {
            if (!pop)
            {
                if (transform.position.y < .25f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, transform.position + Vector3.up * .05f, .035f);
                    //transform.position = Vector3.Lerp(transform.position, transform.position + Vector3.up, .0225f);
                    if (transform.position.y >= .25f) pop = true;
                }
            }
            else
            {
                if (transform.position.y > 0)
                {
                    transform.position = Vector3.MoveTowards(transform.position, transform.position - Vector3.up * .05f, .035f);
                }
                else break;
            }

            yield return null;
        }
    }

    public IEnumerator Shake(float duration, float force)
    {
        Vector3 original = transform.position;

        while (duration > 0)
        {
            float x = Random.Range(transform.position.x + force, transform.position.x - force);
            float y = Random.Range(transform.position.y + force, transform.position.y - force);

            transform.position = new Vector3(x, y, transform.position.z);

            duration -= Time.deltaTime;

            yield return null;
        }

        transform.position = original;
    }
}
