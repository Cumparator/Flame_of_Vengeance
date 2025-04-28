using UnityEngine;
using UnityEngine.UI;
using FlameOfVengeance.Interfaces;
using Player;

[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerStamina))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Основные Настройки")][SerializeField] private int maxHealth = 100;
    [Tooltip("Примерная длительность анимации смерти в секундах")]
    [SerializeField] private float deathSequenceDuration = 2.0f;

    [Header("Интерфейс")][SerializeField] private Image healthSlider;

    private int _currentHealth;
    private PlayerAnimator _playerAnimator;
    private PlayerInput _playerInput;
    private PlayerMovement _playerMovement;
    private PlayerCombat _playerCombat;
    private PlayerStamina _playerStamina;
    private Rigidbody2D _rb;
    private bool _isDead = false;

    private void Awake()
    {
        _playerAnimator = GetComponent<PlayerAnimator>();
        _playerInput = GetComponent<PlayerInput>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerCombat = GetComponent<PlayerCombat>();
        _playerStamina = GetComponent<PlayerStamina>();
        _rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        _currentHealth = maxHealth;
        _isDead = false;
        SetPlayerComponentsEnabled(true);
        UpdateHealthUI();
    }

    public void TakeDamage(int damageAmount)
    {
        if (_isDead || _playerMovement && _playerMovement.IsRolling) 
            return;
        
        if (_playerCombat && _playerCombat.IsBlocking)
        {
            _playerStamina.SpendStaminaOnBlockHit(); 
            return;
        }

        _currentHealth -= damageAmount;
        UpdateHealthUI();

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Die();
        }
        else
        {
            _playerAnimator.TriggerHurt();
        }
    }

    public void Heal(int amount)
    {
        if (_isDead) return;

        _currentHealth += amount;
        if (_currentHealth > maxHealth)
        {
            _currentHealth = maxHealth;
        }
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (!healthSlider) return;
        
        if (maxHealth > 0) 
        {
            healthSlider.fillAmount = (float)_currentHealth / maxHealth;
        }
        else
        {
            healthSlider.fillAmount = 0;
        }
    }

    void Die()
    {
        if (_isDead) return;
        
        _isDead = true;

        _playerAnimator.TriggerDeath();

        SetPlayerComponentsEnabled(false);

        if (_rb) { 
            _rb.linearVelocity = Vector2.zero; 
            _rb.bodyType = RigidbodyType2D.Kinematic;
        }
        
        Destroy(gameObject, deathSequenceDuration); 

        // Можно добавить вызов логики Game Over здесь или в HandleDeathAnimationEnd()
    }

    private void SetPlayerComponentsEnabled(bool isEnabled)
    {
        if (_playerInput)
        {
            _playerInput.enabled = isEnabled;
        }
        if (_playerMovement)
        {
            _playerMovement.enabled = isEnabled;
        }
        if (_playerCombat)
        {
            _playerCombat.enabled = isEnabled;
        }

        // Можно добавить коллайдер, если нужно его отключить
        // Collider2D collider = GetComponent<Collider2D>();
        // if (collider != null) collider.enabled = isEnabled;
    }
}