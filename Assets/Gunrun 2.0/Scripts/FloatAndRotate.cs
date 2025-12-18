using UnityEngine;

public class FloatAndRotate : MonoBehaviour
{
    // Yüzme/süzülme ayarlarý
    public float floatSpeed = 1f;     // Saniyedeki süzülme hýzý
    public float floatAmplitude = 0.5f; // Süzülme yüksekliði

    // Dönme ayarlarý
    public Vector3 rotationSpeed = new Vector3(0f, 30f, 0f); // Yön baþýna derece/sn

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Yukarý-aþaðý hareket (sinüs dalgasý)
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Yavaþça dönme
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
