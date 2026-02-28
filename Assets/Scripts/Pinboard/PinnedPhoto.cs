using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PinnedPhoto : MonoBehaviour
{
    [SerializeField] Renderer photoRenderer;
    [SerializeField] Text labelText;

    public void Init(PinboardEntry entry)
    {
        if (photoRenderer != null && File.Exists(entry.texturePath))
        {
            byte[] bytes = File.ReadAllBytes(entry.texturePath);
            var tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            photoRenderer.material = new Material(photoRenderer.material);
            photoRenderer.material.mainTexture = tex;
        }

        if (labelText != null)
            labelText.text = entry.grade + "\n$" + entry.payout;

        Vector3 euler = transform.localEulerAngles;
        euler.z = entry.rotation;
        transform.localEulerAngles = euler;
    }
}
