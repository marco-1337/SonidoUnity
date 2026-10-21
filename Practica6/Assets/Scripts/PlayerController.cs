using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    InputActionAsset actions;

    InputActionMap playerInputMap;

    InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;
    InputAction crouchAction;

    Vector2 moveInput;
    Vector3 moveDir = new Vector3(0f, 0f, 0f);

    Vector2 lookInput;
    Vector2 lookDir;

    [SerializeField]
    Camera camera;

    [SerializeField]
    float speed = 10f;

    [SerializeField]
    float lookSpeed = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInputMap = actions.FindActionMap("Player");
        playerInputMap.Enable();
        moveAction = playerInputMap.FindAction("Move");
        lookAction = playerInputMap.FindAction("Look");
        jumpAction = playerInputMap.FindAction("Jump");
        crouchAction = playerInputMap.FindAction("Crouch");
    }

    // Update is called once per frame
    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        moveDir.x = moveInput.x;
        moveDir.z = moveInput.y;

        moveDir = transform.rotation * moveDir;
        moveDir.y = 0;
        moveDir.y += jumpAction.IsPressed() ? 1 : 0f;
        moveDir.y -= crouchAction.IsPressed() ? 1 : 0f;

        if (moveDir.sqrMagnitude > 0) 
        {
            transform.position += speed * Time.deltaTime * moveDir;
        }

        lookInput = lookAction.ReadValue<Vector2>();

        if (lookInput.sqrMagnitude > 0) 
        {
            lookDir += lookSpeed * Time.deltaTime * lookInput;

            lookDir.x = Mathf.Clamp(lookDir.x, -180f, 180f);
            lookDir.y = Mathf.Clamp(lookDir.y, -90f, 90f);

            transform.localRotation = Quaternion.Euler(0f, lookDir.x, 0f);
            camera.transform.localRotation = Quaternion.Euler(-lookDir.y, 0f, 0f);
        }
    }
}