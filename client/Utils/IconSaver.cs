using System;
using System.Collections;
using UnityEngine;
using System.IO;
using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.UI;
using SPT.Reflection.Utils;
using SPTLeaderboard.Data;
using UnityEngine.UI;

namespace SPTLeaderboard.Utils
{
    public class IconSaver : MonoBehaviour
    {
        private bool _isShowed;
        private GClass907 _presetIcon;
        private PlayerModelView _targetPlayerModelView;
        
        public GameObject clonePlayerModelViewObj;
        
        /// <summary>
        /// Create icon with face player model 500*500
        /// </summary>
        public void CreateIcon()
        {
            var profile = PlayerHelper.GetProfile();

            XYCellSizeStruct textureSize = new XYCellSizeStruct(500, 500);

            _presetIcon = Singleton<GClass905>.Instance.method_11(new GClass910(profile.Inventory.Equipment.CloneVisibleItem(), profile.Customization), textureSize);

            if (_presetIcon.Sprite == null)
            {
                _presetIcon.Changed.Bind(IconChangedState);
            }
            else
            {
                LeaderboardPlugin.SendProfileIcon(_presetIcon.Sprite.texture, false);
                SaveSpriteAsPNG(GlobalData.LeaderboardIconPath, _presetIcon.Sprite);
            }
        }
        
        /// <summary>
        /// Create icon with full body player model 1295*2160
        /// </summary>
        public void CreateFullBodyIcon()
        {
            if (clonePlayerModelViewObj != null)
            {
#if DEBUG
                LeaderboardPlugin.logger.LogWarning("Found obj PlayerModelView");
#endif
                _targetPlayerModelView = clonePlayerModelViewObj.GetComponentInChildren<PlayerModelView>();
                if (_targetPlayerModelView)
                {
#if DEBUG
                    LeaderboardPlugin.logger.LogWarning("Found component PlayerModelView");
#endif
                    if (!_isShowed)
                    {
                        ISession backEndSession = PatchConstants.BackEndSession;
                        if (backEndSession?.Profile != null)
                        {
                            StaticManager.Instance.StartCoroutine(WaitForLoadingComplete());
                            _targetPlayerModelView.Show(PatchConstants.BackEndSession.Profile, null, null, 0f, null, false).ConfigureAwait(false);
                        }
                        else
                        {
                            LeaderboardPlugin.logger.LogError("CreateFullIcon - BackEndSession.Profile is null. Cannot show player model view script.");
                        } 
                    }
                    else
                    {
                        PlayerModelLoaded();
                    }
                }
                else
                {
                    LeaderboardPlugin.logger.LogWarning("Not found component PlayerModelView");
                }
            }
        }
        
        /// <summary>
        /// Capture screenshot from raw image and save by path
        /// </summary>
        /// <param name="rawImage"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private bool CaptureFromRenderTexture(RawImage rawImage, string filePath)
        {
            Texture source = rawImage.texture;
            if (source is not RenderTexture renderTexture)
            {
                LeaderboardPlugin.logger.LogError("The texture in RawImage is not a RenderTexture!");
                return false;
            }
            
            RenderTexture currentActiveRT = RenderTexture.active;

            try
            {
                Texture2D texture2D = new(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
                
                RenderTexture.active = renderTexture;
                
                texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                
                texture2D.Apply();

                Texture2D croppedTexture = CropCentered(texture2D);
                
                LeaderboardPlugin.SendProfileIcon(croppedTexture, true);
                
                byte[] bytes = croppedTexture.EncodeToPNG();
                File.WriteAllBytes(filePath, bytes);
                
                Destroy(texture2D);
                Destroy(croppedTexture);
            }
            catch (Exception ex)
            {
                NotificationManagerClass.DisplayMessageNotification("Error 4. Failed to save screenshot", ENotificationDurationType.Default, ENotificationIconType.Alert, null);
                LeaderboardPlugin.logger.LogError($"Ошибка при сохранении скриншота: {ex.Message}");
                LeaderboardPlugin.logger.LogError($"{ex.StackTrace}");
                return false;
            }
            finally
            {
                RenderTexture.active = currentActiveRT;
            }
            
            return true;
        }

        /// <summary>
        /// Method when player model initialized and loaded
        /// </summary>
        public void PlayerModelLoaded()
        {
            RawImage rawImage = clonePlayerModelViewObj.GetComponent<RawImage>();

            if (rawImage)
            {
                if (CaptureFromRenderTexture(rawImage, GlobalData.LeaderboardFullImagePath))
                {
                    LeaderboardPlugin.logger.LogInfo("Saved fullbody icon");
                }
                else
                {
                    LeaderboardPlugin.logger.LogWarning("Not saved fullbody icon");
                }
            }
            else
            {
                LeaderboardPlugin.logger.LogWarning("RawImage not found in PlayerModelView");
            }
        }
        
        /// <summary>
        /// Create clone gameObject and place it in other gameObject
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public GameObject CreateClonedPlayerModelView(GameObject parent, GameObject prefab)
        {
            var flag = parent == null || prefab == null;
            GameObject clonedPlayerModelObject;
            if (flag)
            {
                LeaderboardPlugin.logger.LogError("CreateClonedPlayerModelView - Parent or Prefab is null.");
                clonedPlayerModelObject = null;
            }
            else
            {
                GameObject o = Instantiate(prefab, parent.transform);
                o.name = "[SPTLeaderboard] PlayerModelView";
                o.SetActive(true);
                o.transform.position = GetOffScreenPosition();
                clonedPlayerModelObject = o;
            }
            return clonedPlayerModelObject;
        }

        #region Utils
        
        private void SaveSpriteAsPNG(string filePath, Sprite spriteToSave)
        {
            Texture2D texture = spriteToSave.texture;
            byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, pngData);
            LeaderboardPlugin.logger.LogInfo($"Saved player icon in {filePath}");
        }
        
        private void IconChangedState()
        {
            if (_presetIcon.Sprite)
            {
                LeaderboardPlugin.SendProfileIcon(_presetIcon.Sprite.texture, false);
                SaveSpriteAsPNG(GlobalData.LeaderboardIconPath, _presetIcon.Sprite);
            }
        }
        
        /// <summary>
        /// Used for search ideal size for crop
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        private Texture2D CropToVisible(Texture2D original)
        {
            int width = original.width;
            int height = original.height;
            Color[] pixels = original.GetPixels();

            int left = width, right = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = pixels[y * width + x];
                    if (pixel.a > 0.01f || pixel.r + pixel.g + pixel.b > 0.01f)
                    {
                        if (x < left) left = x;
                        if (x > right) right = x;
                    }
                }
            }
            
            left = Mathf.Clamp(left, 0, width - 1);
            right = Mathf.Clamp(right, 0, width - 1);

            int croppedWidth = right - left + 1;
            int croppedHeight = height;

            if (croppedWidth <= 0 || croppedHeight <= 0)
            {
                LeaderboardPlugin.logger.LogWarning("[Screenshot] Nothing to crop horizontally, returning full image");
                return original;
            }

            if (left + croppedWidth > width)
            {
                LeaderboardPlugin.logger.LogError("[Screenshot] Cropping region is out of bounds");
                return original;
            }

            Texture2D cropped = new Texture2D(croppedWidth, croppedHeight, TextureFormat.ARGB32, false);
            cropped.SetPixels(original.GetPixels(left, 0, croppedWidth, croppedHeight));
            cropped.Apply();

            return cropped;
        }
        
        /// <summary>
        /// Cropping image by target size
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        private Texture2D CropCentered(Texture2D original)
        {
            const int targetWidth = 1295;
            const int targetHeight = 2160;

            int centerX = original.width / 2;
            int centerY = original.height / 2;

            int startX = centerX - (targetWidth / 2);
            int startY = centerY - (targetHeight / 2);

            // Защита от выхода за пределы изображения
            startX = Mathf.Clamp(startX, 0, original.width - targetWidth);
            startY = Mathf.Clamp(startY, 0, original.height - targetHeight);

            Color[] pixels = original.GetPixels(startX, startY, targetWidth, targetHeight);
            Texture2D cropped = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
            cropped.SetPixels(pixels);
            cropped.Apply();

            return cropped;
        }
        
        /// <summary>
        /// Little latency for capture screenshot FullBody
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForLoadingComplete()
        {
            while (!_targetPlayerModelView.LoadingComplete)
            {
                yield return null;
            }

            _isShowed = true;
            yield return new WaitForSeconds(2f);

            PlayerModelLoaded();
        }
        
        private Vector3 GetOffScreenPosition(float offset = 500f)
        {
            return new Vector3(Screen.width + offset, Screen.height / 2f, 0f);
        }
        
        private void SetChildActive(GameObject parent, string childName, bool isActive)
        {
            var flag = parent == null || string.IsNullOrEmpty(childName);
            if (!flag)
            {
                Transform transform = parent.transform.Find(childName);
                bool flag2 = transform != null;
                if (flag2)
                {
                    transform.gameObject.SetActive(isActive);
                }
                else
                {
                    LeaderboardPlugin.logger.LogDebug(childName + " not found in " + parent.name + ".");
                }
            }
        }
        
        public void HidePlayerModelExtraElements()
        {
            
            var flag = clonePlayerModelViewObj == null;
            if (flag)
            {
                LeaderboardPlugin.logger.LogWarning("HidePlayerModelExtraElements - modelObj is null.");
            }
            else
            {
                SetChildActive(clonePlayerModelViewObj, "IconsContainer", false);
                SetChildActive(clonePlayerModelViewObj, "DragTrigger", false);
                SetChildActive(clonePlayerModelViewObj, "BottomField", false);
            }
        }
        
        #endregion
    }
}
