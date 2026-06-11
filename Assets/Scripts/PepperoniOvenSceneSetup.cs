using UnityEngine;

public class PepperoniOvenSceneSetup : MonoBehaviour
{
    public GameObject pepperoniSlicePrefab;
    public Transform wholePizza;

    private void Start()
    {
        if (wholePizza == null)
        {
            Debug.LogError("PepperoniOvenSceneSetup: WholePizza is not assigned.");
            return;
        }

        if (pepperoniSlicePrefab == null)
        {
            Debug.LogError("PepperoniOvenSceneSetup: PepperoniSlicePrefab is not assigned.");
            return;
        }

        RebuildPepperoniFromState();
    }

    private void RebuildPepperoniFromState()
    {
        for (int i = 0; i < PizzaState.pepperoniLocalPositions.Count; i++)
        {
            Vector3 localPos = PizzaState.pepperoniLocalPositions[i];
            Vector3 worldPos = wholePizza.TransformPoint(localPos);
            worldPos.z = 0f;

            GameObject slice = Instantiate(pepperoniSlicePrefab, worldPos, Quaternion.identity);
            slice.transform.SetParent(wholePizza, true);
        }

        Debug.Log("Rebuilt pepperoni slices in oven scene: " + PizzaState.pepperoniLocalPositions.Count);
    }
}