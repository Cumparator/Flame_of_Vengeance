using UnityEngine;
using UnityEngine.Events;
using FlameOfVengeance.Interfaces;

[RequireComponent(typeof(BanditAI))]
[RequireComponent(typeof(Animator))]
public class BanditHealth : MonoBehaviour, IDamageable
{
    private static readonly int Hurt = Animator.StringToHash("Hurt");
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private UnityEvent OnDeath;

    private int _currentHealth;
    private BanditAI _banditAI;
    private Animator _animator;
    private bool _isDead = false;

    private void Awake()
    {
        _banditAI = GetComponent<BanditAI>();
        _animator = GetComponent<Animator>();
        _currentHealth = maxHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        if (_isDead) return;

        _currentHealth -= damageAmount;

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Die();
        }
        else
        {
            if (_animator)
            {
                _animator.SetTrigger(Hurt);
            }
        }
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        if (_banditAI)
        {
             _banditAI.Die();
        }

        OnDeath?.Invoke();
        
        Destroy(gameObject, 2f);
    }
}