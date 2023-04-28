using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderManager : MonoBehaviour
{
    public Text sliderText;

    private Dice dice;
    private Slider slider;

    void Start()
    {
        dice = FindObjectOfType<Dice>();
        slider = GetComponent<Slider>();
    }

    public void UpdateSlider(int value, int maxValue)
    {
        if (maxValue != (int)slider.maxValue) // Enlarge
        {
            slider.maxValue = maxValue;
            StartCoroutine(EnlargeSlider());
        }
        StartCoroutine(SmoothSlider(value));
        if (value > 0) sliderText.text = value + "/" + maxValue;
        else sliderText.text = 0 + "/" + maxValue;
    }

    public IEnumerator EnlargeSlider()
    {
        float oldValue = transform.localScale.x + slider.maxValue / 240;
        while (transform.localScale.x < oldValue)
        {
            transform.localScale = Vector3.Lerp(transform.localScale,
                new Vector3(transform.localScale.x + slider.maxValue / 240, transform.localScale.y, transform.localScale.z), .075f);
            if (transform.localScale.x >= 2) break;
            yield return null;
        }
    }

    public IEnumerator SmoothSlider(int desired)
    {
        if (slider.value > desired)
        {
            float difference = slider.value - desired; 
            while (slider.value > desired) 
            {
                slider.value -= difference / 4;
                yield return null;
            }
        }
        else if (slider.value < desired)
        {
            float difference = desired - slider.value;
            while (slider.value < desired)
            {
                slider.value += difference / 4;
                yield return null;
            }
        }

        slider.value = desired;
    }
}
