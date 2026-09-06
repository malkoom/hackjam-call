using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ConfettiEffect : MonoBehaviour
{
    [Header("Área de Confeti")]
    public Vector3 areaSize = new Vector3(40f, 2f, 40f);

    [Header("Comportamiento")]
    public float confettiRate = 120f;
    public float gravity = 0.08f;
    public float windTurbulence = 0.7f;
    public float paperLifetime = 8f;

    [Header("Tamaño Visible de las Tiras")]
    [Tooltip("Tamaño aumentado para que se vea perfectamente a distancia")]
    public Vector2 paperSize = new Vector2(0.35f, 0.75f);

    [Header("Colores")]
    public Color[] confettiColors = new Color[]
    {
        new Color(1f, 0.2f, 0.2f),   // Rojo
        new Color(0.2f, 0.55f, 1f),  // Azul
        new Color(1f, 0.85f, 0.1f),  // Amarillo
        new Color(0.2f, 0.9f, 0.3f),  // Verde
        new Color(1f, 0.3f, 0.85f),  // Magenta
        new Color(0.1f, 0.95f, 0.95f) // Cian
    };

    private ParticleSystem ps;

    void Start()
    {
        SetupAndPlay();
    }

    [ContextMenu("Regenerar y Reproducir")]
    public void SetupAndPlay()
    {
        ps = GetComponent<ParticleSystem>();

        // Forzar rotación mirando hacia abajo
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        var main = ps.main;
        main.playOnAwake = true;
        main.loop = true;
        main.startLifetime = paperLifetime;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.gravityModifier = gravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 2000;

        // Tamaño visible
        main.startSize3D = true;
        main.startSizeX = paperSize.x;
        main.startSizeY = paperSize.y;
        main.startSizeZ = 0.05f;

        // Rotación aleatoria
        main.startRotation3D = true;
        main.startRotationX = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.startRotationY = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.startRotationZ = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);

        // Colores
        var gradient = new Gradient();
        var colorKeys = new GradientColorKey[confettiColors.Length];
        for (int i = 0; i < confettiColors.Length; i++)
        {
            colorKeys[i] = new GradientColorKey(confettiColors[i], (float)i / (confettiColors.Length - 1));
        }
        gradient.SetKeys(colorKeys, new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        main.startColor = new ParticleSystem.MinMaxGradient(gradient) { mode = ParticleSystemGradientMode.RandomColor };

        // Emisión
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = confettiRate;

        // Área
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = areaSize;

        // Turbulencia
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = windTurbulence;
        noise.frequency = 0.35f;

        // Giro 3D
        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.separateAxes = true;
        rot.x = new ParticleSystem.MinMaxCurve(-3f, 3f);
        rot.y = new ParticleSystem.MinMaxCurve(-4f, 4f);
        rot.z = new ParticleSystem.MinMaxCurve(-2f, 2f);

        // Material blanco básico que toma los colores de las partículas
        var rend = GetComponent<ParticleSystemRenderer>();
        Shader s = Shader.Find("Sprites/Default");
        if (s != null) rend.material = new Material(s);

        ps.Clear();
        ps.Play();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, areaSize);
    }
}