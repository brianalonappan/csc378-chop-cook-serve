using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BowlController : MonoBehaviour
{
    public float mixProgress;
    public bool saltAdded;
    public bool pepperAdded;

    [SerializeField] private SpriteRenderer bowlRenderer;
    [SerializeField] private Sprite[] mixStages;
    [SerializeField] private Color mixedPotatoColor = new Color(0.95f, 0.82f, 0.48f, 1f);
    [SerializeField] private Color saltParticleColor = Color.white;
    [SerializeField] private Color pepperParticleColor = Color.black;
    [SerializeField] private ParticleSystem ingredientParticles;
    [SerializeField] private Transform ingredientDropTarget;
    [SerializeField] private float ingredientDropRadius = 1.2f;
    [SerializeField] private string returnSceneName = "UpDown";
    [SerializeField] private float returnDelay = 1.2f;

    private bool mixingComplete;
    private Color startingPotatoColor = Color.white;

    private void Awake()
    {
        if (bowlRenderer == null)
            bowlRenderer = GetComponent<SpriteRenderer>();

        if (bowlRenderer != null)
            startingPotatoColor = bowlRenderer.color;

        if (ingredientDropTarget == null)
            ingredientDropTarget = transform;

        UpdateMixSprite();
    }

    public void OnIngredientAdded(string ingredient)
    {
        TryAddIngredient(ingredient, ingredientDropTarget.position);
    }

    public bool TryAddIngredient(string ingredient, Vector2 dropPosition)
    {
        if (string.IsNullOrEmpty(ingredient))
            return false;

        if (Vector2.Distance(dropPosition, ingredientDropTarget.position) > ingredientDropRadius)
            return false;

        string normalizedIngredient = ingredient.ToLowerInvariant();

        if (normalizedIngredient == "salt")
            saltAdded = true;

        if (normalizedIngredient == "pepper")
            pepperAdded = true;

        SpawnParticles(normalizedIngredient, dropPosition);
        return true;
    }

    public void OnStirComplete()
    {
        if (mixingComplete || (!saltAdded && !pepperAdded))
            return;

        mixProgress = Mathf.Clamp01(mixProgress + 0.25f);
        UpdateMixSprite();

        if (mixProgress >= 1f)
            StartCoroutine(FinishMixing());
    }

    private void UpdateMixSprite()
    {
        if (bowlRenderer == null)
            return;

        if (mixStages != null && mixStages.Length > 0 && mixStages[0] != null)
            bowlRenderer.sprite = mixStages[0];

        bowlRenderer.color = Color.Lerp(startingPotatoColor, mixedPotatoColor, mixProgress);
    }

    private void SpawnParticles(string ingredient, Vector2 dropPosition)
    {
        if (ingredientParticles != null)
        {
            ingredientParticles.transform.position = dropPosition;
            ApplyParticleColor(ingredientParticles, ingredient);
            ingredientParticles.Clear();
            ingredientParticles.Play();
            ingredientParticles.Emit(24);
            return;
        }

        ParticleSystem burst = CreateIngredientParticleBurst(ingredient, dropPosition);
        burst.Play();
        Destroy(burst.gameObject, 2f);
    }

    private ParticleSystem CreateIngredientParticleBurst(string ingredient, Vector2 dropPosition)
    {
        GameObject particleObject = new GameObject(ingredient + " Particles");
        particleObject.transform.position = new Vector3(dropPosition.x, dropPosition.y, -0.5f);

        ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();
        ParticleSystemRenderer particleRenderer = particleObject.GetComponent<ParticleSystemRenderer>();
        particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;

        if (bowlRenderer != null)
        {
            particleRenderer.sortingLayerID = bowlRenderer.sortingLayerID;
            particleRenderer.sortingOrder = bowlRenderer.sortingOrder + 10;

            if (bowlRenderer.sharedMaterial != null)
                particleRenderer.sharedMaterial = bowlRenderer.sharedMaterial;
        }

        ParticleSystem.MainModule main = particles.main;
        main.duration = 0.35f;
        main.startLifetime = 0.45f;
        main.startSpeed = 0.7f;
        main.startSize = 0.5f;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = ingredient == "pepper" ? pepperParticleColor : saltParticleColor;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 24)
        });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.8f;

        ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.y = new ParticleSystem.MinMaxCurve(-0.6f, -0.15f);

        return particles;
    }

    private void ApplyParticleColor(ParticleSystem particles, string ingredient)
    {
        ParticleSystem.MainModule main = particles.main;
        main.startColor = ingredient == "pepper" ? pepperParticleColor : saltParticleColor;
    }

    private IEnumerator FinishMixing()
    {
        mixingComplete = true;

        if (OrderManager.Instance != null)
        {
            if (!OrderManager.Instance.CompletePotatoMixingForActiveReceipt())
                Debug.Log("Potato mix finished, but the active receipt did not accept the topping step.");
        }

        yield return new WaitForSeconds(returnDelay);

        string sceneToLoad = OrderManager.Instance != null && !string.IsNullOrEmpty(OrderManager.Instance.kitchenSceneName)
            ? OrderManager.Instance.kitchenSceneName
            : returnSceneName;

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogError("Cannot load scene: " + sceneToLoad + ". Check that it is added to Build Settings and the name is correct.");
            yield break;
        }

        if (OrderManager.Instance != null)
            OrderManager.Instance.PrepareForSceneLoad();

        SceneManager.LoadScene(sceneToLoad);
    }
}
