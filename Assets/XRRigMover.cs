using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

public class XRRigMover : MonoBehaviour
{
    [Header("XR Rig Reference")]
    public Transform xrRig;
    public Transform head;

    [Header("XR Ray Interactor")]
    public XRRayInteractor rayInteractor;

    [Header("Movement Settings")]
    public float moveSpeed = 1.0f;
    public float verticalSpeed = 1.0f;
    public float rotateSpeed = 45f;

    [Header("Terrain Follow")]
    public bool followTerrain = true;
    public float yOffset = 0.0f;
    public float groundRayLength = 10f;
    public LayerMask groundMask = ~0;
    public bool adjustWhileRotating = true;

    // ---------- INTERNAL STATE ----------
    private Vector3 moveInput = Vector3.zero;
    private float verticalInput = 0f;
    private float rotateInput = 0f;

    // Terrain failsafe
    private float lastSafeY;
    private bool hasLastSafeY = false;

    // Trigger state
    private bool triggerHeld = false;

    void Awake()
    {
        if (xrRig != null)
        {
            lastSafeY = xrRig.position.y;
            hasLastSafeY = true;
        }
    }

    void Update()
    {
        HandleMovement();
        HandleVerticalMovement();
        HandleRotation();
    }

    // ===========================
    // MOVEMENT
    // ===========================
    public void MoveForward()  => moveInput = Vector3.forward;
    public void MoveBackward() => moveInput = Vector3.back;
    public void MoveLeft()     => moveInput = Vector3.left;
    public void MoveRight()    => moveInput = Vector3.right;

    public void StopMovement()
    {
        moveInput = Vector3.zero;
        verticalInput = 0f;
    }

    private void HandleMovement()
    {
        if (xrRig == null || head == null) return;
        if (moveInput == Vector3.zero) return;

        Vector3 dir =
            head.forward * moveInput.z +
            head.right   * moveInput.x;

        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        xrRig.position += dir.normalized * moveSpeed * Time.deltaTime;

        if (followTerrain)
            AdjustHeightToTerrain();
    }

    // ===========================
    // VERTICAL
    // ===========================
    public void MoveUp()   => verticalInput =  1f;
    public void MoveDown() => verticalInput = -1f;

    private void HandleVerticalMovement()
    {
        if (xrRig == null) return;
        if (Mathf.Abs(verticalInput) < 0.0001f) return;

        Vector3 pos = xrRig.position;
        pos.y += verticalInput * verticalSpeed * Time.deltaTime;
        xrRig.position = pos;

        if (followTerrain)
            AdjustHeightToTerrain();
    }

    // ===========================
    // ROTATION
    // ===========================
    public void RotateLeft()  => rotateInput = -1f;
    public void RotateRight() => rotateInput =  1f;

    public void StopRotation()
    {
        rotateInput = 0f;
    }

    // ===========================
    // TRIGGER CLICK / RELEASE
    // ===========================
    public void TriggerClick()
    {
        if (triggerHeld) return;
        triggerHeld = true;

        if (rayInteractor == null) return;

        // Check what the ray is currently hovering over
        if (rayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult uiHit))
        {
            Button button = uiHit.gameObject.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.Invoke();
            }
        }
    }

    public void TriggerRelease()
    {
        triggerHeld = false;
    }

    private void HandleRotation()
    {
        if (xrRig == null) return;
        if (Mathf.Abs(rotateInput) < 0.0001f) return;

        xrRig.Rotate(Vector3.up, rotateInput * rotateSpeed * Time.deltaTime);

        if (followTerrain && adjustWhileRotating)
            AdjustHeightToTerrain();
    }

    // ===========================
    // TERRAIN CLAMP
    // ===========================
    private void AdjustHeightToTerrain()
    {
        if (head == null || xrRig == null) return;

        RaycastHit hit;
        Vector3 origin = head.position;
        bool foundTerrain = false;

        if (Physics.Raycast(origin, Vector3.down, out hit, groundRayLength, groundMask, QueryTriggerInteraction.Ignore))
        {
            Terrain terrain = hit.collider.GetComponent<Terrain>();
            if (terrain != null)
            {
                float terrainHeight =
                    terrain.SampleHeight(hit.point) + terrain.transform.position.y;

                float targetY = terrainHeight + yOffset;

                lastSafeY = targetY;
                hasLastSafeY = true;

                Vector3 pos = xrRig.position;
                pos.y = Mathf.Max(pos.y, targetY);
                xrRig.position = pos;

                foundTerrain = true;
            }
        }

        if (!foundTerrain && hasLastSafeY)
        {
            Vector3 pos = xrRig.position;
            pos.y = Mathf.Max(pos.y, lastSafeY);
            xrRig.position = pos;
        }
    }

    // ===========================
    // MASTER STOP
    // ===========================
    public void StopAllMotion()
    {
        StopMovement();
        StopRotation();
        TriggerRelease();
    }
}
