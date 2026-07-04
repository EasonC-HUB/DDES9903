using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ArchiveGameManager : MonoBehaviour
{
    public static ArchiveGameManager Instance;

    public Light[] lights;
    public float dimSpeed = 1f;

    public GameObject storyPopup;
    public TMP_Text storyText;

    public GameObject lilaSilhouette;
    public GameObject exitTrigger;
    public AudioSource musicAudio;

    private int cluesCollected = 0;
    private int totalClues = 4;
    private bool lockerUnlocked = false;
    private bool gameEnded = false;
    private HashSet<string> collected = new HashSet<string>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(ShowPopup("Night shift. Sort the archives. Don't lose anything.", 5f));
        RenderSettings.ambientIntensity = 0.3f;
    }

    public void CollectClue(string clueText)
    {
        if (gameEnded || cluesCollected >= totalClues) return;
        if (collected.Contains(clueText)) return;
        collected.Add(clueText);
        cluesCollected++;

        StartCoroutine(ShowPopup(clueText, 3f));

        if (cluesCollected <= lights.Length)
        {
            StartCoroutine(DimLight(lights[cluesCollected - 1]));
        }

        RenderSettings.ambientIntensity = Mathf.Lerp(0.3f, 0.05f, (float)cluesCollected / totalClues);

        if (cluesCollected >= totalClues)
        {
            lockerUnlocked = true;
            StartCoroutine(ShowPopup("...Click. The locker at the end has unlocked.", 3f));
        }
    }

    public void TriggerClimax()
    {
        if (gameEnded) return;
        gameEnded = true;

        StartCoroutine(ClimaxSequence());
    }

    IEnumerator ClimaxSequence()
    {
        foreach (Light l in lights) l.intensity = 4f;
        yield return new WaitForSeconds(0.3f);
        foreach (Light l in lights) l.intensity = 0f;
        RenderSettings.ambientIntensity = 0.02f;

        string diaryText = "17 Oct 1993\nI'm leaving. Don't look for me.\nLet the things hidden in these archives stay here forever.\n！！ Lila";
        StartCoroutine(ShowPopup(diaryText, 5f));

        if (musicAudio != null) musicAudio.Play();

        yield return new WaitForSeconds(3f);

        lilaSilhouette.SetActive(true);
        yield return new WaitForSeconds(2f);

        StartCoroutine(ShowPopup("...Time to leave.", 2f));
        yield return new WaitForSeconds(1f);
        exitTrigger.SetActive(true);
        Invoke("TriggerEnding", 5f);
    }

    public void TriggerEnding()
    {
        StartCoroutine(EndingSequence());
    }

    IEnumerator EndingSequence()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var controller = player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        foreach (Light l in lights) l.intensity = 0f;
        RenderSettings.ambientIntensity = 0f;
        Camera.main.backgroundColor = Color.black;
        Camera.main.clearFlags = CameraClearFlags.SolidColor;

        storyText.text = "！！ The End ！！";
        storyPopup.SetActive(true);
        yield return new WaitForSeconds(5f);
        Application.Quit();
    }

    IEnumerator ShowPopup(string text, float duration)
    {
        storyText.text = text;
        storyPopup.SetActive(true);
        yield return new WaitForSeconds(duration);
        storyPopup.SetActive(false);
    }

    IEnumerator DimLight(Light light)
    {
        while (light.intensity > 0)
        {
            light.intensity -= dimSpeed * Time.deltaTime;
            yield return null;
        }
        light.intensity = 0;
    }

    public bool IsLockerUnlocked()
    {
        return lockerUnlocked;
    }
}