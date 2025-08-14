using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxUVLayer2D : MonoBehaviour
{
    [Header("Cámara (si está vacío usa Main Camera)")]
    [SerializeField] private Transform cam;

    [Header("Parallax (0 = sin parallax; 1 = capa lejana típica)")]
    [Range(-2f, 2f)] public float xStrength = 0.3f;
    [Range(-2f, 2f)] public float yStrength = 0f;

    [Header("ST del shader (_MainTex_ST)")]
    public Vector2 tiling = Vector2.one;   // escala UV
    public Vector2 offset = Vector2.zero;  // offset base manual
    public Vector2 scrollSpeed = Vector2.zero; // auto-scroll opcional

    private SpriteRenderer sr;
    private MaterialPropertyBlock mpb;
    private int _MainTex_ST;

    private Vector3 camStart;
    private Vector2 runningOffset;
    private float widthWorld, heightWorld;

    void Reset() { sr = GetComponent<SpriteRenderer>(); }

    void Awake()
    {
        EnsureInit();
        CacheBounds();
    }

    void OnEnable()
    {
        EnsureInit();
        Reanchor();
        Apply(); // escribe el ST inicial
    }

    void OnValidate()
    {
        EnsureInit();     // <- clave: mpb nunca nulo en editor
        CacheBounds();
        Apply();
    }

    void LateUpdate()
    {
        if (!cam) { if (Camera.main) cam = Camera.main.transform; else return; }

        if (scrollSpeed.sqrMagnitude > 0f)
            runningOffset += scrollSpeed * Time.deltaTime;

        Vector3 delta = cam.position - camStart;

        float uvPerUnitX = (widthWorld > 1e-6f) ? (tiling.x / widthWorld) : 0f;
        float uvPerUnitY = (heightWorld > 1e-6f) ? (tiling.y / heightWorld) : 0f;

        Vector2 parallaxUV = new Vector2(
            delta.x * xStrength * uvPerUnitX,
            delta.y * yStrength * uvPerUnitY
        );

        Vector2 finalOffset = offset + runningOffset + parallaxUV;

        Apply(finalOffset);
    }

    public void Reanchor()
    {
        if (!cam && Camera.main) cam = Camera.main.transform;
        camStart = cam ? cam.position : Vector3.zero;
        runningOffset = Vector2.zero;
    }

    private void CacheBounds()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        if (sr && sr.sprite)
        {
            var s = sr.bounds.size; // incluye escala
            widthWorld = Mathf.Max(1e-6f, s.x);
            heightWorld = Mathf.Max(1e-6f, s.y);
        }
    }

    private void EnsureInit()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        if (mpb == null) mpb = new MaterialPropertyBlock();
        if (_MainTex_ST == 0) _MainTex_ST = Shader.PropertyToID("_MainTex_ST"); // cambia a _BaseMap_ST si tu shader usa ese
        if (!cam && Camera.main) cam = Camera.main.transform;
    }

    private void Apply() => Apply(offset);

    private void Apply(Vector2 uvOffset)
    {
        if (!sr) return;
        EnsureInit();

        mpb.Clear();
        mpb.SetVector(_MainTex_ST, new Vector4(tiling.x, tiling.y, uvOffset.x, uvOffset.y));
        sr.SetPropertyBlock(mpb);
    }
}
