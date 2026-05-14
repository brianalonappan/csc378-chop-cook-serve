using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance;

    public ReceiptOrder[] receiptPrefabs;
    public Transform receiptSpawnParent; // drag ReceiptLine here

    public float spawnInterval = 20f;
    public int maxReceiptsOnScreen = 3;

    private List<ReceiptOrder> receiptQueue = new List<ReceiptOrder>();
    private ReceiptOrder activeReceipt;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(SpawnReceiptRoutine());
    }

    private IEnumerator SpawnReceiptRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (receiptQueue.Count < maxReceiptsOnScreen)
            {
                SpawnRandomReceipt();
            }
        }
    }

    private void SpawnRandomReceipt()
    {
        int randomIndex = Random.Range(0, receiptPrefabs.Length);

        ReceiptOrder newReceipt = Instantiate(
            receiptPrefabs[randomIndex],
            receiptSpawnParent
        );

        RectTransform rect = newReceipt.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one * 0.6f;

        receiptQueue.Add(newReceipt);

        if (activeReceipt == null)
        {
            activeReceipt = newReceipt;
        }
    }

    public void SetActiveReceipt(ReceiptOrder receipt)
    {
        activeReceipt = receipt;
        Debug.Log(receipt.orderType + " is now active.");
    }

    public void TryUseStation(StationType stationType)
    {
        if (activeReceipt == null)
        {
            Debug.Log("No active receipt.");
            return;
        }

        bool completedSomething =
            activeReceipt.TryCompleteStationTask(stationType);

        if (completedSomething)
        {
            Debug.Log("Worked on active receipt.");
        }
        else
        {
            Debug.Log("Active receipt does not need this station right now.");
        }
    }

    public void CompleteReceipt(ReceiptOrder completedReceipt)
    {
        if (receiptQueue.Contains(completedReceipt))
        {
            receiptQueue.Remove(completedReceipt);
        }

        if (activeReceipt == completedReceipt)
        {
            activeReceipt = receiptQueue.Count > 0 ? receiptQueue[0] : null;
        }

        Destroy(completedReceipt.gameObject);
    }
}