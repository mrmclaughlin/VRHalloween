using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR.Hands;

public class ThumbsUpResetXRHands : MonoBehaviour
{
    [Header("Which hand triggers reset?")]
    public bool useLeftHand = true;

    [Header("Timing")]
    public float holdSeconds = 0.5f;
    public float cooldownSeconds = 2.0f;

    [Header("Gesture tuning")]
    [Tooltip("Tip-to-proximal distance for curled fingers (meters).")]
    public float curledThreshold = 0.040f;

    [Tooltip("Thumb tip must be far enough from thumb proximal (meters).")]
    public float thumbExtendedThreshold = 0.035f;

    [Header("Debug")]
    public bool debug = true;
    public float debugIntervalSeconds = 1.0f;

    XRHandSubsystem _hands;
    float _held;
    float _nextAllowed;
    float _nextDebug;

    void Awake()
    {
        _hands = FindHandsSubsystem();

        if (_hands != null)
            Debug.Log("[ThumbsUpReset] XRHandSubsystem found.");
        else
            Debug.LogError("[ThumbsUpReset] XRHandSubsystem NOT found. Check OpenXR → Features → XR Hands.");
    }

    void Update()
    {
        if (_hands == null)
        {
            ThrottledLog("[ThumbsUpReset] No XRHandSubsystem.");
            return;
        }

        XRHand hand = useLeftHand ? _hands.leftHand : _hands.rightHand;

        if (!hand.isTracked)
        {
            ThrottledLog($"[ThumbsUpReset] {(useLeftHand ? "Left" : "Right")} hand not tracked.");
            _held = 0f;
            return;
        }

        if (Time.time < _nextAllowed)
        {
            ThrottledLog($"[ThumbsUpReset] Cooldown {(_nextAllowed - Time.time):0.00}s");
            return;
        }

        bool thumbsUp = IsThumbsUp(hand, out string reason, out float thumbLen,
                                  out float idx, out float mid, out float ring, out float lit);

        ThrottledLog(
            $"[ThumbsUpReset] thumbsUp={thumbsUp} held={_held:0.00}/{holdSeconds:0.00} " +
            $"thumbLen={thumbLen:0.000} idx={idx:0.000} mid={mid:0.000} ring={ring:0.000} lit={lit:0.000} " +
            $"fail='{reason}'"
        );

        if (thumbsUp)
        {
            _held += Time.deltaTime;
            if (_held >= holdSeconds)
            {
                _nextAllowed = Time.time + cooldownSeconds;
                _held = 0f;

                var scene = SceneManager.GetActiveScene();
                Debug.Log($"[ThumbsUpReset] RESET → Reloading '{scene.name}' (index {scene.buildIndex})");

                if (scene.buildIndex < 0)
                    Debug.LogError("[ThumbsUpReset] Scene not in Build Settings!");

                SceneManager.LoadScene(scene.buildIndex);
            }
        }
        else
        {
            _held = 0f;
        }
    }

    bool IsThumbsUp(
        XRHand hand,
        out string reason,
        out float thumbLen,
        out float idxCurl,
        out float midCurl,
        out float ringCurl,
        out float litCurl)
    {
        reason = "";
        thumbLen = idxCurl = midCurl = ringCurl = litCurl = -1f;

        if (!TryPos(hand, XRHandJointID.ThumbProximal, out var thumbProx) ||
            !TryPos(hand, XRHandJointID.ThumbTip, out var thumbTip))
        {
            reason = "Thumb pose missing";
            return false;
        }

        thumbLen = Vector3.Distance(thumbTip, thumbProx);
        if (thumbLen < thumbExtendedThreshold)
        {
            reason = "Thumb not extended";
            return false;
        }

        if (!TryCurl(hand, XRHandJointID.IndexTip, XRHandJointID.IndexProximal, out idxCurl) ||
            !TryCurl(hand, XRHandJointID.MiddleTip, XRHandJointID.MiddleProximal, out midCurl) ||
            !TryCurl(hand, XRHandJointID.RingTip, XRHandJointID.RingProximal, out ringCurl) ||
            !TryCurl(hand, XRHandJointID.LittleTip, XRHandJointID.LittleProximal, out litCurl))
        {
            reason = "Finger pose missing";
            return false;
        }

        if (idxCurl >= curledThreshold)  { reason = "Index not curled"; return false; }
        if (midCurl >= curledThreshold)  { reason = "Middle not curled"; return false; }
        if (ringCurl >= curledThreshold) { reason = "Ring not curled"; return false; }
        if (litCurl >= curledThreshold)  { reason = "Little not curled"; return false; }

        reason = "OK";
        return true;
    }

    bool TryCurl(XRHand hand, XRHandJointID tip, XRHandJointID prox, out float dist)
    {
        dist = -1f;
        if (!TryPos(hand, tip, out var tipPos)) return false;
        if (!TryPos(hand, prox, out var proxPos)) return false;
        dist = Vector3.Distance(tipPos, proxPos);
        return true;
    }

    bool TryPos(XRHand hand, XRHandJointID id, out Vector3 pos)
    {
        var joint = hand.GetJoint(id);
        if (!joint.TryGetPose(out Pose pose))
        {
            pos = default;
            return false;
        }
        pos = pose.position;
        return true;
    }

    void ThrottledLog(string msg)
    {
        if (!debug) return;
        if (Time.time < _nextDebug) return;
        _nextDebug = Time.time + debugIntervalSeconds;
       // Debug.Log(msg);
    }

    XRHandSubsystem FindHandsSubsystem()
    {
        var subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        return subsystems.Count > 0 ? subsystems[0] : null;
    }
}
