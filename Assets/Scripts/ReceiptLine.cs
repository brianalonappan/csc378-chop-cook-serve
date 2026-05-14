using UnityEngine;
using UnityEngine.EventSystems;

public class ReceiptDropZone : MonoBehaviour, IDropHandler
{
    public Vector3 receiptScale = new Vector3(0.6f, 0.6f, 0.6f);

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;

        if (dropped != null)
        {
            dropped.transform.SetParent(transform, false);

            RectTransform rect = dropped.GetComponent<RectTransform>();
            rect.localScale = receiptScale;
            rect.anchoredPosition = Vector2.zero;
        }
    }
}