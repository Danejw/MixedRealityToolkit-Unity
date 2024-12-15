using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Microsoft.MixedReality.OpenXR;
using MixedReality.Toolkit;
using UnityEngine.XR;
using MixedReality.Toolkit.Subsystems;
using UnityEngine.UIElements;
using System.Collections;
using UnityEngine.XR.OpenXR.Input;
using MixedReality.Toolkit.UX;



namespace ClearView
{
    public class MeasurementManager : MonoBehaviour
    {
        [Header("Prefabs and References")]
        [SerializeField] private GameObject pointPrefab;
        [SerializeField] private GameObject distanceTextPrefab;
        [SerializeField] private GameObject lineRendererPrefab;
        [SerializeField] private GameObject deleteButtonPrefab;

        [SerializeField] private GameObject measuringTip;
        [SerializeField] private GameObject thumbTip;

        [Header("Settings")]
        [SerializeField] private Vector3 textOffset = new Vector3(0, 0.05f, 0);

        [SerializeField] private List<Measurement> measurements = new List<Measurement>();
        [SerializeField] private Measurement currentMeasurement;
        [SerializeField] private bool isMeasuring;

        private HandsAggregatorSubsystem aggregator;

        [SerializeField] private List<GameObject> handJoints = new List<GameObject>();

        private void Start()
        {
            measurements.Clear();


        }

        private void Update()
        {
            UdateMeasuringTipPosition();

            if (isMeasuring && currentMeasurement != null)
            {
                UpdateLineRenderer();
                UpdateDistanceTexts();
            }

        }

        IEnumerator EnableWhenSubsystemAvailable()
        {
            yield return new WaitUntil(() => XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>() != null);
            aggregator = XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>();

            // TODO: Setup...
        }



        private void UdateMeasuringTipPosition()
        {
            if (aggregator == null) return;

            // Left Hand
            bool jointIsValid = aggregator.TryGetJoint(TrackedHandJoint.IndexTip, XRNode.LeftHand, out HandJointPose jointPose);
            bool thumbIsValid = aggregator.TryGetJoint(TrackedHandJoint.ThumbTip, XRNode.LeftHand, out HandJointPose thumbPose);

            // Right Hand
            bool jointIsValidRight = aggregator.TryGetJoint(TrackedHandJoint.IndexTip, XRNode.RightHand, out HandJointPose jointPoseRight);
            bool thumbIsValidRight = aggregator.TryGetJoint(TrackedHandJoint.ThumbTip, XRNode.RightHand, out HandJointPose thumbPoseRight);


            if (measuringTip != null && jointIsValidRight)
            {
                measuringTip.transform.position = jointPoseRight.Pose.position;
                measuringTip.transform.rotation = jointPoseRight.Pose.rotation;
            }

            if (thumbTip != null && thumbIsValidRight)
            {
                thumbTip.transform.position = thumbPoseRight.Pose.position;
                thumbTip.transform.rotation = thumbPoseRight.Pose.rotation;
            }
        }

        public void StartMeasuringAtFingertip()
        {
            if (!isMeasuring)
            {

                GameObject measurementParent = new GameObject("Measurement");
                measurementParent.transform.parent = transform;
                currentMeasurement = new Measurement
                {
                    parent = measurementParent,
                    points = new List<GameObject>(),
                    distanceTexts = new List<GameObject>()
                };

                GameObject lineRendererObject = Instantiate(lineRendererPrefab, measurementParent.transform);
                lineRendererObject.name = "LineRenderer";
                currentMeasurement.lineRenderer = lineRendererObject.GetComponent<LineRenderer>();

                // Set the initial positions
                currentMeasurement.lineRenderer.positionCount = 2;
                Vector3 measuringTipPosition = measuringTip.transform.position;
                currentMeasurement.lineRenderer.SetPosition(0, measuringTipPosition);

                currentMeasurement.lineRenderer.SetPosition(1, measuringTip.transform.position);

                measurements.Add(currentMeasurement);
            }

            AddPointToMeasurement();
            isMeasuring = true;
        }


        public void AddPointToMeasurement()
        {
            if (currentMeasurement == null) return;

            GameObject newPoint = Instantiate(pointPrefab, measuringTip.transform.position, Quaternion.identity, currentMeasurement.parent.transform);
            newPoint.name = "Point";
            currentMeasurement.points.Add(newPoint);

            GameObject distanceText = Instantiate(distanceTextPrefab, measuringTip.transform.position + textOffset, Quaternion.identity, currentMeasurement.parent.transform);
            distanceText.name = "Distance";

            // Add a delete button and childs it to the distance text
            GameObject deleteButton = Instantiate(deleteButtonPrefab, Vector3.zero, Quaternion.identity, distanceText.transform);
            deleteButton.transform.localPosition = Vector3.zero;
            deleteButton.name = "Delete";
            bool isButton = deleteButton.TryGetComponent<PressableButton>(out PressableButton buttonComponent);
            if (isButton) buttonComponent.OnClicked?.AddListener(() => DeleteMeasurement(currentMeasurement));

            currentMeasurement.distanceTexts.Add(distanceText);

            UpdateLineRenderer();
        }


        private void DeleteMeasurement(Measurement measurement)
        {
            if (measurement == null || !measurements.Contains(measurement)) return;

            foreach (var point in measurement.points)
            {
                Destroy(point);
            }

            foreach (var text in measurement.distanceTexts)
            {
                Destroy(text);
            }

            Destroy(measurement.lineRenderer.gameObject);
            Destroy(measurement.parent);

            measurements.Remove(measurement);

            currentMeasurement = null;
            isMeasuring = false;
        }

        private void UpdateLineRenderer()
        {
            if (currentMeasurement == null || currentMeasurement.lineRenderer == null) return;

            int pointCount = currentMeasurement.points.Count;
            currentMeasurement.lineRenderer.positionCount = pointCount + 1;

            for (int i = 0; i < pointCount; i++)
            {
                currentMeasurement.lineRenderer.SetPosition(i, currentMeasurement.points[i].transform.position);
            }

            // Set the last position to follow the measuringTip
            currentMeasurement.lineRenderer.SetPosition(pointCount, measuringTip.transform.position);
        }



        private void UpdateDistanceTexts()
        {
            if (currentMeasurement == null || currentMeasurement.points == null || currentMeasurement.distanceTexts == null) return;

            for (int i = 0; i < currentMeasurement.points.Count; i++)
            {
                if (i >= currentMeasurement.distanceTexts.Count) break; // Prevent out-of-bounds errors

                Vector3 start = currentMeasurement.points[i].transform.position;
                Vector3 end;

                // If it's the last segment, measure to the measuringTip
                if (i == currentMeasurement.points.Count - 1)
                {
                    end = measuringTip.transform.position;
                }
                else
                {
                    end = currentMeasurement.points[i + 1].transform.position;
                }

                Vector3 center = (start + end) / 2 + textOffset;
                float distance = Vector3.Distance(start, end);

                // Update the correct distance text
                GameObject distanceText = currentMeasurement.distanceTexts[i];
                if (distanceText != null)
                {
                    distanceText.transform.position = center;

                    TMP_Text textComponent = distanceText.GetComponentInChildren<TMP_Text>();
                    if (textComponent != null)
                    {
                        textComponent.text = $"{distance:F2} m";
                    }
                }
            }
        }



        public void ClearMeasurements()
        {
            foreach (var measurement in measurements)
            {
                DeleteMeasurement(measurement);
            }
            measurements.Clear();
            currentMeasurement = null;
            isMeasuring = false;
        }

        public void StopMeasuringAtFingertip()
        {
            if (!isMeasuring) return;

            isMeasuring = false;
        }

    }
}
