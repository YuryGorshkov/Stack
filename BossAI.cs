using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossAI : PhysicsObjects
{
    public float lifeEnemy = 250.0f;
    public float maxSpeed = 1.0f;
    public int deltaTime = 4;
    public float scores = 200;    
    public GameObject Bullet;
    public GameObject explosive;
    public GameObject fireLeft;
    public ParticleSystem fireLeftPart;
    public GameObject fireRight;
    public ParticleSystem fireRightPart;
    public GameObject miniMegaBoom;

    [Header("Hit collider triger")]
    public Vector2 boxSize;
    public Vector3 offset;
    public LayerMask maskHit;

    [Header("Hit damage triger")]
    public Vector2 boxSizeDamage;
    public Vector3 offsetDamage;
    public LayerMask maskBullet;

    [Header("Hit Boss-Stage triger")]
    public Collider2D[] colliders;
    public Vector2 bossSizeDamage;
    public Vector3 positionStageBoss;
    
    [Space]
    public Image lifeMax;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float getMove = -1.0f;
    private float targetLifeEnemy = 0;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();       
        animator = GetComponent<Animator>();
        targetLifeEnemy = lifeEnemy;
        StartCoroutine(StopPosition(.1f));
    }

    protected override void ComputeVelocity()
    {
        TrigerCollider();

        DamageCollider();

        BossCollider();

        Vector2 move = Vector2.zero;

        move.x = getMove;

        if (move.x > 0.01f)
        {
            spriteRenderer.flipX = true;           

            if (fireLeftPart.isPlaying) fireLeftPart.Stop();
            if (fireRightPart.isStopped) fireRightPart.Play();
        }
        else if (move.x < -0.01f)
        {
            spriteRenderer.flipX = false;           

            if (fireLeftPart.isStopped) fireLeftPart.Play();
            if (fireRightPart.isPlaying) fireRightPart.Stop();
        }

        targetVelocity = move * maxSpeed;       

        float distLife = targetLifeEnemy / lifeEnemy;
        if (distLife < 0.3f) lifeMax.color = Color.red;

        lifeMax.rectTransform.sizeDelta = new Vector2(1.5f * distLife, 0.14f);
    }
    
    private IEnumerator playerAttack()
    {
        yield return new WaitForSeconds(deltaTime);

        getMove = 0;

        animator.Play("Idle");

        if (fireLeftPart.isPlaying) fireLeftPart.Stop();
        if (fireRightPart.isPlaying) fireRightPart.Stop();        
        
        StartCoroutine(playerAttack());
    }   

    void OnDrawGizmosSelected()
    {
        Vector3 target = transform.position + offset;

        Gizmos.DrawWireCube(target, boxSize);

        Vector3 targetDamage = transform.position + offsetDamage;

        Gizmos.DrawWireCube(targetDamage, boxSizeDamage);

        Gizmos.DrawWireCube(positionStageBoss, bossSizeDamage);       
    }

    bool sleep = true;

    void TrigerCollider()
    {
        Collider2D[] hitObject = Physics2D.OverlapBoxAll(transform.position + offset, boxSize, 0, maskHit);
        
        foreach (Collider2D hit in hitObject)
        {
            if (hit.tag == "Player" && getMove !=0)
            {
                if (sleep)
                {
                    //StopAllCoroutines();
                    StartCoroutine(playerAttack()); // место разрыва, из-за этого прекращается преследование героя

                    animator.Play("RunShoot");
                    sleep = false;
                }

                if (hit.transform.position.x > transform.position.x)
                {
                    getMove = 1;
                }
                else
                {
                    getMove = -1;
                }                
            }            
        }        
    }

    void DamageCollider()
    {
        Collider2D[] hitObject = Physics2D.OverlapBoxAll(transform.position + offsetDamage, boxSizeDamage, 0, maskBullet);

        foreach (Collider2D hit in hitObject)
        {
            if (hit.tag == "BulletPlayer")
            {
                Destroy(hit.gameObject);
                targetLifeEnemy -= 10f;
                if (targetLifeEnemy <= 0)
                {
                    Destroyd(true);                    
                }
            }            
        }
    }

    bool areaBoss = false;

    void BossCollider()
    {
        if (areaBoss) return;
        
        Collider2D[] hitObject = Physics2D.OverlapBoxAll(positionStageBoss, bossSizeDamage, 0, maskHit);

        foreach (Collider2D hit in hitObject)
        {
            if (hit.tag == "Player")
            {
                areaBoss = true;

                foreach (var item in colliders)
                {
                    item.isTrigger = false;
                }
            }

        }
    }

    float stopPosition = 0;

    IEnumerator StopPosition(float time) // определяет когда босс останавливается ,,,
    {
        yield return new WaitForSeconds(time);

        if(getMove != 0)
        {
            if(stopPosition == transform.position.x)
            {
                getMove = -getMove;
            }
            else
            {
                stopPosition = transform.position.x;
            } 
        }

        StartCoroutine(StopPosition(.1f));
    }

    void Shoot() // работает через аниматор, проверить вкл/выкл
    {
        Vector3 targetBullet = Vector3.zero;
        if (spriteRenderer.flipX)
            targetBullet.x = 1.3f;
        else
            targetBullet.x = -1.3f;

        targetBullet.y = -0.2f;

        Instantiate(Bullet, transform.position + targetBullet, Quaternion.identity).SendMessage("direction", spriteRenderer.flipX ? -1 : 1, SendMessageOptions.DontRequireReceiver);
        
    }

    void MiniMegaBoom() // работает через аниматор, проверить вкл/выкл
    {
        getMove = 1;

        animator.Play("RunShoot");

        Instantiate(miniMegaBoom, transform.position, Quaternion.identity);

        print("MiniBoom");
    }

    void Destroyd(bool addScores = false)
    {
        Instantiate(explosive, transform.position + offset, Quaternion.identity);

        if (addScores)
        {
            SinglVar.scores += scores;
            SinglVar.scoresAll += scores;
        }

        foreach (var item in colliders)
        {
            item.isTrigger = true;
        }

        Destroy(gameObject);

    }
}
