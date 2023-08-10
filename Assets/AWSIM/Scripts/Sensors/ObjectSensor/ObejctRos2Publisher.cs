using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        public string objectTopic = "/awsim/ground_truth/perception/objects";

        /// <summary>
        /// Object sensor frame id.
        /// </summary>
        public string frameId = "gnss_link";

        /// <summary>
        /// QoS settings.
        /// </summary>
        public QoSSettings qosSettings;

        IPublisher<autoware_auto_perception_msgs.msg.PredictedObjects> objectPublisher;
        autoware_auto_perception_msgs.msg.PredictedObjects msg;
        ObjectSensor objectSensor;

        void Start()
        {
            // Get ObjectSensor component.
            objectSensor = GetComponent<ObjectSensor>();

            // Set callback.
            objectSensor.OnOutputData += Publish;

            // Create msg.
            predictedObjects = new autoware_auto_perception_msgs.msg.PredictedObjects();

            // Create publisher.
            var qos = qosSettings.GetQoSProfile();
            objectPublisher = SimulatorROS2Node.CreatePublisher<autoware_auto_perception_msgs.msg.PredictedObjects>(objectTopic, qos);
        }

        void Publish(ObjectSensor.OutputData outputData)
        {
            // Converts data output from ObjectSensor to ROS2 msg
            predictedObjectsMsg.Objects.Kinematics.Initial_pose_with_covariance.X = 1;
 
            // Update msg header.
            var header = msg as MessageWithHeader;
            SimulatorROS2Node.UpdateROSTimestamp(ref header);

            // Publish to ROS2.
            objectPublisher.Publish(objectPublisher);
        }

        void OnDestroy()
        {
            SimulatorROS2Node.RemovePublisher<autoware_auto_perception_msgs.msg.PredictedObjects>(objectPublisher);
        }
    }
}