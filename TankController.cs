using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TankController : MonoBehaviour
{
    public Transform tank;
    public Transform tower;
    public VariableJoystick variableJoystick;
    public GameObject megaBoom;
    public float lifePlayer = 100;
    public float speed = 5f;
    public float offsetAngle = 0;
    public int numberScene;
    public GameObject newSound;

    public bool useJoystick = true;

    [Header("Mega Boom custom")]
    public float boomCount = 0;
    public float speedRecBoomCount = .005f;
    public Text countMegaBoom;

    [Header("MONEY")]
    public float moneyCount = 0;
    public Text moneyCountT;

    [Header("Hit collider triger")]
    public Vector2 boxSize;
    public Vector3 boxSizeDetect;
    public Vector3 offset;
    public LayerMask maskHit;
    private Vector3 target;

    [Header("Indiсator's Menu")]
    public Image lifeMin;
    public Image lifeMax;
    public Text scoresT;

    [Space]
    public GameObject dopolnitMenu; // обращаемся для отображения интерфейса и запуска рекламы

    private float targetLifePlayer = 0;
    private float targetBoomCount = 0;
    private float targetMoneyCount = 0;
    private float scores = 0;

    CharacterController controller;

    Vector3 moveDirection = Vector3.zero;   

    private bool megaBoomClick = false;
    private bool backClick = false;
    private bool moneyAddClick = false;
    private bool boomAddClick = false;

    void Start()
    {
        int lastScene = PlayerPrefs.GetInt("lastScene", 0);

        if (SceneManager.GetActiveScene().buildIndex == 6)
        {
            lastScene = 6;
        }

        PlayerPrefs.SetInt("lastScene", lastScene);

        SinglVar.startGame = true;
        controller = GetComponent<CharacterController>();        
        targetLifePlayer = lifePlayer;
        targetBoomCount = boomCount;
        targetMoneyCount = moneyCount;

        dopolnitMenu.SendMessage("ShowMissionPanel", SendMessageOptions.DontRequireReceiver);
    }   

    void Update()
    {
        if (SinglVar.startGame)
        {
            TrigerCollider();

            Move();

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

            if (megaBoomClick)
            {
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "megaBoom");
                Instantiate(megaBoom, transform.position, Quaternion.identity);
            }

            if (moneyAddClick)
            {
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "lightningADD");
                dopolnitMenu.SendMessage("ShowGreenMagaz", SendMessageOptions.DontRequireReceiver); 
            }

            if (boomAddClick)
            {
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "lightningADD");
                dopolnitMenu.SendMessage("ShowVioletMagaz", SendMessageOptions.DontRequireReceiver); 
            }

            if (backClick)
            {
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "backClick");
                SceneManager.LoadScene(0);
            }

            megaBoomClick = false;
            backClick = false;
            moneyAddClick = false;
            boomAddClick = false;           

            if (scores != SinglVar.scoresAll)
            {
                scores = SinglVar.scoresAll;
                if (scores < 0) SinglVar.scoresAll = scores = 0;
                scoresT.text = scores.ToString();
            }

            if (boomCount != SinglVar.countBoom)
            {
                boomCount = SinglVar.countBoom;
                if (boomCount < 0) SinglVar.countBoom = boomCount = 0;
                countMegaBoom.text = boomCount.ToString();
                print(SinglVar.countBoom);
            }

            if (moneyCount != SinglVar.countMoney)
            {
                moneyCount = SinglVar.countMoney;
                if (moneyCount <= 0) SinglVar.countMoney = moneyCount = 0;
                moneyCountT.text = moneyCount.ToString();
                print(SinglVar.countMoney);
            }

            if (targetLifePlayer <= 0)
            {
                dopolnitMenu.SendMessage("CosmoLoad", transform, SendMessageOptions.DontRequireReceiver);
                SinglVar.showADS++;
            }
        }
    }

    void Move()
    {
        Vector3 direction = Vector3.zero;

        direction.x = variableJoystick.Horizontal;
        direction.y = variableJoystick.Vertical;

        float angle = Mathf.Atan2(variableJoystick.Vertical, variableJoystick.Horizontal) * Mathf.Rad2Deg + offsetAngle;

        if (direction != Vector3.zero)
            tank.rotation = Quaternion.Euler(0, 0, angle);

        direction = transform.TransformDirection(direction);
        direction *= speed;

        controller.Move(direction * Time.deltaTime);

        tower.rotation = Quaternion.Lerp(tower.rotation, tank.rotation, Time.deltaTime * 3);
    }

    void OnDrawGizmosSelected()
    {
        target = transform.position + offset;

        Gizmos.DrawWireCube(target, boxSize);
        Gizmos.DrawWireCube(target, boxSizeDetect);
    }

    void TrigerCollider()
    {
        target = transform.position + offset;

        Collider2D[] hitObject = Physics2D.OverlapBoxAll(target, boxSize, 0, maskHit);
        
        foreach (Collider2D hit in hitObject)
        {
            print(hit.tag);

            if( hit.tag == "TankPort")
            {
                AddStarsUpLevel(11);
                print("TankPort");
            }

            if (hit.tag == "VertolBullet") 
            {
                targetLifePlayer -= 10.0f;
                SinglVar.scoresAll -= 50f;
                print(targetLifePlayer);
                Destroy(hit.gameObject);
            }

            if (hit.tag == "Health") 
            {
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "kristal");
                targetLifePlayer += lifePlayer / 25;
                if (targetLifePlayer > lifePlayer) targetLifePlayer = lifePlayer;
                SinglVar.scoresLife += 100;
                SinglVar.scoresAll += 100;
                Destroy(hit.gameObject);
            }

            if (hit.tag == "BoomLight")
            {
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "kristal");
                targetBoomCount += 1;
                SinglVar.countBoom += 1;
                SinglVar.scoresBoom += 200;
                SinglVar.scoresAll += 200;
                Destroy(hit.gameObject);
            }

            if (hit.tag == "Money")
            {
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "kristal");
                targetMoneyCount += 1;
                SinglVar.countMoney += 1;
                Destroy(hit.gameObject);
            }

            if (hit.tag == "Coins")
            {
                Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "kristal");
                SinglVar.scoresAll += 50f;
                Destroy(hit.gameObject);
            }
        }       

        Collider[] detectObject = Physics.OverlapBox(target, boxSizeDetect, Quaternion.identity, maskHit);

        foreach (Collider detect in detectObject)
        {
            if (detect.tag == "inactive")
            {
                detect.SendMessage("DetectPlayer", SendMessageOptions.DontRequireReceiver);
            }

        }
    }

    void MinaDamage()
    {
        Instantiate(newSound, transform.position, Quaternion.identity).SendMessage("Play", "zubikBOOM");
        targetLifePlayer -= 10f;
        SinglVar.scoresTank -= 50f;
        print(targetLifePlayer);
        print("MINA_tank");
    }

    void LavaDamage()
    {
        
        targetLifePlayer -= 0.5f;
        SinglVar.scoresTank -= 1f;
        print(targetLifePlayer);
        print("LAVA_TANK");
    }

    void Respawn()
    {
        targetLifePlayer = lifePlayer;
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

    void AddStarsUpLevel(int currentStage) // добавление звезд и переход на след уровень
    {
        int showADS = 3 - SinglVar.showADS;
        SinglVar.showADS = 0;

        int currentStars = showADS >= 0 ? showADS : 0;
        if (currentStars > SinglVar.stars[currentStage]) SinglVar.stars[currentStage] = currentStars;
        print(SinglVar.stars[currentStage] + " stars");

        if (currentStage == SinglVar.stage) SinglVar.stage++;
        SinglVar.SaveStars();
        print("AddStarsUpLevel " + SinglVar.stage);        
    }
}
