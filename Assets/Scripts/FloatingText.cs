using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float destroyTime = 1.5f;
    public Vector3 offset = new Vector3(0, 1, 0);
    public float floatSpeed = 1f;

    private void Start()
    {
        transform.position += offset;
        Destroy(gameObject, destroyTime);
    }

    private void Update()
    {
        transform.position += new Vector3(0, floatSpeed * Time.deltaTime, 0);
    }

    public void SetText(string text)
    {
        GetComponent<TextMeshPro>().text = text;
    }
}

