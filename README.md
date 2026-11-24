# Karinderya

A 3D Filipino karinderya management game built in Unity 6. The player farms ingredients, cooks Filipino dishes, stocks buffet trays, serves lunch and dinner customers, completes daily requests, and grows the village's trust.

## Requirements

- Unity `6000.3.9f1`
- Universal Render Pipeline (URP)
- Windows

## Run the project

1. Open the repository folder in Unity Hub.
2. Open `Assets/Scenes/MainMenuScene.unity`.
3. Press Play, then start the game from the main menu.

The gameplay scene is `Assets/Scenes/PlayingScene.unity`. Core managers and UI are authored directly in the scene and are visible in the Hierarchy and Inspector.

## Controls

| Input | Action |
|---|---|
| W A S D | Move |
| Mouse | Look |
| Left click | Interact / serve |
| E | Context action |
| I | Inventory |
| B | Seed shop |
| Esc | Close notebook / pause |
| F1 | Skip tutorial |

## Game loop

1. Plant, water, and harvest vegetables.
2. Cook dishes at the pots.
3. Put cooked dishes on their matching buffet trays.
4. Serve customers during the lunch and dinner rushes.
5. Read daily requests from the request board.
6. Close the stall, review the day summary, and continue the next morning.

## Notes

- Saves are stored at `Application.persistentDataPath/karinderya_save.json`.
- On Windows, the folder is `%USERPROFILE%\AppData\LocalLow\DefaultCompany\Karinderya`.
- Delete `karinderya_save.json` to start a fresh game.
- The core systems are scene-authored; dynamic gameplay objects such as customers, crops, and recipe rows are instantiated at runtime.
