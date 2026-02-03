using UnityEngine;

public class FloatingImage : MonoBehaviour
{
    [Header("Süzülme ve Salınım")]
    public float verticalSpeed = 2f;    // Yukarı-aşağı hızı
    public float verticalAmount = 15f; // Yukarı-aşağı mesafesi
    public float swaySpeed = 1.5f;     // Sağa-sola salınım hızı
    public float swayAmount = 10f;     // Sağa-sola salınım mesafesi

    [Header("Dönüş")]
    public float rotationSpeed = 45f;  // Z ekseni dönme hızı

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        // 1. Dikey Süzülme (Y ekseni)
        float newY = Mathf.Sin(Time.time * verticalSpeed) * verticalAmount;

        // 2. Yatay Salınım (X ekseni) - Farklı bir faz için Cosinus kullandık
        float newX = Mathf.Cos(Time.time * swaySpeed) * swayAmount;

        // Pozisyonu uygula
        transform.localPosition = startPos + new Vector3(newX, newY, 0);

        // 3. Z Ekseninde Dönüş
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}