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
        private GClass907 _presetIcon;
        private bool _isShowed;
        private PlayerModelView targetPlayerModelView;
        
        public void SaveSpriteAsPNG(string filePath, Sprite spriteToSave)
        {
            Texture2D texture = spriteToSave.texture;
            byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, pngData);
            LeaderboardPlugin.logger.LogWarning($"Saved player icon in {filePath}");
        }
        
        public void IconChangedState()
        {
            bool flag = _presetIcon.Sprite != null;
            LeaderboardPlugin.logger.LogWarning($"Is loaded icon? {flag}");

            if (flag)
            {
                LeaderboardPlugin.SendProfileIcon(_presetIcon);
                new IconSaver().SaveSpriteAsPNG(GlobalData.LeaderboardIconPath, _presetIcon.Sprite);
            }
        }
        
        public void SetChildActive(GameObject parent, string childName, bool isActive)
        {
            bool flag = parent == null || string.IsNullOrEmpty(childName);
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
        
        public void HidePlayerModelExtraElements(GameObject modelInstance)
        {
            bool flag = modelInstance == null;
            if (flag)
            {
                LeaderboardPlugin.logger.LogWarning("HidePlayerModelExtraElements - modelInstance is null.");
            }
            else
            {
                SetChildActive(modelInstance, "IconsContainer", false);
                SetChildActive(modelInstance, "DragTrigger", false);
                SetChildActive(modelInstance, "BottomField", false);
            }
        }
        
        //This code is taken from https://github.com/jbobyh/JBOBYH_ItemPreviewQoL/blob/master/Patches/ItemSpecifications_Show_Patch.cs
        private bool CaptureFromRenderTexture(RawImage rawImage, string filePath)
        {
            // 1. Get RenderTexture from RawImage
            Texture source = rawImage.texture;
            if (source is not RenderTexture renderTexture)
            {
                NotificationManagerClass.DisplayMessageNotification("Error 3. Failed to save screenshot", ENotificationDurationType.Default, ENotificationIconType.Alert, null);
                LeaderboardPlugin.logger.LogError("Текстура в RawImage не является RenderTexture!");
                // You can add logic for regular Texture2D here if you need it
                return false;
            }

            // Save a reference to the currently active RenderTexture to return it later.
            RenderTexture currentActiveRT = RenderTexture.active;

            try
            {
                // 2. Create a temporary Texture2D for copying pixels
                Texture2D texture2D = new(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);

                // 3. Make our RenderTexture active
                RenderTexture.active = renderTexture;

                // 4. Copy pixels from the active RenderTexture to our Texture2D
                texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

                // 5. Apply changes to load pixels into the texture
                texture2D.Apply();

                Texture2D croppedTexture = CropToVisible(texture2D);

                // 6. Now that we have Texture2D, we can encode it into a PNG
                byte[] bytes = croppedTexture.EncodeToPNG();
                File.WriteAllBytes(filePath, bytes);

                // Clean the temporary texture
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
                // 7. ВОЗВРАЩАЕМ ОБРАТНО исходную активную RenderTexture. Это КРИТИЧЕСКИ ВАЖНО!
                RenderTexture.active = currentActiveRT;
            }
            return true;
        }
        
        public void FindPlayerModelStats()
        {
            GameObject playerModelView = LeaderboardPlugin.Instance.ClonePlayerModelViewPrefab;

            if (playerModelView != null)
            {
                LeaderboardPlugin.logger.LogWarning("Нашли объект PlayerModelView");
                targetPlayerModelView = playerModelView.GetComponentInChildren<PlayerModelView>();
                if (targetPlayerModelView)
                {
                    LeaderboardPlugin.logger.LogWarning("Нашли компонент PlayerModelView");

                    if (!_isShowed)
                    {
                       ISession backEndSession = PatchConstants.BackEndSession;
                       if (((backEndSession != null) ? backEndSession.Profile : null) != null)
                       {
                           StaticManager.Instance.StartCoroutine(WaitForLoadingComplete());
                           targetPlayerModelView.Show(PatchConstants.BackEndSession.Profile, null, null, 0f, null, false).ConfigureAwait(false);
                       }
                       else
                       {
                           LeaderboardPlugin.logger.LogError("FindPlayerModelStats - BackEndSession.Profile is null. Cannot show player model view script.");
                       } 
                    }
                    else
                    {
                        PlayerModelLoaded();
                    }
                }
                else
                {
                    LeaderboardPlugin.logger.LogWarning("Не нашли компонент PlayerModelView, грустно. Плакать");
                }
            }
            else
            {
                LeaderboardPlugin.logger.LogWarning("Не найден объект PlayerModelView по указанному пути");
            }
        }

        public void PlayerModelLoaded()
        {
            GameObject playerModelView = LeaderboardPlugin.Instance.ClonePlayerModelViewPrefab;

            RawImage rawImage = playerModelView.GetComponent<RawImage>();

            if (rawImage)
            {
                LeaderboardPlugin.logger.LogWarning("Нашли RawImage: " + rawImage.name);
                if (CaptureFromRenderTexture(rawImage, GlobalData.LeaderboardFullImagePath))
                {
                    LeaderboardPlugin.logger.LogWarning("В теории сохранили чудика");
                }
                else
                {
                    LeaderboardPlugin.logger.LogWarning("В теории НЕТУ чудика");
                }
            }
            else
            {
                LeaderboardPlugin.logger.LogWarning("RawImage не найден в PlayerModelView");
            }
        }
        
        public GameObject CreateClonedPlayerModelView(GameObject parent, GameObject prefab)
        {
            bool flag = parent == null || prefab == null;
            GameObject clonedPlayerModelObject;
            if (flag)
            {
                LeaderboardPlugin.logger.LogError("CreateClonedPlayerModelView - Parent or Prefab is null.");
                clonedPlayerModelObject = null;
            }
            else
            {
                GameObject o = Instantiate(prefab, parent.transform);
                o.name = "[SPTLeaderboard]PlayerModelView";
                o.SetActive(true);
                o.transform.position = GetOffScreenPosition();
                clonedPlayerModelObject = o;
            }
            return clonedPlayerModelObject;
        }
        
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
        
        private IEnumerator WaitForLoadingComplete()
        {
            while (!targetPlayerModelView.LoadingComplete)
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
        
        public void Create(bool isIcon = true)
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
                LeaderboardPlugin.SendProfileIcon(_presetIcon);
                SaveSpriteAsPNG(GlobalData.LeaderboardIconPath, _presetIcon.Sprite);
            }
        }
    }
}
