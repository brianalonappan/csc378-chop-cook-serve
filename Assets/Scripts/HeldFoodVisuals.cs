using UnityEngine;

public class HeldFoodVisuals : MonoBehaviour
{
    public GameObject heldPepperoniPizza;
    public GameObject heldCheesePizza;

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
}