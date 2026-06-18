using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;
using System.Collections.Generic;
using UnityEngine.U2D;
using UnityEngine.SceneManagement;
using System;

public class Player : MonoBehaviour
{
    [SerializeField] private InputAction strafeAction;
    public float velocity;
    [SerializeField] float speedMult, accelerationMult, velocityMax, acceleration;
    public float heatMult;
    public int heat;
    [SerializeField] float heatRate = 0.1f;
    float lastHeat;
    [SerializeField] float skinWidth = 0.01f;
    HeatState heatState = HeatState.low;
    [SerializeField] Collider2D col;
    [SerializeField] LayerMask collisionMask;
    [SerializeField] ChunkGen chunkGen;
    [SerializeField] private SpriteRenderer guySprite;
    [SerializeField] private SpriteRenderer flameSprite;
    [SerializeField] GameObject gameOverUI;
    [SerializeField] GameObject scoreTextUI;
    [SerializeField] AudioSource fireDeath;
    [SerializeField] AudioSource collisionDeath;
    [SerializeField] AudioSource hitSound;
    private bool end = false;
    public bool flipControls = false;
    [SerializeField] Animator animator;
    [SerializeField] Animator flameAnimator;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        velocityMax = speedMult;
        //sprite = GetComponent<SpriteRenderer>();
        strafeAction.Enable();
        strafeAction.performed += ctx => { Move(ctx); };
        strafeAction.canceled += ctx => { Move(ctx); };
        lastHeat = Time.time;
    }



    // Update is called once per frame
    void Update()
    {
        float negMult = 1;
        if (velocity != 0)
        {
            negMult = (velocity / Mathf.Abs(velocity));
        }

        if (acceleration != 0)
        {
            velocity += acceleration * Time.deltaTime;
            velocity = Mathf.Clamp(velocity, -velocityMax, velocityMax);
        }
        else
        {
            velocity = Mathf.Abs(velocity) - (accelerationMult * Time.deltaTime);
            if (velocity <= 0)
            {
                velocity = 0;
            }
            else velocity *= negMult;
        }
        if (heat <= 400 && Time.time - lastHeat > heatRate)
        {
            if (ChunkGen.Instance.pauseDuration > 0) return;
            lastHeat = Time.time;
            heat++;
        }
        if (end) return;
        heatState = (HeatState)(heat / 100);
        switch (heatState)
        {
            
            case HeatState.low:
                heatMult = 1f;
                flameAnimator.Play("Base");
                guySprite.color = Color.white;
                break;
            case HeatState.mid:
                heatMult = 1.5f;
                flameAnimator.Play("Fire_Low");
                guySprite.color = Color.yellow;
                break;
            case HeatState.high:
                heatMult = 1.75f;
                flameAnimator.Play("Fire_Mid");
                guySprite.color = new Color(0.96f, 0.44f, 0.08f);
                break;
            case HeatState.extreme:
                heatMult = 2f;
                flameAnimator.Play("Fire_High");
                guySprite.color = Color.red;
                break;
            case HeatState.unbreakable:
                heatMult = 2f;
                flameAnimator.Play("Burnout");
                fireDeath.Play();
                EndGame();
                
                break;

        }
        if (velocity != 0)
        {
            CheckHorizontal(velocity*Time.deltaTime);
        }
        
    }
    private void Move(InputAction.CallbackContext ctx)
    {
        
        acceleration = ctx.ReadValue<float>() * accelerationMult;
        if (flipControls) acceleration *= -1;

    }
    private void EndGame()
    {
        if (end) return;
        end = true;
        heat = 0;
        heatRate = 9999;
        strafeAction.Disable();
        chunkGen.Stop();
        scoreTextUI.SetActive(false);
        gameOverUI.SetActive(true);
        //Invoke("SceneLoad", 6);
        
    }
    private void SceneLoad()
    {
        SceneManager.LoadScene("MainMenu");

    }
    void CheckHorizontal(float vel)
    {
        RaycastHit2D hit;
        Vector3 endVector = new Vector3(vel, 0, 0);
        float dist = endVector.magnitude + skinWidth;
        hit = Physics2D.CircleCast(transform.position, col.bounds.extents.x, endVector.normalized, dist, collisionMask);
        if (hit)
        {   
            Vector3 snapToSurface = endVector.normalized * (hit.distance - skinWidth);
            endVector = endVector - snapToSurface;
            if(endVector.magnitude <= skinWidth)
            {
                snapToSurface = Vector3.zero;
            }
            Collidable collidable = hit.transform.GetComponent<Collidable>();
            if ((int)collidable.heatState <= (int)heatState)
            {
                Vector3 pos = new Vector3(0, transform.position.y+1, 0);
                ChunkGen.Instance.SetCameraPan(pos, 0.2f, true, false);
                transform.position += new Vector3(vel, 0f, 0f);
                heat = Mathf.Max(0, heat - (100));
                hitSound.Play();
                flameAnimator.Play("Base");
                guySprite.color = Color.white;
                animator.Play("PlayerHit");
                collidable.Hit();
            }
            else
            {
                transform.position += snapToSurface;
            }

            
        }
        else
        {
            transform.position += new Vector3(vel, 0f, 0f);
        }

    }
    public void CheckVertical(float vel)
    {
        RaycastHit2D hit;
        Vector3 endVector = new Vector3(0, -vel, 0);
        float dist = endVector.magnitude + skinWidth;
        hit = Physics2D.CircleCast(transform.position, col.bounds.extents.x, endVector.normalized, dist, collisionMask);
        if (hit)
        {
            Vector3 snapToSurface = endVector.normalized * (hit.distance - skinWidth);
            endVector = endVector - snapToSurface;
            if (endVector.magnitude <= skinWidth)
            {
                snapToSurface = Vector3.zero;
            }
            Collidable collidable = hit.transform.GetComponent<Collidable>();
            if ((int)collidable.heatState <= (int)heatState)
            {
                Vector3 pos = new Vector3(0, transform.position.y+1, 0);
                ChunkGen.Instance.SetCameraPan(pos, 0.2f, true, false);
                //transform.position += new Vector3(vel, 0f, 0f);
                heat = Mathf.Max(0, heat - (100));
                hitSound.Play();
                collidable.Hit();
                animator.Play("PlayerHit");
                //Destroy(collidable.gameObject);

            }
            else
            {
                transform.position += snapToSurface;
                flameSprite.sprite = null;
                collisionDeath.Play();
                animator.Play("PlayerDead");
                EndGame();
                
            }

        }
    }

}
public enum HeatState
{
    low,
    mid,
    high,
    extreme,
    unbreakable
}