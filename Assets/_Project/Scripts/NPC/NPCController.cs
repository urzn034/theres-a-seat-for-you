using UnityEngine;

public class NPCController : MonoBehaviour
{
    public enum NPCState { WalkingToCounter, WaitingForInteraction, WalkingToSeat, SittingWaitingForCoffee, EnjoyingDrink, Leaving }

    [Header("Status Saat Ini")]
    public NPCState currentState = NPCState.WalkingToCounter;

    [Header("Visual")]
    public Animator anim;
    public Sprite idleSprite;
    public Sprite sitSprite;
    public Sprite sitDrinkSpriteA;
    public Sprite sitDrinkSpriteB;
    public float drinkSwitchInterval = 0.6f;
    public Vector3 counterOffset = new Vector3(0f, -1.5f, 0f);
    public Vector3 seatOffset = Vector3.zero;

    public bool gotPerfect;

    [Header("Kesabaran")]
    public float patienceDuration = 45f;
    public Vector3 barOffset = new Vector3(0f, 2.2f, 0f);
    public Vector2 barSize = new Vector2(1.2f, 0.15f);
    private float patience;
    private Transform barRoot, barFill;
    private static Sprite whiteSprite;
    public Transform exitPoint;

    public Transform leaveWaypoint;
    private bool passedWaypoint;

    private NPCVariant variant;
    public float walkFps = 6f;

    public DialogueData introDialogue;
    public DialogueData orderDialogue;

    [Header("Antrean")]
    public float queueSpacing = 1.2f;
    private static readonly System.Collections.Generic.List<NPCController> counterQueue = new System.Collections.Generic.List<NPCController>();

    private Transform doorPoint;
    private Transform counterPoint;
    private Transform seatPoint;
    private Recipe assignedRecipe;
    private string customerName;
    private GameManager gameManager;
    private SpriteRenderer sr;

    private float moveSpeed = 2.5f;
    private bool hasTriggeredOrder = false;
    private bool isServed = false;
    private bool moving;
    private bool passedExit;
    private bool stoodUp;
    private bool standInit;
    private Vector3 standPos;
    private float cachedTableX;
    private bool tableXCached;
    private float enjoyTimer = 0f;
    private float totalEnjoyDuration = 0f;

    public string CustomerName => customerName;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (anim == null) anim = GetComponent<Animator>();
    }

    private void OnDestroy()
    {
        counterQueue.Remove(this);
        if (barRoot != null) Destroy(barRoot.gameObject);
    }

    public void Initialize(Transform door, Transform counter, Transform seat, Recipe recipe, string name, GameManager gm)
    {
        doorPoint = door;
        counterPoint = counter;
        seatPoint = seat;
        assignedRecipe = recipe;
        customerName = name;
        gameManager = gm;

        currentState = NPCState.WalkingToCounter;
        counterQueue.RemoveAll(n => n == null);
        if (!counterQueue.Contains(this)) counterQueue.Add(this);
    }

    private void Update()
    {
        moving = false;
        if (gameManager == null) return;

        switch (currentState)
        {
            case NPCState.WalkingToCounter:
                if (counterPoint == null) return;
                int idx = Mathf.Max(0, counterQueue.IndexOf(this));
                MoveTowards2D(counterPoint.position + counterOffset + new Vector3(0f, -queueSpacing * idx, 0f), () => {
                    if (idx != 0) return;
                    currentState = NPCState.WaitingForInteraction;
                    if (!hasTriggeredOrder)
                    {
                        hasTriggeredOrder = true;
                        Debug.Log(customerName + " tiba di kasir. Tekan E untuk tanya pesanan!");
                        var dm = FindObjectOfType<DialogueManager>();
                        if (dm != null && introDialogue != null) dm.StartDialogue(introDialogue.lines);
                    }
                });
                break;

            case NPCState.WalkingToSeat:
                if (seatPoint == null) return;
                MoveTowards2D(seatPoint.position + seatOffset, () => {
                    currentState = NPCState.SittingWaitingForCoffee;
                    patience = patienceDuration;
                    Debug.Log(customerName + " duduk dan menunggu: " + (assignedRecipe != null ? assignedRecipe.drinkName : "-"));
                });
                break;

            case NPCState.SittingWaitingForCoffee:
                patience -= Time.deltaTime;
                if (patience <= 0f) { patience = 0f; Debug.Log(customerName + " kehabisan sabar!"); }
                break;

            case NPCState.EnjoyingDrink:
                enjoyTimer += Time.deltaTime;
                if (enjoyTimer >= totalEnjoyDuration)
                {
                    currentState = NPCState.Leaving;
                    gameManager.CompletePaymentAndLeave(this, assignedRecipe);
                }
                break;

            case NPCState.Leaving:
                if (doorPoint == null)
                {
                    Destroy(gameObject);
                    return;
                }
                if (!stoodUp && seatPoint != null)
                {
                    if (!standInit)
                    {
                        standInit = true;
                        if (!tableXCached) { cachedTableX = GetTableX(); tableXCached = true; }
                        float dir = seatPoint.position.x > cachedTableX ? 1f : -1f;
                        standPos = transform.position + new Vector3(dir * 1.2f, 0f, 0f);
                    }
                    MoveTowards2D(standPos, () => { stoodUp = true; });
                    break;
                }
                if (!passedWaypoint && leaveWaypoint != null)
                {
                    MoveTowards2D(leaveWaypoint.position, () => { passedWaypoint = true; }, true);
                    break;
                }
                if (!passedExit && exitPoint != null)
                {
                    MoveTowards2D(exitPoint.position, () => { passedExit = true; }, true);
                    break;
                }
                MoveTowards2D(doorPoint.position, () => {
                    Destroy(gameObject);
                });
                break;
        }
    }

    private void LateUpdate()
    {
        UpdateBar();
        if (sr == null) return;

        bool sitting = currentState == NPCState.SittingWaitingForCoffee || currentState == NPCState.EnjoyingDrink;

        if (moving)
        {
            if (variant != null && variant.walk != null && variant.walk.Length > 0)
                sr.sprite = variant.walk[(int)(Time.time * walkFps) % variant.walk.Length];
            else if (anim != null) anim.enabled = true;
            return;
        }

        if (anim != null) anim.enabled = false;

        if (sitting)
        {
            Sprite s = sitSprite;
            if (currentState == NPCState.EnjoyingDrink && sitDrinkSpriteA != null && sitDrinkSpriteB != null)
                s = ((int)(enjoyTimer / drinkSwitchInterval) % 2 == 0) ? sitDrinkSpriteA : sitDrinkSpriteB;
            if (s != null) sr.sprite = s;

            if (seatPoint != null)
            {
                if (!tableXCached) { cachedTableX = GetTableX(); tableXCached = true; }
                sr.flipX = seatPoint.position.x > cachedTableX;
            }
        }
        else
        {
            if (idleSprite != null) sr.sprite = idleSprite;
        }
    }

    public void InteractWithCustomer()
    {
        if (currentState == NPCState.WaitingForInteraction)
        {
            gameManager.TriggerOrderBubble(this, assignedRecipe, customerName);
            counterQueue.Remove(this);
            currentState = NPCState.WalkingToSeat;
        }
    }

    public void ReceiveCoffee()
    {
        if (isServed) return;
        isServed = true;

        currentState = NPCState.EnjoyingDrink;
        enjoyTimer = 0f;
        totalEnjoyDuration = Random.Range(30f, 60f);
        Debug.Log(customerName + " menerima kopi dan mulai menikmati!");
    }

    public void ApplyVariant(NPCVariant v)
    {
        if (v == null) return;
        variant = v;
        idleSprite = v.idle; sitSprite = v.sit;
        sitDrinkSpriteA = v.drinkA; sitDrinkSpriteB = v.drinkB;
        if (v.animator != null && anim != null)
            anim.runtimeAnimatorController = v.animator;
        else if (anim != null) { anim.enabled = false; anim = null; }
    }

    public Recipe GetAssignedRecipe()
    {
        return assignedRecipe;
    }

    private float GetTableX()
    {
        foreach (TableService t in FindObjectsOfType<TableService>())
            if (t.assignedSeatPoints != null && t.assignedSeatPoints.Contains(seatPoint))
                return t.transform.position.x;
        return seatPoint.position.x;
    }

    private void MoveTowards2D(Vector3 targetPos, System.Action onArrival, bool yFirst = false)
    {
        targetPos.z = transform.position.z;
        Vector3 p = transform.position;

        moving = Vector3.Distance(p, targetPos) >= 0.1f;
        if (!moving)
        {
            transform.position = targetPos;
            onArrival?.Invoke();
            return;
        }

        float step = moveSpeed * Time.deltaTime;
        bool needX = Mathf.Abs(targetPos.x - p.x) > 0.05f;
        bool needY = Mathf.Abs(targetPos.y - p.y) > 0.05f;
        bool doY = yFirst ? needY : (!needX && needY);

        if (doY)
            transform.position = new Vector3(p.x, Mathf.MoveTowards(p.y, targetPos.y, step), p.z);
        else if (needX)
        {
            transform.position = new Vector3(Mathf.MoveTowards(p.x, targetPos.x, step), p.y, p.z);
            sr.flipX = targetPos.x < p.x;
        }
    }

    private static Sprite GetWhite()
    {
        if (whiteSprite == null)
        {
            var t = new Texture2D(1, 1);
            t.filterMode = FilterMode.Point;
            t.SetPixel(0, 0, Color.white);
            t.Apply();
            whiteSprite = Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 1f);
        }
        return whiteSprite;
    }

    private void CreateBar()
    {
        barRoot = new GameObject("PatienceBar").transform;
        var bg = new GameObject("BG").AddComponent<SpriteRenderer>();
        bg.transform.SetParent(barRoot, false);
        bg.sprite = GetWhite();
        bg.color = new Color(0.1f, 0.08f, 0.1f, 1f);
        bg.transform.localScale = new Vector3(barSize.x + 0.1f, barSize.y + 0.1f, 1f);
        bg.transform.localPosition = new Vector3(-(barSize.x + 0.1f) / 2f, 0f, 0f);
        var fill = new GameObject("Fill").AddComponent<SpriteRenderer>();
        fill.transform.SetParent(barRoot, false);
        fill.sprite = GetWhite();
        fill.transform.localPosition = new Vector3(-barSize.x / 2f, 0f, 0f);
        if (sr != null) { bg.sortingLayerID = fill.sortingLayerID = sr.sortingLayerID; }
        bg.sortingOrder = 100; fill.sortingOrder = 101;
        barFill = fill.transform;
    }

    private void UpdateBar()
    {
        bool show = currentState == NPCState.SittingWaitingForCoffee;
        if (show && barRoot == null) CreateBar();
        if (barRoot == null) return;
        barRoot.gameObject.SetActive(show);
        if (!show) return;
        float r = Mathf.Clamp01(patience / patienceDuration);
        barRoot.position = transform.position + barOffset;
        barFill.localScale = new Vector3(barSize.x * r, barSize.y, 1f);
        barFill.GetComponent<SpriteRenderer>().color = r > 0.5f ? new Color(0.45f, 0.8f, 0.4f) : r > 0.25f ? new Color(0.95f, 0.8f, 0.3f) : new Color(0.9f, 0.3f, 0.3f);
    }
}