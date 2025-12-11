using UnityEngine;

public class Character : MonoBehaviour
{
    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }

    public virtual void Damage(float damageAmount)
    {
        CurrentHealth -= damageAmount;
        Debug.Log($"Character damaged. CurrentHealth = {CurrentHealth}");
        
        if (CurrentHealth <= 0)
        {
            Debug.Log("died");    
        }
        else
        {
            Debug.Log("Survived");
        }
        
        CurrentHealth = Mathf.Max(0f, CurrentHealth);
    }
}
