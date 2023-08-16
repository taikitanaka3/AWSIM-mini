using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace AWSIM
{
    /// <summary>
    /// ObjectSensor sensor.
    /// Need to set the MgrsReference of the Environment for the MGRS coordinate system.
    /// </summary>
    /// TODO: To improve the reproducibility of ObjectSensor sensors, make it possible to output rotation and covariance.
    public class ObjectSensor : MonoBehaviour
    {
        /// <summary>
        /// This data is output from ObjectSensor at the OutputHz cycle.
        /// </summary>
        public class OutputData
        {
            /// <summary>
            /// Position in the MGRS coordinate system.
            /// (NOTE: This is the position considering the MGRS coordinate system origin set in Environment.cs,
            /// not the Unity world coordinate system position.)
            /// </summary>
            public Rigidbody[] rbs;
            public Vector3 dimensions;
        }

        /// <summary>
        /// Data output hz.
        /// Sensor processing and callbacks are called in this hz.
        /// </summary>
        [Range(0, 10)]
        public int OutputHz = 10;    // Autoware's ObjectSensor basically output at 1hz.

        /// <summary>
        /// Delegate used in callbacks.
        /// </summary>
        /// <param name="outputData">Data output for each hz</param>
        public delegate void OnOutputDataDelegate(OutputData outputData);

        /// <summary>
        /// Called each time data is output.
        /// </summary>
        public OnOutputDataDelegate OnOutputData;
        float timer = 0;
        OutputData outputData = new OutputData();
        Transform m_transform;
        GameObject[] gameObjects;

        void Start()
        {
            m_transform = transform;
            // TODO to adjust in a better way
            outputData.dimensions = new Vector3(3.0f,1.5f,2.0f);
            gameObjects = GameObject.FindGameObjectsWithTag("CAR");
            outputData.rbs = gameObjects
                .Select(go => go.GetComponent<Rigidbody>())
                .Where(rb => rb != null)
                .ToArray();
        }

        void FixedUpdate()
        {
            // Matching output to hz.
            timer += Time.deltaTime;
            var interval = 1.0f / OutputHz;
            interval -= 0.00001f;       // Allow for accuracy errors.
            if (timer < interval)
                return;
            timer = 0;

            //var objectsList = new List<autoware_auto_perception_msgs.msg.DetectedObject>();
            // get game object NPC with "CAR" tag 
            /*
            foreach (var gameObject in gameObjects) // maybe replace with rigidbody to get velocity easier
            {
                var rosPosition = ROS2Utility.UnityToRosPosition(gameObject.transform.position)+ Environment.Instance.MgrsOffsetPosition;
                var rosRotation = ROS2Utility.UnityToRosRotation(gameObject.transform.rotation);

                var obj = new autoware_auto_perception_msgs.msg.DetectedObject();
                // obj.Existence_Probability = 1.0f;
                obj.Classification = new List<autoware_auto_perception_msgs.msg.ObjectClassification>().ToArray();
                obj.Kinematics = new autoware_auto_perception_msgs.msg.DetectedObjectKinematics();
                //obj.Kinematics.Pose_With_Covariance.Pose.Position.X = 0;
                // obj.Kinematics.initial_pose_with_covariance =
                // obj.Kinematics.initial_pose_with_covariance =
                obj.Shape = new autoware_auto_perception_msgs.msg.Shape();
                objectsList.Add(obj);
            }
            */
            // Calls registered callbacks
            OnOutputData.Invoke(outputData);
        }
    }
}