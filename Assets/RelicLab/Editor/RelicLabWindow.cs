using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RelicLab.EditorTools
{
    /// <summary>
    /// Small editor tool for creating safe material copies and applying visual presets.
    /// Place this script in an Editor folder. It expects a Shader Graph with the property
    /// reference names documented in Documentation/ShaderGraph_Spec.md.
    /// </summary>
    public sealed class RelicLabWindow : EditorWindow
    {
        private const string GeneratedFolder = "Assets/RelicLab/GeneratedMaterials";

        private const string BaseColor = "_BaseColor";
        private const string EmissionColor = "_EmissionColor";
        private const string EmissionIntensity = "_EmissionIntensity";
        private const string RimPower = "_RimPower";
        private const string RimIntensity = "_RimIntensity";
        private const string AnimationSpeed = "_AnimationSpeed";

        [SerializeField] private GameObject targetObject;

        private static readonly MaterialPreset ArcanePreset = new MaterialPreset(
            "Arcane",
            new Color(0.10f, 0.28f, 0.72f, 1f),
            new Color(0.12f, 0.72f, 1f, 1f),
            2.4f, 3.2f, 2.0f, 0.65f);

        private static readonly MaterialPreset ToxicPreset = new MaterialPreset(
            "Toxic",
            new Color(0.12f, 0.34f, 0.08f, 1f),
            new Color(0.32f, 1f, 0.08f, 1f),
            1.8f, 4.5f, 1.5f, 0.38f);

        private static readonly MaterialPreset SolarPreset = new MaterialPreset(
            "Solar",
            new Color(0.55f, 0.16f, 0.025f, 1f),
            new Color(1f, 0.36f, 0.055f, 1f),
            3.0f, 2.4f, 2.8f, 0.22f);

        [MenuItem("Tools/Relic Lab/Open Relic Lab")]
        public static void OpenWindow()
        {
            RelicLabWindow window = GetWindow<RelicLabWindow>();

            window.titleContent = new GUIContent("Relic Lab");
            window.minSize = new Vector2(320f, 440f);
            window.Show();
        }

        private void OnEnable()
        {
            if (targetObject == null && Selection.activeGameObject != null)
            {
                targetObject = Selection.activeGameObject;
            }
        }

        public void CreateGUI()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            rootVisualElement.Clear();
            rootVisualElement.style.paddingLeft = 12;
            rootVisualElement.style.paddingRight = 12;
            rootVisualElement.style.paddingTop = 10;
            rootVisualElement.style.paddingBottom = 10;

            Label title = new Label("RELIC LAB");
            title.style.fontSize = 18;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            rootVisualElement.Add(title);

            Label subtitle = new Label("Material iteration toolkit · Technical Art prototype");
            subtitle.style.marginBottom = 10;
            subtitle.style.whiteSpace = WhiteSpace.Normal;
            rootVisualElement.Add(subtitle);

            ObjectField targetField = new ObjectField("Target GameObject")
            {
                objectType = typeof(GameObject),
                allowSceneObjects = true,
                value = targetObject
            };

            targetField.RegisterValueChangedCallback(evt =>
            {
                targetObject = evt.newValue as GameObject;
                BuildUI();
            });

            rootVisualElement.Add(targetField);

            if (targetObject == null)
            {
                rootVisualElement.Add(new HelpBox(
                    "Select a GameObject with a Renderer, or assign one above. The tool works on material slot 0.",
                    HelpBoxMessageType.Info));

                return;
            }

            Renderer renderer = FindRenderer(targetObject);

            if (renderer == null)
            {
                rootVisualElement.Add(new HelpBox(
                    "No Renderer was found on this object or its children. Add a MeshRenderer/SkinnedMeshRenderer or choose another object.",
                    HelpBoxMessageType.Warning));

                return;
            }

            rootVisualElement.Add(new Label("Renderer: " + GetHierarchyPath(renderer.transform)));

            Material material = GetFirstMaterial(renderer);

            if (material == null)
            {
                rootVisualElement.Add(new HelpBox(
                    "The Renderer has no material in slot 0. Assign a Shader Graph material first.",
                    HelpBoxMessageType.Warning));

                return;
            }

            string materialPath = AssetDatabase.GetAssetPath(material);
            bool isWorkingCopy = !string.IsNullOrEmpty(materialPath) &&
                                 materialPath.Replace('\\', '/').StartsWith(GeneratedFolder + "/");

            rootVisualElement.Add(new Label("Material: " + material.name));

            if (!isWorkingCopy)
            {
                rootVisualElement.Add(new HelpBox(
                    "For safety, Relic Lab won't edit the current material directly. Create a working copy in the generated-materials folder first.",
                    HelpBoxMessageType.Info));

                Button copyButton = new Button(() => CreateWorkingCopy(renderer, material))
                {
                    text = "Create Safe Working Copy"
                };

                copyButton.style.height = 32;
                copyButton.style.marginTop = 6;
                rootVisualElement.Add(copyButton);

                return;
            }

            rootVisualElement.Add(new HelpBox(
                "Editing a generated copy. The source material remains unchanged.",
                HelpBoxMessageType.Info));

            AddSectionTitle("PRESETS");

            VisualElement presetRow = new VisualElement();
            presetRow.style.flexDirection = FlexDirection.Row;
            presetRow.style.flexWrap = Wrap.Wrap;
            presetRow.style.marginBottom = 8;

            AddPresetButton(presetRow, renderer, material, ArcanePreset);
            AddPresetButton(presetRow, renderer, material, ToxicPreset);
            AddPresetButton(presetRow, renderer, material, SolarPreset);
            rootVisualElement.Add(presetRow);

            AddSectionTitle("CREATE MATERIAL VARIANT");

            rootVisualElement.Add(new HelpBox(
                "Create a separate material asset from a preset. The current material is preserved.",
                HelpBoxMessageType.Info));

            VisualElement variantRow = new VisualElement();
            variantRow.style.flexDirection = FlexDirection.Row;
            variantRow.style.flexWrap = Wrap.Wrap;
            variantRow.style.marginBottom = 8;

            AddVariantButton(variantRow, renderer, material, ArcanePreset);
            AddVariantButton(variantRow, renderer, material, ToxicPreset);
            AddVariantButton(variantRow, renderer, material, SolarPreset);
            rootVisualElement.Add(variantRow);

            AddSectionTitle("MATERIAL PARAMETERS");

            AddColorControl(material, BaseColor, "Base Color");
            AddColorControl(material, EmissionColor, "Emission Color");
            AddFloatControl(material, EmissionIntensity, "Emission Intensity", 0f, 8f);
            AddFloatControl(material, RimPower, "Rim Power", 0.1f, 8f);
            AddFloatControl(material, RimIntensity, "Rim Intensity", 0f, 8f);
            AddFloatControl(material, AnimationSpeed, "Animation Speed", 0f, 5f);

            List<string> missing = GetMissingProperties(material);

            if (missing.Count > 0)
            {
                rootVisualElement.Add(new HelpBox(
                    "This shader is missing expected properties: " + string.Join(", ", missing) +
                    ". Check the exact Reference names in Documentation/ShaderGraph_Spec.md.",
                    HelpBoxMessageType.Warning));
            }

            Button saveButton = new Button(() =>
            {
                EditorUtility.SetDirty(material);
                AssetDatabase.SaveAssets();
                ShowNotification(new GUIContent("Material asset saved"));
            })
            {
                text = "Save Material Asset"
            };

            saveButton.style.height = 30;
            saveButton.style.marginTop = 8;
            rootVisualElement.Add(saveButton);

            Label footer = new Label("Prototype v0.1 · Material slot 0 only");
            footer.style.marginTop = 10;
            footer.style.unityFontStyleAndWeight = FontStyle.Italic;
            footer.style.color = new StyleColor(new Color(0.58f, 0.58f, 0.58f));
            rootVisualElement.Add(footer);
        }

        private void AddSectionTitle(string text)
        {
            Label label = new Label(text);
            label.style.marginTop = 12;
            label.style.marginBottom = 5;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            rootVisualElement.Add(label);
        }

        private void AddPresetButton(
            VisualElement parent,
            Renderer renderer,
            Material material,
            MaterialPreset preset)
        {
            Button button = new Button(() => ApplyPreset(renderer, material, preset))
            {
                text = preset.Name
            };

            button.style.flexGrow = 1;
            button.style.minWidth = 74;
            button.style.marginRight = 4;
            parent.Add(button);
        }

        private void AddVariantButton(
            VisualElement parent,
            Renderer renderer,
            Material material,
            MaterialPreset preset)
        {
            Button button = new Button(() => CreateMaterialVariant(renderer, material, preset))
            {
                text = "Create " + preset.Name
            };

            button.style.flexGrow = 1;
            button.style.minWidth = 90;
            button.style.marginRight = 4;
            parent.Add(button);
        }

        private void AddColorControl(Material material, string propertyName, string label)
        {
            if (!material.HasProperty(propertyName)) return;

            ColorField field = new ColorField(label)
            {
                value = material.GetColor(propertyName),
                showAlpha = true
            };

            field.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(material, "Change " + label);
                material.SetColor(propertyName, evt.newValue);
                EditorUtility.SetDirty(material);
            });

            rootVisualElement.Add(field);
        }

        private void AddFloatControl(
            Material material,
            string propertyName,
            string label,
            float min,
            float max)
        {
            if (!material.HasProperty(propertyName)) return;

            Slider field = new Slider(label, min, max)
            {
                value = Mathf.Clamp(material.GetFloat(propertyName), min, max),
                showInputField = true
            };

            field.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(material, "Change " + label);
                material.SetFloat(propertyName, evt.newValue);
                EditorUtility.SetDirty(material);
            });

            rootVisualElement.Add(field);
        }

        private void CreateWorkingCopy(Renderer renderer, Material source)
        {
            EnsureGeneratedFolderExists();

            string path = AssetDatabase.GenerateUniqueAssetPath(
                GeneratedFolder + "/" + MakeSafeFileName(source.name) + "_RelicLab.mat");

            Material copy = new Material(source)
            {
                name = System.IO.Path.GetFileNameWithoutExtension(path)
            };

            AssetDatabase.CreateAsset(copy, path);
            AssetDatabase.SaveAssets();

            Undo.RecordObject(renderer, "Assign Relic Lab Working Material");

            Material[] materials = renderer.sharedMaterials;

            if (materials == null || materials.Length == 0)
            {
                materials = new[] { copy };
            }
            else
            {
                materials[0] = copy;
            }

            renderer.sharedMaterials = materials;
            EditorUtility.SetDirty(renderer);

            targetObject = renderer.gameObject;
            Selection.activeObject = copy;
            BuildUI();
        }

        private void CreateMaterialVariant(Renderer renderer, Material source, MaterialPreset preset)
        {
            EnsureGeneratedFolderExists();

            string path = AssetDatabase.GenerateUniqueAssetPath(
                GeneratedFolder + "/M_Relic_" + MakeSafeFileName(preset.Name) + ".mat");

            Material variant = new Material(source)
            {
                name = System.IO.Path.GetFileNameWithoutExtension(path)
            };

            ApplyPresetValues(variant, preset);
            AssetDatabase.CreateAsset(variant, path);
            AssetDatabase.SaveAssets();

            Undo.RecordObject(renderer, "Assign Relic Lab " + preset.Name + " Material");

            Material[] materials = renderer.sharedMaterials;

            if (materials == null || materials.Length == 0)
            {
                materials = new[] { variant };
            }
            else
            {
                materials[0] = variant;
            }

            renderer.sharedMaterials = materials;
            EditorUtility.SetDirty(renderer);

            targetObject = renderer.gameObject;
            Selection.activeObject = variant;
            BuildUI();
        }

        private void ApplyPreset(Renderer renderer, Material material, MaterialPreset preset)
        {
            Undo.RecordObject(material, "Apply Relic Lab " + preset.Name + " Preset");

            ApplyPresetValues(material, preset);

            EditorUtility.SetDirty(material);
            EditorUtility.SetDirty(renderer);

            BuildUI();
        }

        private static void ApplyPresetValues(Material material, MaterialPreset preset)
        {
            SetColorIfSupported(material, BaseColor, preset.BaseColor);
            SetColorIfSupported(material, EmissionColor, preset.EmissionColor);
            SetFloatIfSupported(material, EmissionIntensity, preset.EmissionIntensity);
            SetFloatIfSupported(material, RimPower, preset.RimPower);
            SetFloatIfSupported(material, RimIntensity, preset.RimIntensity);
            SetFloatIfSupported(material, AnimationSpeed, preset.AnimationSpeed);
        }

        private static void SetColorIfSupported(Material material, string property, Color value)
        {
            if (material.HasProperty(property))
            {
                material.SetColor(property, value);
            }
        }

        private static void SetFloatIfSupported(Material material, string property, float value)
        {
            if (material.HasProperty(property))
            {
                material.SetFloat(property, value);
            }
        }

        private static List<string> GetMissingProperties(Material material)
        {
            string[] expected =
            {
                BaseColor,
                EmissionColor,
                EmissionIntensity,
                RimPower,
                RimIntensity,
                AnimationSpeed
            };

            return expected.Where(property => !material.HasProperty(property)).ToList();
        }

        private static Renderer FindRenderer(GameObject target)
        {
            if (target == null)
            {
                return null;
            }

            return target.GetComponent<Renderer>() ?? target.GetComponentInChildren<Renderer>();
        }

        private static Material GetFirstMaterial(Renderer renderer)
        {
            if (renderer == null)
                return null;

            Material[] materials = renderer.sharedMaterials;

            return materials != null && materials.Length > 0 ? materials[0] : null;
        }

        private static string GetHierarchyPath(Transform item)
        {
            if (item == null)
            {
                return "(none)";
            }

            string path = item.name;

            while (item.parent != null)
            {
                item = item.parent;
                path = item.name + "/" + path;
            }

            return path;
        }

        private static string MakeSafeFileName(string fileName)
        {
            foreach (char invalid in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalid, '_');
            }

            return string.IsNullOrWhiteSpace(fileName) ? "RelicMaterial" : fileName;
        }

        private static void EnsureGeneratedFolderExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/RelicLab"))
            {
                AssetDatabase.CreateFolder("Assets", "RelicLab");
            }

            if (!AssetDatabase.IsValidFolder(GeneratedFolder))
            {
                AssetDatabase.CreateFolder("Assets/RelicLab", "GeneratedMaterials");
            }
        }

        private sealed class MaterialPreset
        {
            public readonly string Name;
            public readonly Color BaseColor;
            public readonly Color EmissionColor;
            public readonly float EmissionIntensity;
            public readonly float RimPower;
            public readonly float RimIntensity;
            public readonly float AnimationSpeed;

            public MaterialPreset(
                string name,
                Color baseColor,
                Color emissionColor,
                float emissionIntensity,
                float rimPower,
                float rimIntensity,
                float animationSpeed)
            {
                Name = name;
                BaseColor = baseColor;
                EmissionColor = emissionColor;
                EmissionIntensity = emissionIntensity;
                RimPower = rimPower;
                RimIntensity = rimIntensity;
                AnimationSpeed = animationSpeed;
            }
        }
    }
}
