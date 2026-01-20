using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TMPWavyText : MonoBehaviour
{
    TMP_Text tmpText;

    // base mesh stuff
    Mesh mesh;
    Vector3[] vertices;

    [Header("Wave Settings")]
    public float amplitude = 5f;      // how high the letters move
    public float frequency = 5f;      // how tight the wave is on the line
    public float speed = 3f;          // how fast the wave scrolls

    [Header("Extra Shake (optional)")]
    public float jitterStrength = 0f; // small random shake per letter

    void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
    }

    void LateUpdate()
    {
        // force TMP to update its mesh info
        tmpText.ForceMeshUpdate();

        var textInfo = tmpText.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];

            // skip invisible characters (spaces, etc.)
            if (!charInfo.isVisible)
                continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            // grab the vertices for this character
            vertices = textInfo.meshInfo[materialIndex].vertices;

            // get the middle of the character so I move it from its center
            Vector3 charMid = (vertices[vertexIndex + 0] +
                               vertices[vertexIndex + 1] +
                               vertices[vertexIndex + 2] +
                               vertices[vertexIndex + 3]) * 0.25f;

            // move vertices so (0,0,0) is at center
            Vector3 offsetToCenter = charMid;
            vertices[vertexIndex + 0] -= offsetToCenter;
            vertices[vertexIndex + 1] -= offsetToCenter;
            vertices[vertexIndex + 2] -= offsetToCenter;
            vertices[vertexIndex + 3] -= offsetToCenter;

            // time
            float t = Time.unscaledTime * speed;

            // main vertical wave based on index
            float wave = Mathf.Sin(t + i * 0.3f * frequency) * amplitude;

            // optional tiny random jitter so it feels more chaotic
            Vector2 jitter = Vector2.zero;
            if (jitterStrength > 0f)
            {
                float jx = (Mathf.PerlinNoise(i * 13.37f, t * 4f) - 0.5f) * jitterStrength;
                float jy = (Mathf.PerlinNoise(i * 7.77f, t * 4f + 10f) - 0.5f) * jitterStrength;
                jitter = new Vector2(jx, jy);
            }

            Vector3 finalOffset = new Vector3(jitter.x, wave + jitter.y, 0f);

            // apply offset then put character back in place
            vertices[vertexIndex + 0] += offsetToCenter + finalOffset;
            vertices[vertexIndex + 1] += offsetToCenter + finalOffset;
            vertices[vertexIndex + 2] += offsetToCenter + finalOffset;
            vertices[vertexIndex + 3] += offsetToCenter + finalOffset;
        }

        // push the modified vertices back to the mesh
        for (int m = 0; m < textInfo.meshInfo.Length; m++)
        {
            var meshInfo = textInfo.meshInfo[m];
            mesh = meshInfo.mesh;
            mesh.vertices = meshInfo.vertices;
            tmpText.UpdateGeometry(mesh, m);
        }
    }
}