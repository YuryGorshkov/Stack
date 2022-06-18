using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegaBoom : MonoBehaviour
{
    private float radius = 1.0f;
    private MaterialPropertyBlock _propertyBlock;
    private float _elapsedTime;
    private SpriteRenderer _renderer;
  
    public LayerMask maskHit;
    public float Duration;
    [Space]
    public AnimationCurve ScaleProgress;
    public Vector3 ScalePower;
    [Space]
    public AnimationCurve PropertyProgress;
    public float PropertyPower;
    [Space]
    public AnimationCurve AlphaProgress;
    [Space]
    public bool destroy = true;

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _propertyBlock = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        _elapsedTime = 0f;
    }

    void Update()
    {
        TrigerCollider();

        if (_elapsedTime < Duration)
        {
            var progress = _elapsedTime / Duration;

            var scale = ScaleProgress.Evaluate(progress) * ScalePower;
            var property = PropertyProgress.Evaluate(progress) * PropertyPower;
            var alpha = AlphaProgress.Evaluate(progress);

            transform.localScale = scale;
            //_renderer.material.SetFloat("_DisplacementPower", property);
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat("_DisplacementPower", property / transform.localScale.x);
            _renderer.SetPropertyBlock(_propertyBlock);

            var color = _renderer.color;
            color.a = alpha;
            _renderer.color = color;

            _elapsedTime += Time.deltaTime;
            radius = scale.y;
        }
        else if (destroy)
        {
            Destroy(gameObject);
        }
        else
        {
            _elapsedTime = 0;
        }
    }

    void TrigerCollider()
    {
        Collider2D[] hitObject = Physics2D.OverlapCircleAll(transform.position, radius, maskHit);

        foreach (Collider2D hit in hitObject)
        {
            hit.SendMessage("Destroyd", true, SendMessageOptions.DontRequireReceiver);           
        }
    }
}