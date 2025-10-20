using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    public enum AttackMode
    {
        Melee,
        Ranged
    }

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private AttackMode currentAttackMode = AttackMode.Melee;
    [SerializeField] private TMP_Text attackModeText;

    [Header("Mode Switch Settings")]
    [SerializeField] private float modeSwitchCooldown = 0.5f; // Cooldown in seconds
    private float lastModeSwitchTime = -Mathf.Infinity;

    private float lastAttackTime = -Mathf.Infinity;

    private PlayerInput playerInput;
    private InputAction attackAction;
    private InputAction switchModeAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        attackAction = playerInput.actions.FindAction("Attack");
        switchModeAction = playerInput.actions.FindAction("SwitchMode");
    }

    public void PerformAttack(InputAction.CallbackContext context)
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            if (currentAttackMode == AttackMode.Melee)
            {
                PerformMeleeAttack();
            }
            else if (currentAttackMode == AttackMode.Ranged)
            {
                PerformRangedAttack();
            }
            lastAttackTime = Time.time;
        }
    }

    private void PerformMeleeAttack()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange))
        {
            var enemyHealth = hit.collider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
            }
        }
        Debug.Log("Player performed a melee attack.");
    }

    private void PerformRangedAttack()
    {
        Debug.Log("Player performed a ranged attack.");
        // Add ranged attack logic here (e.g., instantiate a projectile)
    }

    public void SwitchAttackMode(InputAction.CallbackContext context)
    {
        // Ensure cooldown has passed before switching modes
        if (Time.time < lastModeSwitchTime + modeSwitchCooldown)
        {
            return; // Ignore input if cooldown hasn't passed
        }

        Vector2 input = context.ReadValue<Vector2>();

        if (input.y > 0)
        {
            CycleAttackMode(-1);
        }
        else if (input.y < 0)
        {
            CycleAttackMode(1);
        }

        lastModeSwitchTime = Time.time; // Update the last switch time
        UpdateAttackModeText();
    }

    private void CycleAttackMode(int direction)
    {
        Debug.Log($"Current Attack Mode: {currentAttackMode}");
        // Get all enum values
        var modes = System.Enum.GetValues(typeof(AttackMode));
        int modeCount = modes.Length;

        // Calculate the new mode index
        int currentIndex = (int)currentAttackMode;
        int newIndex = (currentIndex + direction + modeCount) % modeCount;

        Debug.Log($"Current Index: {currentIndex}, New Index: {newIndex}");

        // Update the current attack mode
        currentAttackMode = (AttackMode)modes.GetValue(newIndex);

        Debug.Log($"Updated Attack Mode: {currentAttackMode}");
    }

    private void UpdateAttackModeText()
    {
        if (attackModeText != null)
        {
            attackModeText.text = $"Mode: {currentAttackMode}";
        }
    }
}