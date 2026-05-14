using UnityEngine;

public class HeldFoodVisuals : MonoBehaviour
{
    public GameObject heldPepperoniPizza;
    public GameObject heldCheesePizza;
    public GameObject heldFries;

    private void Start()
    {
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

    public void ShowFries()
    {
        HideAllFood();

        if (heldFries != null)
            heldFries.SetActive(true);
    }
}