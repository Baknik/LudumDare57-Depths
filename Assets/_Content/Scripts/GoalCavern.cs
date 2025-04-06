using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GoalCavern : MonoBehaviour
{
    public delegate void GoalReachedHandler();
    public static event GoalReachedHandler GoalReachedEvent;

    private BoxCollider2D _boxCollider2D;

    void Awake()
    {
        _boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.GetComponent<Spelunker>() != null)
        {
            if (GoalReachedEvent != null)
            {
                GoalReachedEvent();
            }
        }
    }
}
