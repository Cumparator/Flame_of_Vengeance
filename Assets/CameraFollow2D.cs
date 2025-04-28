using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Настройки Следования")]
    [Tooltip("Насколько плавно камера следует за игроком (меньше = плавнее)")]
    [SerializeField] private float damping = 1.5f;
    [Tooltip("Смещение камеры относительно игрока")][SerializeField] private Vector2 offset = new(2f, 1f);
    [Tooltip("Игрок начинает смотрящим влево?")]
    [SerializeField] private bool faceLeft;
    
    [Header("Цель Следования")]
    [Tooltip("Перетащи сюда Transform объекта игрока")][SerializeField] private Transform player;

    void Start()
    {
        offset = new Vector2(Mathf.Abs(offset.x), offset.y);
        if (!player) return;
        var initialTarget = faceLeft 
            ? new Vector3(player.position.x - offset.x, player.position.y + offset.y, transform.position.z) 
            : new Vector3(player.position.x + offset.x, player.position.y + offset.y, transform.position.z);
        transform.position = initialTarget;
    }

    void LateUpdate()
    {
        if (!player) return;

        var target = faceLeft 
            ? new Vector3(player.position.x - offset.x, player.position.y + offset.y, transform.position.z) 
            : new Vector3(player.position.x + offset.x, player.position.y + offset.y, transform.position.z);
        var currentPosition = Vector3.Lerp(transform.position, target, damping * Time.deltaTime);
        transform.position = currentPosition;
    }
}