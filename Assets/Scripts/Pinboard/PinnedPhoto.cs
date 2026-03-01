using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PinnedPhoto : MonoBehaviour
{
    [SerializeField] Renderer photoRenderer;
    [SerializeField] Text labelText;

    // Loads the saved photo from disk and applies it as the material texture.
    // Creates a new Material instance so we don't overwrite the shared material.
    public void Init(PinboardEntry entry)
    {
        if (photoRenderer != null && File.Exists(entry.texturePath))
        {
            byte[] bytes = File.ReadAllBytes(entry.texturePath);
            // Texture2D(2,2) is a throwaway size — LoadImage resizes it to match the PNG
            var tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            // Clone the material so each photo has its own texture without affecting others
            photoRenderer.material = new Material(photoRenderer.material);
            photoRenderer.material.mainTexture = tex;
        }

        if (labelText != null)
        {
            string name = !string.IsNullOrEmpty(entry.celebName) ? entry.celebName + "\n" : "";
            labelText.text = name + entry.grade + "\n$" + entry.payout;
        }

        // Apply the random tilt from the entry so each photo looks slightly askew
        Vector3 euler = transform.localEulerAngles;
        euler.z = entry.rotation;
        transform.localEulerAngles = euler;
    }
}
