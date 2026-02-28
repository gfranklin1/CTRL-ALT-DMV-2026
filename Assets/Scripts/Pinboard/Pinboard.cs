using System.Collections.Generic;
using UnityEngine;

public class Pinboard : MonoBehaviour
{
    [SerializeField] GameObject pinnedPhotoPrefab;
    [SerializeField] Vector3 startPosition = Vector3.zero;
    [SerializeField] float spacingX = 0.55f;
    [SerializeField] float spacingY = 0.45f;
    [SerializeField] int columns = 5;

    public void ClearBoard()
    {
        PinboardData.Clear();
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    void Start()
    {
        PinboardData.Load();
        List<PinboardEntry> entries = PinboardData.Entries;

        for (int i = 0; i < entries.Count; i++)
        {
            if (pinnedPhotoPrefab == null) break;

            int col = i % columns;
            int row = i / columns;

            Vector3 localOffset = new Vector3(col * spacingX, row * spacingY, 0f);
            Vector3 worldPos = transform.TransformPoint(startPosition + localOffset);

            GameObject go = Instantiate(pinnedPhotoPrefab, worldPos, transform.rotation, transform);
            go.GetComponent<PinnedPhoto>()?.Init(entries[i]);
        }
    }
}
