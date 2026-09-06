using System.Collections;
using TMPro; // TextMeshPro para textos nítidos
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;

    [Header("Navegación")]
    [Tooltip("Escena del menú principal. Debe estar incluida en Build Settings.")]
    [SerializeField] private string mainMenuSceneName = "IntroScene";

    [Header("Referencias de UI (TextMeshPro)")]
    [Tooltip("Texto grande centrado para la cuenta atrás (3, 2, 1, YA!)")]
    public TextMeshProUGUI countdownText;

    [Tooltip("Panel o texto que muestra el ganador al terminar")]
    public TextMeshProUGUI winnerText;

    [Tooltip("Panel de fondo de victoria (opcional)")]
    public GameObject winnerPanel;
    public GameObject winnerPanel2;
    public GameObject button;

    [Header("Configuración de la Carrera")]
    [Tooltip("Número de vueltas necesarias para ganar")]
    public int totalLaps = 1;

    [Tooltip("Tiempo de cuenta atrás en segundos")]
    public int countdownSeconds = 3;

    // Estado global accesible por los coches
    public static bool isRaceStarted = false;
    public static bool isRaceOver = false;
    private bool isReturningToMenu;

    void Awake()
    {
        Instance = this;
        isRaceStarted = false;
        isRaceOver = false;
    }

    void Start()
    {
        if (winnerPanel != null)
            winnerPanel.SetActive(false);
        if (winnerText != null)
            winnerText.gameObject.SetActive(false);

        if (winnerPanel2 != null)
            winnerPanel2.SetActive(false);

        StartCoroutine(CountdownSequence());
    }

    void Update()
    {
        // Permitir reiniciar la carrera pulsando la tecla 'R' cuando termine
        if (isRaceOver && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private IEnumerator CountdownSequence()
    {
        isRaceStarted = false;

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);

            for (int i = countdownSeconds; i > 0; i--)
            {
                countdownText.text = i.ToString();
                yield return new WaitForSeconds(1f);
            }

            countdownText.text = "YA!";
        }
        else
        {
            yield return new WaitForSeconds(countdownSeconds);
        }

        // ¡Comienza la carrera!
        isRaceStarted = true;

        yield return new WaitForSeconds(1f);
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    // Llamado por el primer coche que cruza la meta
    public void CarFinished(SplineCarController.Team winnerTeam)
    {
        if (isRaceOver)
            return; // Solo el primero activa la victoria
        isRaceOver = true;

        button.SetActive(true);

        if (winnerText != null)
        {
            winnerText.gameObject.SetActive(true);

            if (winnerTeam == SplineCarController.Team.Red_WASD)
            {
                if (winnerPanel != null)
                    winnerPanel.SetActive(true);
            }
            else
            {
                if (winnerPanel2)
                    winnerPanel2.SetActive(true);
            }
        }

        Debug.Log($"CARRERA TERMINADA! Ganador: {winnerTeam}");
    }

    /// <summary>
    /// Vuelve al menú principal y elimina todos los objetos persistentes creados
    /// mediante DontDestroyOnLoad para que la siguiente partida empiece limpia.
    /// Se puede asignar directamente al evento On Click de un botón.
    /// </summary>
    public void ReturnToMainMenu()
    {
        if (isReturningToMenu)
            return;

        isReturningToMenu = true;
        Destroy(UIManager.Singleton.gameObject);
        Destroy(GameManager.Singleton.gameObject);
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
