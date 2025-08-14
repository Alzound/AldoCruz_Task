using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxUV2D : MonoBehaviour
{
    [SerializeField] Vector2 movVelocity = new Vector2(0.1f, 0f);

    static readonly int UV_OFFSET = Shader.PropertyToID("_UVOffset");
    SpriteRenderer sr;
    Material mat;           // instancia propia en Play
    Rigidbody2D rb;
    Vector2 uv;             // acumulador local

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        mat = new Material(sr.sharedMaterial);   // instancia (sin tocar el compartido)
        sr.sharedMaterial = mat;

        var pc = Player_Controller.instance;
        if (pc) rb = pc.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!mat || !rb) return;
        Debug.Log("Parallax"); 
        uv += rb.linearVelocity.x * 0.1f * movVelocity * Time.deltaTime; // acumula
        mat.SetVector("_UVOffset", uv);  // <-- mueve _UVOffset del shader
    }

    void OnDestroy()
    {
        if (mat) Destroy(mat);
    }
}

