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
        /// 
        public enum Classification
        {
            CAR,
            TRUCK,
            BUS,
            Pedestrian
        }
        public class DetectedObject
        {
            public Rigidbody rigidBody;
            public Vector3 dimension;
            public Classification classification; 
        }

        public class OutputData
        {
            public DetectedObject[] objects;
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
            gameObjects = GameObject.FindGameObjectsWithTag("CAR");
            outputData = new OutputData();
            outputData.objects = new DetectedObject[gameObjects.Length];
            for (int i = 0; i < gameObjects.Length; i++)
            {
                var gameObject = gameObjects[i];
                outputData.objects[i] = new DetectedObject();
                outputData.objects[i].rigidBody = gameObject.GetComponent<Rigidbody>();
                // add mesh bounds
                Vector3 minBounds = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                Vector3 maxBounds = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer renderer in renderers)
                {
                    Bounds bounds = renderer.bounds;
                    minBounds = Vector3.Min(minBounds, bounds.min);
                    maxBounds = Vector3.Max(maxBounds, bounds.max);
                }
                const Vector3 totalSize = maxBounds - minBounds;
                outputData.objects[i].dimension = totalSize;
                outputData.objects[i].classification = Classification.CAR;
            }
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

            // Calls registered callbacks
            OnOutputData.Invoke(outputData);
        }
    }
}