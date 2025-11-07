// File: GameSetup.cs
// Attach to an empty GameObject named "GameManager"

using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

public class GameSetup : MonoBehaviour
{
    public Camera playerCamera;
    public Transform targetParent;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI reactionText;
    public TMP_Dropdown agentDropdown;
    public GameObject introCanvas;

    public GameObject realisticAgentPrefab;
    public GameObject robotAgentPrefab;
    public GameObject cartoonAgentPrefab;

    public GameObject leftButton;
    public GameObject rightButton;

    private GameObject currentTarget;
    private float targetStartTime;
    private List<Vector3> targetPositions = new List<Vector3>()
    {
        new Vector3(-2, 1.5f, 5),
        new Vector3(0, 1.5f, 5),
        new Vector3(2, 1.5f, 5)
    };

    private int trialCount = 0;
    private int maxTrials = 20;
    private bool playerTurn = false;
    private bool introPhase = true;

    private Vector3 lastPartnerPosition;
    private bool useSameLocationNext = true;

    private List<string> logLines = new List<string>();
    private string logPath;

    private string selectedAgentType = "Realistic";

    void Start()
    {
        logPath = Path.Combine(Application.persistentDataPath, "sior_log.csv");
        logLines.Add("Trial,PartnerPos,PlayerPos,SameLocation,RTms");

        if (agentDropdown != null)
            selectedAgentType = agentDropdown.options[agentDropdown.value].text;

        introCanvas.SetActive(true);
        statusText.gameObject.SetActive(false);
        reactionText.gameObject.SetActive(false);
    }

    public void StartExperimentAfterIntro()
    {
        BeginExperiment();
    }

    public void BeginExperiment()
    {
        introCanvas.SetActive(false);
        statusText.gameObject.SetActive(true);
        reactionText.gameObject.SetActive(true);
        introPhase = false;
        Invoke("StartFakePartner", 1f);
    }

    void StartFakePartner()
    {
        if (agentDropdown != null)
            selectedAgentType = agentDropdown.options[agentDropdown.value].text;
        statusText.text = "Partner is choosing...";

        lastPartnerPosition = targetPositions[Random.Range(0, 3)];

        GameObject agentPrefab = realisticAgentPrefab;

        switch (selectedAgentType)
        {
            case "Robot": agentPrefab = robotAgentPrefab; break;
            case "Cartoon": agentPrefab = cartoonAgentPrefab; break;
        }

        GameObject fakeTarget = Instantiate(agentPrefab, lastPartnerPosition, Quaternion.identity, targetParent);
        Destroy(fakeTarget, 1.5f);

        useSameLocationNext = Random.value < 0.5f;

        Invoke("StartPlayerTurn", 2f);
    }

    void StartPlayerTurn()
    {
        playerTurn = true;
        statusText.text = "Your turn! Press W or P.";

        currentTarget = useSameLocationNext ? leftButton : rightButton;
        SetButtonColor(currentTarget, Color.green);
        targetStartTime = Time.time;
    }

    void Update()
    {
        if (introPhase && Input.GetKeyDown(KeyCode.Space))
        {
            BeginExperiment();
            return;
        }

        if (!playerTurn || currentTarget == null) return;

        if ((Input.GetKeyDown(KeyCode.W) && currentTarget == leftButton) ||
            (Input.GetKeyDown(KeyCode.P) && currentTarget == rightButton))
        {
            float rt = (Time.time - targetStartTime) * 1000f;
            reactionText.text = "RT: " + Mathf.RoundToInt(rt) + " ms";
            SetButtonColor(currentTarget, Color.red);
            playerTurn = false;

            bool sameLocation = (currentTarget == leftButton && lastPartnerPosition == new Vector3(-2, 1.5f, 5)) ||
                                (currentTarget == rightButton && lastPartnerPosition == new Vector3(2, 1.5f, 5));

            string line = string.Join(",",
                trialCount + 1,
                FormatVec(lastPartnerPosition),
                FormatVec(currentTarget.transform.position),
                sameLocation,
                Mathf.RoundToInt(rt)
            );
            logLines.Add(line);

            trialCount++;
            if (trialCount < maxTrials)
                Invoke("StartFakePartner", 2f);
            else
                FinishExperiment();
        }
    }

    void SetButtonColor(GameObject button, Color color)
    {
        var renderers = button.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.materials)
            {
                if (mat.name.StartsWith("button")) // nur die Spitze!
                {
                    mat.SetColor("_BaseColor", color);
                    mat.SetColor("_EmissionColor", color * 2f);
                    mat.EnableKeyword("_EMISSION");
                }
            }
        }
    }

    void FinishExperiment()
    {
        statusText.text = "Finished! Log written.";
        File.WriteAllLines(logPath, logLines);
#if UNITY_EDITOR
        UnityEngine.Debug.Log("Log saved to: " + logPath);
#endif
    }

    public void OpenLogFolder()
    {
        string folderPath = Path.GetDirectoryName(logPath);
        if (Directory.Exists(folderPath))
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = folderPath,
                UseShellExecute = true
            });
        }
    }

    string FormatVec(Vector3 v)
    {
        return "(" + v.x + "," + v.y + "," + v.z + ")";
    }
}
