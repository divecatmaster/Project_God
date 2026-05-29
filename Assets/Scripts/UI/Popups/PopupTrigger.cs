using UnityEngine;

namespace DiveCat.God.UI.Popups
{
    public class PopupTrigger : MonoBehaviour
    {
        [SerializeField] private GenericPopup testPopup;

        public void OpenTestPopup()
        {
            if (testPopup != null)
            {
                testPopup.Setup(
                    "System Notification", 
                    "This is a production-ready popup system. Press ESC to close the most recent one.",
                    () => Debug.Log("Confirmed!"),
                    () => Debug.Log("Cancelled!")
                );
                testPopup.Open();
            }
        }
    }
}
