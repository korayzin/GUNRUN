using UnityEngine;
using TMPro;

[ExecuteInEditMode]
public class TMP2DBend : MonoBehaviour
{
    public TMP_Text textComponent;
    [Range(-0.5f, 0.5f)] public float curveStrength = 0.1f;

    void Update()
    {
        if (textComponent == null) return;

        textComponent.ForceMeshUpdate(); // Mevcut mesh verilerini al
        TMP_TextInfo textInfo = textComponent.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            // Her karakterin 4 köşesini de merkeze olan X uzaklığına göre kaydır
            for (int j = 0; j < 4; j++)
            {
                Vector3 pos = vertices[vertexIndex + j];
                // Parabolik bükme formülü: y = x^2 * k
                float yOffset = (pos.x * pos.x) * curveStrength;
                vertices[vertexIndex + j] = new Vector3(pos.x, pos.y - yOffset, 0); // Z daima 0
            }
        }

        // Değişiklikleri render et
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}