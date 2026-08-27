using UnityEngine;

// Attached at runtime (by MazeGenerator) to the finish marker instance.
// Detects when the player reaches the finish and notifies the GameManager.
public class FinishZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        if (GameManager.Instance != null)
            GameManager.Instance.PlayerReachedFinish();
    }
}
