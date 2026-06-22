#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Bloomtown.Client.Editor
{
    /// <summary>
    /// Import tekstur Mixamo (.fbm) + pastikan material memakai shader project.
    /// </summary>
    public static class MixamoMaterialUtility
    {
        private static readonly string[] TextureExtensions = { ".png", ".jpg", ".jpeg", ".tga" };

        public static void ConfigureModelMaterials(string modelPath)
        {
            if (string.IsNullOrEmpty(modelPath)) return;

            var fullPath = Path.Combine(
                Path.GetDirectoryName(Application.dataPath) ?? string.Empty,
                modelPath).Replace('\\', '/');
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"[Bloomtown] FBX tidak ada: {modelPath}");
                return;
            }

            var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer == null) return;

            importer.animationType       = ModelImporterAnimationType.Human;
            importer.avatarSetup         = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation     = true;
            importer.materialImportMode  = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.materialLocation    = ModelImporterMaterialLocation.InPrefab;
            importer.materialSearch      = ModelImporterMaterialSearch.Local;
            importer.SaveAndReimport();

            ConfigureTextureImportSettings(modelPath);
            EnsureMaterialsUseProjectShader(modelPath);
        }

        public static Shader FindProjectLitShader()
        {
            return Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Diffuse");
        }

        private static void ConfigureTextureImportSettings(string modelPath)
        {
            foreach (var dir in GetTextureSearchDirs(modelPath))
            {
                if (!Directory.Exists(dir)) continue;

                foreach (var ext in TextureExtensions)
                {
                    foreach (var file in Directory.GetFiles(dir, "*" + ext))
                    {
                        var assetPath = ToAssetPath(file);
                        if (string.IsNullOrEmpty(assetPath)) continue;

                        var texImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                        if (texImporter == null) continue;

                        var lower = Path.GetFileName(file).ToLowerInvariant();
                        texImporter.sRGBTexture = !lower.Contains("normal") && !lower.Contains("specular");
                        texImporter.textureType = lower.Contains("normal")
                            ? TextureImporterType.NormalMap
                            : TextureImporterType.Default;
                        texImporter.alphaIsTransparency = lower.Contains("diffuse");
                        texImporter.SaveAndReimport();
                    }
                }
            }
        }

        private static void EnsureMaterialsUseProjectShader(string modelPath)
        {
            var litShader = FindProjectLitShader();
            if (litShader == null)
            {
                Debug.LogWarning("[Bloomtown] Shader Lit/Standard tidak ditemukan — material Mixamo tidak diperbaiki.");
                return;
            }

            var textures = LoadTexturesForModel(modelPath);
            var fixedAny = false;

            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(modelPath))
            {
                if (asset is not Material mat) continue;
                if (FixMaterial(mat, litShader, textures))
                    fixedAny = true;
            }

            if (fixedAny)
                AssetDatabase.SaveAssets();
        }

        private static bool FixMaterial(Material mat, Shader litShader, Texture2D[] textures)
        {
            if (MaterialLooksValid(mat))
                return false;

            var mainTex = mat.mainTexture
                ?? mat.GetTexture("_MainTex")
                ?? mat.GetTexture("_BaseMap")
                ?? FindBestTexture(mat.name, textures);

            var normalTex = FindNormalTexture(mat.name, textures);
            var color     = mat.HasProperty("_Color") ? mat.GetColor("_Color") : mat.color;

            mat.shader = litShader;

            if (mainTex != null)
            {
                mat.mainTexture = mainTex;
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", mainTex);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", mainTex);
            }

            if (normalTex != null && mat.HasProperty("_BumpMap"))
                mat.SetTexture("_BumpMap", normalTex);

            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.35f);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.35f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);

            EditorUtility.SetDirty(mat);
            return true;
        }

        private static bool MaterialLooksValid(Material mat)
        {
            if (mat == null || mat.shader == null) return false;
            if (mat.shader.name == "Hidden/InternalErrorShader") return false;

            var mainTex = mat.mainTexture ?? mat.GetTexture("_MainTex") ?? mat.GetTexture("_BaseMap");
            return mainTex != null;
        }

        private static Texture2D[] LoadTexturesForModel(string modelPath)
        {
            return GetTextureSearchDirs(modelPath)
                .Where(Directory.Exists)
                .SelectMany(dir => TextureExtensions.SelectMany(ext => Directory.GetFiles(dir, "*" + ext)))
                .Select(ToAssetPath)
                .Where(p => !string.IsNullOrEmpty(p))
                .Distinct()
                .Select(p => AssetDatabase.LoadAssetAtPath<Texture2D>(p))
                .Where(t => t != null)
                .ToArray();
        }

        private static string[] GetTextureSearchDirs(string modelPath)
        {
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var modelDir    = Path.GetDirectoryName(Path.Combine(projectRoot, modelPath));
            if (string.IsNullOrEmpty(modelDir))
                return System.Array.Empty<string>();

            var modelName = Path.GetFileNameWithoutExtension(modelPath);
            return new[]
            {
                modelDir,
                Path.Combine(modelDir, modelName + ".fbm"),
                Path.Combine(modelDir, "Textures"),
                Path.Combine(modelDir, "textures"),
            };
        }

        private static Texture2D FindBestTexture(string materialName, Texture2D[] textures)
        {
            var key = NormalizeName(materialName);

            foreach (var tex in textures)
            {
                var texKey = NormalizeName(tex.name);
                if (texKey.Contains("normal") || texKey.Contains("specular")) continue;
                if (texKey.Contains(key) || key.Contains(texKey))
                    return tex;
            }

            foreach (var tex in textures)
            {
                var texKey = NormalizeName(tex.name);
                if (texKey.Contains("normal") || texKey.Contains("specular")) continue;
                if (texKey.Contains("diffuse") || texKey.Contains("albedo") || texKey.Contains("basecolor"))
                    return tex;
            }

            return textures.FirstOrDefault(t =>
            {
                var n = NormalizeName(t.name);
                return !n.Contains("normal") && !n.Contains("specular");
            });
        }

        private static Texture2D FindNormalTexture(string materialName, Texture2D[] textures)
        {
            var key = NormalizeName(materialName);
            foreach (var tex in textures)
            {
                var texKey = NormalizeName(tex.name);
                if (!texKey.Contains("normal")) continue;
                if (texKey.Contains(key) || key.Contains(texKey.Replace("normal", "")))
                    return tex;
            }

            return textures.FirstOrDefault(t => NormalizeName(t.name).Contains("normal"));
        }

        private static string NormalizeName(string value)
        {
            return value.ToLowerInvariant()
                .Replace(" ", "")
                .Replace("_", "")
                .Replace("-", "")
                .Replace(".", "");
        }

        private static string ToAssetPath(string fullPath)
        {
            fullPath = fullPath.Replace('\\', '/');
            var dataPath = Application.dataPath.Replace('\\', '/');
            if (!fullPath.StartsWith(dataPath))
                return null;

            return "Assets" + fullPath.Substring(dataPath.Length);
        }
    }
}
#endif