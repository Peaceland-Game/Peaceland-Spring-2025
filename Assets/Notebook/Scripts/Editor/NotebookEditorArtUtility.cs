#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Peaceland.Notebook.Editor
{
    public static class NotebookEditorArtUtility
    {
        private const string NotebookArtFolderPath = "Assets/Notebook/notebook-art";
        private const string NotebookClosedArtPath = NotebookArtFolderPath + "/notebook.png";
        private const string NotebookOpenedArtPath = NotebookArtFolderPath + "/notebookOpened.png";

        private static readonly string[] OpenFramePaths =
        {
            NotebookClosedArtPath,
            NotebookArtFolderPath + "/notebookOpen_1.png",
            NotebookArtFolderPath + "/notebookOpen_2.png",
            NotebookArtFolderPath + "/notebookOpen_3.png",
            NotebookOpenedArtPath,
        };

        public static List<Sprite> LoadOpenAnimationFrames()
        {
            List<Sprite> frames = new List<Sprite>();
            for (int i = 0; i < OpenFramePaths.Length; i++)
            {
                Sprite frame = LoadSpriteAsset(OpenFramePaths[i]);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }

            return frames;
        }

        public static void WireOpenAnimation(NotebookAnimationView animationView, Image animationImage)
        {
            if (animationView == null || animationImage == null)
            {
                return;
            }

            List<Sprite> frames = LoadOpenAnimationFrames();
            animationView.Configure(animationImage, frames);

            SerializedObject serialized = new SerializedObject(animationView);
            SerializedProperty framesProperty = serialized.FindProperty("openFrames");
            framesProperty.arraySize = frames.Count;
            for (int i = 0; i < frames.Count; i++)
            {
                framesProperty.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            }

            serialized.FindProperty("animationImage").objectReferenceValue = animationImage;
            SerializedProperty startScale = serialized.FindProperty("openStartScale");
            if (startScale != null && startScale.floatValue <= 0f)
            {
                startScale.floatValue = 0.38f;
            }

            SerializedProperty endScale = serialized.FindProperty("openEndScale");
            if (endScale != null && endScale.floatValue <= 0f)
            {
                endScale.floatValue = 1f;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            animationImage.preserveAspect = true;
        }

        public static Sprite LoadSpriteAsset(string assetPath)
        {
            EnsureSpriteImport(assetPath);
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static void EnsureSpriteImport(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            if (settings.spriteMeshType != SpriteMeshType.Tight)
            {
                settings.spriteMeshType = SpriteMeshType.Tight;
                importer.SetTextureSettings(settings);
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }
    }
}
#endif
