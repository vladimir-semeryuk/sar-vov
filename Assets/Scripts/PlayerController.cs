using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rbPlayer;
    [SerializeField] private float speed = 10f;
    [SerializeField] float acceleration = 6f;
    [SerializeField] float decceleration = 7f;
    [SerializeField] bool isNewControlScheme = true;
    [SerializeField] float velPower = 0.9f;
    [SerializeField] float accelRate; // for debug
    [SerializeField] float frictionAmount = 0.2f;
    private float direction = 0;
    private float fallSpeedRate;
    private CapsuleCollider2D playerCapsule;
    private PlayerInput playerInput;
    private PlayerInputBtns playerInputActions;
    private void Awake()
    {
        rbPlayer = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        playerInputActions = new PlayerInputBtns();
        playerCapsule = GetComponent<CapsuleCollider2D>();
        playerInputActions.Player.Enable();
        playerInputActions.Player.jump.performed += JumpAction;
        playerInputActions.Player.move.performed += move =>
        {
            direction = move.ReadValue<float>();
        };
    }
    public void JumpAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Jump");
            rbPlayer.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
        }
    }

    public void Run()
    {
        if (!isNewControlScheme)
        {
            rbPlayer.velocity = new Vector2(speed * direction, rbPlayer.velocity.y);
        }
        else
        {
            float targetSpeed = direction * speed; // the speed and its direction, as the input returns a negative value too
            float speedDifference = targetSpeed - rbPlayer.velocity.x;
            // if + + => the speed difference is positive, movement to the right
            // if + 0 => medium-paced movement to the right
            // if + - => the difference is positive, faster movement to the right
            // if 0 + => decceleration
            // if 0 0 => nothing
            // if - + => negative difference, fast-paced movement to the left
            // if - 0 => negative, medium-paced movement to the left
            // if - - => negative, slow-paced movement to the left
            // note, that signs don't matter, as the Abs of this value is used. only the amount matters. This way, +- and -+ cases are equal, meaning that if the player was running to one direction and decided to turn back, the difference is huge.
            // we multiply this value by accelRate and raise it to the mysterious velPower that I have no idea about.
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
            // If targetSpeed is greater than 0.01, which happens if a player decides to run in any direction, acceleration rate is used; otherwise, decceleration is used.
            float movement = Mathf.Pow(Mathf.Abs(speedDifference) * accelRate, velPower) * Mathf.Sign(speedDifference);
            // speedDifference is multiplied by the acceleration rate and raised to the velPower. then, it is multiplied by the sign of the speedDifference. VelPower is responsible for how quickly the acceleration starts to affect the player.

            rbPlayer.AddForce(movement * Vector2.right);

            if (Mathf.Abs(direction) < 0.01f)
            {
                float amount = Mathf.Min(Mathf.Abs(rbPlayer.velocity.x), Mathf.Abs(frictionAmount));
                amount *= Mathf.Sign(rbPlayer.velocity.x);
                rbPlayer.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
            }
        }
    }

    public void Fall()
    {
        // implement the ground check to check if the player falls down
        fallSpeedRate = 2f;
        if (rbPlayer.velocity.y < 0)
        {
            rbPlayer.velocity = new Vector2(rbPlayer.velocity.x, rbPlayer.velocity.y * Physics2D.gravity.y * fallSpeedRate * Time.deltaTime);
        }
    }
    private void FixedUpdate()
    {
        Run();
        // Fall();
    }

}

