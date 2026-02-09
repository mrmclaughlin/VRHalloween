using UnityEngine;

public class MovingTarget : MonoBehaviour
{
    public GameObject particleEffectPrefab;  // Drag your Particle Effect Prefab here in the Inspector

    private float speed = 1.0f;
    private Vector3 boxSize = new Vector3(15, 0, 15); // Size of the box (Width, Height, Depth)
    private Vector3 initialPosition;
    private Vector3 nextPosition;

    private float elapsedTime = 0f; // Timer to track elapsed time

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;
        SetNextPosition();

    }

    // Update is called once per frame
    void Update()
    {
        // Move towards the next position
        transform.position = Vector3.MoveTowards(transform.position, nextPosition, speed * Time.deltaTime);

        // Update the elapsed time
        elapsedTime += Time.deltaTime;

        // If the object has reached the next position, set a new random position within the box
        if (transform.position == nextPosition)
        {
            SetNextPosition();
        }
    }

    // Set a random next position within the boxed area
void SetNextPosition()
{
    bool isInsideRestrictedArea = true;
    while (isInsideRestrictedArea)
    {
        nextPosition = new Vector3(
            Random.Range(initialPosition.x - boxSize.x / 2, initialPosition.x + boxSize.x / 2),
            initialPosition.y, // Assuming you don't want to move the object up or down
            Random.Range(initialPosition.z - boxSize.z / 2, initialPosition.z + boxSize.z / 2)
        );

        // Check if the position is outside the 4x4 central area
        if (Mathf.Abs(nextPosition.x - initialPosition.x) > 4 || Mathf.Abs(nextPosition.z - initialPosition.z) > 4)
        {
            isInsideRestrictedArea = false;
        }
    }
}

	void OnCollisionEnter(Collision collision)
    {
        
		
		
		
		if (collision.gameObject.name.Contains("SnowBallPrefab")) // Check if more than 5 seconds have passed
        {
            // Instantiate the particle effect at the target's location
           
        
           
			Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
            // Destroy the target GameObject
            Destroy(gameObject);
        }
    }
}