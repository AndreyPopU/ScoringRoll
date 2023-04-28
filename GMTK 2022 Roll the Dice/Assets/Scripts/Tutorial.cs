using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    public GameObject chatBox;
    public Text chatText;
    public int currentPhrase;
    public string[] phrases;

    public TrailRenderer tutorialTrail;
    private bool writingText;

    void Start()
    {
        DisplayText();
    }

    void Update()
    {
        if (chatBox.activeInHierarchy)
        {
            if(Input.GetButtonUp("Jump") || Input.GetMouseButtonUp(0))
            {
                if (writingText) { chatText.text = phrases[currentPhrase]; StopAllCoroutines(); writingText = false; }
                else DisplayText();
            }
        }
    }

    public void DisplayText()
    {
        currentPhrase++;
        if (currentPhrase == 2) tutorialTrail.GetComponent<Animator>().SetBool("swipe", true);

        if (phrases[currentPhrase].Contains("nanana")) { chatBox.SetActive(false); return; }
        StartCoroutine(ChatboxTextType(phrases[currentPhrase]));
        chatBox.SetActive(true);
    }

    public IEnumerator ChatboxTextType(string phrase)
    {
        chatText.text = "";
        char[] phraseChars = phrase.ToCharArray();
        writingText = true;

        for (int i = 0; i < phraseChars.Length; i++)
        {
            if (!writingText)
            {
                chatText.text = phrase;
                break;
            }
            chatText.text += phraseChars[i];
            yield return new WaitForSeconds(.035f);
        }

        writingText = false;
    }
}
