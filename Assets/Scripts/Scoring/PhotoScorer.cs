using UnityEngine;

public class PhotoScorer : MonoBehaviour
{
    [SerializeField] CelebrityController celebrity;

    void Start()
    {
        if (celebrity == null)
            celebrity = FindFirstObjectByType<CelebrityController>();
    }

    public PhotoResult ScoreShot(Camera cam)
    {
        var result = new PhotoResult();

        if (celebrity == null || cam == null)
        {
            result.gradeLabel = "USELESS";
            return result;
        }

        // 1. Frustum check — is the celebrity visible in the frame?
        Vector3 vp = cam.WorldToViewportPoint(celebrity.transform.position);
        result.celebInFrame = vp.z > 0f && vp.x > 0f && vp.x < 1f && vp.y > 0f && vp.y < 1f;

        if (!result.celebInFrame)
        {
            result.gradeLabel = "USELESS";
            result.totalScore = 0f;
            Debug.Log("[PhotoScorer] Celebrity not in frame.");
            return result;
        }

        // 2. Line-of-sight check — only blocked by geometry, not the celebrity's own collider
        Vector3 dir = (celebrity.transform.position - cam.transform.position).normalized;
        float dist = Vector3.Distance(cam.transform.position, celebrity.transform.position);
        if (Physics.Raycast(cam.transform.position, dir, out RaycastHit losHit, dist))
        {
            bool hitSelf = losHit.transform == celebrity.transform ||
                           losHit.transform.IsChildOf(celebrity.transform);
            if (!hitSelf)
            {
                result.gradeLabel = "USELESS";
                result.totalScore = 0f;
                Debug.Log($"[PhotoScorer] Line of sight blocked by: {losHit.collider.name}");
                return result;
            }
        }

        // 3. Target action match
        CelebrityAction target = MissionManager.Instance?.CurrentMission?.targetAction ?? CelebrityAction.None;
        result.targetActionMatch = celebrity.CurrentAction != CelebrityAction.None &&
                                   celebrity.CurrentAction == target;

        // 4. Distance score (ideal 3–8 m)
        if (dist >= 3f && dist <= 8f)
            result.distanceScore = 100f;
        else if (dist < 3f)
            result.distanceScore = Mathf.Lerp(20f, 100f, Mathf.InverseLerp(1f, 3f, dist));
        else
            result.distanceScore = Mathf.Lerp(100f, 0f, Mathf.InverseLerp(8f, 20f, dist));
        result.distanceScore = Mathf.Clamp(result.distanceScore, 0f, 100f);

        // 5. Center-of-frame score
        float cx = Mathf.Abs(vp.x - 0.5f) * 2f;
        float cy = Mathf.Abs(vp.y - 0.5f) * 2f;
        float centerDist = Mathf.Sqrt(cx * cx + cy * cy);
        result.centerScore = Mathf.Clamp01(1f - centerDist) * 100f;

        // Total
        result.totalScore = (result.distanceScore + result.centerScore) / 2f;
        if (!result.targetActionMatch) result.totalScore *= 0.3f;

        // Payout
        int maxPayout = MissionManager.Instance?.CurrentMission?.payoutAmount ?? 500;
        result.payout = Mathf.RoundToInt(result.totalScore / 100f * maxPayout);

        // Grade
        if (result.totalScore >= 80f)      result.gradeLabel = "MONEY SHOT";
        else if (result.totalScore >= 50f) result.gradeLabel = "PUBLISHABLE";
        else if (result.totalScore >= 20f) result.gradeLabel = "WEAK";
        else                               result.gradeLabel = "USELESS";

        Debug.Log($"[PhotoScorer] {result.gradeLabel} | Score: {result.totalScore:F0} | ActionMatch: {result.targetActionMatch} | Dist: {dist:F1}m | Center: {result.centerScore:F0}");

        return result;
    }
}
