using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // требуем в инспетокре компонент физическое тело для исключения человеческой ошибки

public class PhysicsObjects : MonoBehaviour
{
    [Header("Physics modifier")]
    public float minGroundNormalY = 0.65f; // объявляем уклон коллайдера (спрайта), где 0,65f это примерно 45 градусов, а 0,9f это самый минимальный уклон или почти ровный тайл без кочек и подъёмов, а 0,1f это максимальный уклон до 75 градусов
    public float gravityModifier = 3f; // объявляем гравитационный модификатор

    protected Vector2 targetVelocity; // целевая скорость - своего рода управление скоростью
    protected bool grounded; // объявляем приземление, объявляем переменную которая обозначает на земле он или нет (isGrounded)
    protected Vector2 groundNormal; // создаём вектор плоскости для приземления
    protected Rigidbody2D rb2d; // объявляем физ. тело
    protected Vector2 velocity; // объявляем ускорение/падение/ стартовая скорость
    protected ContactFilter2D contactFilter; // это фильтр коллайдеров, для того чтобы фильтр отрабатывал нужно убрать все триггеры
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16]; // буфер коллайдеров 

    protected const float minMoveDistance = 0.001f; // константа минимального расстояния до столкновения, как только он достигает этого расстояния то скорость замедляется резко;
    protected const float shellRadius = 0.05f; // константа расстояния до столкновения - "утолщает" коллайдер и производит наполнение объекта, это не позволит просочится через объект столкновения

    void OnEnable()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        contactFilter.useTriggers = false; // отключаем все триггеры для физического столкновения.
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer)); // это маска столкновения
        contactFilter.useLayerMask = true; //включаем маску столкновений
    }

    void Update()
    {
        targetVelocity = Vector2.zero; // целевая скорость по умолчанию равна нулю, чтобы избежать покадрового увеличения скорости     
        
    }

    protected virtual void ComputeVelocity() // пустая функция для наследования и вычисления скорости
    {

    }

    // используется физика, которая не любит плавающих объектов, необходимо чётко обозначить физику и фиксировать фпс.
    void FixedUpdate()
    {
        ComputeVelocity(); // вызываем функцию расчёта скорости
        velocity += gravityModifier * Physics2D.gravity * Time.deltaTime; // применяем гравитацию и адаптируем под фрейм
        velocity.x = targetVelocity.x; // устанавливаем целевую скорость

        grounded = false; // регистрируем состояние по умолчанию когда мы всегда летим (типа)

        Vector2 deltaPosition = velocity * Time.deltaTime; // вычисляем следующее положение объекта

        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x); // создаём вектор движения по горизонтали (вдоль земли) одинаково и для горизонтали и для склона

        Vector2 move = moveAlongGround * deltaPosition.x; // передаём положение по горизонтали

        Movement(move, false);

        move = deltaPosition.y * Vector2.up; // передаём положение по вертикали

        Movement(move, true);
    }

    void Movement(Vector2 move, bool movementY) // movementY сообщает о движении по вертикали
    {
        float distance = move.magnitude; // это расстояние до точки перемещения 

        if (distance > minMoveDistance)
        {
            int count = rb2d.Cast(move, contactFilter, hitBuffer, distance + shellRadius); // функция которая сохраняети результаты столкновения коллайдеров в буфер

            for (int i = 0; i < count; i++)
            {
                Vector2 currentNormal = hitBuffer[i].normal; // присваиваем текущую нормаль(перпендикуляр) точек вектора 
                if (currentNormal.y > minGroundNormalY) // сравниваем угол объекта с которым собираемся столкнуться
                {
                    grounded = true; // приземлились
                    if (movementY) // проверяем есть ли движение по вертикали
                    {
                        groundNormal = currentNormal; // сохраняем значение движение по вретикали (останавливаем падение и фиксируем точку)
                        currentNormal.x = 0; // запрещвем перемещение по горизонтали, чтобы не сдвигался в сторону
                    }
                }

                float projection = Vector2.Dot(velocity, currentNormal); // делаем перемещение плавным и убираем зависимость от мощности компютера (чем больше фпс тем плавнее будет перемещение)
                if (projection < 0)
                {
                    velocity = velocity - projection * currentNormal; // ограничение скорости падения
                }

                float modifiedDistance = hitBuffer[i].distance - shellRadius; // создаем "толщину" минимальную дистанцию по окружности радиуса
                distance = modifiedDistance < distance ? modifiedDistance : distance; // вычисляет минимально необходимую для остановки дистанция, которая не позволит провалиться в коллайдер объекта столкновения
            }
        }

        rb2d.position = rb2d.position + move.normalized * distance; // применяем вектор движения и добавляем нормализацию
    }

}
