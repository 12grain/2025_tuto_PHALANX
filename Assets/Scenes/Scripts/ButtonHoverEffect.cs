using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject highlightPanel; // ⬅ 배경 Image 연결할 필드

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlightPanel.SetActive(true); // 마우스 올리면 켜짐
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlightPanel.SetActive(false); // 마우스 나가면 꺼짐
    }
}
