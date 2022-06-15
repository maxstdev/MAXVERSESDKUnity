using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Threading.Tasks;

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    public enum State
    {
        Idle, Moving
    }

    #region Serialized Fields
    
    [SerializeField]
    private UnityEvent OnAnimationStarted;

    [SerializeField]
    private UnityEvent<float> OnAnimationUpdated;

    [SerializeField]
    private UnityEvent OnAnimationFinished;

    [SerializeField]
    private Transform Cursor;

    public float Speed = 0.1f, RotateSpeed = 0.01f;
    
    #endregion

    #region Private Fields

    private State _currentState = State.Idle;
    private float _t;
    private bool _keepPosition = false, _keepRotation = false;
    private Vector3 _animateFromPosition, _animateToPosition;
    private Quaternion _animateFromRotation, _animateToRotation;
    private CursorLockMode _prevCursorLockMode;
    private bool isARMode;

    #endregion

    #region Public Properties

    public Camera Camera { get; private set; }

    public KnnManager KnnManager { get; private set; }

    public PovManager PovManager { get; private set; }

    public IbrManager IbrManager { get; private set; }

    public State CurrentState
    {
        get { return _currentState; }
        set
        {
            if (value == _currentState) return;

            switch (_currentState)
            {
                case State.Idle:
                    _t = 0f;
                    OnAnimationStarted.Invoke();
                    break;
                case State.Moving:
                    _t = 1f;
                    OnAnimationFinished.Invoke();
                    break;
                default:
                    return; // never happens
            }

            _currentState = value;
        }
    }

    public bool Moving { get => CurrentState == State.Moving; }

    public bool IgnoreKeyboardNavigation { get; private set; }
    public bool IgnoreMouseNavigation { get; private set; }

    public GameObject sphere;
    public GameObject cursor;

    #endregion

    #region Unity Messages

    void Awake()
    {
        isARMode = XRStudioController.Instance.ARMode;
        if(!isARMode)
        {
            if(Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
            {
                Speed = 4;
                RotateSpeed = 5;
            }
            KnnManager = FindObjectOfType<KnnManager>(true);
            PovManager = FindObjectOfType<PovManager>(true);
            IbrManager = FindObjectOfType<IbrManager>(true);
            AddListenerIfNotExists(OnAnimationStarted, IbrManager.HandleAnimationStarted);
            AddListenerIfNotExists(OnAnimationUpdated, IbrManager.HandleAnimationUpdated);
            AddListenerIfNotExists(OnAnimationFinished, IbrManager.HandleAnimationFinished);
            Camera = GetComponent<Camera>();
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);
            sphere.SetActive(true);
            cursor.SetActive(true);
            this.enabled = true;
        }
        else
        {
            sphere.SetActive(false);
            cursor.SetActive(false);
            this.enabled = false;
        }
    }

    void OnEnable()
    {
        if (!isARMode)
        {
            _prevCursorLockMode = UnityEngine.Cursor.lockState;
            UnityEngine.Cursor.lockState = CursorLockMode.None;

            IgnoreKeyboardNavigation = false;
            IgnoreMouseNavigation = false;

            var nearestObject = GetNextPovFromPosition(transform.position);
            transform.position = nearestObject.transform.position;
            IbrManager.SetPov(nearestObject.GetComponent<IPov>(), IbrManager.PovType.Primary);
        }
    }

    void OnDisable()
    {
        if (!isARMode)
        {
            UnityEngine.Cursor.lockState = _prevCursorLockMode;
            if (Moving) { FinishAnimating(); }
        }
    }
    
    void Start()
    {
        if (!isARMode)
        {
            if (null == Cursor) { Debug.LogError("Cursor is null"); }
            FinishAnimating();
        }
    }

    async void Update()
    {
        if (!isARMode)
        {
            if (Moving)
            {
                if (_t < 1f)
                {
                    UpdateAnimationParameter();
                }
                else
                {
                    FinishAnimating();
                }
                return;
            }

            if (!IgnoreKeyboardNavigation)
            {
                IgnoreKeyboardNavigation = true;
                await HandleKeyboardNavigation();
                IgnoreKeyboardNavigation = false;
            }

            if (!IgnoreMouseNavigation)
            {
                IgnoreMouseNavigation = true;
                await HandleMouseNavigation();
                IgnoreMouseNavigation = false;
            }

            UpdateCursor();
        }
    }

    void LateUpdate()
    {
        if (!isARMode)
        {
            if (Moving)
            {
                UpdatePositionAndRotation();
            }
            else if (Input.GetMouseButton(0))
            {
                transform.Rotate(0f, -Input.GetAxis("Mouse X") * RotateSpeed, 0f, Space.World);
                transform.Rotate(Input.GetAxis("Mouse Y") * RotateSpeed, 0f, 0f, Space.Self);
            }
            var angles = transform.eulerAngles;
            var symmetricX = Mathf.Asin(Mathf.Sin(Mathf.Deg2Rad * angles.x)) * Mathf.Rad2Deg;
            angles.x = Mathf.Clamp(symmetricX, -30f, 35f - Camera.fieldOfView / 2f);
            transform.rotation = Quaternion.Euler(angles);
        }
    }

    #endregion

    #region Private Methods

    private async Task HandleKeyboardNavigation()
    {
        var dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (dir.sqrMagnitude <= 0) return;
        dir = transform.right * dir.x + transform.forward * dir.z;
        dir = dir.normalized;

        var nextPov = GetNextPovFromDirection(dir);
        await HandleNavigation(nextPov);
    }

    private async Task HandleMouseNavigation()
    {
        if (null == Cursor) return;
        if (!Cursor.gameObject.activeSelf) return;
        if (!Input.GetMouseButtonUp(1)) return;

        var nextPov = GetNextPovFromPosition(Cursor.transform.position);
        await HandleNavigation(nextPov);
    }

    private async Task HandleNavigation(Transform nextPov)
    {
        IbrManager.SetPov(nextPov.GetComponent<IPov>(), IbrManager.PovType.Secondary);
        await IbrManager.UntilReady(IbrManager.PovType.Secondary);
        StartAnimating(nextPov.position);
    }

    private Transform GetNextPovFromPosition(Vector3 position)
    {
        var nearestObject = KnnManager.FindNearest(position);
        return nearestObject.transform;
    }

    private Transform GetNextPovFromDirection(Vector3 direction)
    {
        var m = direction.sqrMagnitude;
        if (m > 1) { direction /= m; }

        var neighborObjects = KnnManager.FindNearestK(transform.position, 5);
        var displacements = neighborObjects.Select(go => go.transform.position - transform.position).ToArray();
        var dirSims = displacements.Select(v => v.normalized).Select(v => Vector3.Dot(v, direction));
        (float _, int i) = displacements.Select((v, i) => (v.sqrMagnitude, i)).Min();
        (float _, int j) = dirSims.Select((s, j) => (s, j)).Where((s, j) => i != j).Max();

        var nearestObject = neighborObjects[j];
        return nearestObject.transform;
    }

    private void UpdateCursor()
    {
        if (Physics.Raycast(Camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            Cursor.transform.position = hit.point;
            Cursor.rotation = Quaternion.FromToRotation(Vector3.forward, -hit.normal);
            Cursor.gameObject.SetActive(true);
        }
        else
        {
            Cursor.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Keep the position, and rotate to new orientation
    /// </summary>
    /// <param name="toRotation">the target rotation</param>
    private void StartAnimating(Quaternion toRotation)
    {
        StartAnimating(transform.position, toRotation);
    }

    /// <summary>
    /// Keep the orientation, and translate to new position
    /// </summary>
    /// <param name="toPosition">the target position</param>
    private void StartAnimating(Vector3 toPosition)
    {
        StartAnimating(toPosition, transform.rotation);
    }

    /// <summary>
    /// Translate and rotate to new position and orientation
    /// </summary>
    /// <param name="toPosition">the target position</param>
    /// <param name="toRotation">the target rotation</param>
    private void StartAnimating(Vector3 toPosition, Quaternion toRotation)
    {
        StartAnimating(transform.position, toPosition, transform.rotation, toRotation);
    }

    /// <summary>
    /// Translate and rotate from a pose to another
    /// </summary>
    /// <param name="fromPosition">the initial position</param>
    /// <param name="toPosition">the target position</param>
    /// <param name="fromRotation">the initial rotation</param>
    /// <param name="toRotation">the target rotation</param>
    private void StartAnimating(Vector3 fromPosition, Vector3 toPosition, Quaternion fromRotation, Quaternion toRotation)
    {
        SetPositionInfo(fromPosition, toPosition);
        SetRotationInfo(fromRotation, toRotation);
        CurrentState = State.Moving;
    }

    /// <summary>
    /// Set position-related info for animation
    /// </summary>
    /// <param name="fromPosition">the initial position</param>
    /// <param name="toPosition">the target position</param>
    private void SetPositionInfo(Vector3 fromPosition, Vector3 toPosition)
    {
        _keepPosition = fromPosition == toPosition;
        _animateFromPosition = fromPosition;
        _animateToPosition = toPosition;
    }

    /// <summary>
    /// Set rotation-related info for animation
    /// </summary>
    /// <param name="fromRotation">the initial rotation</param>
    /// <param name="toRotation">the target rotation</param>
    private void SetRotationInfo(Quaternion fromRotation, Quaternion toRotation)
    {
        _keepRotation = fromRotation == toRotation;
        _animateFromRotation = fromRotation;
        _animateToRotation = toRotation;
    }

    /// <summary>
    /// Let the animation progress
    /// </summary>
    private void UpdateAnimationParameter()
    {
        _t = Mathf.Clamp01(_t + Time.smoothDeltaTime * Speed);
        OnAnimationUpdated.Invoke(_t);
    }

    /// <summary>
    /// Apply new position and rotation based on the animation-related info
    /// </summary>
    private void UpdatePositionAndRotation()
    {
        var nextPosition = Vector3.Lerp(_animateFromPosition, _animateToPosition, _keepPosition ? 0 : _t);
        var nextRotation = Quaternion.Slerp(_animateFromRotation, _animateToRotation, _keepRotation ? 0 : _t);
        transform.SetPositionAndRotation(nextPosition, nextRotation);
    }

    /// <summary>
    /// Finalize any on-going animation
    /// </summary>
    private void FinishAnimating()
    {
        _t = 1f;
        CurrentState = State.Idle;
        SetPositionInfo(Vector3.zero, Vector3.zero);
        SetRotationInfo(Quaternion.identity, Quaternion.identity);
    }

    #endregion

    #region Private Static Methods

    private static void AddListenerIfNotExists(UnityEvent unityEventBase, UnityAction unityAction)
    {
        if (!CheckListenerExists(unityEventBase, nameof(unityAction)))
            unityEventBase.AddListener(unityAction);
    }

    private static void AddListenerIfNotExists<T0>(UnityEvent<T0> unityEventBase, UnityAction<T0> unityAction)
    {
        if (!CheckListenerExists(unityEventBase, nameof(unityAction)))
            unityEventBase.AddListener(unityAction);
    }

    private static bool CheckListenerExists(UnityEventBase unityEventBase, string methodName)
    {
        var eventCount = unityEventBase.GetPersistentEventCount();
        if (0 == eventCount) return false;
        return Enumerable.Range(0, eventCount).Select(unityEventBase.GetPersistentMethodName).Contains(methodName);
    }

    #endregion
}
