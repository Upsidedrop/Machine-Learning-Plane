using UnityEngine;

public class NeuralNetwork : MonoBehaviour
{
    float[] inputs = new float[28];
    [SerializeField]
    Transform objective;
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
                out RaycastHit hit);
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
}
