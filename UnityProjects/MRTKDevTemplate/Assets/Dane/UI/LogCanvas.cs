using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClearView;
using TMPro;

namespace ClearView.UI
{
    public class LogCanvas : MonoBehaviour
    {
        public TMP_Text info;

        private void Start()
        {
            Logger.OnLog += OnLog;
        }

        private void OnLog(Logger.Category category, string message)
        {
            if (category == Logger.Category.Info)
            {
                if (info != null)
                {
                    info.text = $"{message}";
                }
            }
        }
    }
}
