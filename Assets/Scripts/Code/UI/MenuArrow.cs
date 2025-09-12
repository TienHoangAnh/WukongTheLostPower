using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuArrow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image arrowLeft, arrowRight;

    void Awake() { Show(false); }               // ẩn mặc định
    public void OnPointerEnter(PointerEventData e) { Show(true); }
    public void OnPointerExit(PointerEventData e) { Show(false); }

    void Show(bool on)
    {
        if (arrowLeft) arrowLeft.enabled = on;
        if (arrowRight) arrowRight.enabled = on;
    }
}
