using TMPro;
using UnityEngine;

public class FridgeActiveOrderDisplay : MonoBehaviour
{
    public TMP_Text orderText;

    private void Start()
    {
        if (orderText == null)
        {
            if (!TryGetComponent(out orderText))
            {
                orderText = GetComponentInChildren<TMP_Text>();
            }
        }

        if (orderText == null)
        {
            Debug.LogError("FridgeActiveOrderDisplay: orderText is not assigned and no TMP_Text was found on this GameObject or its children.");
        }

        UpdateOrderText();
    }

    private void Update()
    {
        UpdateOrderText();
    }

    private void UpdateOrderText()
    {
        if (orderText == null)
            return;

        if (OrderManager.Instance == null)
        {
            orderText.text = "Active Order: (OrderManager missing)";
            return;
        }

        var activeReceipt = OrderManager.Instance.ActiveReceipt;
        Debug.Log($"[FridgeDisplay] Instance exists: true, ActiveReceipt: {(activeReceipt == null ? "NULL" : activeReceipt.orderType.ToString())}");

        if (activeReceipt == null)
        {
            orderText.text = "Active Order: None";
            return;
        }

        orderText.text = $"Active Order: {GetOrderName(activeReceipt.orderType)}";
    }

    private string GetOrderName(OrderType orderType)
    {
        switch (orderType)
        {
            case OrderType.FrenchFries:
                return "French Fries";
            case OrderType.CheesePizza:
                return "Cheese Pizza";
            case OrderType.PepperoniPizza:
                return "Pepperoni Pizza";
            default:
                return orderType.ToString();
        }
    }
}
