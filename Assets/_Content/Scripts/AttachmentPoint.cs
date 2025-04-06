using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AttachmentPoint : MonoBehaviour
{
    [HideInInspector]
    public Rigidbody2D Rigidbody2D;

    private void Awake()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
    }
}
