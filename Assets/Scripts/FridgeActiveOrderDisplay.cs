using TMPro;
using UnityEngine;

public class FridgeActiveOrderDisplay : MonoBehaviour
{
    public TMP_Text orderText;
    public OrderState orderState;

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

        OrderState state = GetOrderState();

        if (state == null)
        {
            orderText.text = "Active Order: (OrderState missing)";
            return;
        }

        if (!state.hasActiveOrder)
        {
            orderText.text = "Active Order: None";
            return;
        }

        orderText.text = $"Active Order: {GetOrderName(state.activeOrderType)}";
    }

    private OrderState GetOrderState()
    {
        if (orderState != null)
            return orderState;

        return OrderManager.Instance != null ? OrderManager.Instance.State : null;
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
