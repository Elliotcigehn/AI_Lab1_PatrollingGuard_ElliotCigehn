using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]private float movementSpeed = 5.0f;

    private Rigidbody rb;
    [HideInInspector]public InputAction playerMovement;
    private Vector3 movementDirection;

    private void OnEnable()
    {
        playerMovement.Enable();
        
        playerMovement = InputSystem.actions["Player/Move"];
    }

    private void OnDisable()
    {
        playerMovement.Disable();
    }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMovement.Enable();

        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        movementDirection = playerMovement.ReadValue<Vector3>();
        Vector3 movement = movementDirection.normalized * movementSpeed * Time.fixedDeltaTime;
        rb.MovePosition(transform.position + movement);
    }
}
