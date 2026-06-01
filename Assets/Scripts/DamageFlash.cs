using System;
using System.Reflection;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    // Animator to trigger the flash animation. If left empty the component will try to get one on the same GameObject.
    public Animator animator;
    public string flashTriggerName = "FlashRed";

    // Optional: assign the component that holds the health value in the inspector.
    // If left empty the script will try to find a component on this GameObject or its parents that has a health-like field/property.
    public Component healthSource;

    float lastHealth = float.NaN;
    Component cachedHealthComponent;
    readonly string[] candidateNames = { "CurrentHealth", "currentHealth", "health", "Health", "hp", "HP" };

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Start()
    {
        // initialize lastHealth from available source (if any)
        lastHealth = GetHealthValue();
    }

    void Update()
    {
        float current = GetHealthValue();
        if (!float.IsNaN(current) && !float.IsNaN(lastHealth))
        {
            if (current < lastHealth)
            {
                FlashRed();
            }
            // update lastHealth to track future changes (including heals)
            lastHealth = current;
        }
    }

    // Call this method from ANYWHERE to trigger the red flash
    public void FlashRed()
    {
        if (animator == null)
        {
            Debug.LogError("Cannot flash red: No Animator assigned to " + gameObject.name);
            return;
        }

        animator.SetTrigger(flashTriggerName);
    }

    float GetHealthValue()
    {
        Component source = healthSource ?? FindHealthComponent();
        if (source == null)
            return float.NaN;

        Type t = source.GetType();

        // Try properties first
        foreach (var name in candidateNames)
        {
            var prop = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.CanRead)
            {
                object val = prop.GetValue(source);
                if (TryConvertToFloat(val, out float f)) return f;
            }

            var field = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                object val = field.GetValue(source);
                if (TryConvertToFloat(val, out float f)) return f;
            }
        }

        // Nothing found
        return float.NaN;
    }

    Component FindHealthComponent()
    {
        if (cachedHealthComponent != null) return cachedHealthComponent;

        Transform t = transform;
        while (t != null)
        {
            var comps = t.GetComponents<MonoBehaviour>();
            foreach (var comp in comps)
            {
                if (comp == null) continue;
                Type ct = comp.GetType();
                foreach (var name in candidateNames)
                {
                    if (ct.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null ||
                        ct.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
                    {
                        cachedHealthComponent = comp;
                        return cachedHealthComponent;
                    }
                }
            }
            t = t.parent;
        }

        return null;
    }

    bool TryConvertToFloat(object value, out float result)
    {
        result = 0f;
        if (value == null) return false;
        if (value is float f) { result = f; return true; }
        if (value is double d) { result = (float)d; return true; }
        if (value is int i) { result = i; return true; }
        if (value is long l) { result = l; return true; }
        if (value is short s) { result = s; return true; }
        // try numeric convert
        try
        {
            result = Convert.ToSingle(value);
            return true;
        }
        catch { return false; }
    }
}