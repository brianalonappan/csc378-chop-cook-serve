using TMPro;
using UnityEngine;

public enum OrderType
{
    FrenchFries,
    CheesePizza,
    PepperoniPizza
}

public class ReceiptOrder : MonoBehaviour
{
    public OrderType orderType;
    public TMP_Text receiptText;
    public Color activeTextColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    private HeldFoodVisuals heldFoodVisuals;
    private Color originalTextColor;
    private bool hasOriginalTextColor;

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

    private void Start()
    {
        heldFoodVisuals = FindAnyObjectByType<HeldFoodVisuals>();

        if (receiptText != null)
        {
            originalTextColor = receiptText.color;
            hasOriginalTextColor = true;
        }
    }

    public bool TryCompleteStationTask(StationType stationType)
    {
        if (stationType == StationType.TrashCan && foodBurned)
        {
            if (heldFoodVisuals == null)
                heldFoodVisuals = FindAnyObjectByType<HeldFoodVisuals>();

            if (heldFoodVisuals == null ||
                heldFoodVisuals.CurrentFood != HeldFoodVisuals.HeldFoodType.Fries ||
                !heldFoodVisuals.HoldingBurnedFood)
            {
                Debug.Log("Need to hold the burned fries to throw them away.");
                return false;
            }

            if (heldFoodVisuals != null)
                heldFoodVisuals.HideAllFood();

            RemoveReceipt();
            Debug.Log(orderType + " thrown away.");
            return true;
        }

        if (finishedOrder)
            return false;

        if (stationType == StationType.Cashier && !orderReceived)
        {
            orderReceived = true;
            Debug.Log(orderType + " order received");
            return true;
        }

        if (stationType == StationType.Sink && orderReceived && !washedHands)
        {
            washedHands = true;
            CrossOff("Wash Hands");
            Debug.Log("Hands washed");
            return true;
        }

        if (!orderReceived)
        {
            Debug.Log("Need cashier first");
            return false;
        }

        if (!washedHands)
        {
            Debug.Log("Need sink first");
            return false;
        }

        if (orderType == OrderType.FrenchFries)
            return HandleFrenchFries(stationType);

        if (orderType == OrderType.CheesePizza || orderType == OrderType.PepperoniPizza)
            return HandlePizza(stationType);

        return false;
    }

    private bool HandleFrenchFries(StationType stationType)
    {
        if (stationType == StationType.Fridge && !grabbedIngredients)
        {
            grabbedIngredients = true;
            CrossOff("Grab Potato");
            return true;
        }

        if (stationType == StationType.ChoppingBlock && grabbedIngredients && !choppedOrStretched)
        {
            Debug.Log("Entered potato chopping minigame");
            return true;
        }

        if (stationType == StationType.ToppingsTable && choppedOrStretched && !addedToppings)
        {
            addedToppings = true;
            CrossOff("Oil & Salt Potato");
            return true;
        }

        if (stationType == StationType.Stove && addedToppings && !cookedOrBaked)
        {
            Debug.Log("Entered fries frying minigame");
            return true;
        }

        if (stationType == StationType.DropOff && cookedOrBaked && !foodBurned && !served)
        {
            if (!IsHoldingCorrectFood())
            {
                Debug.Log("Need to bring the fries to the customer.");
                return false;
            }

            served = true;
            finishedOrder = true;
            CrossOff("Serve Fries");

            if (heldFoodVisuals != null)
                heldFoodVisuals.HideAllFood();

            Invoke(nameof(RemoveReceipt), 1f);
            Debug.Log("Fries complete");
            return true;
        }

        return false;
    }

    private bool HandlePizza(StationType stationType)
    {
        bool isPepperoni = orderType == OrderType.PepperoniPizza;

        if (stationType == StationType.Fridge && !grabbedIngredients)
        {
            grabbedIngredients = true;

            if (isPepperoni)
                CrossOff("Grab Dough, Cheese & Pepperoni");
            else
                CrossOff("Grab Dough & Cheese");

            return true;
        }

        if (stationType == StationType.ChoppingBlock &&
            grabbedIngredients &&
            !choppedOrStretched &&
            !addedToppings &&
            !cookedOrBaked)
        {
            Debug.Log("Entered pizza prep scene");
            return true;
        }

        if (stationType == StationType.Oven && addedToppings && !cookedOrBaked)
        {
            cookedOrBaked = true;
            CrossOff("Place in Oven");
            Debug.Log(orderType + " placed in oven");
            return true;
        }

        if (stationType == StationType.ChoppingBlock && cookedOrBaked && !cutPizza)
        {
            cutPizza = true;
            CrossOff("Cut pizza into 8");
            Debug.Log(orderType + " cut into 8");
            return true;
        }

        if (stationType == StationType.DropOff && cutPizza && !served)
        {
            if (!IsHoldingCorrectFood())
            {
                Debug.Log("Need to bring the correct pizza to the customer.");
                return false;
            }

            served = true;
            finishedOrder = true;
            CrossOff("Serve Pizza");

            if (heldFoodVisuals != null)
                heldFoodVisuals.HideAllFood();

            Invoke(nameof(RemoveReceipt), 1f);
            Debug.Log(orderType + " complete");
            return true;
        }

        return false;
    }

    private bool IsHoldingCorrectFood()
    {
        if (heldFoodVisuals == null)
            heldFoodVisuals = FindAnyObjectByType<HeldFoodVisuals>();

        return heldFoodVisuals != null && heldFoodVisuals.CanServeOrder(orderType);
    }

    public bool NeedsIngredient(IngredientType ingredient)
    {
        if (grabbedIngredients)
            return false;

        return ContainsFridgeIngredient(ingredient);
    }

    public bool ContainsFridgeIngredient(IngredientType ingredient)
    {
        return OrderState.OrderUsesIngredient(orderType, ingredient);
    }

    public bool TryPickIngredient(IngredientType ingredient)
    {
        if (!NeedsIngredient(ingredient))
            return false;

        TryCompleteStationTask(StationType.Fridge);
        return true;
    }

    private void CrossOff(string taskName)
    {
        if (receiptText == null)
        {
            Debug.LogWarning("Receipt text is missing.");
            return;
        }

        string[] lines = receiptText.text.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string trimmedLine = lines[i].Trim();

            if (trimmedLine == "- " + taskName)
            {
                lines[i] = "<color=red><s>" + trimmedLine + "</s></color>";
                receiptText.text = string.Join("\n", lines);
                Debug.Log("Crossed off: " + taskName);
                return;
            }
        }

        Debug.LogWarning("Task not found: - " + taskName);
        Debug.LogWarning("Receipt text was:\n" + receiptText.text);
    }

    public void SetHighlighted(bool highlighted)
    {
        if (receiptText == null)
            return;

        if (!hasOriginalTextColor)
        {
            originalTextColor = receiptText.color;
            hasOriginalTextColor = true;
        }

        receiptText.color = highlighted ? new Color(0.3f, 0.3f, 0.3f, 1f) : originalTextColor;
    }

    private void RemoveReceipt()
    {
        if (OrderManager.Instance != null)
            OrderManager.Instance.CompleteReceipt(this);
    }

    public void ResetOrderProgress()
    {
        orderReceived = false;
        washedHands = false;
        grabbedIngredients = false;
        choppedOrStretched = false;
        addedToppings = false;
        cookedOrBaked = false;
        cutPizza = false;
        served = false;
        finishedOrder = false;
        foodBurned = false;

        if (heldFoodVisuals != null)
            heldFoodVisuals.HideAllFood();

        if (receiptText != null)
        {
            receiptText.text = receiptText.text.Replace("<color=red><s>", "");
            receiptText.text = receiptText.text.Replace("</s></color>", "");
            receiptText.text = receiptText.text.Replace("<s>", "");
            receiptText.text = receiptText.text.Replace("</s>", "");
        }
    }

    public bool CompletePotatoChopping()
    {
        if (orderType != OrderType.FrenchFries)
            return false;

        if (!grabbedIngredients || choppedOrStretched)
            return false;

        choppedOrStretched = true;
        CrossOff("Chop Potato");
        Debug.Log("Potato chopping complete from minigame");
        return true;
    }

    public bool CompletePotatoMixing()
    {
        if (orderType != OrderType.FrenchFries)
            return false;

        if (!choppedOrStretched || addedToppings)
            return false;

        addedToppings = true;
        CrossOff("Oil & Salt Potato");
        Debug.Log("Potato oil and salt complete from mix minigame");
        return true;
    }

    public bool CompleteFriesFrying(bool burned)
    {
        if (orderType != OrderType.FrenchFries)
            return false;

        if (!addedToppings || cookedOrBaked)
            return false;

        cookedOrBaked = true;
        foodBurned = burned;
        CrossOff("Cook Fries");

        Debug.Log(burned ? "Fries burned in frying minigame" : "Fries cooked in frying minigame");
        return true;
    }

    public bool CompletePizzaPrep()
    {
        bool isPepperoni = orderType == OrderType.PepperoniPizza;
        bool isPizza = orderType == OrderType.CheesePizza || isPepperoni;

        if (!isPizza)
            return false;

        if (!grabbedIngredients || addedToppings)
            return false;

        choppedOrStretched = true;
        addedToppings = true;

        if (isPepperoni)
        {
            CrossOff("Stretch Dough, Sauce Dough & Top with Cheese + Pepperoni");
        }
        else
        {
            CrossOff("Stretch Dough, Sauce Dough & Top with Cheese");
        }

        Debug.Log(orderType + " prep complete from pizza scene");
        return true;
    }

    public StationType? GetNextStation()
    {
        if (finishedOrder)
            return null;

        if (!orderReceived)
            return StationType.Cashier;

        if (!washedHands)
            return StationType.Sink;

        if (!grabbedIngredients)
            return StationType.Fridge;

        if (orderType == OrderType.CheesePizza || orderType == OrderType.PepperoniPizza)
        {
            if (!addedToppings)
                return StationType.ChoppingBlock;

            if (!cookedOrBaked)
                return StationType.Oven;

            if (!cutPizza)
                return StationType.ChoppingBlock;

            if (!served)
                return StationType.DropOff;
        }

        if (orderType == OrderType.FrenchFries)
        {
            if (!choppedOrStretched)
                return StationType.ChoppingBlock;

            if (!addedToppings)
                return StationType.ToppingsTable;

            if (!cookedOrBaked)
                return StationType.Stove;

            if (foodBurned)
                return StationType.TrashCan;

            if (!served)
                return StationType.DropOff;
        }

        return null;
    }
}
