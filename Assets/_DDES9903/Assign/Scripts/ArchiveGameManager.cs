using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArchiveGameManager : MonoBehaviour
{
    public static ArchiveGameManager Instance;

    [Header("Lights (drag ALL lights for climax)")]
    public Light[] lights;
    public Light[] extraLights;
    public float dimSpeed = 0.5f;

    [Header("World Objects")]
    public GameObject lilaSilhouette;
    public GameObject exitTrigger;
    public AudioSource musicAudio;

    [Header("Lila Choice Buttons")]
    public GameObject lilaHelpButton;
    public GameObject lilaIgnoreButton;

    // Ghost choice tracking
    private bool lilaHelped = false;
    private bool marcusHelped = false;
    private bool eleanorHelped = false;

    private int cluesCollected = 0;
    private int totalClues = 4;
    private bool lockerUnlocked = false;
    private bool gameEnded = false;
    private bool showTheEnd = false;
    private HashSet<string> collected = new HashSet<string>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        RenderSettings.ambientIntensity = 0.3f;
        if (exitTrigger != null) exitTrigger.SetActive(false);
    }

    // ===== LILA CLUES =====
    public void CollectClue(string clueText)
    {
        if (gameEnded || cluesCollected >= totalClues) return;
        if (collected.Contains(clueText)) return;
        collected.Add(clueText);
        cluesCollected++;

        Debug.Log("[ArchiveGameManager] Collected clue " + cluesCollected + "/" + totalClues + ": " + clueText);

        RenderSettings.ambientIntensity = Mathf.Lerp(0.3f, 0.05f, (float)cluesCollected / totalClues);

        if (cluesCollected >= totalClues)
        {
            lockerUnlocked = true;
            Debug.Log("[ArchiveGameManager] All clues collected! Locker unlocked.");

            if (extraLights != null)
            {
                foreach (Light l in extraLights)
                {
                    if (l != null) StartCoroutine(DimLight(l));
                }
            }
        }
    }

    // Call this from each clue's OnPrimaryInteract to dim its own nearby light
    public void DimLightNow(Light light)
    {
        if (light != null) StartCoroutine(DimLight(light));
    }

    // ===== GHOST CHOICES (new) =====
    // Call these from the "Help" / "Leave" buttons for each ghost
    public void HelpGhost(string ghostName)
    {
        if (ghostName == "Lila") lilaHelped = true;
        if (ghostName == "Marcus") marcusHelped = true;
        if (ghostName == "Eleanor") eleanorHelped = true;
        Debug.Log("[ArchiveGameManager] Helped " + ghostName);
    }

    public void IgnoreGhost(string ghostName)
    {
        Debug.Log("[ArchiveGameManager] Ignored " + ghostName);
    }

    // Called after the player chooses to help or ignore Lila
    public void OnLilaChoiceMade()
    {
        if (exitTrigger != null) exitTrigger.SetActive(true);
        Debug.Log("[ArchiveGameManager] Lila choice made, exit activated.");
    }

    // ===== CLIMAX =====
    public void TriggerClimax()
    {
        if (gameEnded) return;
        gameEnded = true;
        StartCoroutine(ClimaxSequence());
    }

    IEnumerator ClimaxSequence()
    {
        Debug.Log("[ArchiveGameManager] Climax triggered!");

        // Flash all lights then turn off
        if (lights != null)
        {
            foreach (Light l in lights)
            {
                if (l != null) l.intensity = 4f;
            }
        }
        yield return new WaitForSeconds(0.3f);
        if (lights != null)
        {
            foreach (Light l in lights)
            {
                if (l != null) l.intensity = 0f;
            }
        }
        RenderSettings.ambientIntensity = 0.02f;

        if (musicAudio != null) musicAudio.Play();

        // Show Lila after 3 seconds
        yield return new WaitForSeconds(3f);
        if (lilaSilhouette != null) lilaSilhouette.SetActive(true);

        // Show Lila choice buttons 1 second after she appears
        yield return new WaitForSeconds(1f);
        if (lilaHelpButton != null) lilaHelpButton.SetActive(true);
        if (lilaIgnoreButton != null) lilaIgnoreButton.SetActive(true);

        // Exit trigger is activated by the player's Lila choice button (SetActive via UnityEvent)
    }

    // ===== ENDING =====
    public void TriggerEnding()
    {
        StartCoroutine(EndingSequence());
    }

    IEnumerator EndingSequence()
    {
        int helpedCount = (lilaHelped ? 1 : 0) + (marcusHelped ? 1 : 0) + (eleanorHelped ? 1 : 0);
        Debug.Log("[ArchiveGameManager] Ending. Ghosts helped: " + helpedCount + "/3");

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var controller = player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (helpedCount >= 3)
        {
            // GOOD ENDING: lights fade up softly, peaceful, then black
            Debug.Log("[ArchiveGameManager] GOOD ENDING - all ghosts at peace");
            if (lights != null)
            {
                foreach (Light l in lights)
                {
                    if (l != null) l.intensity = 0.4f;
                }
            }
            RenderSettings.ambientIntensity = 0.2f;
            yield return new WaitForSeconds(4f);
            // Fade to black
            float t = 0;
            while (t < 2f)
            {
                t += Time.deltaTime;
                float v = Mathf.Lerp(0.4f, 0f, t / 2f);
                if (lights != null)
                {
                    foreach (Light l in lights)
                    {
                        if (l != null) l.intensity = v;
                    }
                }
                RenderSettings.ambientIntensity = Mathf.Lerp(0.2f, 0f, t / 2f);
                yield return null;
            }
        }
        else if (helpedCount >= 1)
        {
            // MID ENDING: stay dark for a moment, then black
            Debug.Log("[ArchiveGameManager] MID ENDING - some ghosts remain");
            yield return new WaitForSeconds(3f);
        }
        else
        {
            // BAD ENDING: one violent flash, then darkness
            Debug.Log("[ArchiveGameManager] BAD ENDING - the archives keep their secrets");
            if (lights != null)
            {
                foreach (Light l in lights)
                {
                    if (l != null) l.intensity = 6f;
                }
            }
            yield return new WaitForSeconds(0.15f);
            if (lights != null)
            {
                foreach (Light l in lights)
                {
                    if (l != null) l.intensity = 0f;
                }
            }
            yield return new WaitForSeconds(2f);
        }

        // Final black screen
        RenderSettings.ambientIntensity = 0f;
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = Color.black;
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
        }

        showTheEnd = true;
        yield return new WaitForSeconds(3f);
        showTheEnd = false;
        Application.Quit();
    }

    void OnGUI()
    {
        if (showTheEnd)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 48;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "The End", style);
        }
    }

    IEnumerator DimLight(Light light)
    {
        if (light == null) yield break;
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
