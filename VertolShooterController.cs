using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VertolShooterController : MonoBehaviour
{
    public VariableJoystick variableJoystick;
    public GameObject megaBoom;
    public GameObject newSound;
    public float lifePlayer = 100;
    public float speed = 5;
    public float timeShoot = 0.5f;    
    public int numberScene;
    public GameObject bullet;
    public Transform bulletPosition;
    public GameObject respBomb;
    public bool useJoystick = true;

    [Header("Hit collider triger")]
    public Vector2 boxSize;
    public Vector3 offset;
    public LayerMask maskHit;
    private Vector3 target;

    [Header("Indiсator's Menu")]
    public Image lifeMin;
    public Image lifeMax;
    public Text scoresT;

    [Header("Mega Boom custom")]
    public float boomCount = 0;
    public float speedRecBoomCount = .005f;
    public Text countMegaBoom;

    [Header("MONEY")]
    public float moneyCount = 0;
    public Text moneyCountT;

    [Space]
    public GameObject dopolnitMenu; // обращаемся для отображения интерфейса и запуска рекламы

    private float targetLifePlayer = 0;
    private float targetBoomCount = 0;
    private float targetMoneyCount = 0;
    private float scores = 0;

    CharacterController controller;

    Vector3 moveDirection = Vector3.zero;
    float vertical = 0;
    private bool fireClick = false;
    private bool megaBoomClick = false;
    private bool shootFire = false;
    private bool backClick = false;
    private bool moneyAddClick = false;
    private bool boomAddClick = false;

    void Start()
    {
        SinglVar.startGame = true;
        SinglVar.scoresVertolShooter = 0;
        
        controller = GetComponent<CharacterController>();
        targetLifePlayer = lifePlayer;
        targetBoomCount = boomCount;
        targetMoneyCount = moneyCount;
        StartCoroutine(LevelUp());
        shootFire = true;

        int lastScene = PlayerPrefs.GetInt("lastScene", 0);

        if (SceneManager.GetActiveScene().buildIndex == 3)
        {
            lastScene = 3;
        }

        PlayerPrefs.SetInt("lastScene", lastScene);

        dopolnitMenu.SendMessage("ShowMissionPanel", SendMessageOptions.DontRequireReceiver);
    }


    void Update()
    {
        if (SinglVar.startGame)
        {
            TrigerCollider();           

            if (useJoystick)
            {
                float _y = variableJoystick.Vertical;

                if (_y > 0.1f)
                {
                    _y = 1f;

                }
                else if (_y < -0.1f)
                {
                    _y = -1f;
                }

                else _y = 0f;

                if (_y == 0f)
                {
                }

                moveDirection.y = _y;               
            }
            else
            {
                moveDirection.y = Input.GetAxis("Vertical");
            }

            if (fireClick)
            {
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "cosmoletShoot");
                Instantiate(bullet, bulletPosition.position, Quaternion.identity);
            }

            float distLife = targetLifePlayer / lifePlayer;

            if (distLife > 0.3f)
            {
                lifeMin.rectTransform.sizeDelta = new Vector2(100, 10);
                lifeMax.rectTransform.sizeDelta = new Vector2(200 * (distLife - 0.3f) / 0.7f, 10);
            }
            else
            {
                lifeMin.rectTransform.sizeDelta = new Vector2(100 * (distLife / 0.3f), 10);
                lifeMax.rectTransform.sizeDelta = new Vector2(0, 10);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                Instantiate(bullet, bulletPosition.position, Quaternion.identity);
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                respBomb.SendMessage("UpLevel", SendMessageOptions.DontRequireReceiver);
            }
            if (megaBoomClick)
            {
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "megaBoom");
                Instantiate(megaBoom, transform.position, Quaternion.identity);
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

            if (shootFire)
            {
                StartCoroutine(ShootFire());
            }
            if (backClick)
            {
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "backClick");
                SceneManager.LoadScene(0);
            }
            
            moveDirection.y = transform.TransformDirection(moveDirection).y;
            moveDirection.y *= speed;

            controller.Move(moveDirection * Time.deltaTime);

            fireClick = false;
            megaBoomClick = false;
            backClick = false;
            moneyAddClick = false;
            boomAddClick = false;

            // menu scores

            if (scores != SinglVar.scoresVertolShooter)
            {
                scores = SinglVar.scoresVertolShooter;
                if (scores < 0) SinglVar.scoresVertolShooter = scores = 0;
                scoresT.text = scores.ToString();
            }

            if (boomCount != SinglVar.countBoom)
            {
                boomCount = SinglVar.countBoom;
                if (boomCount < 0) SinglVar.countBoom = boomCount = 0;
                countMegaBoom.text = boomCount.ToString();
            }

            if (moneyCount != SinglVar.countMoney)
            {
                moneyCount = SinglVar.countMoney;
                if (moneyCount <= 0) SinglVar.countMoney = moneyCount = 0;
                moneyCountT.text = moneyCount.ToString();                
            }

            if (scores >= 5000)
            {
                AddStarsUpLevel(7);
                SinglVar.startGame = false;
                SceneManager.LoadScene(numberScene);
            }

            if (targetLifePlayer <= 0) // если герой умер, отправка сообщения на проигрывание рекламы
            {
                dopolnitMenu.SendMessage("CosmoLoad", transform, SendMessageOptions.DontRequireReceiver); 
                SinglVar.showADS++;                
            }
        }
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
            
            if (hit.tag == "KristallBomb") // ущерб и реакция от столкновения с метеоритами
            {                
                targetLifePlayer -= 5f;
                SinglVar.scoresVertolShooter -= 197f;
                hit.SendMessage("Damage", SendMessageOptions.DontRequireReceiver);                
            }

            if (hit.tag == "Health") // пополнение здоровья от собранных красных молний
            {
                targetLifePlayer += lifePlayer / 25;
                if (targetLifePlayer > lifePlayer) targetLifePlayer = lifePlayer;
                SinglVar.scoresLife += 100;
                SinglVar.scoresAll += 100;
                Destroy(hit.gameObject);
            }

            if (hit.tag == "BoomLight")
            {
                targetBoomCount += 1;
                SinglVar.countBoom += 1;
                SinglVar.scoresBoom += 200;
                SinglVar.scoresAll += 200;
                Destroy(hit.gameObject);
            }

            if (hit.tag == "Money")
            {
                targetMoneyCount += 1;
                SinglVar.countMoney += 1;
                Destroy(hit.gameObject);                
            }

        }
    }

    void AddStarsUpLevel(int currentStage) // добавление звезд и переход на след уровень
    {
        int showADS = 3 - SinglVar.showADS;
        SinglVar.showADS = 0;

        int currentStars = showADS >= 0 ? showADS : 0;
        if (currentStars > SinglVar.stars[currentStage]) SinglVar.stars[currentStage] = currentStars;
        print(SinglVar.stars[currentStage] + " stars");

        if (currentStage == SinglVar.stage) SinglVar.stage++;
        SinglVar.SaveStars();        
    }

    public void FireClick()
    {
        fireClick = true;
    }

    public void MegaBoomClick()
    {
        if (SinglVar.countBoom > 0)
        {
            megaBoomClick = true;
            SinglVar.countBoom -= 1;
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

    public void BackClick()
    {
        backClick = true;
    }

    void Respawn()
    {
        targetLifePlayer = lifePlayer;
    }

    IEnumerator LevelUp()
    {        
        yield return new WaitForSeconds(10f);
        respBomb.SendMessage("UpLevel", SendMessageOptions.DontRequireReceiver);
        StartCoroutine(LevelUp());
    }

    IEnumerator ShootFire()
    {        
        Instantiate(bullet, bulletPosition.position, Quaternion.identity);
        shootFire = false;
        yield return new WaitForSeconds(timeShoot);
        shootFire = true;
    }
}
