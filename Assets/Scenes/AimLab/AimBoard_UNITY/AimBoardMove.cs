using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    public float floatSpeed = 1f; // Yukarý-aþaðý hareket hýzý
    public float floatAmount = 0.5f; // Yukarý-aþaðý hareket mesafesi
    public float swaySpeed = 0.8f; // Sola-saða hareket hýzý
    public float swayAmount = 0.3f; // Sola-saða hareket mesafesi

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position; // Baþlangýç pozisyonunu kaydet
    }

    void Update()
    {
        // Yukarý-aþaðý hareket
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;

        // Sola-saða hareket
        float newX = startPosition.x + Mathf.Cos(Time.time * swaySpeed) * swayAmount;

        // Yeni pozisyonu uygula
        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}
