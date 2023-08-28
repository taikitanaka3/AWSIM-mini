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
        public string objectTopic = "/awsim/ground_truth/perception/object_recognition/detection/objects";

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
            var objectsList = new List<autoware_auto_perception_msgs.msg.DetectedObject>();
            foreach (var detectedObject in outputData.objects)
            {
                var rb = detectedObject.rigidBody;
                var dim = detectedObject.dimension;
                var obj = new autoware_auto_perception_msgs.msg.DetectedObject();
                obj.Existence_probability = 1.0f;
                // TODO(tanaka): add more classes
                var classification = new autoware_auto_perception_msgs.msg.ObjectClassification();
                {
                    classification.Label = autoware_auto_perception_msgs.msg.ObjectClassification.CAR;
                    classification.Probability = 1.0f;
                }
                obj.Classification = new List<autoware_auto_perception_msgs.msg.ObjectClassification>{classification}.ToArray();

                var kinematics = new autoware_auto_perception_msgs.msg.DetectedObjectKinematics();
                // Add pose
                {
                    var p = ROS2Utility.UnityToRosPosition(rb.transform.position)+ Environment.Instance.MgrsOffsetPosition;
                    kinematics.Pose_with_covariance.Pose.Position.X = p.x;
                    kinematics.Pose_with_covariance.Pose.Position.Y = p.y;
                    kinematics.Pose_with_covariance.Pose.Position.Z = p.z;
                    var r = ROS2Utility.UnityToRosRotation(rb.transform.rotation);
                    kinematics.Pose_with_covariance.Pose.Orientation.X = r.x;
                    kinematics.Pose_with_covariance.Pose.Orientation.Y = r.y;
                    kinematics.Pose_with_covariance.Pose.Orientation.Z = r.z;
                }
                // Add twist
                {
                    var t = ROS2Utility.UnityToRosPosition(rb.velocity);
                    kinematics.Twist_with_covariance.Twist.Linear.X = t.x;
                    kinematics.Twist_with_covariance.Twist.Linear.Y = t.y;
                    kinematics.Twist_with_covariance.Twist.Linear.Z = t.z;
                    var a = ROS2Utility.UnityToRosPosition(rb.angularVelocity);
                    kinematics.Twist_with_covariance.Twist.Angular.X = t.x;
                    kinematics.Twist_with_covariance.Twist.Angular.Y = t.y;
                    kinematics.Twist_with_covariance.Twist.Angular.Z = t.z;
                }
                // Add covariance
                {
                    kinematics.Has_position_covariance = true;
                    kinematics.Orientation_availability = autoware_auto_perception_msgs.msg.DetectedObjectKinematics.AVAILABLE;
                    kinematics.Has_twist = true;
                    kinematics.Has_twist_covariance = true;
                    // Add covariance 6x6
                    const int size = 6;
                    for (int i = 0; i < size; i++)
                    {
                        kinematics.Pose_with_covariance.Covariance[i * size + i] = 1;
                        kinematics.Twist_with_covariance.Covariance[i * size + i] = 1;
                    }
                }
                obj.Kinematics = kinematics;

                var shape = new autoware_auto_perception_msgs.msg.Shape();
                {
                    shape.Type = autoware_auto_perception_msgs.msg.Shape.BOUNDING_BOX;
                    shape.Dimensions.X = dim.x;
                    shape.Dimensions.Y = dim.y;
                    shape.Dimensions.Z = dim.z;

                    // TODO add foot print calculation
                    //shape.Footprint = 
                    obj.Shape = shape;
                }
                
                objectsList.Add(obj);
            }
            // Converts data output from ObjectSensor to ROS2 msg
            objectsMsg.Objects = objectsList.ToArray();
            // Update msg header.
            var header = objectsMsg as MessageWithHeader;
            SimulatorROS2Node.UpdateROSTimestamp(ref header);
            objectsMsg.Header.Frame_id = frameId;

            // Publish to ROS2.
            objectPublisher.Publish(objectsMsg);
        }

        void OnDestroy()
        {
           SimulatorROS2Node.RemovePublisher<autoware_auto_perception_msgs.msg.DetectedObjects>(objectPublisher);
        }
    }
}
