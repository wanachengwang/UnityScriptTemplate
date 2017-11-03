
using UnityEditor;
using UnityEngine;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace ScriptTemplates {

    // Note: This class uses UnityEditorInternal which is an undocumented internal feature
    /// <summary>
    /// Template generator for TagLayerScene script.
    /// </summary>
    [ScriptTemplate("TagLayerScene Definition", 300)]
    public class TagLayeSceneTemplate : ScriptTemplateGenerator {

        static bool _bUpperCaseWithUnderscore = false;

        /// <inheritdoc/>
        public override bool WillGenerateEditorScript {
            get { return IsEditorScript; }
        }

        /// <inheritdoc/>
		public override void OnGUI() {
            GUILayout.Label("Output Options:", EditorStyles.boldLabel);
            _bUpperCaseWithUnderscore = EditorGUILayout.ToggleLeft("Naming Style: TAG_SAMPLE", _bUpperCaseWithUnderscore);
        }

        public override string GenerateScript(string scriptName, string ns) {
            var sb = CreateScriptBuilder();

            sb.AppendLine("// WARNING: This class is auto-generated DO NOT modify!");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(ns))
                sb.BeginNamespace("namespace " + ns + OpeningBraceInsertion);

            // Build type declaration string.
            var declaration = new List<string>();

            if (TypeVisibility == TypeVisibility.Public)
                declaration.Add("public");
            else if (TypeVisibility == TypeVisibility.Internal)
                declaration.Add("internal");

            declaration.Add("static");
            declaration.Add("class");
            declaration.Add(scriptName);

            sb.BeginNamespace(string.Join(" ", declaration.ToArray()) + OpeningBraceInsertion);

            sb.AppendLine("// Tag Definitions");
            WriteTags(sb);

            sb.AppendLine("// Scene Definitions");
            WriteScenes(sb);

            sb.AppendLine("// Layer Definitions");
            WriteLayers(sb);

            sb.EndNamespace("}");

            if (!string.IsNullOrEmpty(ns))
                sb.EndNamespace("}");

            return sb.ToString();
        }
        private static void WriteTags(ScriptBuilder sb) {
            string[] tags = UnityEditorInternal.InternalEditorUtility.tags;
            foreach(string tag in tags) {
                sb.AppendLine("public const string " + FormatString("Tag " + tag) + " = " + '"' + tag + '"' + ";");
            }
        }
        private static void WriteScenes(ScriptBuilder sb) {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            string[] sceneNames = new string[scenes.Length];
            for (var n = 0; n < scenes.Length; n++) {
                string sceneName = Path.GetFileNameWithoutExtension(scenes[n].path);
                sb.AppendLine("public const string " + FormatString("Scene " + sceneName) + " = " + '"' + sceneName + '"' + ";");
                sb.AppendLine("public const int " + FormatString("Scene Id " + sceneName) + " = " + '"' + n + '"' + ";");
            }
            sb.AppendLine("public const int " + FormatString("Scenes Cnt") + " = " + scenes.Length + ";");
        }
        private static void WriteLayers(ScriptBuilder sb) {
            string[] layers = UnityEditorInternal.InternalEditorUtility.layers;
            foreach (string layer in layers) {
                sb.AppendLine("public const int " + FormatString("Layer " + layer) + " = " + LayerMask.NameToLayer(layer) + ";");
            }
            sb.AppendLine();
            sb.BeginNamespace("public static int LayerMaskIncluding( params int[] layers )" + OpeningBraceInsertion);
            sb.AppendLine("int mask = 0;");
            sb.AppendLine("for( var i = 0; i < layers.Length; i++ )  mask |= ( 1 << layers[i] );");
            sb.AppendLine("return mask;");
            sb.EndNamespace("}");

            sb.AppendLine();
            sb.BeginNamespace("public static int LayerMaskAllBut( params int[] layers )" + OpeningBraceInsertion);
            sb.AppendLine("return ~LayerMaskIncluding( layers );");
            sb.EndNamespace("}");
        }
        private static string FormatString(string input) {
            if (_bUpperCaseWithUnderscore) {
                return ToUpperWithUnderscore(input);
            } else {
                return ToPascalCase(input);
            }
        }
        private static string ToPascalCase(string input) {
            char[] seperators = { ' '};
            string str = Regex.Replace(input, @"[^a-zA-Z\d]+", " ");
            string[] words = str.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i < words.Length; i++) {
                if (char.IsLower(words[i][0])) {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
                }
            }
            return string.Concat(words);
        }
        private static string ToUpperWithUnderscore(string input) {
            char[] seperators = { ' ' };
            string str = Regex.Replace(input, @"[^a-zA-Z\d]+", " ");
            string[] words = str.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++) {
                words[i] = i != 0 ? "_"+words[i].ToUpper() : words[i].ToUpper();
            }
            return string.Concat(words);
        }
        
    }

}
