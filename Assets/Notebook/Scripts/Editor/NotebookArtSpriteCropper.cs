#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Peaceland.Notebook.Editor
{
    /// <summary>
    /// Physically crops notebook PNGs to their opaque pixels, then reimports as tight single sprites.
    /// Metadata-only rect changes are unreliable (and Trim is blocked while 9-slice borders are set).
    /// </summary>
    public static class NotebookArtSpriteCropper
    {
        private const string ArtFolder = "Assets/Notebook/notebook-art";
        private const byte AlphaCutoff = 6;
        private const int PaddingPixels = 2;

        [MenuItem("Peaceland/Notebook/Crop Notebook Art Sprites")]
        public static void CropAllMenu()
        {
            int cropped = CropAllInFolder();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Notebook art crop finished. Physically cropped " + cropped + " PNG(s).");
        }

        [MenuItem("Peaceland/Notebook/Crop Notebook Opened Sprite (Selected)")]
        public static void CropSelectedMenu()
        {
            Object selected = Selection.activeObject;
            string path = selected != null ? AssetDatabase.GetAssetPath(selected) : string.Empty;
            if (string.IsNullOrEmpty(path) || !path.StartsWith(ArtFolder))
            {
                path = ArtFolder + "/notebookOpened.png";
            }

            if (CropTextureSprite(path, log: true))
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogWarning("Notebook art crop failed for: " + path);
            }
        }

        public static int CropAllInFolder()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { ArtFolder });
            int cropped = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (CropTextureSprite(path, log: true))
                {
                    cropped++;
                }
            }

            return cropped;
        }

        private static bool CropTextureSprite(string assetPath, bool log)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return false;
            }

            bool wasReadable = importer.isReadable;
            ConfigureImporterForCrop(importer, wasReadable);
            importer.SaveAndReimport();

            Texture2D source = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (source == null || !TryGetOpaqueBounds(source, out RectInt bounds))
            {
                RestoreReadable(importer, wasReadable);
                return false;
            }

            bounds = ApplyPadding(bounds, source.width, source.height);
            if (bounds.width <= 0 || bounds.height <= 0)
            {
                RestoreReadable(importer, wasReadable);
                return false;
            }

            if (bounds.x == 0 && bounds.y == 0 && bounds.width == source.width && bounds.height == source.height)
            {
                if (log)
                {
                    Debug.Log("Notebook art crop skipped (already tight): " + assetPath);
                }

                ClearSpriteBorders(importer);
                RestoreReadable(importer, wasReadable);
                return true;
            }

            Texture2D cropped = new Texture2D(bounds.width, bounds.height, TextureFormat.RGBA32, false);
            Color[] pixels = source.GetPixels(bounds.x, bounds.y, bounds.width, bounds.height);
            cropped.SetPixels(pixels);
            cropped.Apply();

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string absolutePath = Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
            File.WriteAllBytes(absolutePath, cropped.EncodeToPNG());
            Object.DestroyImmediate(cropped);

            importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return false;
            }

            ConfigureImporterForCrop(importer, wasReadable);
            ClearSpriteBorders(importer);

            if (!ApplyFullSpriteRect(importer, assetPath, bounds.width, bounds.height))
            {
                RestoreReadable(importer, wasReadable);
                return false;
            }

            RestoreReadable(importer, wasReadable);
            importer.SaveAndReimport();

            if (log)
            {
                Debug.Log(
                    "Notebook art cropped " + assetPath +
                    " -> " + bounds.width + "x" + bounds.height +
                    " (removed transparent margins; sprite border cleared).");
            }

            return true;
        }

        private static void ConfigureImporterForCrop(TextureImporter importer, bool wasReadable)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.isReadable = true;

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.Tight;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spriteGenerateFallbackPhysicsShape = true;
            importer.SetTextureSettings(settings);
        }

        private static void ClearSpriteBorders(TextureImporter importer)
        {
            importer.spriteBorder = Vector4.zero;
        }

        private static void RestoreReadable(TextureImporter importer, bool wasReadable)
        {
            if (importer == null || wasReadable)
            {
                return;
            }

            importer.isReadable = false;
        }

        private static bool ApplyFullSpriteRect(TextureImporter importer, string assetPath, int width, int height)
        {
            SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            if (dataProvider == null)
            {
                return false;
            }

            dataProvider.InitSpriteEditorDataProvider();

            string spriteName = Path.GetFileNameWithoutExtension(assetPath);
            Rect fullRect = new Rect(0f, 0f, width, height);

            List<SpriteRect> spriteRects = dataProvider.GetSpriteRects().ToList();
            GUID spriteId;
            if (spriteRects.Count == 0)
            {
                spriteId = GUID.Generate();
                spriteRects.Add(new SpriteRect
                {
                    name = spriteName,
                    spriteID = spriteId,
                    rect = fullRect,
                    alignment = SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    border = Vector4.zero,
                });
            }
            else
            {
                SpriteRect spriteRect = spriteRects[0];
                spriteId = spriteRect.spriteID;
                if (spriteId.Empty())
                {
                    spriteId = GUID.Generate();
                }

                spriteRect.name = string.IsNullOrEmpty(spriteRect.name) ? spriteName : spriteRect.name;
                spriteRect.spriteID = spriteId;
                spriteRect.rect = fullRect;
                spriteRect.alignment = SpriteAlignment.Center;
                spriteRect.pivot = new Vector2(0.5f, 0.5f);
                spriteRect.border = Vector4.zero;
                spriteRects[0] = spriteRect;
            }

            dataProvider.SetSpriteRects(spriteRects.ToArray());

            ISpriteNameFileIdDataProvider nameFileIdProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (nameFileIdProvider != null)
            {
                SpriteRect primary = spriteRects[0];
                List<SpriteNameFileIdPair> pairs = new List<SpriteNameFileIdPair>
                {
                    new SpriteNameFileIdPair(primary.name, primary.spriteID),
                };
                nameFileIdProvider.SetNameFileIdPairs(pairs);
            }

            dataProvider.Apply();
            return true;
        }

        private static bool TryGetOpaqueBounds(Texture2D texture, out RectInt bounds)
        {
            bounds = default;
            Color32[] pixels = texture.GetPixels32();
            int width = texture.width;
            int height = texture.height;
            int minX = width;
            int minY = height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (pixels[(y * width) + x].a <= AlphaCutoff)
                    {
                        continue;
                    }

                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y);
                    maxY = Mathf.Max(maxY, y);
                }
            }

            if (maxX < minX || maxY < minY)
            {
                return false;
            }

            bounds = new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
            return true;
        }

        private static RectInt ApplyPadding(RectInt bounds, int textureWidth, int textureHeight)
        {
            int xMin = Mathf.Max(0, bounds.xMin - PaddingPixels);
            int yMin = Mathf.Max(0, bounds.yMin - PaddingPixels);
            int xMaxExclusive = Mathf.Min(textureWidth, bounds.xMax + PaddingPixels);
            int yMaxExclusive = Mathf.Min(textureHeight, bounds.yMax + PaddingPixels);
            return new RectInt(xMin, yMin, xMaxExclusive - xMin, yMaxExclusive - yMin);
        }
    }
}
#endif
