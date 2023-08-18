using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public List<Character> charactersHit = new List<Character>();
    public Vector2 direction;
    public float damage;
    public Character self;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && other.gameObject.GetComponent<Character>() != self)
        {
            if (other.GetComponent<Character>() != self)
            {

            }
        }
    }
}
