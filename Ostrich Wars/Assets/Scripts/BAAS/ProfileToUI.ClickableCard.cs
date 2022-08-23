using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public partial class ProfileToUI
{
    private class ClickableCard : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public InventoryObject AssignedSO;
        private Transform Inventory;

        private void Awake()
        {
            Inventory = transform.parent;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            transform.parent = null; 
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = Mouse.current.position.ReadDefaultValue(); 
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            //if dragging into teamspace use the click and if
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 1)
            {
                instance.Selected = AssignedSO;
                CardSelected = gameObject; 
            }
        }
    }
}
