using UnityEngine;
using UnityEngine.EventSystems;

public class MenuOptionButton : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    [SerializeField] SelectionMenuController menu;
    [SerializeField] int index;

    public void Configure(SelectionMenuController menuController, int optionIndex)
    {
        menu = menuController;
        index = optionIndex;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (menu != null)
            menu.Select(index);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
