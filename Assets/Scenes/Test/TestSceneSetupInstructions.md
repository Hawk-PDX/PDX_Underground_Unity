# PDX Underground Test Scene Setup

## Overview
This document explains how to set up the PDX Underground test scene using the TestSceneInitializer component.

## Setting Up the Scene

### 1. Create a New Scene
1. Open Unity
2. Go to File > New Scene
3. Save the scene as `PDXUndergroundTestScene.unity` in the `Assets/Scenes/Test` directory

### 2. Create Required GameObjects
1. Create an empty GameObject named "TestSceneSetup"
2. Add the `TestSceneInitializer` component to it
3. Configure the inspector settings (detailed below)

### 3. Required Prefab References
The TestSceneInitializer needs references to these prefabs:
- **Player Prefab**: A prefab with GamblerCharacter and PlayerController components
- **MainGameController Prefab**: A prefab with the MainGameController component
- **BuzzUIController Prefab**: A prefab with the BuzzUIController component

If these don't exist yet, you'll need to create them:

#### Creating Player Prefab

