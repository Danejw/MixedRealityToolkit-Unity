using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace ClearView.UI
{
    public class OnlineModelUIItem : MonoBehaviour
    {
        public TMP_Text tMP_Text;


        public void SetText(string text)
        {
            if (!tMP_Text) return;

            tMP_Text.text = text;
        }
    }
}
