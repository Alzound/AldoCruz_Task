using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Platform_Behaviour : MonoBehaviour
{
    public enum PlatformMovementType { None, Horizontal, Vertical, Diagonal }

    [Header("Movimiento")]
    public PlatformMovementType type = PlatformMovementType.Horizontal;
    public float distance = 3f;
    public float speed = 1f;
    public Vector2 diagonalDir = new(1f, 1f);

    private Vector3 startPos;

    // --- Simple carry por tag "Player" ---
    [Header("Carry simple por tag")]
    public string playerTag = "Player";
    private Transform playerCurrent;
    private Transform playerOriginalParent;

    private void Awake() => startPos = transform.position;

    private void Update()
    {
        Vector2 dir = GetDir();
        if (dir.sqrMagnitude < 0.0001f) return;

        //This makes the platform move back and forth between -distance and +distance
        float t = Mathf.PingPong(Time.time * speed, 1f) * 2f - 1f;
        transform.position = startPos + (Vector3)(dir.normalized * (distance * t));
    }

    private Vector2 GetDir()
    {
        switch (type)
        {
            case PlatformMovementType.Horizontal: return Vector2.right;
            case PlatformMovementType.Vertical: return Vector2.up;
            case PlatformMovementType.Diagonal: return diagonalDir;
            default: return Vector2.zero;
        }
    }

    // --- Parent/Unparent sólo por tag Player ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (playerCurrent) return; // ya hay uno montado
        if (!collision.transform.CompareTag(playerTag)) return;

        playerCurrent = collision.transform;
        playerOriginalParent = playerCurrent.parent;
        playerCurrent.SetParent(transform, worldPositionStays: true);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!playerCurrent) return;
        if (collision.transform != playerCurrent) return;

        // restaurar padre original y limpiar refs
        playerCurrent.SetParent(playerOriginalParent, worldPositionStays: true);
        playerCurrent = null;
        playerOriginalParent = null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying) startPos = transform.position;
    }
#endif

    private void OnDisable()
    {
        // seguridad: si se desactiva, suelta al player si está montado
        if (playerCurrent)
        {
            playerCurrent.SetParent(playerOriginalParent, worldPositionStays: true);
            playerCurrent = null;
            playerOriginalParent = null;
        }
    }
}
