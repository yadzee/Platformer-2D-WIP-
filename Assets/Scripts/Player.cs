using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 2.5f;
    public float Speed
    {
        get { return speed; }
        set
        {
            if (value > 0.5)
                speed = value;
        }
    }
    [SerializeField] private float force;
    [SerializeField] private Rigidbody2D rigidboby;
    [SerializeField] private float minimalHeight;
    [SerializeField] private bool isCheatMode;
    [SerializeField] private GroundDetection groundDetection;
    [SerializeField] private Vector3 direction;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Arrow arrow;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private float shootForce = 5;
    [SerializeField] private float cooldown;
    [SerializeField] private int arrowsCount = 3;
    [SerializeField] private Health health;
    [SerializeField] private Item item;
    [SerializeField] private BuffReciever buffReciever;
    public Health Health { get { return health; } }
    private Arrow currentArrow;
    private float bonusForce;
    private float bonusDamage;
    private float bonusHealth;
    private List<Arrow> arrowPool;
    private bool isJumping;
    private bool isCooldown;
    private UICharacterController controller;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        arrowPool = new List<Arrow>();
        for (int i = 0; i < arrowsCount; i++)
        {
            var arrowTemp = Instantiate(arrow, arrowSpawnPoint);
            arrowPool.Add(arrowTemp);
            arrowTemp.gameObject.SetActive(false);
        }

        buffReciever.OnBuffsChanged += ApplyBuffs;
    }

    public void InitController(UICharacterController uiController)
    {
        controller = uiController;
        controller.Fire.onClick.AddListener(CheckShoot);
        controller.Jump.onClick.AddListener(Jump);
    }

    #region Singleton
    public static Player Instance { get; set; }
    #endregion

    private void ApplyBuffs()
    {
        var forceBuff = buffReciever.Buffs.Find(t => t.type == BuffType.Force);
        var damageBuff = buffReciever.Buffs.Find(t => t.type == BuffType.Damage);
        var armorBuff = buffReciever.Buffs.Find(t => t.type == BuffType.Armor);
        bonusForce = forceBuff == null ? 0 : forceBuff.additiveBonus;
        bonusHealth = armorBuff == null ? 0 : armorBuff.additiveBonus;
        health.SetHealth((int)bonusHealth);
        bonusDamage = damageBuff == null ? 0 : damageBuff.additiveBonus;
    }

    private void FixedUpdate()
    {
        Move();
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
            Jump();
#endif
        animator.SetFloat("Speed", Mathf.Abs(rigidboby.velocity.x));
        CheckFall();
    }

    private void Move()
    {
        animator.SetBool("isGrounded", groundDetection.isGrounded);
        if (!isJumping && !groundDetection.isGrounded)
        {
            animator.SetTrigger("StartFall");
        }
        isJumping = isJumping && !groundDetection.isGrounded;
        direction = Vector3.zero;
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.A))
            direction = Vector3.left;
        if (Input.GetKey(KeyCode.D))
            direction = Vector3.right;
#endif 
        if (controller.Left.IsPressed)
            direction = Vector3.left;
        if (controller.Right.IsPressed)
            direction = Vector3.right;
        direction *= speed;
        direction.y = rigidboby.velocity.y;
        rigidboby.velocity = direction;

        if (direction.x > 0)
            spriteRenderer.flipX = false;
        if (direction.x < 0)
            spriteRenderer.flipX = true;
    }

    private void Jump()
    {
        if (groundDetection.isGrounded)
        {
            rigidboby.AddForce(Vector2.up * (force + bonusForce), ForceMode2D.Impulse);
            animator.SetTrigger("StartJump");
            isJumping = true;
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.OnClickPause();
        }
    }

    private void CheckShoot()
    {
        if (!isCooldown)
        {
            animator.SetTrigger("StartShoot");
        }
    }

    public void InitArrow()
    {
        currentArrow = GetArrowFromPool();
        currentArrow.SetImpulse(Vector2.right, 0, 0, this);
    }

    private void Shoot()
    {
        currentArrow.SetImpulse
                (Vector2.right, spriteRenderer.flipX ?
                -force * shootForce : force * shootForce, (int)bonusDamage, this);

        StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isCooldown = false;
    }

    private Arrow GetArrowFromPool()
    {
        if (arrowPool.Count > 0)
        {
            var arrowTemp = arrowPool[0];
            arrowPool.Remove(arrowTemp);
            arrowTemp.gameObject.SetActive(true);
            arrowTemp.transform.parent = null;
            arrowTemp.transform.position = arrowSpawnPoint.transform.position;
            return arrowTemp;
        }
        return Instantiate
                (arrow, arrowSpawnPoint.position, Quaternion.identity);
    }

    public void ReturnArrowToPool(Arrow arrowTemp)
    {
        if (!arrowPool.Contains(arrowTemp))
            arrowPool.Add(arrowTemp);

        arrowTemp.transform.parent = arrowSpawnPoint;
        arrowTemp.transform.position = arrowSpawnPoint.transform.position;
        arrowTemp.gameObject.SetActive(false);
    }

    private void CheckFall()
    {
        if (transform.position.y < minimalHeight && isCheatMode)
        {
            rigidboby.velocity = new Vector2(0, 0);
            transform.position = new Vector2(0, 0);
        }
        else if (transform.position.y < minimalHeight && !isCheatMode)
            Destroy(gameObject);
    }


}



