using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // NEW INPUT SYSTEM

public enum Button
{
    ATTACK,
    SHOOT,
    DEFEND,
    SPECIAL,
    JUMP,
    RELOAD,
    SWAP,
    TAUNT
}

public struct AxesInfo
{
    public enum Direction
    {
        NONE,
        UP,
        DOWN,
        RIGHT,
        LEFT
    }

    public float x;
    public float y;
    public Direction direction;
    public Direction directionLast;
    public int tiltLevel;
    public bool isTapInput;
    public bool isBufferedTapInput;

    public Vector2 asVector
    {
        get { return new Vector2(x, y); }
    }
}

// base input manager class
public class InputManager : MonoBehaviour
{
    public enum InputScheme
    {
        CLASSIC,
        TWINSTICK,
        KBMOUSE
    }

    // kept, but no longer needed for aliasing
    const int BUFFER_SIZE = 2;
    const float AXIS_TILT_THRESHOLD = 0.25f;

    [Header("Player")]
    [SerializeField] int playerIndex;

    [Tooltip("OLD alias array is now unused, kept only for backwards compatibility.")]
    [SerializeField] string[] inputAlias = new string[11];

    [Header("New Input System")]
    [SerializeField] private PlayerInput playerInput;

    // These must match the action names in your Input Actions asset / PlayerInput
    [SerializeField] private string moveActionName = "Move";    // Vector2 (left stick / WASD)
    [SerializeField] private string aimActionName = "Aim";     // Vector2 (right stick / mouse)
    [SerializeField] private string attackActionName = "Attack";
    [SerializeField] private string shootActionName = "Shoot";
    [SerializeField] private string defendActionName = "Defend";
    [SerializeField] private string specialActionName = "Special";
    [SerializeField] private string jumpActionName = "Jump";
    [SerializeField] private string reloadActionName = "Reload";
    [SerializeField] private string swapActionName = "Swap";
    [SerializeField] private string tauntActionName = "Taunt";

    // Axes data used by Avatar and other code
    public AxesInfo leftAxes;
    public AxesInfo rightAxes;

    // Keep the same virtual properties
    public virtual AxesInfo moveAxes
    {
        get { return leftAxes; }
    }

    public virtual AxesInfo aimAxes
    {
        get { return leftAxes; }
    }

    public virtual AxesInfo spcAxes
    {
        get { return leftAxes; }
    }

    public virtual AxesInfo cStick
    {
        get { return rightAxes; }
    }

    // Cached InputActions
    private InputAction moveAction;
    private InputAction aimAction;
    private InputAction attackAction;
    private InputAction shootAction;
    private InputAction defendAction;
    private InputAction specialAction;
    private InputAction jumpAction;
    private InputAction reloadAction;
    private InputAction swapAction;
    private InputAction tauntAction;

    void Awake()
    {
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }

        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component is missing on the same GameObject as InputManager.");
            return;
        }

        CacheActions();
    }

    void OnEnable()
    {
        if (playerInput != null && moveAction == null)
        {
            CacheActions();
        }

        EnableActions(true);
    }

    void OnDisable()
    {
        EnableActions(false);
    }

    private void CacheActions()
    {
        if (playerInput?.actions is null)
            return;

        var actions = playerInput.actions;

        moveAction = GetActionSafe(actions, moveActionName);
        aimAction = GetActionSafe(actions, aimActionName);
        attackAction = GetActionSafe(actions, attackActionName);
        shootAction = GetActionSafe(actions, shootActionName);
        defendAction = GetActionSafe(actions, defendActionName);
        specialAction = GetActionSafe(actions, specialActionName);
        jumpAction = GetActionSafe(actions, jumpActionName);
        reloadAction = GetActionSafe(actions, reloadActionName);
        swapAction = GetActionSafe(actions, swapActionName);
        tauntAction = GetActionSafe(actions, tauntActionName);
    }

    private InputAction GetActionSafe(InputActionAsset asset, string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        var action = asset.FindAction(name, throwIfNotFound: false);
        if (action == null)
        {
            Debug.LogWarning($"InputManager: could not find action '{name}' in PlayerInput actions.");
        }
        return action;
    }

    private void EnableActions(bool enable)
    {
        if (enable)
        {
            moveAction?.Enable();
            aimAction?.Enable();
            attackAction?.Enable();
            shootAction?.Enable();
            defendAction?.Enable();
            specialAction?.Enable();
            jumpAction?.Enable();
            reloadAction?.Enable();
            swapAction?.Enable();
            tauntAction?.Enable();
        }
        else
        {
            moveAction?.Disable();
            aimAction?.Disable();
            attackAction?.Disable();
            shootAction?.Disable();
            defendAction?.Disable();
            specialAction?.Disable();
            jumpAction?.Disable();
            reloadAction?.Disable();
            swapAction?.Disable();
            tauntAction?.Disable();
        }
    }

    // OLD: SetAlias used the classic Input Manager names (playerIndex + alias)
    // With the new Input System we do not need this anymore.
    void Start()
    {
        // left intentionally empty – alias system is obsolete with the new Input System
    }

    protected void UpdateMoveAxes()
    {
        Vector2 v = Vector2.zero;
        if (moveAction != null)
            v = moveAction.ReadValue<Vector2>();

        leftAxes.x = v.x;
        leftAxes.y = v.y;
    }

    protected virtual void UpdateAimAxes()
    {
        Vector2 v = Vector2.zero;
        if (aimAction != null)
            v = aimAction.ReadValue<Vector2>();

        rightAxes.x = v.x;
        rightAxes.y = v.y;
    }

    protected virtual void UpdateInfo(ref AxesInfo axes)
    {
        AxesInfo.Direction directionBeforeLast = axes.directionLast;
        axes.directionLast = axes.direction;
        axes.isBufferedTapInput = axes.isTapInput;
        axes.isTapInput = false;
        axes.tiltLevel = 0;
        axes.direction = AxesInfo.Direction.NONE;
        if (axes.x != 0.0f || axes.y != 0.0f)
        {
            if (Mathf.Abs(axes.x) > Mathf.Abs(axes.y)) 
            {
                if (Mathf.Abs(axes.x) > AXIS_TILT_THRESHOLD)
                {
                    axes.tiltLevel = 2;
                }
                else
                {
                    axes.tiltLevel = 1;
                }
                if (axes.x > 0.0f)
                {
                    axes.direction = AxesInfo.Direction.RIGHT;
                }
                else
                {
                    axes.direction = AxesInfo.Direction.LEFT;
                }
            }
            else 
            {
                if (Mathf.Abs(axes.y) > AXIS_TILT_THRESHOLD)
                {
                    axes.tiltLevel = 2;
                }
                else
                {
                    axes.tiltLevel = 1;
                }
                if (axes.y > 0.0f)
                {
                    axes.direction = AxesInfo.Direction.UP;
                }
                else
                {
                    axes.direction = AxesInfo.Direction.DOWN;
                }
            }
        }
        if (axes.tiltLevel == 2)
        {
            if (axes.direction != axes.directionLast || axes.direction != directionBeforeLast)
            {
                axes.isTapInput = true;
                axes.isBufferedTapInput = false;
                axes.directionLast = axes.direction;
            }
        }
        else
        {
            axes.isBufferedTapInput = false;
        }
    }

    public virtual void UpdateAxes()
    {
        UpdateMoveAxes();
        UpdateAimAxes();
        UpdateInfo(ref leftAxes);
        UpdateInfo(ref rightAxes);
    }

    public virtual bool GetButtonDown(Button button)
    {
        switch (button)
        {
            case Button.ATTACK:
                return attackAction != null && attackAction.WasPressedThisFrame();
            case Button.SHOOT:
                return shootAction != null && shootAction.WasPressedThisFrame();
            case Button.DEFEND:
                return defendAction != null && defendAction.WasPressedThisFrame();
            case Button.SPECIAL:
                return specialAction != null && specialAction.WasPressedThisFrame();
            case Button.JUMP:
                return jumpAction != null && jumpAction.WasPressedThisFrame();
            case Button.RELOAD:
                return reloadAction != null && reloadAction.WasPressedThisFrame();
            case Button.SWAP:
                return swapAction != null && swapAction.WasPressedThisFrame();
            case Button.TAUNT:
                return tauntAction != null && tauntAction.WasPressedThisFrame();
            default:
                return false;
        }
    }

    public virtual bool GetButtonHeld(Button button)
    {
        switch (button)
        {
            case Button.ATTACK:
                return attackAction != null && attackAction.IsPressed();
            case Button.SHOOT:
                return shootAction != null && shootAction.IsPressed();
            case Button.DEFEND:
                return defendAction != null && defendAction.IsPressed();
            case Button.SPECIAL:
                return specialAction != null && specialAction.IsPressed();
            case Button.JUMP:
                return jumpAction != null && jumpAction.IsPressed();
            case Button.RELOAD:
                return reloadAction != null && reloadAction.IsPressed();
            case Button.SWAP:
                return swapAction != null && swapAction.IsPressed();
            case Button.TAUNT:
                return tauntAction != null && tauntAction.IsPressed();
            default:
                return false;
        }
    }

    public virtual bool GetButtonUp(Button button)
    {
        switch (button)
        {
            case Button.ATTACK:
                return attackAction != null && attackAction.WasReleasedThisFrame();
            case Button.SHOOT:
                return shootAction != null && shootAction.WasReleasedThisFrame();
            case Button.DEFEND:
                return defendAction != null && defendAction.WasReleasedThisFrame();
            case Button.SPECIAL:
                return specialAction != null && specialAction.WasReleasedThisFrame();
            case Button.JUMP:
                return jumpAction != null && jumpAction.WasReleasedThisFrame();
            case Button.RELOAD:
                return reloadAction != null && reloadAction.WasReleasedThisFrame();
            case Button.SWAP:
                return swapAction != null && swapAction.WasReleasedThisFrame();
            case Button.TAUNT:
                return tauntAction != null && tauntAction.WasReleasedThisFrame();
            default:
                return false;
        }
    }
}
