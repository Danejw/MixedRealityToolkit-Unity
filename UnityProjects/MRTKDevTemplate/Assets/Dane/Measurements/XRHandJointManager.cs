using MixedReality.Toolkit.Subsystems;
using MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;

namespace ClearView
{
    public class XRHandJointManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField, Tooltip("Log debug information.")]
        private bool debugLogging = false;

        private HandsAggregatorSubsystem handsAggregator;
        private Dictionary<string, Transform> jointDictionary = new Dictionary<string, Transform>();

        public static XRHandJointManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            StartCoroutine(EnableWhenSubsystemAvailable());
        }

        private IEnumerator EnableWhenSubsystemAvailable()
        {
            yield return new WaitUntil(() => XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>() != null);
            handsAggregator = XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>();

            if (handsAggregator != null)
            {
                Logger.Log(Logger.Category.Info, "HandsAggregatorSubsystem found and initialized.");
                InitializeJoints();
            }
            else
            {
                Logger.Log(Logger.Category.Error, "Failed to find HandsAggregatorSubsystem.");
            }
        }

        private void InitializeJoints()
        {
            foreach (TrackedHandJoint joint in System.Enum.GetValues(typeof(TrackedHandJoint)))
            {
                // Create a GameObject to represent the joint
                GameObject jointObject = new GameObject(joint.ToString());
                jointObject.transform.parent = transform;
                jointDictionary[joint.ToString()] = jointObject.transform;

                if (debugLogging)
                {
                    Logger.Log(Logger.Category.Info, $"Joint {joint} initialized.");
                }
            }
        }

        private void Update()
        {
            UpdateJoints(XRNode.LeftHand);
            UpdateJoints(XRNode.RightHand);
        }

        private void UpdateJoints(XRNode handNode)
        {
            if (handsAggregator == null) return;

            if (handsAggregator.TryGetEntireHand(handNode, out IReadOnlyList<HandJointPose> jointPoses))
            {
                for (int i = 0; i < jointPoses.Count; i++)
                {
                    string jointName = ((TrackedHandJoint)i).ToString() + $"{handNode}";
                    if (jointDictionary.ContainsKey(jointName))
                    {
                        Transform jointTransform = jointDictionary[jointName];
                        jointTransform.position = jointPoses[i].Pose.position;
                        jointTransform.rotation = jointPoses[i].Pose.rotation;
                    }
                }
            }
        }

        public Transform GetJointTransform(string jointName)
        {
            if (jointDictionary.TryGetValue(jointName, out Transform jointTransform))
            {
                return jointTransform;
            }

            Logger.Log(Logger.Category.Warning, $"Joint {jointName} not found.");
            return null;
        }

        public Dictionary<string, Transform> GetJoints()
        {
            return jointDictionary;
        }

        public void AttachObjectToJoint(string jointName, GameObject objToAttach)
        {
            Transform jointTransform = GetJointTransform(jointName);

            if (jointTransform != null)
            {
                objToAttach.transform.SetParent(jointTransform);
                objToAttach.transform.localPosition = Vector3.zero;
                objToAttach.transform.localRotation = Quaternion.identity;
            }
        }

        // Detach object from joint
        public void DetachObjectFromJoint(string jointName, GameObject objToDetach)
        {
            Transform jointTransform = GetJointTransform(jointName);

            if (jointTransform != null)
            {
                objToDetach.transform.SetParent(null);
            }
        }
    }
}
