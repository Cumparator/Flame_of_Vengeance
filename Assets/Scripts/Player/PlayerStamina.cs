using Player;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    [Header("Настройки Выносливости")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float staminaRegenDelay = 1.0f;
    [SerializeField] private bool regenerateWhileBlocking = false;

    [Header("Стоимость Действий")]
    [SerializeField] public float rollStaminaCost = 30f;
    [SerializeField] public float blockInitialStaminaCost = 10f;
    [SerializeField] public float blockHitStaminaCost = 20f;

    [Header("Интерфейс (Опционально)")]
    [SerializeField] private Image staminaSlider;

    public float CurrentStamina { get; private set; }
    public bool IsRegenerating { get; private set; }

    private float _timeSinceLastSpend = 0f;
    private PlayerCombat _playerCombat;


    private void Awake()
    {
        CurrentStamina = maxStamina;
        TryGetComponent(out _playerCombat);
    }

    private void Start()
    {
        UpdateStaminaUI();
    }

    private void Update()
    {
        if (_timeSinceLastSpend < staminaRegenDelay)
        {
            _timeSinceLastSpend += Time.deltaTime;
            IsRegenerating = false;
        }
        else
        {
            var canRegenerate = !(!regenerateWhileBlocking && _playerCombat && _playerCombat.IsBlocking);

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


    private void RegenerateStamina()
    {
        CurrentStamina += staminaRegenRate * Time.deltaTime;
        CurrentStamina = Mathf.Clamp(CurrentStamina, 0, maxStamina);
        UpdateStaminaUI();
    }

    public bool HasEnoughStamina(float cost)
    {
        return CurrentStamina >= cost;
    }

    public bool TrySpendStamina(float cost)
    {
        if (!HasEnoughStamina(cost)) return false;
        CurrentStamina -= cost;
        CurrentStamina = Mathf.Clamp(CurrentStamina, 0, maxStamina);
        _timeSinceLastSpend = 0f;
        IsRegenerating = false;
        UpdateStaminaUI();
        return true;
    }

    public void SpendStaminaOnBlockHit()
    {
        CurrentStamina -= blockHitStaminaCost;
        CurrentStamina = Mathf.Clamp(CurrentStamina, 0, maxStamina);
        _timeSinceLastSpend = 0f;
        IsRegenerating = false;
        UpdateStaminaUI();
    }

    private void UpdateStaminaUI()
    {
        if (!staminaSlider) return;
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