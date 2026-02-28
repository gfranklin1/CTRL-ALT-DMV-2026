using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    [SerializeField] Renderer rend;
    Collider col;

    void Start()
    {
        col = GetComponent<Collider>();
        if (rend != null) rend.material.color = Color.green;

        GameManager.OnStateChanged += OnStateChanged;
        // Start hidden — only active during Escaping
        OnStateChanged(GameManager.Instance?.CurrentState ?? GameState.Playing);
    }

    void OnDestroy() => GameManager.OnStateChanged -= OnStateChanged;

    void OnStateChanged(GameState state)
    {
        bool active = state == GameState.Escaping;
        if (rend != null) rend.enabled = active;
        if (col != null)  col.enabled  = active;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GameManager.Instance?.TransitionTo(GameState.Win);
    }
}
