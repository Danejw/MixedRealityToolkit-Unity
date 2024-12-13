using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit.UX;

namespace ClearView
{
    public class SharedMaterialUI : SharedMaterialController
    {
        public Slider xSlider;
        public Slider ySlider;
        public Slider zSlider;

        public Slider spreadSlider;


        private void Start()
        {
            base.Start();


            xSlider?.OnValueUpdated.AddListener(UpdateX);
            ySlider?.OnValueUpdated.AddListener(UpdateY);
            zSlider?.OnValueUpdated.AddListener(UpdateZ);
            spreadSlider?.OnValueUpdated.AddListener(UpdateSpread);
        }

        private void UpdateX(SliderEventData eventData)
        {
            x = eventData.NewValue;
        }

        private void UpdateY(SliderEventData eventData)
        {
            y = eventData.NewValue;
        }

        private void UpdateZ(SliderEventData eventData)
        {
            z = eventData.NewValue;
        }

        private void UpdateSpread(SliderEventData eventData)
        {
            spread = eventData.NewValue;
        }

        private void OnDestroy()
        {
            xSlider?.OnValueUpdated.RemoveListener(UpdateX);
            ySlider?.OnValueUpdated.RemoveListener(UpdateY);
            zSlider?.OnValueUpdated.RemoveListener(UpdateZ);
            spreadSlider?.OnValueUpdated.RemoveListener(UpdateSpread);
        }
    }
}