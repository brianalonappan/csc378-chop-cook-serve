using UnityEngine;

public class PepperoniOvenSceneSetup : MonoBehaviour
{
    public GameObject pepperoniSlicePrefab;
    public Transform wholePizza;
    public GameObject sauceOnDough;
    public GameObject cheeseOnDough;

    private void Start()
    {
        if (wholePizza == null)
        {
            Debug.LogError("PepperoniOvenSceneSetup: WholePizza is not assigned.");
            return;
        }

        wholePizza.gameObject.SetActive(true);

        if (pepperoniSlicePrefab == null)
        {
            Debug.LogError("PepperoniOvenSceneSetup: PepperoniSlicePrefab is not assigned.");
            return;
        }

        ApplySavedPizzaLayers();
        RebuildPepperoniFromState();
    }

    private void ApplySavedPizzaLayers()
    {
        if (sauceOnDough != null)
            sauceOnDough.SetActive(PizzaState.sauceAdded);

        if (cheeseOnDough != null)
            cheeseOnDough.SetActive(PizzaState.cheeseAdded);
    }

    private void RebuildPepperoniFromState()
    {
        for (int i = 0; i < PizzaState.pepperoniLocalPositions.Count; i++)
        {
            Vector3 localPos = PizzaState.pepperoniLocalPositions[i];
            Vector3 worldPos = wholePizza.TransformPoint(localPos);
            worldPos.z = 0f;

            Quaternion localRotation = i < PizzaState.pepperoniLocalRotations.Count
                ? PizzaState.pepperoniLocalRotations[i]
                : Quaternion.identity;

            GameObject slice = Instantiate(pepperoniSlicePrefab, worldPos, wholePizza.rotation * localRotation);
            slice.transform.SetParent(wholePizza, true);
        }

        Debug.Log("Rebuilt pepperoni slices in oven scene: " + PizzaState.pepperoniLocalPositions.Count);
    }
}
