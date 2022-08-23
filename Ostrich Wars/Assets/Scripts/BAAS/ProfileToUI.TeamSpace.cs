using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class ProfileToUI
{
    private class TeamSpace : MonoBehaviour, IPointerClickHandler
    {
        private UnitSO currentUnit;
        public UnitSO CurrentUnit
        {
            get => currentUnit; 
            set
            {
                currentUnit = value;
                if(CurrentUnitrPortrait == null)
                {
                    CurrentUnitrPortrait = transform.GetChild(0).gameObject.GetComponent<Image>();
                }


                if (CurrentUnit != null)
                {
                    CurrentUnitrPortrait.sprite = CurrentUnit.Portrait;
                    CurrentUnitrPortrait.gameObject.SetActive(true);
                }
            }
        }
        private Image CurrentUnitrPortrait;
        private int spaceOnTeam;
        public int SpaceOnTeam
        {
            get => spaceOnTeam; 
            set
            {
               spaceOnTeam = value; 
               CurrentUnit = instance.profile.CurrentTeam.Count > spaceOnTeam ? instance.profile.CurrentTeam[spaceOnTeam] : null;
            }
        }

        public void AssignUnitToSpace()
        {
            //add new unit to the space
            CurrentUnit = instance.Selected as UnitSO;
            CurrentUnitrPortrait.sprite = currentUnit.Portrait;
            instance.profile.AddToTeam(CurrentUnit, SpaceOnTeam);
            instance.profile.UnitInventory.Remove(currentUnit);
            CurrentUnitrPortrait.gameObject.SetActive(true);
        }

        public void RemoveFromSpace()
        {
            instance.profile.RemoveFromTeam(spaceOnTeam);
            instance.profile.UnitInventory.Add(currentUnit);
            instance.CreateNewClickableCard(currentUnit); 
            CurrentUnitrPortrait.sprite = null;
            CurrentUnitrPortrait.gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            instance.Selected = CurrentUnit; 
        }
    }
}
