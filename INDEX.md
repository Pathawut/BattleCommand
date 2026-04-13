# BattleCommand Folder Index

This index gives a quick map of the repository layout and where to look first.

## Top-Level Folders

- `Assets/` - Game content, scenes, scripts, prefabs, and project assets.
- `Packages/` - Unity package configuration (manifest and lock file).
- `ProjectSettings/` - Unity project-wide settings.
- `UserSettings/` - Local/editor user settings.
- `Library/` - Unity generated cache (do not edit manually).
- `Temp/` - Unity temporary build/import files.
- `Logs/` - Unity/editor log output.

## Key Project Files

- `BattleCommand.sln` - Solution file for external C# IDEs.
- `NavMeshPlus.csproj` - Runtime assembly project.
- `NavMeshPlusEditor.csproj` - Editor assembly project.
- `.gitignore` - Git ignore rules.
- `.vsconfig` - Visual Studio workload hints.

## Core Content Areas

- `Assets/Scenes/` - Main gameplay scene(s), including `SampleScene.unity`.
- `Assets/Settings/` - Render pipeline, scene templates, and build profile assets.
- `Assets/NavMeshComponents/` - NavMesh Plus runtime/editor scripts and asmdef files.
- `Assets/Images/` - Sprites and map textures (base, tanks, terrains).
- `Assets/Prefab/` - Prefab assets.

## Unity Version

- Editor: `6000.2.15f1` (from `ProjectSettings/ProjectVersion.txt`).

## Installed Package Highlights

- `com.unity.render-pipelines.universal` (URP)
- `com.unity.feature.2d`
- `com.unity.inputsystem`
- `com.unity.2d.tilemap.extras`
- `com.unity.test-framework`

Source: `Packages/manifest.json`

## Notes

- If you are new to this project, start with `Assets/Scenes/` and `Assets/NavMeshComponents/`.
- `Library/`, `Temp/`, and `Logs/` are generated/runtime folders and are typically not tracked for source changes.
