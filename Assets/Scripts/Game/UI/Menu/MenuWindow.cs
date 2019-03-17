using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Menu
{
    public class MenuWindow : MonoBehaviour
    {
        public Text TitleText;

        protected void Awake()
        {
            if (TitleText != null)
                TitleText.text = Application.productName;
        }
    }
}