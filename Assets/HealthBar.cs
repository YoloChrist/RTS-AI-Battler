using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    public void Start()
    {
        UnitRuntimeStats unitStats = GetComponentInParent<UnitRuntimeStats>();

        if (unitStats != null)
        {
            unitStats.HealthChanged += OnHealthChanged;
            updateBarValue(unitStats.CurrentHealth, unitStats.MaxHealth);
        }
        else
        {
            Debug.LogWarning("HealthBar: No UnitRuntimeStats found in parent objects.");
        }
    }

    private void OnHealthChanged(int currentHealth)
    {
        UnitRuntimeStats unitStats = GetComponentInParent<UnitRuntimeStats>();
        if (unitStats != null)
        {
            updateBarValue(currentHealth, unitStats.MaxHealth);
        }
    }

    private void OnDestroy()
    {
        UnitRuntimeStats unitStats = GetComponentInParent<UnitRuntimeStats>();
        if (unitStats != null)
            unitStats.HealthChanged -= OnHealthChanged;
    }

    public void updateBarValue(float currentHealth, float maxHealth)
    {
        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);
        float invertedPercentage = 1f - healthPercentage;

        if (slider != null)
            slider.value = invertedPercentage;
        else
            Debug.LogWarning("HealthBar: No Slider component found.");
    }
}
