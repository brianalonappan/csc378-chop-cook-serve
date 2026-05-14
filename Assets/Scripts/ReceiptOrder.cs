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
    public TextMeshPro receiptText;

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

    public bool TryCompleteStationTask(StationType stationType)
    {
        if (stationType == StationType.TrashCan && foodBurned)
        {
            ResetOrderProgress();
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

        if (stationType == StationType.Sink &&
            orderReceived &&
            !washedHands)
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

        if (orderType == OrderType.CheesePizza ||
            orderType == OrderType.PepperoniPizza)
            return HandlePizza(stationType);

        return false;
    }

    private bool HandleFrenchFries(StationType stationType)
    {
        if (stationType == StationType.Fridge &&
            !grabbedIngredients)
        {
            grabbedIngredients = true;
            CrossOff("Grab Potato");
            return true;
        }

        if (stationType == StationType.ChoppingBlock &&
            grabbedIngredients &&
            !choppedOrStretched)
        {
            choppedOrStretched = true;
            CrossOff("Chop Potato");
            return true;
        }

        if (stationType == StationType.ToppingsTable &&
            choppedOrStretched &&
            !addedToppings)
        {
            addedToppings = true;
            CrossOff("Oil & Salt Potato");
            return true;
        }

        if (stationType == StationType.Stove &&
            addedToppings &&
            !cookedOrBaked)
        {
            cookedOrBaked = true;
            CrossOff("Cook Fries");
            return true;
        }

        if (stationType == StationType.DropOff &&
            cookedOrBaked &&
            !served)
        {
            served = true;
            finishedOrder = true;
            CrossOff("Serve Fries");
            Invoke("RemoveReceipt", 1f);
            Debug.Log("Fries complete");
            return true;
        }

        return false;
    }

    private bool HandlePizza(StationType stationType)
    {
        bool isPepperoni =
            orderType == OrderType.PepperoniPizza;

        if (stationType == StationType.Fridge &&
            !grabbedIngredients)
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
            !choppedOrStretched)
        {
            choppedOrStretched = true;
            CrossOff("Stretch Dough (on chopping block)");
            return true;
        }

        if (stationType == StationType.ToppingsTable &&
            choppedOrStretched &&
            !addedToppings)
        {
            addedToppings = true;

            if (isPepperoni)
                CrossOff("Sauce Dough & Top with Cheese + Pepperoni");
            else
                CrossOff("Sauce Dough & Top with Cheese");

            return true;
        }

        if (stationType == StationType.Oven &&
            addedToppings &&
            !cookedOrBaked)
        {
            cookedOrBaked = true;
            CrossOff("Place in Oven");
            return true;
        }

        if (stationType == StationType.ChoppingBlock &&
            cookedOrBaked &&
            !cutPizza)
        {
            cutPizza = true;
            CrossOff("Cut pizza into 8");
            return true;
        }

        if (stationType == StationType.DropOff &&
            cutPizza &&
            !served)
        {
            served = true;
            finishedOrder = true;
            CrossOff("Serve Pizza");
            Invoke("RemoveReceipt", 1f);
            Debug.Log(orderType + " complete");
            return true;
        }

        return false;
    }

    private void CrossOff(string taskName)
    {
        string normalText = "- " + taskName;
        string crossedText = "<s>- " + taskName + "</s>";

        if (receiptText.text.Contains(normalText))
        {
            receiptText.text =
                receiptText.text.Replace(normalText, crossedText);
        }
        else
        {
            Debug.LogWarning("Task not found: " + normalText);
        }
    }

    private void RemoveReceipt()
    {
        OrderManager.Instance.CompleteReceipt(this);
    }

    public void ResetOrderProgress()
    {
        grabbedIngredients = false;
        choppedOrStretched = false;
        addedToppings = false;
        cookedOrBaked = false;
        cutPizza = false;
        served = false;
        finishedOrder = false;
        foodBurned = false;

        receiptText.text = receiptText.text.Replace("<s>", "");
        receiptText.text = receiptText.text.Replace("</s>", "");
    }
}