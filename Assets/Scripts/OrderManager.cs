using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance;

    public ReceiptOrder[] receiptPrefabs;
    public Transform[] receiptSlots;

    public float spawnInterval = 20f;
    public int maxReceiptsOnScreen = 3;

    private List<ReceiptOrder> receiptQueue =
        new List<ReceiptOrder>();

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
        int randomIndex =
            Random.Range(0, receiptPrefabs.Length);

        ReceiptOrder newReceipt =
            Instantiate(
                receiptPrefabs[randomIndex],
                receiptSlots[receiptQueue.Count].position,
                Quaternion.identity
            );

        receiptQueue.Add(newReceipt);

        UpdateReceiptPositions();
    }

    public void TryUseStation(StationType stationType)
    {
        if (receiptQueue.Count == 0)
        {
            Debug.Log("No receipts in queue.");
            return;
        }

        ReceiptOrder activeReceipt =
            receiptQueue[0];

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

        Destroy(completedReceipt.gameObject);

        UpdateReceiptPositions();
    }

    private void UpdateReceiptPositions()
    {
        for (int i = 0; i < receiptQueue.Count; i++)
        {
            receiptQueue[i].transform.position =
                receiptSlots[i].position;
        }
    }
}