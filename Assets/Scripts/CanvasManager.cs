using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    Canvas canvas;
    public GameObject[] profilePrefabs;
    public List<Playable> characters = new List<Playable>();
    
    private void Awake()
    {
        canvas = FindObjectOfType<Canvas>();
    }
    private void Start()
    {
        int length = characters.Count - 2;
        GameObject profile = Instantiate(profilePrefabs[length], canvas.transform);
        for (int i = 0; i < characters.Count; i++)
        {
            Playable character = characters[i];
            Profile prof = profile.GetComponent<ProfilePrefab>().profiles[i];
            prof.playerTag.text = character.NAME;
            prof.profileImage.sprite = character.PROFILE_IMAGE;
            for (int j = 0; j < character.stocks; j++)
            {
                prof.stocks[j].sprite = character.STOCK;
                prof.stocks[j].gameObject.SetActive(true);
            }
            character.profile = prof;
        }
    }
}
