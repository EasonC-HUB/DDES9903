using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ArchiveGameManager : MonoBehaviour
{
    public static ArchiveGameManager Instance;

    [Header("Lights")]
    public Light[] lights;
    public float dimSpeed = 1f;

    [Header("EZPZ Buttons (drag in, text typed directly on each button's TMP)")]
    public GameObject introButton;
    public GameObject[] clueButtons;
    public GameObject lockerUnlockedButton;
    public GameObject diaryLetterButton;
    public GameObject timeToLeaveButton;
    public GameObject endingButton;

    [Header("World Objects")]
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
        if (introButton != null) introButton.SetActive(true);
        RenderSettings.ambientIntensity = 0.3f;
    }

    public void CollectClue(string clueText)
    {
        if (gameEnded || cluesCollected >= totalClues) return;
        if (collected.Contains(clueText)) return;
        collected.Add(clueText);
        cluesCollected++;

        if (cluesCollected - 1 < clueButtons.Length && clueButtons[cluesCollected - 1] != null)
        {
            clueButtons[cluesCollected - 1].SetActive(true);
        }

        if (cluesCollected <= lights.Length)
        {
            StartCoroutine(DimLight(lights[cluesCollected - 1]));
        }

        RenderSettings.ambientIntensity = Mathf.Lerp(0.3f, 0.05f, (float)cluesCollected / totalClues);

        if (cluesCollected >= totalClues)
        {
            lockerUnlocked = true;
            if (lockerUnlockedButton != null) lockerUnlockedButton.SetActive(true);
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

        if (diaryLetterButton != null) diaryLetterButton.SetActive(true);

        if (musicAudio != null) musicAudio.Play();

        yield return new WaitForSeconds(3f);
        lilaSilhouette.SetActive(true);
        yield return new WaitForSeconds(2f);

        if (timeToLeaveButton != null) timeToLeaveButton.SetActive(true);
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

        if (endingButton != null) endingButton.SetActive(true);
        yield return new WaitForSeconds(5f);
        Application.Quit();
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