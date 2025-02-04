using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public class CelestialManager : MonoBehaviour
{
    public CelestialBody[] celestialBodies = new CelestialBody[0];
    public float G = 10;

    public GameObject BodyPrefab;
    public float lineThickness = 0.1f;
    public Color[] pathCols = new Color[0];
    public int stepCount;

    public bool isPlaying = false;
    private void Update()
    {
        if (isPlaying)
        {
            UpdateCelestialBodies(ref celestialBodies, 1 / 60);
            ApplyForces();
        }
        else
        {
            InitializeBodies();
            DrawPredictedPaths(celestialBodies, pathCols, stepCount, 1 / 60);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPlaying = true;
        }
    }

    void ApplyForces()
    {
        for (int i = 0; i < celestialBodies.Length; i++)
        {
            celestialBodies[i].obj.GetComponent<Rigidbody>().AddForce(celestialBodies[i].force);
        }
    }
    private void Start()
    {
        /*
        for( int i = 0;i < GameObject.FindGameObjectsWithTag("body").Length; i++)
        {
            Destroy(GameObject.FindGameObjectsWithTag("body")[i]);
        }
        */
        InitializeBodies();
    }
    void InitializeBodies()
    {

        pathCols = new Color[celestialBodies.Length];

        for (int i = 0; i < celestialBodies.Length; i++)
        {
            pathCols[i] = celestialBodies[i].Color;

            // Check if a GameObject already exists
            if (celestialBodies[i].obj == null)
            {

                celestialBodies[i].obj = Instantiate(BodyPrefab);
                celestialBodies[i].obj.tag = "body"; // Ensure it has the correct tag
            }

            GameObject obj = celestialBodies[i].obj;

            // Set properties
            obj.transform.position = celestialBodies[i].Position;
            obj.transform.localScale = Vector3.one * celestialBodies[i].Radius / 2;

            // Set color
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor("_Color", celestialBodies[i].Color);
            obj.GetComponent<MeshRenderer>().SetPropertyBlock(propertyBlock);

            // Set physics properties
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            rb.mass = celestialBodies[i].Mass;
            rb.linearVelocity = celestialBodies[i].Velocity; // linearVelocity isn't valid
        }

    }



    public void UpdateCelestialBodies(ref CelestialBody[] bodies, float deltaTime)
    {
        Vector3[] accelerations = new Vector3[bodies.Length];

        for (int a = 0; a < bodies.Length - 1; a++)
        {
            for (int b = a + 1; b < bodies.Length; b++)
            {
                Vector3 offsetToA = bodies[a].Position - bodies[b].Position;
                float sqrDst = Vector3.Dot(offsetToA, offsetToA);
                Vector3 dirToA = offsetToA / Mathf.Sqrt(sqrDst);

                Vector3 force = dirToA * (G * bodies[a].Mass * bodies[b].Mass) / sqrDst;
                bodies[a].force = -force;
                bodies[b].force = force;
                accelerations[a] -= force / bodies[a].Mass;
                accelerations[b] += force / bodies[b].Mass;
            }
        }

        for (int i = 0; i < bodies.Length; i++)
        {
            bodies[i].Velocity += accelerations[i] * deltaTime;
            if (isPlaying)
            {
                bodies[i].Position = bodies[i].obj.transform.position;
            }
            else
            {
                bodies[i].Position += bodies[i].Velocity * deltaTime;
                Debug.Log(accelerations[i]);
            }
        }
    }

    public void DrawPredictedPaths(CelestialBody[] bodies, Color[] pathCols, int stepCount, float deltaTime)
    {
        // Ensure each celestial body has a LineRenderer component
        for (int i = 0; i < bodies.Length; i++)
        {
            if (bodies[i].obj.TryGetComponent(out LineRenderer lineRenderer))
            {
                lineRenderer.positionCount = stepCount;
                lineRenderer.startColor = pathCols[i];
                lineRenderer.endColor = pathCols[i];
                lineRenderer.startWidth = lineThickness;
                lineRenderer.endWidth = lineThickness;
            }
            else
            {
                // Add a LineRenderer if missing
                lineRenderer = bodies[i].obj.AddComponent<LineRenderer>();
                lineRenderer.positionCount = stepCount;
                lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Simple unlit shader
            }
        }

        // Create a deep copy of bodies for simulation
        CelestialBody[] predictedBodies = bodies.Select(b => new CelestialBody
        {
            Position = b.Position,
            Velocity = b.Velocity,
            Radius = b.Radius,
            Mass = b.Mass,
            Color = b.Color,
            obj = b.obj
        }).ToArray();

        // Simulate motion and record positions
        Parallel.For(0, stepCount, (step) =>
        {

        });
        for (int step = 0; step < stepCount; step++)
        {
            CelestialBody[] tempBodies = predictedBodies.ToArray();
            UpdateCelestialBodies(ref tempBodies, (1f / 60f));
            predictedBodies = tempBodies;

            for (int i = 0; i < predictedBodies.Length; i++)
            {
                CelestialBody body = predictedBodies[i];

                if (body.obj.TryGetComponent(out LineRenderer lineRenderer))
                {
                    lineRenderer.SetPosition(step, body.Position);
                }
            }
        }
    }
}

[Serializable]
public struct CelestialBody
{
    public Vector3 Position;
    public Vector3 Velocity;
    public float Radius;
    public float Mass;
    public Color Color;
    [NonSerialized]
    public GameObject obj;
    [NonSerialized]
    public Vector3 force;
}
