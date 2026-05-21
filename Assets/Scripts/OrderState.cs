using UnityEngine;

[CreateAssetMenu(fileName = "OrderState", menuName = "Scriptable Objects/OrderState")]
public class OrderState : ScriptableObject
{
    public bool hasActiveOrder;
    public OrderType activeOrderType;

    public bool orderReceived;
    public bool washedHands;
    public bool grabbedIngredients;
    public bool choppedOrStretched;
    public bool addedToppings;
    public bool cookedOrBaked;
    public bool cutPizza;
    public bool served;
    public bool foodBurned;
    public bool finishedOrder;

    public bool pickedDough;
    public bool pickedPotato;
    public bool pickedCheese;
    public bool pickedPepperoni;

    public void Clear()
    {
        hasActiveOrder = false;
        activeOrderType = default;
        orderReceived = false;
        washedHands = false;
        grabbedIngredients = false;
        choppedOrStretched = false;
        addedToppings = false;
        cookedOrBaked = false;
        cutPizza = false;
        served = false;
        foodBurned = false;
        finishedOrder = false;
        ClearPickedIngredients();
    }

    public void CaptureFromReceipt(ReceiptOrder receipt)
    {
        if (receipt == null)
        {
            Clear();
            return;
        }

        bool changedActiveOrder = !hasActiveOrder || activeOrderType != receipt.orderType;

        hasActiveOrder = true;
        activeOrderType = receipt.orderType;
        orderReceived = receipt.orderReceived;
        washedHands = receipt.washedHands;
        grabbedIngredients = receipt.grabbedIngredients;
        choppedOrStretched = receipt.choppedOrStretched;
        addedToppings = receipt.addedToppings;
        cookedOrBaked = receipt.cookedOrBaked;
        cutPizza = receipt.cutPizza;
        served = receipt.served;
        foodBurned = receipt.foodBurned;
        finishedOrder = receipt.finishedOrder;

        if (changedActiveOrder || grabbedIngredients)
        {
            ClearPickedIngredients();
        }
    }

    public bool ActiveOrderNeedsIngredient(IngredientType ingredient)
    {
        return hasActiveOrder &&
            !grabbedIngredients &&
            ActiveOrderUsesIngredient(ingredient) &&
            !HasPickedIngredient(ingredient);
    }

    public bool ActiveOrderUsesIngredient(IngredientType ingredient)
    {
        return hasActiveOrder &&
            OrderUsesIngredient(activeOrderType, ingredient);
    }

    public static bool OrderUsesIngredient(OrderType orderType, IngredientType ingredient)
    {
        switch (orderType)
        {
            case OrderType.FrenchFries:
                return ingredient == IngredientType.Potato;

            case OrderType.CheesePizza:
                return ingredient == IngredientType.Dough ||
                    ingredient == IngredientType.Cheese;

            case OrderType.PepperoniPizza:
                return ingredient == IngredientType.Dough ||
                    ingredient == IngredientType.Cheese ||
                    ingredient == IngredientType.Pepperoni;
        }

        return false;
    }

    public void PickIngredient(IngredientType ingredient)
    {
        switch (ingredient)
        {
            case IngredientType.Dough:
                pickedDough = true;
                break;
            case IngredientType.Potato:
                pickedPotato = true;
                break;
            case IngredientType.Cheese:
                pickedCheese = true;
                break;
            case IngredientType.Pepperoni:
                pickedPepperoni = true;
                break;
        }
    }

    public bool HasPickedIngredient(IngredientType ingredient)
    {
        switch (ingredient)
        {
            case IngredientType.Dough:
                return pickedDough;
            case IngredientType.Potato:
                return pickedPotato;
            case IngredientType.Cheese:
                return pickedCheese;
            case IngredientType.Pepperoni:
                return pickedPepperoni;
        }

        return false;
    }

    public bool HasPickedAllRequiredIngredients()
    {
        if (!hasActiveOrder || grabbedIngredients)
            return false;

        switch (activeOrderType)
        {
            case OrderType.FrenchFries:
                return pickedPotato;

            case OrderType.CheesePizza:
                return pickedDough && pickedCheese;

            case OrderType.PepperoniPizza:
                return pickedDough && pickedCheese && pickedPepperoni;
        }

        return false;
    }

    private void ClearPickedIngredients()
    {
        pickedDough = false;
        pickedPotato = false;
        pickedCheese = false;
        pickedPepperoni = false;
    }
}
