using MixedReality.Toolkit.UX.Experimental;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace ClearView
{
    public class KeyboardInput : MonoBehaviour
    {
        public TMP_InputField inputField;
        public NonNativeKeyboard keyboard;

        private void OnEnable()
        {
            keyboard.OnTextUpdate.AddListener(OnTextUpdated);
            inputField.onSelect.AddListener(OnSelect);
            inputField.onDeselect.AddListener(OnDeselect);
        }

        private void OnDeselect(string arg0)
        {
            keyboard.gameObject.SetActive(false);
        }

        private void OnSelect(string arg0)
        {
            keyboard.gameObject.SetActive(true);
        }

        private void OnTextUpdated(string text)
        {
            inputField.text = text;
        }
    }
}
