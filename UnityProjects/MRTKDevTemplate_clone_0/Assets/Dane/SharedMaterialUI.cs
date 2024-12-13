using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit.UX;

namespace ClearView
{
    public class SharedMaterialUI : SharedMaterialController
    {
        public Slider xSlider;



        private void Start()
        {
            base.Start();

            xSlider?.OnValueUpdated.AddListener(UpdateX);
        }

        private void UpdateX(SliderEventData eventData)
        {
            x = eventData.NewValue;

            Debug.Log("X: " + x);
        }


        private void OnDestroy()
        {
            xSlider?.OnValueUpdated.RemoveListener(UpdateX);
        }
    }
}