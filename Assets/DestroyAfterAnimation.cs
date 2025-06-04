using UnityEngine;

// Прикрепи этот скрипт к твоему префабу HitEffect
public class DestroyAfterAnimation : MonoBehaviour
{
    void Start()
    {
        // Получаем компонент Animation на этом же объекте
        Animation anim = GetComponent<Animation>();

        if (anim != null && anim.clip != null)
        {
            // Ждем длительность анимации, затем уничтожаем объект
            Destroy(gameObject, anim.clip.length);
        }
        else
        {
            // Если нет Animation компонента или клипа, уничтожаем сразу (или через короткое время)
            Debug.LogWarning("Animation component or clip not found on " + gameObject.name, this);
            Destroy(gameObject, 0.5f); // Уничтожить через 0.1 секунды на всякий случай
        }
    }
}
