using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace CodyDreams.Solutions.FakeFog
{
// This class is a private helper and not a MonoBehaviour, so it won't appear in the editor.
    internal static class FakeFogReviewManager
    {
        private const string FILE_NAME = "ReviewData.json";
        private static readonly string _filePath;

        [Serializable]
        internal class AssetReviewData
        {
            public string AssetID;
            public int UsageCount;
            public string LastRequestedDate;
            public bool HasConsideredReview;
            public bool HasReviewed;

            public float AssetPrice;
            public string AssetVersion;
        }

        [Serializable]
        private class AllAssetData
        {
            public List<AssetReviewData> assets = new();
        }

        private static AllAssetData _data;

        // Unique ID for the current asset. This is a constant as requested.
        private const string CurrentAssetID = "Fake Fog";

        private const string FirstTimeMessage =
            "Thank you for considering \"{0}\" asset. We love to see that and it is big for us your kind review." +
            " ,Your review helps us push updates faster";

        private const string ReturningUserMessage =
            "Thank you for returning for us again, we will try our best to give to you a good product. Mind you leaving a review? \"{0}\" ." +
            " Disclaimer - we are not collecting data about you and send it over the internet, all the data is retain in your own local machine";
        // putting a discliamer because user can be paranoid , and only this pop up would show to user only once so this discliamer will
        // be seen only once by the user for his entire experince with us
        private const string LoyalUserMessage =
            "Thank you for your loyalty! Mind us helping to reach more people? ( \"{0}\" )";

        private const string LongTimeMessage =
            "Hey! We’ve noticed you’ve been using \"{0}\" for a long time and exploring its features. If you have" +
            " a minute, we’d love your thoughts — your feedback helps us improve the experience for you and other developers.";

        private const string AssetUrl = "https://assetstore.unity.com/packages/tools/particles-effects/fake-fog-296903";

        private const int UsageThersold = 30;

        static FakeFogReviewManager()
        {
            _filePath = Path.Combine(Application.persistentDataPath, FILE_NAME);
            LoadData();
        }

        // This is the public method you'll call from your tiny bootstrapper script.
        [InitializeOnLoadMethod]
        private static void OnEditorStart()
        {
            OnAssetUsed(CurrentAssetID);
        }

        [MenuItem("Window/Cody Dremas/FeedBack windows/Fake Fog Feedback")]
        public static void RunFromMenu()
        {
            OnAssetUsed(CurrentAssetID, true);
        }


        private static void OnAssetUsed(string assetId, bool voluntary = false)
        {
            var currentAsset = _data.assets.Find(x => x.AssetID == assetId);
            // this asset is previously made with old legacy feedback system which does not contain enough information
            // and so we will by pass those limitation to check the user by literally see he has installed the same
            // asset before
            var IsNewAsset = false;
            if (currentAsset == null)
            {
                IsNewAsset = false;
                currentAsset = new AssetReviewData { AssetID = assetId };
                _data.assets.Add(currentAsset);
            }
            else
            {
                IsNewAsset = true;
            }

            if (!voluntary)
                currentAsset.UsageCount++;
            // We trigger on UsageCount == 2 (not 1) because Unity often does a double domain reload 
            // on first import. This avoids bothering users while compilation is still happening.

            if (!IsNewAsset && currentAsset.UsageCount == 2 && !_data.assets.Exists(x => x.AssetID == "Insta Polish"))
                // we dumped the migration stats reading because old one are too unrealible and lack of data
                if (EditorUtility.DisplayDialog("patch note for older users", "we have created a better " +
                                                                              "review pop up system , that will enchance your user experince . new features - we have created a new system to place" +
                                                                              " dynamic reflections via decal projector without ray tracing (see more in the documentation). also we believe our newest " +
                                                                              "insta polish asset pack would " +
                                                                              "be a great companion pack for your project , if you are enjoying this pack",
                        "Lets check the insta polish asset out",
                        "ok i will check it later"
                    ))
                    Application.OpenURL(
                        "https://assetstore.unity.com/packages/vfx/shaders/insta-polish-instant-polish-your-game-329157");

            if (currentAsset.HasConsideredReview) ShowReviewConfirmation(currentAsset);
            else if (CanShowPrompt(currentAsset) || voluntary) ShowReviewPrompt(currentAsset);

            SaveData();
        }

        private static bool CanShowPrompt(AssetReviewData data)
        {
            if (data.HasReviewed) return false;

            // Only show if the asset has been used a certain number of times.
            if (data.UsageCount < UsageThersold) return false;

            // Don't show again too quickly if they haven't reviewed yet.
            if (data.HasConsideredReview)
                if (!string.IsNullOrEmpty(data.LastRequestedDate))
                {
                    var lastRequest = DateTime.Parse(data.LastRequestedDate);
                    if ((DateTime.Now - lastRequest).TotalDays < 7) return false;
                }

            return true;
        }

        private static void ShowReviewPrompt(AssetReviewData data)
        {
            var message = GetPersonalizedMessage(data);

            // This is the pop-up window with buttons.
            var UserChoice = EditorUtility.DisplayDialogComplex(
                "A quick question!",
                message,
                "Yes, I'd like to help!",
                "No, maybe later",
                "Never show this again"
            );

            data.LastRequestedDate = DateTime.Now.ToString();

            if (UserChoice == 0)
            {
                Application.OpenURL(AssetUrl);
                data.HasConsideredReview = true;
            }
            else if (UserChoice == 2)
            {
                data.HasReviewed = true;
            }
        }

        private static void ShowReviewConfirmation(AssetReviewData data)
        {
            // This is the second pop-up window
            var confirmedReview = EditorUtility.DisplayDialog(
                "Thank you!",
                "Looks like you left a review. Thank you for your support!",
                "I already reviewed",
                "I'll review later"
            );

            if (confirmedReview)
                data.HasReviewed = true;
            else
                // Reset the 'HasConsideredReview' flag to show the pop-up again later.
                data.HasConsideredReview = false;
        }

        private static string GetPersonalizedMessage(AssetReviewData data)
        {
            var totalAssets = _data.assets.Count;

            if (data.UsageCount >= 1000)
                return string.Format(LongTimeMessage, data.AssetID);
            if (totalAssets == 1) return string.Format(FirstTimeMessage, data.AssetID);

            if (totalAssets == 2) return string.Format(ReturningUserMessage, data.AssetID);

            return string.Format(LoyalUserMessage, data.AssetID);
        }

        // --- Data Management Methods (as discussed previously) ---
        private static void LoadData()
        {
            if (File.Exists(_filePath))
                try
                {
                    var json = File.ReadAllText(_filePath);
                    _data = JsonUtility.FromJson<AllAssetData>(json) ?? new AllAssetData();
                }
                catch
                {
                    _data = new AllAssetData();
                }
            else
                _data = new AllAssetData();
        }


        private static void SaveData()
        {
            var json = JsonUtility.ToJson(_data, true); // Set to 'true' for pretty printing
            File.WriteAllText(_filePath, json);
        }
    }
}