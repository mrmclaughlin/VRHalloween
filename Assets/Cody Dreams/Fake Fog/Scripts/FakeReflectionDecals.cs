using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal; // Needed for DecalProjector

namespace CodyDreams.Solutions.FakeFog
{
    public class FakeReflectionDecals : MonoBehaviour
    {
        [Header("Reflection Settings")] public int maxReflectionBounces = 3; // how many "reflections" we want
        public int raysPerBounce = 3; // more = smoother normal approximation
        public float rayOffset = 0.05f; // offset for extra rays
        public float maxDistance = 50f;

        [Header("Decal Settings")] public GameObject decalPrefab;
        public float baseDecalSize = 1f;
        public float baseOpacity = 0.8f;

        private List<DecalProjector> spawnedDecals = new List<DecalProjector>();

        [ContextMenu("Spawn Decals")]
        public void GenerateReflections()
        {
            ClearDecals(); // Clear old decals before new run

            Vector3 currentPos = transform.position;
            Vector3 currentDir = transform.forward;

            for (int bounce = 0; bounce < maxReflectionBounces; bounce++)
            {
                Vector3 averagedNormal;
                Vector3 hitPoint;

                if (!CastMultipleRays(currentPos, currentDir, out hitPoint, out averagedNormal))
                    break; // no hit, stop reflections

                // Place decal at hit point
                float opacity = baseOpacity * Mathf.Pow(0.7f, bounce); // reduce opacity per bounce
                SpawnDecal(hitPoint, averagedNormal, currentDir, opacity);

                // Reflect direction
                currentDir = Vector3.Reflect(currentDir, averagedNormal).normalized;

                // Move slightly off surface to avoid self-intersection
                currentPos = hitPoint + currentDir * 0.01f;
            }
        }

        private bool CastMultipleRays(Vector3 origin, Vector3 dir, out Vector3 avgPoint, out Vector3 avgNormal)
        {
            List<Vector3> hitPoints = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();

            // main ray
            if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDistance))
            {
                hitPoints.Add(hit.point);
                normals.Add(hit.normal);
            }
            else
            {
                avgPoint = Vector3.zero;
                avgNormal = Vector3.zero;
                return false;
            }

            // offset rays
            Vector3[] offsets = new Vector3[]
            {
                Vector3.right, Vector3.left, Vector3.up, Vector3.down
            };

            foreach (var off in offsets)
            {
                Vector3 newOrigin = origin + off * rayOffset;
                if (Physics.Raycast(newOrigin, dir, out RaycastHit offsetHit, maxDistance))
                {
                    hitPoints.Add(offsetHit.point);
                    normals.Add(offsetHit.normal);
                }
            }

            // Average results
            avgPoint = Vector3.zero;
            foreach (var p in hitPoints) avgPoint += p;
            avgPoint /= hitPoints.Count;

            avgNormal = Vector3.zero;
            foreach (var n in normals) avgNormal += n;
            avgNormal.Normalize();

            return true;
        }

        private void SpawnDecal(Vector3 position, Vector3 normal, Vector3 incomedir, float opacity)
        {
            if (decalPrefab == null) return;

            GameObject obj = Instantiate(decalPrefab, position, Quaternion.LookRotation(
                Vector3.Reflect(incomedir, normal)));
            DecalProjector decal = obj.GetComponent<DecalProjector>();
            decal.size = new Vector3(baseDecalSize, baseDecalSize, 1f);
            decal.fadeFactor = opacity;

            spawnedDecals.Add(decal);
        }

        public void ClearDecals()
        {
            foreach (var d in spawnedDecals)
            {
                if (d != null) DestroyImmediate(d.gameObject);
            }

            spawnedDecals.Clear();
        }
    }
}
