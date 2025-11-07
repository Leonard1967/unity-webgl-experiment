using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    public GameObject introCanvas;
    public GameObject gameUI;
    public GameSetup gameSetup;
    public MouseLook mouseLook; // direkte Referenz auf MouseLook

    private bool introShown = true;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        introCanvas.SetActive(true);
        gameUI.SetActive(false);

        // Sicherheitshalber deaktivieren
        if (mouseLook != null)
            mouseLook.controlEnabled = false;
    }

    void Update()
    {
        if (introShown && Input.GetKeyDown(KeyCode.Space))
        {
            StartExperiment();
        }
    }

    void StartExperiment()
    {
        introCanvas.SetActive(false);
        gameUI.SetActive(true);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        introShown = false;

        if (mouseLook != null)
            mouseLook.controlEnabled = true;

        gameSetup.BeginExperiment();
    }
}
