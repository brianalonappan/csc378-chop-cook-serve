using TMPro;
using UnityEngine;

public class PizzaStepInstruction : MonoBehaviour
{
    public TMP_Text instructionText;

    public Transform dough;
    public float doughStretchThreshold = 2.0f;

    public GameObject sauceOnDough;
    public GameObject cheeseOnDough;

    public bool isPepperoniScene = false;

    private void Update()
    {
        if (instructionText == null)
            return;

        bool doughDone = dough != null && dough.localScale.x >= doughStretchThreshold;
        bool sauceAdded = sauceOnDough != null && sauceOnDough.activeSelf;
        bool cheeseAdded = cheeseOnDough != null && cheeseOnDough.activeSelf;

        if (!doughDone)
        {
            instructionText.text = "Click and drag your mouse to stretch the dough";
        }
        else if (!sauceAdded)
        {
            instructionText.text = "Click and drag the sauce onto the pizza";
        }
        else if (!cheeseAdded)
        {
            instructionText.text = "Click and drag the cheese over the pizza";
        }
        else if (isPepperoniScene)
        {
            instructionText.text = "Click and drag pepperoni onto the pizza";
        }
        else
        {
            instructionText.text = "";
        }
    }
}