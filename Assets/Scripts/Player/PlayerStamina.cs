using UnityEngine;
using UnityEngine.UI; // Если будешь использовать UI Slider

public class PlayerStamina : MonoBehaviour
{
    [Header("Настройки Выносливости")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 15f; // Единиц в секунду
    [SerializeField] private float staminaRegenDelay = 1.0f; // Задержка перед началом регенерации (сек)
    [SerializeField] private bool regenerateWhileBlocking = false; // Регенерировать ли стамину во время блока?

    [Header("Стоимость Действий")]
    [SerializeField] public float rollStaminaCost = 30f; // Публичное для проверок в PlayerMovement
    [SerializeField] public float blockInitialStaminaCost = 10f; // Публичное для проверок в PlayerCombat
    [SerializeField] public float blockHitStaminaCost = 20f; // Стоимость заблокированного удара

    [Header("Интерфейс (Опционально)")]
    [SerializeField] private Image staminaSlider;

    // --- Публичные Свойства ---
    public float CurrentStamina { get; private set; }
    public float MaxStamina => maxStamina;
    public bool IsRegenerating { get; private set; }

    // --- Приватные Поля ---
    private float _timeSinceLastSpend = 0f;
    private PlayerCombat _playerCombat; // Для проверки состояния блока

    // --- Методы Жизненного Цикла ---

    private void Awake()
    {
        CurrentStamina = maxStamina;
        // Попробуем получить PlayerCombat, если он есть, для проверки блока при регенерации
        TryGetComponent<PlayerCombat>(out _playerCombat);
    }

    private void Start()
    {
        UpdateStaminaUI();
    }

    private void Update()
    {
        // Увеличиваем таймер, если не тратили стамину
        if (_timeSinceLastSpend < staminaRegenDelay)
        {
            _timeSinceLastSpend += Time.deltaTime;
            IsRegenerating = false;
        }
        else
        {
            // Проверяем, можно ли регенерировать (не блокируем, если так настроено)
            bool canRegenerate = true;
            if (!regenerateWhileBlocking && _playerCombat != null && _playerCombat.IsBlocking)
            {
                canRegenerate = false;
            }

            if (canRegenerate && CurrentStamina < maxStamina)
            {
                IsRegenerating = true;
                RegenerateStamina();
            }
            else
            {
                IsRegenerating = false;
            }
        }
    }

    // --- Основные Методы ---

    private void RegenerateStamina()
    {
        CurrentStamina += staminaRegenRate * Time.deltaTime;
        CurrentStamina = Mathf.Clamp(CurrentStamina, 0, maxStamina); // Ограничиваем сверху и снизу
        UpdateStaminaUI();
    }

    public bool HasEnoughStamina(float cost)
    {
        return CurrentStamina >= cost;
    }

    // Пытается потратить стамину, возвращает true если успешно
    public bool TrySpendStamina(float cost)
    {
        if (HasEnoughStamina(cost))
        {
            CurrentStamina -= cost;
            CurrentStamina = Mathf.Clamp(CurrentStamina, 0, maxStamina); // На всякий случай
            _timeSinceLastSpend = 0f; // Сбрасываем таймер задержки регенерации
            IsRegenerating = false;
            UpdateStaminaUI();
            return true;
        }
        // Недостаточно стамины
        Debug.Log("Not enough stamina!"); // Для отладки
        return false;
    }

    // Вызывается из PlayerHealth при блокировании удара
    public void SpendStaminaOnBlockHit()
    {
        // Не используем TrySpendStamina, т.к. стамина должна потратиться,
        // даже если её мало (иначе блок будет бесконечно эффективным при 0 стамины)
        CurrentStamina -= blockHitStaminaCost;
        CurrentStamina = Mathf.Clamp(CurrentStamina, 0, maxStamina);
        _timeSinceLastSpend = 0f;
        IsRegenerating = false;
        UpdateStaminaUI();
        Debug.Log($"Stamina reduced by block hit. Current: {CurrentStamina}");

        // Опционально: Если стамина кончилась, можно вызвать событие "блок пробит"
        // if (CurrentStamina <= 0) { /* Block broken event? */ }
    }


    private void UpdateStaminaUI()
    {
        if (staminaSlider != null)
        {
            if (maxStamina > 0)
            {
                staminaSlider.fillAmount = CurrentStamina / maxStamina;
            }
            else
            {
                staminaSlider.fillAmount = 0;
            }
        }
    }
} 