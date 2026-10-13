# Relic Lab

**A Unity Editor tool for creating and iterating on stylized relic materials.**

Relic Lab combines a custom **URP Shader Graph** with a **C# EditorWindow** to make material experimentation faster and safer. Artists and developers can explore three visual presets, adjust shader parameters with immediate feedback, and decide whether to apply, discard, or save their work as a separate material asset.

> **Status:** Portfolio prototype — functional material editing workflow. Visual presentation and demo media are in progress.

## Features

- **Three starting presets:** Arcane, Toxic, and Solar.
- **Live, non-destructive preview:** Adjust properties on the selected Renderer without immediately changing its material asset.
- **Apply / Discard:** Commit previewed values to the current material or revert the preview.
- **Create material variants:** Generate independent material assets from the built-in presets without automatically assigning them to the selected object.
- **Save Preview As New Material:** Save the current preview as a new `.mat` asset while preserving the original material and the object's material assignment.
- **Safe working copies:** Create a generated material copy before editing materials outside the tool's generated-materials folder.

## Tech Stack

- **Unity:** 6.6 (`6000.6.5f1`)
- **Render pipeline:** Universal Render Pipeline (URP)
- **Shader:** Unity Shader Graph (Lit)
- **Editor tooling:** C#, Unity Editor API, UI Toolkit

## Getting Started

1. Open the project in **Unity 6.6**.
2. Open the sample scene at `Assets/Scenes/SampleScene.unity`.
3. Select a GameObject with a Renderer and a material using the Relic Lab shader.
4. In the Unity menu, choose **Tools > Relic Lab > Open Relic Lab**.
5. Assign the GameObject in the **Target GameObject** field if needed. Relic Lab works with **material slot 0**.
6. If prompted, select **Create Safe Working Copy** before editing.
7. Choose a preset or adjust individual parameters. Changes are previewed on the selected object.
8. Use **Apply Changes**, **Discard Changes**, or **Save Preview As New Material** depending on whether you want to update, revert, or preserve a new variation.

Generated material assets are stored in `Assets/RelicLab/GeneratedMaterials/`.

## Shader Parameters

| Property | Purpose |
| --- | --- |
| Base Color | Main surface color |
| Emission Color | Color of the emissive effect |
| Emission Intensity | Strength of emission |
| Rim Power | Width/falloff of the Fresnel-based rim effect |
| Rim Intensity | Strength of the rim contribution |
| Animation Speed | Speed of the pulsing emission |
| Metallic | Metallic response of the Lit surface |
| Smoothness | Surface smoothness and reflection sharpness |

The shader combines **Lit surface shading**, **Fresnel-based rim emission**, and **time-driven emission pulsing**. Metallic appearance depends on the scene's lighting and reflection environment, not only on the Metallic value.

## Editing Workflow

```text
Select an object and material
            |
      Choose a preset
       or edit values
            |
       Live preview
            |
     +------+------+------------------+
     |             |                  |
 Apply Changes  Discard Changes  Save Preview As New Material
     |             |                  |
 Update current  Revert preview  Create independent .mat
 material asset                  (no automatic reassignment)
```

**Preset variants** and **saved previews** serve different purposes: creating a preset variant uses that preset's predefined values; saving a preview captures the values currently being previewed.

## Implementation Notes

- Preview values are applied to the selected Renderer using `MaterialPropertyBlock`, keeping the source `.mat` unchanged until changes are explicitly applied.
- The tool uses the first material slot of the selected Renderer.
- New material assets receive unique paths to avoid overwriting existing variants.
- The project is an **Editor workflow prototype**, not a runtime material-customization system.

## Project Structure

```text
Assets/
├── RelicLab/
│   ├── Editor/                 # RelicLabWindow.cs
│   ├── GeneratedMaterials/     # Generated .mat assets
│   └── Shaders/               # SG_RelicLit.shadergraph
└── Scenes/
    └── SampleScene.unity
```

## Demo and Screenshots

*Coming soon:* Screenshots of the EditorWindow, side-by-side material variants, and a short demonstration of the non-destructive editing workflow.

## Project Goal

Relic Lab is a portfolio project focused on the intersection of **Technical Art** and **Tools Programming**. Its goal is to demonstrate shader authoring, editor tooling, asset workflow design, and practical usability improvements for material iteration.
