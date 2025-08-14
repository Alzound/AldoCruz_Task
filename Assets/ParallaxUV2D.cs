using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class ParallaxUV2D : MonoBehaviour
{
    [SerializeField] private Vector2 movVelocity; 
    private Vector2 _uvOffset;
    private Material _material;

    private void Awake()
    {
        _material = GetComponent<SpriteRenderer>().material;
    }

    private void Update()
    {
        _uvOffset = (Player_Controller.instance.gameObject.GetComponent<Rigidbody2D>().linearVelocity.x * 0.1f) * movVelocity * Time.deltaTime;
        _material.mainTextureOffset += _uvOffset;
    }
}
