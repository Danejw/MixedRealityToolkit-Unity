using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;

namespace ClearView
{
    /// <summary>
    /// Represents a group of points for a measurement.
    /// </summary>
    [System.Serializable]
    public class Measurement
    {
        public GameObject parent;
        public List<GameObject> points;
        public List<GameObject> distanceTexts;
        public LineRenderer lineRenderer;
    }
}