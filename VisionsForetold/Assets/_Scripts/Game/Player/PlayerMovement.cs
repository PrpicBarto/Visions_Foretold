using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform aimTarget;
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private CinemachinePositionComposer positionComposer;
    [SerializeField] private float sensitivity = 2f;

    private Vector2 movementInput;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Move();
        Aim();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    private void Move()
    {
        Vector2 movementInput = this.movementInput.normalized;
        Vector3 velocity = new Vector3(movementInput.x, 0f, movementInput.y) * moveSpeed;
        playerRigidbody.linearVelocity = new Vector3(velocity.x, playerRigidbody.linearVelocity.y, velocity.z);
    }

    private void Aim()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hitInfo))
        {
            aimTarget.position = hitInfo.point;
            Vector3 aimDirection = aimTarget.position - transform.position;
            aimDirection.y = 0;
            positionComposer.TargetOffset = aimDirection.normalized * sensitivity;
            transform.rotation = Quaternion.LookRotation(aimDirection);
        }
    }
}
