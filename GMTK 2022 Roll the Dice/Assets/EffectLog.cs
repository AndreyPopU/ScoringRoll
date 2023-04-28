using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectLog : MonoBehaviour
{
    public Text effectText;
    public RawImage icon;
    public Texture[] icons;

    public void UpdateEffect(string message, Vector3 pos) // Healht, Energy, Broken Health, Freeze
    {
        if (message.Contains("-")) effectText.color = Color.red;

        int type = 0;

        if (message.Contains("Energy")) type = 1;
        else if (message.Contains("Freeze")) type = 2;
        else type = 3;

        effectText.text = message;
        icon.texture = icons[type];

        StartCoroutine(AdjustPos(pos));

        Destroy(gameObject, 6);
    }

    IEnumerator AdjustPos(Vector3 pos)
    {
        if (transform.localPosition.y < pos.y)
        {
            while (transform.localPosition.y < pos.y)
            {
                transform.localPosition += Vector3.up * 25;
                yield return null;
            }
        }
        else
        {
            while (transform.localPosition.y > pos.y)
            {
                transform.localPosition -= Vector3.up * 25;
                yield return null;
            }
        }

        yield return new WaitForSecondsRealtime(2);

        StartCoroutine(AdjustPos(Vector3.up * -2000));
    }
}
