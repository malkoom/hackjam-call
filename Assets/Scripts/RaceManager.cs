using System.Collections;
using TMPro; // TextMeshPro para textos nítidos
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;

    [Header("Referencias de UI (TextMeshPro)")]
    [Tooltip("Texto grande centrado para la cuenta atrás (3, 2, 1, YA!)")]
    public TextMeshProUGUI countdownText;

    [Tooltip("Panel o texto que muestra el ganador al terminar")]
    public TextMeshProUGUI winnerText;

    [Tooltip("Panel de fondo de victoria (opcional)")]
    public GameObject winnerPanel;
    public GameObject winnerPanel2;

    [Header("Configuración de la Carrera")]
    [Tooltip("Número de vueltas necesarias para ganar")]
    public int totalLaps = 1;

    [Tooltip("Tiempo de cuenta atrás en segundos")]
    public int countdownSeconds = 3;

    // Estado global accesible por los coches
    public static bool isRaceStarted = false;
    public static bool isRaceOver = false;

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
}
