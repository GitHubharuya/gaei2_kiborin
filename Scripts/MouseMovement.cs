using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // ← UIを使うために必要

public class MouseMovement : MonoBehaviour
{
    public Transform player;
    private float speed = 5.0f;

    private int cheeseCount = 0; // ← チーズの数をカウント
    public TextMeshProUGUI cheeseText;      // ← UIへの参照（Inspectorで設定）

    void Start()
    {
        UpdateCheeseUI();
    }

    void Update()
    {
        if (player == null)
        {
            FindNearestCheese();
        }

        if (player != null)
        {
            Vector3 direction = player.position - transform.position;
            direction.Normalize();
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    void FindNearestCheese()
    {
        GameObject[] cheeses = GameObject.FindGameObjectsWithTag("Cheese");
        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject cheese in cheeses)
        {
            float distance = Vector3.Distance(transform.position, cheese.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = cheese.transform;
            }
        }

        player = closest;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cheese"))
        {
            Destroy(other.gameObject);
            cheeseCount++;
            UpdateCheeseUI();
            player = null;
        }
    }

    void UpdateCheeseUI()
    {
        if (cheeseText != null)
        {
            cheeseText.text = "Cheese: " + cheeseCount.ToString();
        }
    }
}
