using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerPlatformerController : PhysicsObjects
{
    public float lifePlayer = 100f;
    public float maxSpeed = 2.0f;
    public float jumpTake = 12.0f;      
    public VariableJoystick variableJoystick;
    public float fly = 5f;    
    public float timeFly = 7.0f;
    public float speedRecoverTimeFly = 5;     
    public float dropDamage = 25;

    public bool useJoystick = true;

    public Button fire;
    
    [Header("Mega Boom custom")]
    public float boomCount = 0;
    public float speedRecBoomCount = .005f;
    public Text countMegaBoom;

    [Header("MONEY")]
    public float moneyCount = 0;
    public Text moneyCountT;

    [Header("FLY")]
    public float flyCount = 0;
    public Text flyCountT;

    [Header("LIFE")]
    public float lifeCount = 0;
    public Text lifeCountT;

    [Header("Hit collider triger")]
    public Vector2 boxSize;
    public Vector3 offset;
    public LayerMask maskHit;

    [Header("Indiсator's")]
    public Image flyMin;
    public Image flyMax;
    public Text scoresT;

    [Space]
    public Image lifeMin;
    public Image lifeMax;

    [Space]
    public GameObject dopolnitMenu;
    public GameObject newSound;
    public GameObject student;

    [Space]
    public SpriteRenderer bullet;

    public SpriteRenderer spriteRenderer;
    public Sprite[] bullets;
    public GameObject megaBoom;
    public AudioSource soundFly;

   
    private new CapsuleCollider2D collider;

    
    
    [SerializeField]
    private AudioClip[] sound;
    
    private float arrow = 0;
    private bool fireClick = false;
    private bool jumpClick = false;
    private bool flyClick = false;
    private bool megaBoomClick = false;
    private bool backClick = false;
    private bool flyAddClick = false;
    private bool lifeAddClick = false;
    private bool moneyAddClick = false;
    private bool boomAddClick = false;

    private Vector3 targetBullet;
    private Vector3 target;

    private Animator animator;
    private bool shoot;
    private bool runShoot;
    private bool addForceDamageUp = false;
    private float addForceDamage = 0;
    private float targetTimeFly = 0;
    private float targetLifePlayer = 0;
    private float targetBoomCount = 0;
    private float targetMoneyCount = 0;
    private float targetLifeCount = 0;
    private float targetFlyCount = 0;

    private new AudioSource audio;
    private float scores = 0;

    private void Awake()
    {        
        collider = GetComponent<CapsuleCollider2D>();               

        fire.onClick.AddListener(Fire);
               
        animator = GetComponent<Animator>();
        shoot = false;

        audio = GetComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.mute = false;
        audio.loop = false;

        targetBullet.x = 1.2f;
        targetBullet.y = 0.9f;

        targetTimeFly = timeFly;
        targetLifePlayer = lifePlayer; 
        targetBoomCount = boomCount;
        targetMoneyCount = moneyCount;
        targetLifeCount = lifeCount;
        targetFlyCount = flyCount;

        SinglVar.startGame = true;
        SinglVar.startRespVertol = true;
        SinglVar.scoresZubik = 0;
        SinglVar.scores = 0;
        SinglVar.scoresTank = 0;
        SinglVar.scoresVertolShooter = 0;
        SinglVar.scoresKristall = 0;

        int lastScene = PlayerPrefs.GetInt("lastScene", 0);

        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 1:
            case 2:
            case 5:
            case 9:
                lastScene = SceneManager.GetActiveScene().buildIndex;
                break;
        }

        PlayerPrefs.SetInt("lastScene", lastScene);

        if(SceneManager.GetActiveScene().buildIndex == 8)
        {
            student.SendMessage("Start", SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            dopolnitMenu.SendMessage("ShowMissionPanel", SendMessageOptions.DontRequireReceiver);
        }

        transform.position = SinglVar.startPosition;        

        if (SinglVar.fightClick)
        {
            SinglVar.portalNumber = PlayerPrefs.GetInt("fightClickPortalNumber", 0);
            dopolnitMenu.SendMessage("LoadPoinFightClick", transform, SendMessageOptions.DontRequireReceiver);             
            SinglVar.fightClick = false;
        }        
    }

    float dropDown = 0;    

    protected override void ComputeVelocity()
    {        
        if (SinglVar.startGame)
        {
            TrigerCollider();

            Vector2 move = Vector2.zero;

            if (useJoystick) move.x = arrow;            
            else move.x = Input.GetAxis("Horizontal");
            
            if(velocity.y < 0)
            {
                dropDown = velocity.y;
            }
            
            if(dropDown < -dropDamage && velocity.y >= 0)
            {
                targetLifePlayer += dropDown + dropDamage;
                dropDown = 0;                
            }             

            if (Mathf.Abs(move.x) > 0)
            {
                if (!audio.isPlaying)
                {
                    audio.PlayOneShot(sound[1]);
                }
            }            

            if (megaBoomClick)
            {
                Instantiate(megaBoom, transform.position, Quaternion.identity);
                audio.PlayOneShot(sound[3]);
            }

            float dist = targetTimeFly / timeFly;

            if (flyClick)
            {
                if (targetTimeFly > 0)
                {
                    velocity.y = fly;
                    grounded = false;
                    targetTimeFly -= Time.fixedDeltaTime * 3.5f;
                    if (!soundFly.isPlaying) soundFly.Play();                   
                }
                else soundFly.Stop();
            }

            else if (targetTimeFly < timeFly) 
            {
                targetTimeFly += speedRecoverTimeFly / 50000;
                soundFly.Stop();
            }

            else soundFly.Stop();

            if (dist > 0.3f)
            {
                flyMin.rectTransform.sizeDelta = new Vector2(100, 10);
                flyMax.rectTransform.sizeDelta = new Vector2(200 * (dist - 0.3f) / 0.7f, 10);
            }
            else
            {
                flyMin.rectTransform.sizeDelta = new Vector2(100 * (dist / 0.3f), 10);
                flyMax.rectTransform.sizeDelta = new Vector2(0, 10);
            }

            if (flyAddClick)
            { 
                if (targetTimeFly / timeFly <= 1)
                {
                    Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "lightningADD"); 
                    targetTimeFly += timeFly / 10;
                    if (targetTimeFly > timeFly) targetTimeFly = timeFly; 
                }
            }

            if (lifeAddClick)
            {
                if (targetLifePlayer / lifePlayer <= 1)
                {
                    Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "lightningADD");
                    targetLifePlayer += lifePlayer / 20;
                    if (targetLifePlayer > lifePlayer) targetLifePlayer = lifePlayer; 
                }
            }

            float distLife = targetLifePlayer / lifePlayer;

            if (distLife > 0.3f)
            {
                lifeMin.rectTransform.sizeDelta = new Vector2(100 , 10);
                lifeMax.rectTransform.sizeDelta = new Vector2(200 * (distLife - 0.3f) / 0.7f, 10);
            }
            else
            {
                lifeMin.rectTransform.sizeDelta = new Vector2(100 * (distLife / 0.3f), 10);
                lifeMax.rectTransform.sizeDelta = new Vector2(0, 10);
            }
            
            if (jumpClick && grounded)
            {
                velocity.y = jumpTake;
                audio.PlayOneShot(sound[0]);
            }            

            if (addForceDamageUp && !grounded)
            {
                velocity.y = jumpTake / 2.0f;
                addForceDamageUp = false;
            }
                  
            if (targetLifePlayer <= 0)
            {
                SinglVar.startGame = false;
                SinglVar.showADS++;
                dopolnitMenu.SendMessage("LoadPoint", transform, SendMessageOptions.DontRequireReceiver); 

            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            if (ShootFire)
            { 
                if (spriteRenderer.flipX)
                {
                    targetBullet.x = -1.2f;
                }
                else targetBullet.x = 1.2f;

                audio.PlayOneShot(sound[2]); 

                bullet.sprite = bullets[1];
                Instantiate(bullet, (transform.position + targetBullet), (spriteRenderer.flipX ? new Quaternion(0, 0, 180, 0) : new Quaternion(0, 0, 0, 0)));

                if (Mathf.Abs(move.x) > 0 && !runShoot)
                {
                    StartCoroutine(Rshoot());
                    runShoot = true;
                }
                else shoot = true;
                ShootFire = false;                
            }

            if (backClick)
            {
                SinglVar.startRespVertol = true;
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "backClick");
                SceneManager.LoadScene(0);                
            }

            if (moneyAddClick)
            {
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "lightningADD");
                dopolnitMenu.SendMessage("ShowGreenMagaz", SendMessageOptions.DontRequireReceiver); // открывает магазин из уровня
            }

            if (boomAddClick)
            {
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "lightningADD");
                dopolnitMenu.SendMessage("ShowVioletMagaz", SendMessageOptions.DontRequireReceiver); // открывает магазин из уровня
            }

            if (addForceDamage != 0)
            {
                move.x = addForceDamage;
                addForceDamage = 0;
            } 

            if(scores != SinglVar.scoresAll)
            {
                scores = SinglVar.scoresAll;
                if (scores < 0) SinglVar.scoresAll = scores = 0;
                scoresT.text = scores.ToString();                
            }

            if(boomCount != SinglVar.countBoom)
            {
                boomCount = SinglVar.countBoom;
                if (boomCount < 0) SinglVar.countBoom = boomCount = 0;
                countMegaBoom.text = boomCount.ToString();               
            }

            if(moneyCount != SinglVar.countMoney)
            {
                moneyCount = SinglVar.countMoney;
                if (moneyCount <= 0) SinglVar.countMoney = moneyCount = 0;
                moneyCountT.text = moneyCount.ToString();                
            }

            if(flyCount != SinglVar.countFly)
            {
                flyCount = SinglVar.countFly;
                if (flyCount <= 0) SinglVar.countFly = flyCount = 0;
                flyCountT.text = flyCount.ToString();                
            }

            if(lifeCount != SinglVar.countLife)
            {
                lifeCount = SinglVar.countLife;
                if (lifeCount <= 0) SinglVar.countLife = lifeCount = 0;
                lifeCountT.text = lifeCount.ToString();
                
            }
            
            if (!runShoot) animator.SetFloat("Horizontal", Mathf.Abs(move.x));
            animator.SetBool("Ground", grounded);
            animator.SetBool("Shoot", shoot);
            animator.SetBool("RunShoot", runShoot);

            targetVelocity = move * maxSpeed + new Vector2(getMove, 0);
            getMove = 0;
        } 

        fireClick = false;
        jumpClick = false;
        megaBoomClick = false;
        backClick = false;
        flyAddClick = false;
        lifeAddClick = false;
        moneyAddClick = false;
        boomAddClick = false;
    }

    public void ArrowMoveClick(float h)
    {
        arrow = h;

    }
        
    public void FireClick()
    {
        ShootFire = true;        
    }

    public void JumpClick()
    {
        jumpClick = true;
    }

    public void MegaBoomClick()
    {
        if (SinglVar.countBoom > 0)
        {
            megaBoomClick = true;
            SinglVar.countBoom -= 1;
        }
    }

    public void FlyClick(bool f)
    {
        if (targetTimeFly / timeFly >= 0.3f && f)
            flyClick = true;
        else flyClick = false;
    }

    public void FlyADDClick()
    {
        if (SinglVar.countFly > 0)
        {
            if (targetTimeFly / timeFly < 1)
            {
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "lightningADD");
                flyAddClick = true;
                SinglVar.countFly -= 1;
            }
            else
                flyAddClick = false;
        }
        else
        {
            Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "lightningADD");
            dopolnitMenu.SendMessage("ShowBlueMagaz", SendMessageOptions.DontRequireReceiver); // открывает магазин из уровня
        }
    }

    public void LifeADDClick()
    {
        if (SinglVar.countLife > 0)
        {
            if (targetLifePlayer / lifePlayer < 1)
            {
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "lightningADD");
                lifeAddClick = true;
                SinglVar.countLife -= 1;
            }
            else
                lifeAddClick = false;
        }
        else
        {
            Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "lightningADD");
            dopolnitMenu.SendMessage("ShowRedMagaz", SendMessageOptions.DontRequireReceiver);
        }
            
    }

    public void MoneyADDClick()
    {
        moneyAddClick = true;
    }

    public void BoomADDClick()
    {
        boomAddClick = true;
    }
    
    public void Back()
    {
        backClick = true;
    }

    IEnumerator FlyAmount()
    {
        yield return new WaitForSeconds(.2f); 
    }

    void OnDrawGizmosSelected()
    {
        target = transform.position + offset;

        Gizmos.DrawWireCube(target, boxSize);
    }

    void TrigerCollider()
    {
        target = transform.position + offset;

        Collider2D[] hitObject = Physics2D.OverlapBoxAll(target, boxSize, 0, maskHit);

        foreach (Collider2D hit in hitObject)
        {
            if (hit.name == "bot")
            {
                //print("Collision bot");
            }
                        
            if (hit.tag == "Platform")
            {
                grounded = true;
                hit.SendMessage("sendPlatform", gameObject, SendMessageOptions.DontRequireReceiver);                
            }

            if (hit.tag == "PlatformDD")
            {                
                hit.SendMessage("StartShake", gameObject, SendMessageOptions.DontRequireReceiver);                
            }

            if (hit.tag == "Kristall") 
            {
                addForceDamageUp = true;
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "damageKris");
                animator.SetTrigger("Damage");
                SinglVar.scoresAll -= 5f;
                targetLifePlayer -= .5f;                

                if (hit.transform.position.x > transform.position.x)
                {
                    addForceDamage = -20.0f;
                }
                else
                    addForceDamage = 20.0f;
            }

            if(hit.tag == "Bullet") 
            {
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "damageBullet");
                targetLifePlayer -= 5.0f;                
                Destroy(hit.gameObject);                
            }

            if(hit.tag == "VertolBullet")
            {
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "damageBullet");
                targetLifePlayer -= 10.0f;
                Destroy(hit.gameObject);                
            }

            if(hit.tag == "Fuel") 
            {                
                SinglVar.sccoresFuel += 50;
                SinglVar.scoresAll += 50;
                SinglVar.countFly += 1;
                SinglVar.SaveStars();
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "kristal");
                Destroy(hit.gameObject);
            }

            if(hit.tag == "Health") 
            {               
                SinglVar.scoresLife += 100;
                SinglVar.scoresAll += 100;
                SinglVar.countLife += 1;
                SinglVar.SaveStars();
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "kristal");
                Destroy(hit.gameObject);                
            }

            if(hit.tag == "BoomLight")
            {
                targetBoomCount += 1;
                SinglVar.countBoom += 1;
                SinglVar.scoresBoom += 200;
                SinglVar.scoresAll += 200;
                SinglVar.SaveStars();
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "kristal");
                Destroy(hit.gameObject);
            }

            if(hit.tag == "Money")
            {
                targetMoneyCount += 1;
                SinglVar.countMoney += 1;
                SinglVar.SaveStars();
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "kristal");
                Destroy(hit.gameObject);
            }

            if (hit.tag == "Coins")
            {
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "kristal");                
                SinglVar.scoresAll += 50f;
                Destroy(hit.gameObject);
            }

            if (hit.tag == "CoinStudent")
            {                
                student.SendMessage("CoinStudent", SendMessageOptions.DontRequireReceiver);
                SinglVar.scoresAll += 50f;                
                Destroy(hit.gameObject);
            }

            if (hit.tag == "MoneyStudent")
            {                
                student.SendMessage("MoneyStudent", SendMessageOptions.DontRequireReceiver);
                SinglVar.scoresAll += 50f;
                Destroy(hit.gameObject);
            }

            if (hit.tag == "HealthStudent")
            {                
                student.SendMessage("HealthStudent", SendMessageOptions.DontRequireReceiver);
                SinglVar.scoresAll += 50f;
                Destroy(hit.gameObject);
            }

            if (hit.tag == "FuelStudent")
            {                
                student.SendMessage("FuelStudent", SendMessageOptions.DontRequireReceiver);
                SinglVar.scoresAll += 50f;
                Destroy(hit.gameObject);
            }

            if (hit.tag == "BoomStudent")
            {               
                student.SendMessage("BoomStudent", SendMessageOptions.DontRequireReceiver);
                SinglVar.scoresAll += 50f;
                Destroy(hit.gameObject);
            }

            if (hit.tag == "Lava") 
            {
                targetLifePlayer -= 10.0f;                
                animator.SetTrigger("Damage");               
            }
            
            if (hit.tag == "Portal") 
            {
                hit.tag = "Untagged";
                AddStarsUpLevel(1);
                teleport = new Vector2(1076, 27); 
                Teleport.SetActive(true);
                SinglVar.portalNumber = 2; 
                SinglVar.SaveStars();
            }

            if (hit.tag == "Portal_1") 
            {
                hit.tag = "Untagged";
                AddStarsUpLevel(0);
                teleport = new Vector2(608, -1);
                Teleport.SetActive(true);
                SinglVar.portalNumber = 1;
                SinglVar.SaveStars();
            }

            if (hit.tag == "Portal_2")
            {
                hit.tag = "Untagged";
                AddStarsUpLevel(2);
                teleport = new Vector2(1442, 169);
                Teleport.SetActive(true);
                SinglVar.portalNumber = 3;
                SinglVar.SaveStars();
            }

            if (hit.tag == "Portal_3")
            {
                hit.tag = "Untagged";
                AddStarsUpLevel(3);
                teleport = new Vector2(2009, 170);
                Teleport.SetActive(true);
                SinglVar.portalNumber = 4;
                SinglVar.SaveStars();
            }
            
            if(hit.tag == "Portal_4")
            {
                hit.tag = "Untagged";
                AddStarsUpLevel(4);
                teleport = new Vector2(2458, 199);
                Teleport.SetActive(true);
                SinglVar.portalNumber = 5;
                SinglVar.SaveStars();
            }

            if(hit.tag == "Portal_5")
            {
                PlayerPrefs.SetFloat("miniPointX", 35.07f);
                PlayerPrefs.SetFloat("miniPointY", 2.27f);
                hit.tag = "Untagged";
                AddStarsUpLevel(5);
                Teleport.SetActive(true);
                SinglVar.portalNumber = 0;
                SinglVar.startPosition = new Vector3(35.07f, 2.27f, 0);
                SinglVar.SaveStars();
            }

            if (hit.tag == "Portal_6")
            {
                PlayerPrefs.SetFloat("miniPointX", 35.07f);
                PlayerPrefs.SetFloat("miniPointY", 2.27f);
                hit.tag = "Untagged";
                AddStarsUpLevel(13);
                Teleport.SetActive(true);
                SinglVar.portalNumber = 0;
                SinglVar.startPosition = new Vector3(35.07f, 2.59f, 0);
                SinglVar.SaveStars();
            }

            if (hit.tag == "Portal_Student")
            {
                hit.tag = "Untagged";
                Teleport.SetActive(true);
                SinglVar.portalNumber = 0;
                transform.position = SinglVar.startPosition;
                SinglVar.SaveStars();
                SceneManager.LoadScene(1);
                
            }
        }
    }

    private float getMove = 0;

    void TranslatePlatform(float _getMove)
    {
        getMove = _getMove;       
    }

    int countShoot = 0;

    void ShootTrigger()
    {        
        shoot = false;
    }

    private IEnumerator Rshoot()
    {        
        yield return new WaitForSeconds(0.5f);
        runShoot = false;
    }

    bool ShootFire = false;

    void Fire()
    {
        ShootFire = true;
    }

    void Damage()
    {
        animator.SetTrigger("Damage");
        SinglVar.scoresAll -= 5f;
        targetLifePlayer -= 2.5f;
    }

    void DamageBoss(float force) 
    {
        animator.SetTrigger("Damage");
        SinglVar.scoresAll -= 1f * force;
        targetLifePlayer -= force;
    }

    void DamageParticles()
    {        
        SinglVar.scoresAll -= 1f;
        targetLifePlayer -= .5f;
    }

    void Respawn()
    {
        targetLifePlayer = lifePlayer;
    }
    
    Vector3 teleport = Vector3.zero;

    public GameObject Teleport;

    
    void Portal()
    {
        transform.position = teleport;
    }

    void AddStarsUpLevel(int currentStage) 
    {
        int showADS = 3 - SinglVar.showADS;
        SinglVar.showADS = 0;
        int currentStars = showADS >= 0 ? showADS : 0;
        if (currentStars > SinglVar.stars[currentStage]) SinglVar.stars[currentStage] = currentStars;
        if (currentStage == SinglVar.stage) SinglVar.stage++;
        SinglVar.SaveStars();        
    }
}
