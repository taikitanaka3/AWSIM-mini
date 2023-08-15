using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ROS2;

namespace AWSIM
{
    /// <summary>
    /// Convert the data output from ObjectSensor to ROS2 msg and Publish.
    /// </summary>
    [RequireComponent(typeof(ObjectSensor))]
    public class ObjectRos2Publisher : MonoBehaviour
    {
        /// <summary>
        /// Topic name in pose msg.
        /// </summary>
        public string objectTopic = "/awsim/ground_truth/perception/object_recognition/objects";

        /// <summary>
        /// Object sensor frame id.
        /// </summary>
        public string frameId = "base_link";

        /// <summary>
        /// QoS settings.
        /// </summary>
        public QoSSettings qosSettings = new QoSSettings()
        {
            ReliabilityPolicy = ReliabilityPolicy.QOS_POLICY_RELIABILITY_BEST_EFFORT,
            DurabilityPolicy = DurabilityPolicy.QOS_POLICY_DURABILITY_VOLATILE,
            HistoryPolicy = HistoryPolicy.QOS_POLICY_HISTORY_KEEP_LAST,
            Depth = 1,
        };

        IPublisher<autoware_auto_perception_msgs.msg.DetectedObjects> objectPublisher;
        autoware_auto_perception_msgs.msg.DetectedObjects objectsMsg;
        ObjectSensor objectSensor;

        void Start()
        {
            // Get ObjectSensor component.
            objectSensor = GetComponent<ObjectSensor>();

            // Set callback.
            objectSensor.OnOutputData += Publish;

            // Create msg.
            objectsMsg = new autoware_auto_perception_msgs.msg.DetectedObjects();

            // Create publisher.
            var qos = qosSettings.GetQoSProfile();
            objectPublisher = SimulatorROS2Node.CreatePublisher<autoware_auto_perception_msgs.msg.DetectedObjects>(objectTopic, qos);
        }

        void Publish(ObjectSensor.OutputData outputData)
        {
            //var objectsList = new List<autoware_auto_perception_msgs.msg.DetectedObject>();
            //foreach (var objects in outputData.detectedObjects)
            //{
            //    var obj = new autoware_auto_perception_msgs.msg.DetectedObject();
            //    //obj.Existence_Probability = 1.0f;
            //    obj.Classification = new List<autoware_auto_perception_msgs.msg.ObjectClassification>().ToArray();
            //    obj.Kinematics = new autoware_auto_perception_msgs.msg.DetectedObjectKinematics();
            //    // obj.Kinematics.Pose_With_Covariance =
            //    // obj.Kinematics.initial_pose_with_covariance =
            //    // obj.Kinematics.initial_pose_with_covariance =
            //    obj.Shape = new autoware_auto_perception_msgs.msg.Shape();
            //    objectsList.Add(obj);
            //}
            // Converts data output from ObjectSensor to ROS2 msg
            objectsMsg.Objects = outputData.detectedObjects;
            // Update msg header.
            var header = objectsMsg as MessageWithHeader;
            SimulatorROS2Node.UpdateROSTimestamp(ref header);

            // Publish to ROS2.
            objectPublisher.Publish(objectsMsg);
        }

        void OnDestroy()
        {
           SimulatorROS2Node.RemovePublisher<autoware_auto_perception_msgs.msg.DetectedObjects>(objectPublisher);
        }
    }
}