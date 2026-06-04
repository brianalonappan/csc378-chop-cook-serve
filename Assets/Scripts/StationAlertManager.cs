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
        SetAlert(cashierAlert, station == StationType.Cashier);
        SetAlert(sinkAlert, station == StationType.Sink);
        SetAlert(fridgeAlert, station == StationType.Fridge);
        SetAlert(choppingAlert, station == StationType.ChoppingBlock);
        SetAlert(toppingAlert, station == StationType.ToppingsTable);
        SetAlert(ovenAlert, station == StationType.Oven);
        SetAlert(stoveAlert, station == StationType.Stove);
        SetAlert(dropOffAlert, station == StationType.DropOff);
        SetAlert(trashAlert, station == StationType.TrashCan);
    }

    private void SetAlert(GameObject alert, bool active)
    {
        if (alert != null)
            alert.SetActive(active);
    }
}
