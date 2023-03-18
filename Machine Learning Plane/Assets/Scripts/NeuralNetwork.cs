using System.Collections;
using UnityEngine;

public class NeuralNetwork : MonoBehaviour
{
    [SerializeField]
    float[] inputs = new float[28];
    [SerializeField]
    Transform objective;
    Node[][] nodes = new Node[10][];
    Rigidbody rb;
    [SerializeField]
    float fitness = 0;
    public static float bestFitness = 0;
    public static Node[][] bestNodes = new Node[10][];
    float xRotation = 0;
    float yRotation = 0;
    float zRotation = 0;
    private void Awake()
    {
        CreateNodes();
        rb = GetComponent<Rigidbody>();
        StartCoroutine(TimeAlive());
    }
    void GetInputs()
    {
        //I wish there was a better way to do this...
        Vector3[] directions =
        {
            transform.forward,
            transform.forward + transform.right,
            transform.forward + -transform.right,
            transform.forward+transform.up,
            transform.forward + transform.right+transform.up,
            transform.forward + -transform.right+transform.up,
            transform.forward+(-transform.up),
            transform.forward + transform.right+(-transform.up),
            transform.forward + -transform.right+(-transform.up),
            transform.right,
            -transform.right,
            transform.up,
            transform.right+transform.up,
            -transform.right+transform.up,
            (-transform.up),
            transform.right+(-transform.up),
            -transform.right+(-transform.up),
            -transform.forward,
            -transform.forward + transform.right,
            -transform.forward + -transform.right,
            -transform.forward+transform.up,
            -transform.forward + transform.right+transform.up,
            -transform.forward + -transform.right+transform.up,
            -transform.forward+(-transform.up),
            -transform.forward + transform.right+(-transform.up),
            -transform.forward + -transform.right+(-transform.up),
        };
        for (int i = 0; i < 26; i++)
        {
            Physics.Raycast(
                transform.position + (-transform.right * 1.3f),
                directions[i],
                out RaycastHit hit,
                float.PositiveInfinity,
                ~4);
            Debug.DrawLine(
                transform.position + (-transform.right * 1.3f),
                (transform.position + (-transform.right * 1.3f) + directions[i]),
                Color.blue);
            inputs[i] = hit.distance;
        }
        inputs[26] = Vector3.Distance(transform.position, objective.position);
        inputs[27] = DirectionToGameobject(objective);
    }
    private void Update()
    {
        GetInputs();
        AssignNodes();
        transform.rotation = Quaternion.Normalize(Quaternion.Euler(new Vector3(
            CheckForInvalid(Mathf.Repeat(xRotation, 360)),
            CheckForInvalid(Mathf.Repeat(yRotation, 360)),
            CheckForInvalid(Mathf.Repeat(zRotation, 360)))));
    }
    float DirectionToGameobject(Transform goal)
    {
        // If there is a closest gameobject
        if (goal != null)
        {
            // Calculate the direction from the current gameobject to the closest gameobject
            Vector3 direction = (goal.transform.position - transform.position).normalized;

            // Calculate the angle between the x-axis and the direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Return the angle
            return angle;
        }
        return 0;
    }
    void CreateNodes()
    {
        for (int i = 0; i < 10; i++)
        {
            nodes[i] = new Node[10];
            bestNodes[i] = new Node[10];
            for (int r = 0; r < 10; r++)
            {
                nodes[i][r].value = 0;
                nodes[i][r].inputWeights = new float[28];
                nodes[i][r].bias = 0;
                bestNodes[i][r].inputWeights = new float[28];
                bestNodes[i][r].bias = 0;
                bestNodes[i][r].value = 0;
            }
        }
    }
    float Sum(float[] arguments)
    {
        if (arguments.Length == 0)
        {
            return 0;
        }

        // Initialize a variable to hold the sum
        float sum = 0;

        // Loop through each element in the array
        foreach (float argument in arguments)
        {
            // Add the current element to the sum
            sum += argument;
        }

        // Return the sum of the elements in the array
        return sum;
    }
    float CalculateNodes(float[] values, float[] weights, float bias)
    {
        float[] alteredValues = new float[values.Length];
        float result;
        for (int i = 0; i < values.Length; i++)
        {
            alteredValues[i] = values[i] * weights[i];
        }
        result = Sum(alteredValues) + bias;
        return result;
    }
    void AssignNodes()
    {
        // Initialize two arrays of floating point numbers to hold the values of nodes and the values of nodes from the previous iteration
        float[] values = new float[10];
        float[] lastValues = new float[10];

        // Loop through each layer of nodes in the network
        for (int i = 0; i < 10; i++)
        {
            // Loop through each individual node in the current layer
            for (int r = 0; r < 10; r++)
            {
                // Store the current value of the node in an array
                values[r] = nodes[i][r].value;

                // If the current layer is not the input layer, calculate the value of the current node based on the previous layer's values
                if (i != 0)
                {
                    nodes[i][r].value = CalculateNodes(
                        lastValues,
                        nodes[i][r].inputWeights,
                        nodes[i][r].bias);
                }
                // If the current layer is the input layer, calculate the value of the current node based on the input values
                else
                {
                    nodes[i][r].value = CalculateNodes(
                        inputs,
                        nodes[i][r].inputWeights,
                        nodes[i][r].bias);
                }
            }

            // Copy the values of the current layer's nodes to the lastValues array to be used in the next iteration
            for (int i1 = 0; i1 < values.Length; i1++)
            {
                float item = values[i1];
                lastValues[i1] = item;
            }
        }
        rb.velocity = new Vector3(
            (-transform.right.x) * Mathf.Clamp(CheckForInvalid(nodes[9][0].value), 0, 15),
            (-transform.right.y) * Mathf.Clamp(CheckForInvalid(nodes[9][0].value), 0, 15),
            (-transform.right.z) * Mathf.Clamp(CheckForInvalid(nodes[9][0].value), 0, 15))
            + (Vector3.down * 3);
        xRotation += Mathf.Clamp(nodes[9][1].value, -5, 5);
        yRotation += Mathf.Clamp(nodes[9][2].value, -5, 5);
        zRotation += Mathf.Clamp(nodes[9][3].value, -5, 5);
    }
    void Death()
    {
        transform.SetPositionAndRotation(
            Vector3.zero,
            Quaternion.Euler(Vector3.zero));
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        xRotation = 0;
        yRotation = 0;
        zRotation = 0;
        if (fitness > bestFitness)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                for (int i1 = 0; i1 < nodes[i].Length; i1++)
                {
                    bestNodes[i][i1] = nodes[i][i1];
                    bestFitness = fitness;
                }
            }
            print(bestFitness);
        }
        fitness = 0;
        Mutate(bestNodes, -1, 1);
    }
    public void Mutate(Node[][] parentNodes, float mutationMin, float mutationMax)
    {
        for (int i1 = 0; i1 < parentNodes.Length; i1++)
        {
            for (int i = 0; i < parentNodes[i1].Length; i++)
            {
                nodes[i1][i] = parentNodes[i1][i];
                nodes[i1][i].bias += Random.Range(mutationMin, mutationMax);
                for (int i2 = 0; i2 < 7; i2++)
                {
                    nodes[i1][i].inputWeights[i2] += Random.Range(mutationMin, mutationMax);
                }
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        Death();
    }
    IEnumerator TimeAlive()
    {
        yield return new WaitForSeconds(0.1f);
        fitness += 0.1f;
        StartCoroutine(TimeAlive());
    }
    float CheckForInvalid(float value)
    {
        if (float.IsNaN(value))
        {
            return 0;
        }
        if (float.IsInfinity(value))
        {
            return 0;
        }
        return value;
    }
}
public struct Node
{
    public float bias;
    public float[] inputWeights;
    public float value;
}
