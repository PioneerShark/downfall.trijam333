using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Collidable : MonoBehaviour
{
    [SerializeField] public HeatState heatState = HeatState.extreme;
    private SpriteRenderer sprite;
    private Collider2D col;
    private Animator animator;

    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        //hitSound = GetComponent<AudioSource>();
        sprite = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        switch (heatState)
        {
            case HeatState.low:
                sprite.color = Color.green;
                break;
            case HeatState.mid:
                sprite.color = Color.yellow;
                break;
            case HeatState.high:
                sprite.color = new Color(0.96f, 0.44f, 0.08f);
                break;
            case HeatState.extreme:
                sprite.color = Color.red;
                break;
            case HeatState.unbreakable:
                sprite.color = Color.white;
                break;



        }
    }

    public void Hit()
    {
        col.enabled = false;
        sprite.enabled = false;
        Invoke("Destroy", 5f);
    }
    private void Destroy()
    {
        Destroy(this.gameObject);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (animator != null && Random.Range(0,5000) == 1) {
            animator.Play("Blink");
        }
    }
}
