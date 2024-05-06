using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum BezierObjectPlaceType
{
    Randomized,
    Alternate
}

[RequireComponent(typeof(BezierPath))]
public class BezierObjectPlacer : MonoBehaviour
{
#if UNITY_EDITOR
    public BezierObjectPlaceType type = BezierObjectPlaceType.Randomized;
    public int seed = 0;

    [Header("Position")]
    [Range(0, 1)]
    public float startNormOffset = 0;
    public float spacing = 1;
    public bool fenceMode = false;
    public float widthRandomness = 0;
    [Range(0, 1)]
    public float offsetRandomness = 0;
    public float groundOffset = 0;

    [Header("Scale")]
    public float objectSizeMultiplier = 1;
    [Range(0, 1)]
    public float scaleRandomness = 0;

    [Header("Rotation")]
    public bool normalUp = true;
    public bool randomRotation = false;
    [Range(0, 90)]
    public float randomSlant = 0;
    public Vector3 extraRotation = Vector3.zero;

    [Header("Raycast")]
    public bool raycast = true;
    public float maxRaycastDistance = 100;
    public LayerMask raycastLayerMask = -1;

    [Header("Objects")]
    [SerializeField]
    Transform objectParent;
    [Tooltip("Make sure the object is in project folder and not in scene!")]
    public GameObject[] prefabs;

    public void Apply()
    {
        // checks
        spacing = Mathf.Clamp(spacing, 0.1f, 100);

        BezierPath path = GetComponent<BezierPath>();
        if (path == null || path.cachedPoints == null || prefabs == null || prefabs.Length == 0 || Application.isPlaying)
            return;

        // destroy old objects
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);

        Random.InitState(seed);
        float forwardcheck = 0.01f;
        int objectindex = 0;
        for (float distance = startNormOffset * path.totalDistance; distance + forwardcheck < path.totalDistance; distance += spacing, ++objectindex)
        {
            // caculate point on line
            distance += Random.Range(-spacing, spacing) * 0.5f * offsetRandomness;
            distance = Mathf.Clamp(distance, 0, path.totalDistance);
            Vector3 position = path.GetPointAtDistance(distance).position;

            Vector3 forward;
            if (fenceMode)
            {
                if (distance + spacing + forwardcheck > path.totalDistance)
                    break;

                Vector3 endpos = path.GetPointAtDistance(distance + spacing).position;
                position = (position + endpos) * 0.5f;
                forward = endpos - position;
            }
            else
                forward = (path.GetPointAtDistance(distance + forwardcheck).position - position).normalized;

            // calculate random perpendicular offset
            Vector3 perp = Vector3.Cross(forward, Vector3.up);
            position += perp * Random.Range(-widthRandomness, widthRandomness);

            // snap to ground
            Vector3 up = Vector3.up;
            if (raycast)
            {
                float raycastsafecheck = 2;
                if (!Physics.Raycast(position + Vector3.up * raycastsafecheck, Vector3.down, out RaycastHit hit, maxRaycastDistance + raycastsafecheck, raycastLayerMask))
                    continue;
                position = hit.point + hit.normal * groundOffset;
                if (normalUp)
                    up = hit.normal;
            }

            // rotation
            Vector3 extraangle = extraRotation;
            if (randomRotation)
                extraangle.y += Random.Range(0, 360);
            Quaternion rotation = Quaternion.LookRotation(forward, up) * Quaternion.Euler(extraangle);
            rotation *= Quaternion.Euler(Random.Range(-randomSlant, randomSlant), Random.Range(-randomSlant, randomSlant), 0);

            // instantiate
            int index = 0;
            if (type == BezierObjectPlaceType.Randomized)
                index = Random.Range(0, prefabs.Length);
            else if (type == BezierObjectPlaceType.Alternate)
                index = objectindex % prefabs.Length;
            GameObject prefab = prefabs[index];
            if (prefab == null)
                continue;
            Debug.Log(prefab.name);
            if(objectParent == null)
            {
                objectParent = transform;
            }
            GameObject objtransform = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            objtransform.transform.parent = objectParent;
            objtransform.transform.SetPositionAndRotation(position, rotation);
            objtransform.transform.localScale *= objectSizeMultiplier * (1 + Random.Range(0, scaleRandomness));
        }
    }
#endif
}
