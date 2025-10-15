// RenameChildren.cs
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class RenameChildren : MonoBehaviour
{
    [Tooltip("Only rename direct children of this GameObject.")]
    public bool onlyFirstChildren = true;

    [Tooltip("Also rename grandchildren and deeper objects.")]
    public bool includeDescendants = false;

    [Tooltip("If true, only logs what would change.")]
    public bool dryRun = false;

    [ContextMenu("Rename Children Now")]
    public void RenameNow()
    {
        int count = 0;

        // Choose which transforms to iterate
        var targets = includeDescendants
            ? GetComponentsInChildren<Transform>(true)
            : transform.Cast<Transform>(); // direct children

        foreach (var t in targets)
        {
            if (t == transform) continue;                 // skip self
            if (onlyFirstChildren && t.parent != transform) continue;

            string oldName = t.name;
            string newName = Normalize(oldName);

            if (oldName != newName)
            {
                if (dryRun) Debug.Log($"Would rename '{oldName}' → '{newName}'", t);
                else t.name = newName;

                count++;
            }
        }

        Debug.Log($"Renamed {count} object(s) under '{name}'.");
    }

    static readonly TextInfo TI = new CultureInfo("en-US", false).TextInfo;

    public static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        // Drop Unity's duplicate suffixes like ".001"
        input = Regex.Replace(input, @"\.\d{3,}$", "");

        // If there's an explicit 'segmentation' token, prefer everything after it
        var m = Regex.Match(input, @"(?i)\bsegmentation\b[\s_:\/-]*(.*)$");
        if (m.Success && !string.IsNullOrWhiteSpace(m.Groups[1].Value))
            input = m.Groups[1].Value;

        // Handle things like "segment:segmentation:duodenum" -> take the last chunk
        // before we do global replacements
        var tail = Regex.Match(input, @"(?i).*[:_\/-]\s*([^:_\/-]+(?:[\s_][^:_\/-]+)*)$");
        if (tail.Success && !Regex.IsMatch(input, @"^\s*[^:_\/-]+\s*$"))
            input = tail.Groups[1].Value;

        // Replace underscores, collapse spaces, trim
        input = input.Replace('_', ' ');
        input = Regex.Replace(input, @"\s+", " ").Trim();

        // If any leading codes/numbers leaked through, drop them
        input = Regex.Replace(input, @"^(?:[A-Za-z]{1,3}\d+|\d+(?:\.\d+)?|\w{1,4}\d*)\s+", "");

        // Title case the final label
        input = TI.ToTitleCase(input.ToLowerInvariant());

        return input;
    }
}

// Helper so "transform.Cast<Transform>()" works even without LINQ in some setups
public static class TransformEnumerableExtensions
{
    public static System.Collections.Generic.IEnumerable<Transform> Cast<T>(this Transform parent)
    {
        foreach (Transform child in parent) yield return child;
    }
}
