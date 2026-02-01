using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    float maxHealth = 100f;
    float currentHealth;
    float playerCurrentHealth;
    public Image testCube;
    public GameObject playerTransform;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        playerCurrentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
       if(Input.GetKeyDown(KeyCode.Escape))
        {
            currentHealth -= 10;
            UIManager.Instance.UpdateBossHP(currentHealth / maxHealth);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            UIManager.Instance.ShowHitFeedback(0);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            UIManager.Instance.ShowHitFeedback(1);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            UIManager.Instance.ShowHitFeedback(2);
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            UIManager.Instance.ShowBossDialogue("ÄãºÃ£¬ÊÀ½ç");
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            UIManager.Instance.ShowPlayerRetort("Hello World");
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            playerCurrentHealth -= 10;
            UIManager.Instance.UpdatePlayerHP(playerCurrentHealth / maxHealth);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            playerCurrentHealth += 5;
            UIManager.Instance.UpdatePlayerHP(playerCurrentHealth / maxHealth);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            UIManager.Instance.ShowEmoji("goblin", playerTransform.transform.position);
        }
    }
}
