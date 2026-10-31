using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Text mainMenuBestText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    private GameManager subscribedManager;

    private void OnEnable()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
        if (subscribedManager != null)
        {
            ApplyState(subscribedManager.CurrentState);
        }
    }

    private void OnDisable()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        if (subscribedManager != null)
        {
            subscribedManager.OnStateChanged -= ApplyState;
            subscribedManager = null;
        }
    }

    private void TrySubscribe()
    {
        if (subscribedManager != null || GameManager.Instance == null)
        {
            return;
        }

        subscribedManager = GameManager.Instance;
        subscribedManager.OnStateChanged += ApplyState;
    }

    private void ApplyState(GameState state)
    {
        bool inMenu = state == GameState.Menu;
        if (inMenu && mainMenuBestText != null)
        {
            mainMenuBestText.text = "Best Score: " + HighScoreService.Load();
        }

        SetActive(mainMenuPanel, inMenu);
        if (inMenu)
        {
            Select(startButton);
        }
    }

    private static void Select(Button button)
    {
        if (button != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target != null && target.activeSelf != active)
        {
            target.SetActive(active);
        }
    }

    private void OnStartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
