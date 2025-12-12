using UnityEngine;
using UnityEngine.SceneManagement; // 씬 로드 이벤트를 사용하기 위해 추가

public class PlayerMovement : MonoBehaviour
{
    // 1. 싱글톤 패턴 추가
    public static PlayerMovement Instance { get; private set; }

    // 캐릭터가 보는 방향을 나타내는 상수 정의
    private const int DIRECTION_RIGHT = 1;
    private const int DIRECTION_LEFT = 2;

    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float arrivalDistance = 0.1f;

    // [핵심 플래그] 수동 움직임 제어 플래그
    private bool canMoveManual = true;

    private Rigidbody2D rb;
    private Vector2 movement;
    // DialogueManager는 씬마다 파괴/생성될 수 있으므로, 씬 로드 시마다 재참조가 필요합니다.
    private Animator animator;

    private int currentDirection = DIRECTION_RIGHT;

    // 자동 이동 및 상호작용 플래그
    private bool isMovingToTarget = false;
    private Vector2 targetPosition;

    // --- 중요: ItemCollector는 프로젝트 내에 별도의 스크립트로 정의되어 있어야 합니다. ---
    private bool isInteracting = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴 방지

            // 씬 로드 이벤트 구독: 씬이 로드될 때마다 참조를 업데이트하기 위함
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 오브젝트가 파괴될 때 이벤트 구독 해제 (클린업)
    void OnDestroy()
    {
        // 씬 로드 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // Start는 최초 1회만 실행되므로, 여기서 초기 컴포넌트만 가져옵니다.
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // 씬에 따라 DialogueManager가 존재하지 않을 수 있으므로, 최초 로드 시 한 번만 찾습니다.
        // OnSceneLoaded에서 다시 찾을 것입니다.
    }

    // 씬 로드 시 호출되는 이벤트 핸들러
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새로운 씬에서 DialogueManager를 다시 찾아서 참조를 업데이트합니다.
        // 이전 씬의 DialogueManager는 파괴되었을 수 있기 때문입니다.

        Debug.Log($"[PlayerMovement] Scene Loaded: {scene.name}. DialogueManager reference updated.");

        // 자동 이동/상호작용 상태를 강제로 초기화하여 다음 씬에서 움직임을 허용합니다.
        ResetAllMovementStates();
    }

    void Update()
    {
        // 이 오브젝트가 파괴되었거나, 초기화 중이라면 아무것도 하지 않습니다.
        if (this == null || !gameObject.activeInHierarchy) return;

        // --- 키 입력 차단 조건 디버깅 시작 ---
        bool blockingCondition = false;
        string blockingReason = "";

        // 1. canMoveManual 상태 확인 (현재 계속 true여야 함)
        if (!canMoveManual)
        {
            blockingCondition = true;
            blockingReason = "canMoveManual is false";
        }
        // 2. 대화 중 상태 확인
        // [수정된 부분]: dialogueManager와 dialoguePanel 모두 null 체크를 추가하여
        // 파괴된 오브젝트에 접근하는 MissingReferenceException을 방지합니다.
       
        // 3. 자동 이동/상호작용 상태 확인
        else if (isMovingToTarget)
        {
            blockingCondition = true;
            blockingReason = "isMovingToTarget is true";
        }
        else if (isInteracting)
        {
            blockingCondition = true;
            blockingReason = "isInteracting is true";
        }

        if (blockingCondition)
        {
            movement = Vector2.zero;
            if (animator != null) animator.SetBool("IsMoving", false);

            // 디버깅을 위해 계속 로그 출력 유지
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"[PlayerMovement BLOCKED] Movement is blocked due to: {blockingReason}");
            }
            return;
        }
        // --- 키 입력 차단 조건 디버깅 종료 ---

        // 2. 키 입력 처리
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.Normalize();

        // 3. 애니메이션 방향 및 걷기 상태 업데이트
        UpdateAnimationState(movement.x);
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        if (isMovingToTarget)
        {
            // 자동 이동 로직 처리
            HandleAutomaticMovement();
        }
        else if (canMoveManual && !isInteracting)
        {
            // 수동 이동 실행
            Vector2 positionChange = movement * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + positionChange);
        }
        else
        {
            // 움직임이 막혔을 때 강제로 멈춥니다.
            rb.linearVelocity = Vector2.zero;
        }
    }

    // ----------------------------------------------------
    // DialogueManager가 호출하는 움직임 제어 메서드
    // ----------------------------------------------------
    public void TogglePlayerMovement(bool enableMovement)
    {
        canMoveManual = enableMovement;

        if (!canMoveManual)
        {
            // 움직임이 금지될 때 즉시 멈춥니다.
            if (rb != null) rb.linearVelocity = Vector2.zero;
            movement = Vector2.zero;
            if (animator != null) animator.SetBool("IsMoving", false);
            Debug.Log($"[PlayerMovement Toggle] Movement DISABLED.");
        }
        else
        {
            // 움직임이 허용될 때 로그 출력 (가장 중요)
            Debug.Log($"[PlayerMovement Toggle] Movement ENABLED. canMoveManual set to true.");
        }
    }

    // 모든 상호작용 및 자동 이동 플래그를 재설정합니다.
    public void ResetAllMovementStates()
    {
        isMovingToTarget = false;
        isInteracting = false;
        movement = Vector2.zero;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        if (animator != null) animator.SetBool("IsMoving", false);
        Debug.Log("[PlayerMovement] All movement states forcefully reset.");
    }
    // ----------------------------------------------------

    private void UpdateAnimationState(float horizontalInput)
    {
        bool isMoving = horizontalInput != 0 || movement.y != 0;
        if (animator != null) animator.SetBool("IsMoving", isMoving);

        if (horizontalInput > 0)
        {
            currentDirection = DIRECTION_RIGHT;
        }
        else if (horizontalInput < 0)
        {
            currentDirection = DIRECTION_LEFT;
        }

        if (animator != null) animator.SetInteger("Direction", currentDirection);
    }

   
    private void HandleAutomaticMovement()
    {
        float distanceX = Mathf.Abs(targetPosition.x - rb.position.x);

        if (distanceX <= arrivalDistance)
        {
            // 2. 도착: 이동 중지, 상호작용 시작
            isMovingToTarget = false;
            rb.position = targetPosition;
            if (animator != null) animator.SetBool("IsMoving", false);

            
            return;
        }

        // 3. 이동: 타겟 방향으로 X축 이동 및 걷기 애니메이션 재생
        if (animator != null) animator.SetBool("IsMoving", true);
        Vector2 direction = (targetPosition - rb.position).normalized;

        // 이동 중 방향 업데이트
        if (direction.x > 0) currentDirection = DIRECTION_RIGHT;
        else if (direction.x < 0) currentDirection = DIRECTION_LEFT;
        if (animator != null) animator.SetInteger("Direction", currentDirection);

        Vector2 velocity = new Vector2(direction.x * moveSpeed, 0);
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    // 상호작용 애니메이션을 트리거하고 제어하는 함수
    public void TriggerInteraction(string baseAnimName)
    {
        if (isInteracting) return;

        isInteracting = true;

        // 현재 방향에 따라 최종 애니메이션 이름 결정 (예: Hand_Right 또는 Hand_Left)
        string finalAnimName;
        if (currentDirection == DIRECTION_RIGHT)
        {
            finalAnimName = baseAnimName + "_Right";
        }
        else
        {
            finalAnimName = baseAnimName + "_Left";
        }

        if (animator != null) animator.SetTrigger(finalAnimName);

        float interactionDuration = GetAnimationClipLength(finalAnimName);

    }

   

    private float GetAnimationClipLength(string clipName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return 1.0f;

        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip.length;
            }
        }
        Debug.LogWarning($"애니메이션 클립 '{clipName}'의 길이를 찾을 수 없습니다. 기본값 1.0초를 사용합니다.");
        return 1.0f;
    }
}
