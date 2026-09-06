using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RectTransform))]
public class PlayerControlsOverlay : MonoBehaviour
{
    [Header("Placeholders de Sprites (Opcionales)")]
    [Tooltip("Icono para Frenar de Jugador Rojo (ej: tecla A)")]
    public Sprite redBrakeSprite;
    [Tooltip("Icono para Acelerar de Jugador Rojo (ej: tecla D)")]
    public Sprite redAccelSprite;

    [Tooltip("Icono para Frenar de Jugador Azul (ej: tecla Flecha Izquierda)")]
    public Sprite blueBrakeSprite;
    [Tooltip("Icono para Acelerar de Jugador Azul (ej: tecla Flecha Derecha)")]
    public Sprite blueAccelSprite;

    [Header("Posición en Pantalla")]
    [Tooltip("Altura desde el borde inferior de la pantalla")]
    public float bottomOffset = 45f;

    [Header("Tiempos de Visualización")]
    public float fadeInDuration = 1.0f;
    public float displayDuration = 8.5f;
    public float fadeOutDuration = 1.5f;

    private CanvasGroup canvasGroup;
    private Image redBrakeImg, redAccelImg;
    private Image blueBrakeImg, blueAccelImg;

    private Color normalKeyColor = new Color(0.18f, 0.18f, 0.18f, 0.9f);
    private Color pressedKeyColor = new Color(0.1f, 1f, 0.25f, 1f);

    void Start()
    {
        SetupUI();
        StartCoroutine(DisplaySequence());
    }

    private void SetupUI()
    {
        // 1. Asegurar que este GameObject es un RectTransform estirado a pantalla completa
        RectTransform myRect = GetComponent<RectTransform>();
        myRect.anchorMin = Vector2.zero;
        myRect.anchorMax = Vector2.one;
        myRect.offsetMin = Vector2.zero;
        myRect.offsetMax = Vector2.zero;

        // 2. Comprobar que está dentro de un Canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            GameObject cObj = new GameObject("ControlsCanvas");
            canvas = cObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = cObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            cObj.AddComponent<GraphicRaycaster>();
            transform.SetParent(cObj.transform, false);

            myRect.anchorMin = Vector2.zero;
            myRect.anchorMax = Vector2.one;
            myRect.offsetMin = Vector2.zero;
            myRect.offsetMax = Vector2.zero;
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // 3. Panel Izquierdo (Jugador Rojo - WASD): Anclado al 25% horizontal (centro de la pantalla izquierda)
        CreatePlayerControlsPanel("RedPlayerControls", new Vector2(0.25f, 0f), 
            "JUGADOR 1 (ROJO)", new Color(1f, 0.35f, 0.35f), "A", "FRENAR", redBrakeSprite, out redBrakeImg,
            "D", "ACELERAR", redAccelSprite, out redAccelImg);

        // 4. Panel Derecho (Jugador Azul - Flechas): Anclado al 75% horizontal (centro de la pantalla derecha)
        CreatePlayerControlsPanel("BluePlayerControls", new Vector2(0.75f, 0f), 
            "JUGADOR 2 (AZUL)", new Color(0.35f, 0.65f, 1f), "←", "FRENAR", blueBrakeSprite, out blueBrakeImg,
            "→", "ACELERAR", blueAccelSprite, out blueAccelImg);
    }

    private GameObject CreatePlayerControlsPanel(string name, Vector2 anchorX, string title, Color teamColor,
        string key1Name, string label1, Sprite icon1, out Image img1,
        string key2Name, string label2, Sprite icon2, out Image img2)
    {
        GameObject panelObj = new GameObject(name);
        panelObj.transform.SetParent(transform, false);

        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = anchorX;
        panelRect.anchorMax = anchorX;
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, bottomOffset);
        panelRect.sizeDelta = new Vector2(300f, 105f);

        // Fondo oscuro para que resalte sobre el suelo de la pista
        Image bg = panelObj.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.08f, 0.08f, 0.75f);

        // Título del jugador
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.text = title;
        titleTmp.fontSize = 17;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.color = teamColor;
        titleTmp.alignment = TextAlignmentOptions.Center;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0f, 34f);
        titleRect.sizeDelta = new Vector2(280f, 25f);

        // Contenedor de las teclas
        GameObject keysContainer = new GameObject("Keys");
        keysContainer.transform.SetParent(panelObj.transform, false);
        RectTransform kcRect = keysContainer.AddComponent<RectTransform>();
        kcRect.anchoredPosition = new Vector2(0f, -10f);
        kcRect.sizeDelta = new Vector2(260f, 60f);

        // Botón 1 (Freno)
        CreateKeyButton(keysContainer.transform, new Vector2(-60f, 0f), key1Name, label1, icon1, out img1);

        // Botón 2 (Acelerador)
        CreateKeyButton(keysContainer.transform, new Vector2(60f, 0f), key2Name, label2, icon2, out img2);

        return panelObj;
    }

    private GameObject CreateKeyButton(Transform parent, Vector2 pos, string keyLetter, string actionLabel, Sprite sprite, out Image iconImg)
    {
        GameObject keyObj = new GameObject("Key_" + keyLetter);
        keyObj.transform.SetParent(parent, false);

        RectTransform rt = keyObj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(48f, 48f);

        iconImg = keyObj.AddComponent<Image>();

        if (sprite != null)
        {
            iconImg.sprite = sprite;
            iconImg.color = Color.white;
        }
        else
        {
            // Placeholder: Tecla cuadrada estilizada
            iconImg.color = normalKeyColor;

            GameObject textObj = new GameObject("KeyText");
            textObj.transform.SetParent(keyObj.transform, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = keyLetter;
            tmp.fontSize = 24;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            textObj.GetComponent<RectTransform>().sizeDelta = new Vector2(48f, 48f);
        }

        // Texto descriptivo ("FRENAR" / "ACELERAR")
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(keyObj.transform, false);
        TextMeshProUGUI labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
        labelTmp.text = actionLabel;
        labelTmp.fontSize = 11;
        labelTmp.fontStyle = FontStyles.Bold;
        labelTmp.alignment = TextAlignmentOptions.Center;
        labelTmp.color = new Color(0.9f, 0.9f, 0.9f, 0.95f);
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(0f, -32f);
        labelRect.sizeDelta = new Vector2(90f, 20f);

        return keyObj;
    }

    void Update()
    {
        if (Keyboard.current == null || canvasGroup == null || canvasGroup.alpha <= 0.05f) return;

        // Feedback interactivo en vivo al pulsar
        bool redBraking = Keyboard.current.aKey.isPressed || Keyboard.current.sKey.isPressed;
        bool redAcceling = Keyboard.current.dKey.isPressed || Keyboard.current.wKey.isPressed;

        AnimateKeyPress(redBrakeImg, redBraking);
        AnimateKeyPress(redAccelImg, redAcceling);

        bool blueBraking = Keyboard.current.leftArrowKey.isPressed || Keyboard.current.downArrowKey.isPressed;
        bool blueAcceling = Keyboard.current.rightArrowKey.isPressed || Keyboard.current.upArrowKey.isPressed;

        AnimateKeyPress(blueBrakeImg, blueBraking);
        AnimateKeyPress(blueAccelImg, blueAcceling);
    }

    private void AnimateKeyPress(Image img, bool isPressed)
    {
        if (img == null) return;

        Transform t = img.transform;
        if (isPressed)
        {
            t.localScale = Vector3.Lerp(t.localScale, Vector3.one * 0.88f, Time.deltaTime * 25f);
            if (img.sprite == null) img.color = pressedKeyColor;
        }
        else
        {
            float pulse = 1f + Mathf.Sin(Time.time * 3f) * 0.035f;
            t.localScale = Vector3.Lerp(t.localScale, Vector3.one * pulse, Time.deltaTime * 10f);
            if (img.sprite == null) img.color = normalKeyColor;
        }
    }

    private IEnumerator DisplaySequence()
    {
        // Fade In
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Mostrar durante el tiempo configurado
        yield return new WaitForSeconds(displayDuration);

        // Fade Out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        gameObject.SetActive(false);
    }
}