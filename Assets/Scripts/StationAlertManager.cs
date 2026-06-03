using UnityEngine;

public class StationAlertManager : MonoBehaviour
{
    public GameObject cashierAlert;
    public GameObject sinkAlert;
    public GameObject fridgeAlert;
    public GameObject choppingAlert;
    public GameObject toppingAlert;
    public GameObject ovenAlert;
    public GameObject stoveAlert;
    public GameObject dropOffAlert;
    public GameObject trashAlert;

    private ReceiptOrder activeOrder;
    private StationType? lastStation;

    private void Update()
    {
        activeOrder = OrderManager.GlobalActiveReceipt;

        StationType? nextStation = null;

        if (activeOrder != null)
            nextStation = activeOrder.GetNextStation();

        if (nextStation == StationType.Cashier && !CustomerReachedRegister())
            nextStation = null;

        if (nextStation == lastStation)
            return;

        lastStation = nextStation;
        ShowOnlyAlert(nextStation);
    }

    private bool CustomerReachedRegister()
    {
        if (OrderManager.Instance == null)
            return false;

        if (OrderManager.Instance.customerObject == null)
            return false;

        CustomerWalker walker =
            OrderManager.Instance.customerObject.GetComponent<CustomerWalker>();

        return walker != null && walker.reachedRegister;
    }

    private void ShowOnlyAlert(StationType? station)
    {
        cashierAlert.SetActive(station == StationType.Cashier);
        sinkAlert.SetActive(station == StationType.Sink);
        fridgeAlert.SetActive(station == StationType.Fridge);
        choppingAlert.SetActive(station == StationType.ChoppingBlock);
        toppingAlert.SetActive(station == StationType.ToppingsTable);
        ovenAlert.SetActive(station == StationType.Oven);
        stoveAlert.SetActive(station == StationType.Stove);
        dropOffAlert.SetActive(station == StationType.DropOff);
        trashAlert.SetActive(station == StationType.TrashCan);
    }
}