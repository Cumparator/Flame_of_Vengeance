using UnityEngine;
using UnityEngine.UI;

// Добавляем зависимости от других компонентов игрока
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerStamina))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Основные Настройки")][SerializeField] private int maxHealth = 100;
    [Tooltip("Примерная длительность анимации смерти в секундах")]
    [SerializeField] private float deathSequenceDuration = 2.0f; // Время до уничтожения объекта

    [Header("Интерфейс")][SerializeField] private Image healthSlider;

    // --- Приватные Поля ---
    private int _currentHealth;
    private PlayerAnimator _playerAnimator;
    private PlayerInput _playerInput;
    private PlayerMovement _playerMovement;
    private PlayerCombat _playerCombat;
    private PlayerStamina _playerStamina;
    private Rigidbody2D _rb; // Добавляем поле для Rigidbody2D
    private bool _isDead = false;

    // --- Методы Жизненного Цикла Unity ---

    private void Awake()
    {
        // Получаем ссылки на компоненты
        _playerAnimator = GetComponent<PlayerAnimator>();
        _playerInput = GetComponent<PlayerInput>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerCombat = GetComponent<PlayerCombat>();
        _playerStamina = GetComponent<PlayerStamina>();
        _rb = GetComponent<Rigidbody2D>(); // Получаем Rigidbody здесь
    }

    void Start()
    {
        _currentHealth = maxHealth;
        _isDead = false; // Убедимся, что не мертвы в начале
        // Включаем компоненты на старте (на случай, если они были выключены в редакторе)
        SetPlayerComponentsEnabled(true);
        UpdateHealthUI();
    }

    // --- Основные Методы --- 

    public void TakeDamage(int damage)
    {
        if (_isDead) return; // Нельзя ранить мертвого

        // Проверка на неуязвимость во время переката
        if (_playerMovement != null && _playerMovement.IsRolling)
        {
            Debug.Log("Damage ignored due to Roll!");
            // Здесь можно добавить эффект уворота
            return; // Выходим, не получая урон
        }

        // Проверка на блок
        if (_playerCombat != null && _playerCombat.IsBlocking)
        {
            Debug.Log("Damage blocked!");
            // Тратим стамину за блокированный удар
            _playerStamina.SpendStaminaOnBlockHit();
            // Здесь можно добавить эффект блока
            // Опционально: Проверить, хватило ли стамины, и если нет - пробить блок?
            // if (_playerStamina.CurrentStamina <= 0) { /* Пробить блок - нанести урон? */ }
            return; // Выходим, не получая урон (или получаем сниженный урон)
        }

        // Если не катимся и не блокируем - получаем урон
        _currentHealth -= damage;
        UpdateHealthUI();

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Die();
        }
        else
        {
            // Запускаем анимацию получения урона, если еще живы
            _playerAnimator.TriggerHurt();
        }
    }

    public void Heal(int amount)
    {
        if (_isDead) return; // Нельзя лечить мертвого

        _currentHealth += amount;
        if (_currentHealth > maxHealth)
        {
            _currentHealth = maxHealth;
        }
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            // Убедимся, что maxHealth не ноль, чтобы избежать деления на ноль
            if (maxHealth > 0) 
            {
                healthSlider.fillAmount = (float)_currentHealth / maxHealth;
            }
            else
            {
                healthSlider.fillAmount = 0;
            }
        }
    }

    void Die()
    {
        if (_isDead) return; // Предотвращаем повторный вызов
        
        _isDead = true;
        Debug.Log("Player Died - Starting death sequence");

        // 1. Запускаем анимацию смерти
        _playerAnimator.TriggerDeath();

        // 2. Отключаем управление и физику
        SetPlayerComponentsEnabled(false);

        // Используем кешированную ссылку
        if (_rb != null) { 
            _rb.linearVelocity = Vector2.zero; 
            _rb.bodyType = RigidbodyType2D.Kinematic; // Возвращаем современный способ
        }
        else
        {
             Debug.LogWarning("Rigidbody2D component not found on player!", this);
        }

        // 3. Уничтожаем объект после задержки
        // TODO: Рассмотреть использование Animation Event в конце анимации смерти
        // для более точного вызова HandleDeathAnimationEnd()
        Destroy(gameObject, deathSequenceDuration); 

        // Можно добавить вызов логики Game Over здесь или в HandleDeathAnimationEnd()
        // Например: GameManager.Instance.PlayerDied();
    }

    // Вспомогательный метод для включения/отключения компонентов
    private void SetPlayerComponentsEnabled(bool isEnabled)
    {
         // Используем явную проверку на null перед присваиванием
        if (_playerInput != null)
        {
            _playerInput.enabled = isEnabled;
        }
        if (_playerMovement != null)
        {
            _playerMovement.enabled = isEnabled;
        }
        if (_playerCombat != null)
        {
            _playerCombat.enabled = isEnabled;
        }

        // Можно добавить коллайдер, если нужно его отключить
        // Collider2D collider = GetComponent<Collider2D>();
        // if (collider != null) collider.enabled = isEnabled;
    }

    // Опционально: Метод для вызова из Animation Event
    /*
    public void HandleDeathAnimationEnd()
    {
        Debug.Log("Death animation finished.");
        // Здесь можно перезагрузить уровень, показать UI и т.д.
        // Destroy(gameObject); // Если уничтожение не было вызвано с задержкой в Die()
        // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    */
}