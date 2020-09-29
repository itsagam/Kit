﻿using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Menu
{
    public class MenuWindow : MonoBehaviour
    {
        public Text TitleText;
        public GameObject ModButton;

        protected void Awake()
        {
            if (TitleText != null)
                TitleText.text = Application.productName;

            #if !MODDING
                ModButton.gameObject.SetActive(false);
            #endif
        }
    }
}