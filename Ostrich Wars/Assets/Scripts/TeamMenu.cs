using UnityEngine;

public partial class ProfileToUI
{
    public class TeamMenu : MonoBehaviour
    {
        private void OnEnable()
        {

            instance.FillUnitContentBox(instance.profile.UnitInventory);
            instance.Selected = instance.profile.UnitInventory[0];

            instance.S1.SpaceOnTeam = 0;
            instance.S2.SpaceOnTeam = 1;
            instance.S3.SpaceOnTeam = 2;

        }

        private void OnDisable()
        {
            instance.ClearContentBox();
            GameManager.instance.Player.SaveProfile();

        }
    }
}
