using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public List<Playable> charactersHit = new List<Playable>();
    public Vector2 direction;
    public float damage;
    public Playable self;
    public AudioClip hitSound;
    public List<Attack> otherHitboxes = new List<Attack>();
    public bool canBeHitMultipleTimes = false;
    public Dictionary<Playable, float> charactersHitAndTimeSinceLastHit = new Dictionary<Playable, float>();
    public float timeBetweenHits = 0.5f;
    public bool fixedKnockBack = false;

    private void Update()
    {
        List<Playable> keysToUpdate = new List<Playable>(charactersHitAndTimeSinceLastHit.Keys);

        foreach (Playable play in keysToUpdate)
        {
            charactersHitAndTimeSinceLastHit[play] += Time.deltaTime;
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            TriggerMethod(collision);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            TriggerMethod(collision);
        }
    }

    private void TriggerMethod(Collider2D collision)
    {
        
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && collision.GetComponent<Playable>() != self)
        {

            if (!charactersHit.Contains(collision.GetComponent<Playable>()) || canBeHitMultipleTimes)
            {
                if (canBeHitMultipleTimes)
                {
                    if (charactersHitAndTimeSinceLastHit.ContainsKey(collision.GetComponent<Playable>()))
                    {
                        if (charactersHitAndTimeSinceLastHit[collision.GetComponent<Playable>()] >= timeBetweenHits)
                        {
                            collision.GetComponent<Playable>().TakeDamage(damage, direction, fixedKnockBack);
                            charactersHitAndTimeSinceLastHit[collision.GetComponent<Playable>()] = 0f;
                        }
                    }
                    else
                    {
                        collision.GetComponent<Playable>().TakeDamage(damage, direction, fixedKnockBack);
                        charactersHitAndTimeSinceLastHit[collision.GetComponent<Playable>()] = 0f;
                    }
                }
                else
                {
        
        Debug.Log("OK");
        collision.GetComponent<Playable>().TakeDamage(damage, direction, fixedKnockBack);
        charactersHit.Add(collision.GetComponent<Playable>());
    }

            }
}
    }

    private void OnDisable()
    {
        charactersHit.Clear();
    }
}