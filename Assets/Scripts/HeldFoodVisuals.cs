using UnityEngine;

public class HeldFoodVisuals : MonoBehaviour
{
    public GameObject heldPepperoniPizza;
    public GameObject heldCheesePizza;
    public GameObject heldFries;
    public Color normalFriesColor = Color.white;
    public Color burnedFriesColor = new Color(0.18f, 0.12f, 0.07f, 1f);

    private SpriteRenderer[] heldFriesRenderers;

    private void Start()
    {
        CacheHeldFriesRenderers();
        HideAllFood();
    }

    public void HideAllFood()
    {
        if (heldPepperoniPizza != null)
            heldPepperoniPizza.SetActive(false);

        if (heldCheesePizza != null)
            heldCheesePizza.SetActive(false);

        if (heldFries != null)
            heldFries.SetActive(false);
    }

    public void ShowPepperoniPizza()
    {
        HideAllFood();

        if (heldPepperoniPizza != null)
            heldPepperoniPizza.SetActive(true);
    }

    public void ShowCheesePizza()
    {
        HideAllFood();

        if (heldCheesePizza != null)
            heldCheesePizza.SetActive(true);
    }

    public void ShowFries(bool burned = false)
    {
        HideAllFood();

        if (heldFries != null)
        {
            ApplyFriesColor(burned);
            heldFries.SetActive(true);
        }
    }

    private void ApplyFriesColor(bool burned)
    {
        CacheHeldFriesRenderers();

        Color friesColor = burned ? burnedFriesColor : normalFriesColor;

        foreach (SpriteRenderer friesRenderer in heldFriesRenderers)
        {
            if (friesRenderer != null)
                friesRenderer.color = friesColor;
        }
    }

    private void CacheHeldFriesRenderers()
    {
        if (heldFries == null || heldFriesRenderers != null)
            return;

        heldFriesRenderers = heldFries.GetComponentsInChildren<SpriteRenderer>(true);
    }
}
