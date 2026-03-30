// PrefabAnatomyProcessor.cs — batch mode + prefab/model handling + cleanup + root components
// Categorization uses CategoryTag (no extra children).
// Overrides use case-insensitive CONTAINS matching.
// Updated: Added Bone, Muscle, Fat, Skin, Nerve categories with defaults + classification.
// NEW: Root rename options (TitleCase or use full Normalize) for the parent object.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public enum AnatomyCategory
{
    Arteries,
    Veins,
    Organs,
    HollowGI,
    Glands,
    Bone,
    Muscle,
    Fat,
    Skin,
    Nerve,
    Other
}

public enum ProcessingMode
{
    SingleTarget,
    AllFirstChildren
}

[Serializable]
public class StructureOverride
{
    [Tooltip("Keyword/pattern to look for in the child's normalized name, e.g. 'Kidney' or 'Inferior Vena Cava'.")]
    public string name;
    public Material material;

    [Tooltip("More keywords that should also map to this material, e.g. 'IVC', 'Common Iliac Vein'.")]
    public string[] aliases;
}

[ExecuteAlways]
public class PrefabAnatomyProcessor : MonoBehaviour
{
    [Header("Mode")]
    public ProcessingMode processingMode = ProcessingMode.SingleTarget;

    [Header("Single Target (prefab asset, model asset, or scene instance)")]
    public GameObject target;

    [Header("When target is a model asset")]
    public string outputFolder = "";

    [Header("Scope")]
    public bool onlyFirstChildren = true;
    public bool includeDescendants = false;

    [Header("Steps")]
    public bool doRename = true;

    [Tooltip("Annotate each first child with a CategoryTag component (no extra child objects).")]
    public bool doCategorize = true;

    public bool doApplyMaterials = true;

    [Header("Root rename")]
    [Tooltip("Rename the parent/root object (TitleCase by default).")]
    public bool renameRoot = true;

    [Tooltip("If on, uses full Normalize() on the parent; otherwise just TitleCase with minimal cleanup.")]
    public bool rootUseFullNormalize = false;

    [Header("Root Components (after materials)")]
    public bool addComponents = true;
    public bool ptvSyncPosition = true;
    public bool ptvSyncRotation = true;
    public bool ptvSyncScale = true;

    [Header("Cleanup (first-level children only)")]
    public bool removeTopLevelExtras = true;
    public string[] removeIfNameContains = new[] { "camera", "light", "cube" };

    [Tooltip("If older runs created '[Category]' helper children, remove them.")]
    public bool removeLegacyCategoryMarkers = true;

    [Header("Scene prefab instances")]
    public bool unpackSceneInstance = true;

    [Header("Category defaults (fallbacks)")]
    public Material arteriesDefault, veinsDefault, organsDefault, hollowGiDefault, glandsDefault, boneDefault, muscleDefault, fatDefault, skinDefault, nerveDefault, otherDefault;

    [Header("Per-structure overrides (first match wins; uses case-insensitive CONTAINS)")]
    public List<StructureOverride> overrides = new();

#if UNITY_EDITOR
    [ContextMenu("Process Now")]
    public void ProcessNow()
    {
        if (processingMode == ProcessingMode.AllFirstChildren)
        {
            ProcessAllFirstChildrenOfThis();
            return;
        }

        if (!target) { Debug.LogWarning("No target assigned."); return; }

        if (target.scene.IsValid())
        {
            if (unpackSceneInstance && PrefabUtility.IsPartOfPrefabInstance(target))
            {
                var rootToUnpack = PrefabUtility.IsOutermostPrefabInstanceRoot(target)
                    ? target
                    : PrefabUtility.GetOutermostPrefabInstanceRoot(target);
                if (rootToUnpack)
                    PrefabUtility.UnpackPrefabInstance(rootToUnpack, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            }

            int count = ProcessRoot(target.transform);
            if (addComponents) SetupRootComponents(target);
            Debug.Log($"Processed scene object '{target.name}' — touched {count} items.");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(target);
        if (string.IsNullOrEmpty(assetPath)) { Debug.LogWarning("Could not resolve asset path."); return; }

        if (Path.GetExtension(assetPath).Equals(".prefab", StringComparison.OrdinalIgnoreCase))
        {
            ProcessPrefabAssetAtPath(assetPath);
            return;
        }

        string ext = Path.GetExtension(assetPath).ToLowerInvariant();
        bool isModel = ext is ".fbx" or ".glb" or ".gltf" or ".obj";
        if (!isModel) { Debug.LogWarning($"Not a prefab or supported model file: {assetPath}"); return; }

        string saveDir = string.IsNullOrWhiteSpace(outputFolder) ? Path.GetDirectoryName(assetPath) : outputFolder;
        if (string.IsNullOrWhiteSpace(saveDir)) saveDir = "Assets";
        if (!AssetDatabase.IsValidFolder(saveDir)) { Debug.LogWarning($"Output folder not found: {saveDir}"); return; }

        var modelRoot = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (!modelRoot) { Debug.LogWarning($"Could not load model at {assetPath}"); return; }

        var temp = (GameObject)PrefabUtility.InstantiatePrefab(modelRoot);
        temp.SetActive(false);
        temp.transform.position = Vector3.zero;

        string baseName = Path.GetFileNameWithoutExtension(assetPath);
        string savePath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(saveDir, baseName + "_processed.prefab"));

        bool ok = PrefabUtility.SaveAsPrefabAsset(temp, savePath, out bool success);
        UnityEngine.Object.DestroyImmediate(temp);
        if (!ok || !success) { Debug.LogWarning($"Failed to create prefab at {savePath}"); return; }

        ProcessPrefabAssetAtPath(savePath);
    }

    void ProcessAllFirstChildrenOfThis()
    {
        int processed = 0;
        foreach (Transform child in transform)
        {
            var go = child.gameObject;

            if (unpackSceneInstance && PrefabUtility.IsPartOfPrefabInstance(go))
            {
                var rootToUnpack = PrefabUtility.IsOutermostPrefabInstanceRoot(go)
                    ? go
                    : PrefabUtility.GetOutermostPrefabInstanceRoot(go);
                if (rootToUnpack)
                    PrefabUtility.UnpackPrefabInstance(rootToUnpack, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            }

            int count = ProcessRoot(go.transform);
            if (addComponents) SetupRootComponents(go);
            processed++;
            Debug.Log($"Processed child '{go.name}' — touched {count} items.");
        }

        Debug.Log(processed == 0
            ? "No first children found to process."
            : $"Batch complete. Processed {processed} first-child object(s) under '{name}'.");
    }

    void ProcessPrefabAssetAtPath(string path)
    {
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            int count = ProcessRoot(root.transform);
            if (addComponents) SetupRootComponents(root);

            PrefabUtility.SaveAsPrefabAsset(root, path, out bool success);
            Debug.Log(success
                ? $"Processed prefab '{Path.GetFileName(path)}' — touched {count} items, saved."
                : $"Processed prefab but save failed for '{path}'.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    int ProcessRoot(Transform root)
    {
        // rename the root/parent first
        if (renameRoot && root != null)
        {
            var before = root.name;
            var after = rootUseFullNormalize ? Normalize(before) : Titleize(before);
            if (!string.IsNullOrEmpty(after) && after != before)
                root.name = after;
        }

        if (removeTopLevelExtras)
            CleanupFirstChildren(root);

        var targets = includeDescendants
            ? root.GetComponentsInChildren<Transform>(true).Where(t => t != root)
            : EnumerateDirect(root);

        int touched = 0;
        foreach (var t in targets)
        {
            if (onlyFirstChildren && t.parent != root) continue;

            string original = t.name;
            string nice = doRename ? Normalize(original) : original;
            if (doRename && original != nice) t.name = nice;

            var cat = Classify(nice);

            if (doApplyMaterials)
            {
                var mat = GetOverrideMaterialByContains(nice);
                if (!mat) mat = CategoryMat(cat);

                if (mat)
                    foreach (var r in t.GetComponentsInChildren<Renderer>(true))
                        r.sharedMaterial = mat;
            }

            if (doCategorize)
            {
                var tag = t.GetComponent<CategoryTag>() ?? t.gameObject.AddComponent<CategoryTag>();
                tag.category = cat;

                if (removeLegacyCategoryMarkers)
                {
                    var toRemove = new List<Transform>();
                    foreach (Transform c in t)
                        if (c.name.Length >= 2 && c.name[0] == '[' && c.name[c.name.Length - 1] == ']')
                            UnityEngine.Object.DestroyImmediate(c.gameObject);
                }
            }

            touched++;
        }

        if (root.gameObject.scene.IsValid())
            EditorSceneManager.MarkSceneDirty(root.gameObject.scene);

        return touched;
    }

    // Overrides: case-insensitive CONTAINS matching on normalized names
    Material GetOverrideMaterialByContains(string normalizedChildName)
    {
        if (overrides == null || overrides.Count == 0) return null;
        var hay = normalizedChildName?.ToLowerInvariant() ?? "";
        foreach (var o in overrides)
        {
            if (o == null || o.material == null) continue;
            if (ContainsWordLike(hay, o.name)) return o.material;
            if (o.aliases != null)
                foreach (var a in o.aliases)
                    if (ContainsWordLike(hay, a)) return o.material;
        }
        return null;
    }

    bool ContainsWordLike(string hay, string needle)
    {
        if (string.IsNullOrWhiteSpace(needle)) return false;
        var n = needle.ToLowerInvariant().Trim();
        if (hay.Contains(n)) return true;
        return Regex.IsMatch(hay, $@"\b{Regex.Escape(n)}\b", RegexOptions.IgnoreCase);
    }

    void CleanupFirstChildren(Transform root)
    {
        var toDelete = new List<GameObject>();
        foreach (Transform child in root)
        {
            bool kill = false;

            if (removeIfNameContains != null && removeIfNameContains.Length > 0)
            {
                var lower = child.name.ToLowerInvariant();
                foreach (var kw in removeIfNameContains)
                {
                    if (!string.IsNullOrWhiteSpace(kw) && lower.Contains(kw.ToLowerInvariant()))
                    {
                        kill = true; break;
                    }
                }
            }

            if (kill) toDelete.Add(child.gameObject);
        }

        foreach (var go in toDelete)
            UnityEngine.Object.DestroyImmediate(go);
    }

    IEnumerable<Transform> EnumerateDirect(Transform parent) { foreach (Transform c in parent) yield return c; }

    void SetupRootComponents(GameObject rootGO)
    {
        EnsureComponent(rootGO, "ModelLayerSync");
        EnsureComponent(rootGO, "ShaderRenderingModeSwitcher");

#if PHOTON_UNITY_NETWORKING
        var photonView = rootGO.GetComponent<Photon.Pun.PhotonView>() ?? rootGO.AddComponent<Photon.Pun.PhotonView>();
        var ptv = rootGO.GetComponent<Photon.Pun.PhotonTransformView>() ?? rootGO.AddComponent<Photon.Pun.PhotonTransformView>();

        ptv.m_SynchronizePosition = ptvSyncPosition;
        ptv.m_SynchronizeRotation = ptvSyncRotation;
        ptv.m_SynchronizeScale = ptvSyncScale;

        if (photonView.ObservedComponents == null)
            photonView.ObservedComponents = new List<Component>();
        if (!photonView.ObservedComponents.Contains(ptv))
            photonView.ObservedComponents.Add(ptv);
#else
        Debug.LogWarning("Photon not found. Import PUN 2 (PHOTON_UNITY_NETWORKING) to add Photon components.");
#endif
    }

    Component EnsureComponent(GameObject go, string typeName)
    {
        var t = Type.GetType(typeName) ?? AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(x => x.Name == typeName);
        if (t == null) { Debug.LogWarning($"Missing script '{typeName}' in project."); return null; }
        return go.GetComponent(t) ?? go.AddComponent(t);
    }

    Material CategoryMat(AnatomyCategory cat) =>
        cat switch
        {
            AnatomyCategory.Arteries => arteriesDefault,
            AnatomyCategory.Veins => veinsDefault,
            AnatomyCategory.Organs => organsDefault,
            AnatomyCategory.HollowGI => hollowGiDefault,
            AnatomyCategory.Glands => glandsDefault,
            AnatomyCategory.Bone => boneDefault,
            AnatomyCategory.Muscle => muscleDefault,
            AnatomyCategory.Fat => fatDefault,
            AnatomyCategory.Skin => skinDefault,
            AnatomyCategory.Nerve => nerveDefault,
            _ => otherDefault
        };

    public static string Normalize(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return s;
        s = Regex.Replace(s, @"\.\d{3,}$", "");
        var m = Regex.Match(s, @"(?i)\bsegmentation\b[\s_:\/-]*(.*)$");
        if (m.Success && !string.IsNullOrWhiteSpace(m.Groups[1].Value)) s = m.Groups[1].Value;
        var tail = Regex.Match(s, @"(?i).*[:_\/-]\s*([^:_\/-]+(?:[\s_][^:_\/-]+)*)$");
        if (tail.Success && !Regex.IsMatch(s, @"^\s*[^:_\/-]+\s*$")) s = tail.Groups[1].Value;
        s = s.Replace('_', ' ');
        s = Regex.Replace(s, @"\s+", " ").Trim();

        // normalize artery abbreviations
        s = Regex.Replace(s, @"(?i)\bart\.?\b", "Artery");
        s = Regex.Replace(s, @"(?i)\barter\b", "Artery");
        s = Regex.Replace(s, @"(?i)\barte\b", "Artery");

        // remove any . or , at the end
        s = Regex.Replace(s, @"[.,]$", "").Trim();

        var lower = s.ToLowerInvariant();
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(lower);
    }

    // minimal TitleCase (no aggressive trimming) for root names
    public static string Titleize(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return s;
        s = s.Replace('_', ' ');
        s = Regex.Replace(s, @"\s+", " ").Trim();
        s = Regex.Replace(s, @"[.,]$", "").Trim();
        var lower = s.ToLowerInvariant();
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(lower);
    }

    static AnatomyCategory Classify(string n)
    {
        var x = (n ?? "").ToLowerInvariant();
        x = Regex.Replace(x.Replace("_", " "), @"\s+", " ");

        // Veins
        if (Regex.IsMatch(x, @"\bvein\b|\bvenous\b|\bvena cava\b|\bivc\b"))
            return AnatomyCategory.Veins;

        // Arteries (artery/arterial/aorta + 'art', 'art.' or 'arte' tokens)
        if (Regex.IsMatch(x, @"\bartery\b|\barterial\b|\baorta\b|\bart\.?\b|\barte\b|arteries"))
            return AnatomyCategory.Arteries;

        // Nerves
        if (Regex.IsMatch(x, @"\bnerve\b|\bneural\b|\bplexus\b|\bganglion\b|\bspinal cord\b|\bnerve root\b|\bsciatic\b"))
            return AnatomyCategory.Nerve;

        // Hollow GI
        if (Regex.IsMatch(x, @"\besophagus\b|\bstomach\b|\bduodenum\b|\bjejunum\b|\bileum\b|\bsmall bowel\b|\bcolon\b|\brectum\b"))
            return AnatomyCategory.HollowGI;

        // Glands
        if (Regex.IsMatch(x, @"\badrenal\b|\bgland\b"))
            return AnatomyCategory.Glands;

        // Bone
        if (Regex.IsMatch(x,
              @"\bbone\b|\brib\b|\bribs\b|\bvertebra\b|\bvertebrae\b|\bspine\b|\bskull\b|\bcranium\b|\bmandible\b|\bmaxilla\b|\bclavicle\b|\bscapula\b|\bsternum\b|\bpelvis\b|\bsacrum\b|\bcoccyx\b|\bfemur\b|\btibia\b|\bfibula\b|\bhumerus\b|\bradius\b|\bulna\b|\bpatella\b|\bilium\b|\bischium\b|\bpubis\b"))
            return AnatomyCategory.Bone;

        // Muscle
        if (Regex.IsMatch(x,
              @"\bmuscle\b|\bpsoas\b|\billio?psoas\b|\bdiaphragm\b|\brectus\b|\boblique\b|\bintercostal\b|\bdeltoid\b|\bbiceps\b|\btriceps\b|\bglute(?:us|al)\b|\blatissimus\b|\bpiriformis\b"))
            return AnatomyCategory.Muscle;

        // Fat
        if (Regex.IsMatch(x, @"\bfat\b|\badipose\b|\bomentum\b|\bomental\b|\bmesenteric fat\b|\bsubcutaneous\b"))
            return AnatomyCategory.Fat;

        // Skin
        if (Regex.IsMatch(x, @"\bskin\b|\bdermis\b|\bepidermis\b|\bcutaneous\b"))
            return AnatomyCategory.Skin;

        // Solid organs
        if (Regex.IsMatch(x, @"\bliver\b|\bspleen\b|\bpancreas\b|\bkidney\b|\bgallbladder\b|\bheart\b"))
            return AnatomyCategory.Organs;

        return AnatomyCategory.Other;
    }
#endif
}
