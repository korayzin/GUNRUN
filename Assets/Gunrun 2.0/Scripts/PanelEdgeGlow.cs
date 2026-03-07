using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class PanelEdgeGlow : MonoBehaviour
{
    [Header("Glow Çizgisi Ayarları")]
    public Color glowColor = new Color(0f, 0.8f, 1f, 1f); // Varsayılan: Mavi/Cyan
    public float speed = 400f; 
    public float lineLength = 150f;
    public float lineThickness = 6f;
    public bool clockwise = true;

    private RectTransform panelRect;
    private RectTransform glowLine;
    private float currentDistance = 0f;
    private float totalPerimeter;
    private Vector2[] corners;
    private float[] segmentLengths;

    void Start()
    {
        panelRect = GetComponent<RectTransform>();
        CreateSafeGlowLine();
        UpdateCorners();
    }

    void CreateSafeGlowLine()
    {
        // Varsa eski çizgiyi temizle (Kodu aç/kapat yapınca çiftlenmesin)
        Transform oldGlow = transform.Find("SafeGlowLine");
        if (oldGlow != null) Destroy(oldGlow.gameObject);

        // Yeni bir alt obje oluştur
        GameObject go = new GameObject("SafeGlowLine");
        go.transform.SetParent(transform, false);
        
        // Objenin UI düzenlerini bozmasını engellemek için (Panel kaybolma sorununun kesin çözümü)
        LayoutElement layoutElement = go.AddComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;

        // Tıklamaları engellemesi için raycast'i kapat
        Image img = go.AddComponent<Image>();
        img.color = glowColor;
        img.raycastTarget = false; 

        glowLine = go.GetComponent<RectTransform>();
        
        // Çizgiyi arka plana (içeriklerin arkasına) atar
        go.transform.SetAsFirstSibling(); 

        // Merkezleme ve boyut ayarları
        glowLine.anchorMin = new Vector2(0.5f, 0.5f);
        glowLine.anchorMax = new Vector2(0.5f, 0.5f);
        glowLine.pivot = new Vector2(0.5f, 0.5f);
        glowLine.sizeDelta = new Vector2(lineLength, lineThickness);
    }

    void UpdateCorners()
    {
        Rect rect = panelRect.rect;
        corners = new Vector2[4];
        
        if (clockwise)
        {
            corners[0] = new Vector2(rect.xMin, rect.yMax); // Sol Üst
            corners[1] = new Vector2(rect.xMax, rect.yMax); // Sağ Üst
            corners[2] = new Vector2(rect.xMax, rect.yMin); // Sağ Alt
            corners[3] = new Vector2(rect.xMin, rect.yMin); // Sol Alt
        }
        else
        {
            corners[0] = new Vector2(rect.xMin, rect.yMax); // Sol Üst
            corners[1] = new Vector2(rect.xMin, rect.yMin); // Sol Alt
            corners[2] = new Vector2(rect.xMax, rect.yMin); // Sağ Alt
            corners[3] = new Vector2(rect.xMax, rect.yMax); // Sağ Üst
        }

        segmentLengths = new float[4];
        totalPerimeter = 0f;

        for (int i = 0; i < 4; i++)
        {
            int nextIndex = (i + 1) % 4;
            float dist = Vector2.Distance(corners[i], corners[nextIndex]);
            segmentLengths[i] = dist;
            totalPerimeter += dist;
        }
    }

    void Update()
    {
        if (glowLine == null || totalPerimeter == 0) return;

        // Panel boyutu anlık değişirse yeniden hesapla
        if (Mathf.Abs((panelRect.rect.width * 2 + panelRect.rect.height * 2) - totalPerimeter) > 1f)
        {
            UpdateCorners();
        }

        currentDistance += speed * Time.deltaTime;
        currentDistance %= totalPerimeter;
        if (currentDistance < 0) currentDistance += totalPerimeter;

        float traveled = 0f;
        for (int i = 0; i < 4; i++)
        {
            if (currentDistance >= traveled && currentDistance <= traveled + segmentLengths[i])
            {
                float t = (currentDistance - traveled) / segmentLengths[i];
                int nextIndex = (i + 1) % 4;

                glowLine.localPosition = Vector2.Lerp(corners[i], corners[nextIndex], t);

                Vector2 direction = corners[nextIndex] - corners[i];
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                glowLine.localRotation = Quaternion.Euler(0, 0, angle);

                break;
            }
            traveled += segmentLengths[i];
        }
    }
}