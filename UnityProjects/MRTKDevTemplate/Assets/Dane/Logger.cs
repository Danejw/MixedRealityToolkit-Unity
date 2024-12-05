using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClearView
{
    public static class Logger
    {
        public enum Category
        {
            Info,
            Warning,
            Error
        }


        // Define an event
        public static event Action<Category, string> OnLog;

        // Method to log a message with a category
        public static void Log(Category category, string message)
        {
            // Trigger the event if there are any subscribers
            OnLog?.Invoke(category, message);

            Debug.Log($"[{category}] : {message}");
        }
    }
}