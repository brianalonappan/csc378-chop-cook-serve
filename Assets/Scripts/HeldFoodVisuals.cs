using UnityEngine;

public class HeldFoodVisuals : MonoBehaviour
{
    public enum HeldFoodType
    {
        None,
        PepperoniPizza,
        CheesePizza,
        Fries
    }

    public GameObject heldPepperoniPizza;
    public GameObject heldCheesePizza;
    public GameObject heldFries;
    public Color normalFriesColor = Color.white;
    public Color burnedFriesColor = new Color(0.18f, 0.12f, 0.07f, 1f);

    private SpriteRenderer[] heldFriesRenderers;

    public HeldFoodType CurrentFood { get; private set; }
    public bool HoldingBurnedFood { get; private set; }
    public bool IsHoldingFood => CurrentFood != HeldFoodType.None;

    private void Start()
    {
        CacheHeldFriesRenderers();
        HideAllFood();
    }

    public void HideAllFood()
    {
        CurrentFood = HeldFoodType.None;
        HoldingBurnedFood = false;

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

        CurrentFood = HeldFoodType.PepperoniPizza;
        HoldingBurnedFood = false;
    }

    public void ShowCheesePizza()
    {
        HideAllFood();

        if (heldCheesePizza != null)
            heldCheesePizza.SetActive(true);

        CurrentFood = HeldFoodType.CheesePizza;
        HoldingBurnedFood = false;
    }

    public void ShowFries(bool burned = false)
    {
        HideAllFood();

        if (heldFries != null)
        {
            ApplyFriesColor(burned);
            heldFries.SetActive(true);
        }

        CurrentFood = HeldFoodType.Fries;
        HoldingBurnedFood = burned;
    }

    public bool CanServeOrder(OrderType orderType)
    {
        switch (orderType)
        {
            case OrderType.FrenchFries:
                return CurrentFood == HeldFoodType.Fries && !HoldingBurnedFood;

            case OrderType.CheesePizza:
                return CurrentFood == HeldFoodType.CheesePizza;

            case OrderType.PepperoniPizza:
                return CurrentFood == HeldFoodType.PepperoniPizza;
        }

        return false;
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
