# Cooperative 2D Adventure

A **2D cooperative exploration and puzzle game** built with **Unity and C#**. The project focuses on procedural world generation, multiplayer gameplay, resource management, and mechanics that encourage two players to work together.

> **Status:** In development. Features and systems are being implemented iteratively.

## Overview

Each session generates a new **7 × 7 grid-based world**. Players explore zones independently while sharing the same world and coordinating to gather resources, use items, and progress towards the final objective.

The map includes a starting house, a goal location, a river that shapes exploration, and a Wolf Den. Other zones are generated to add variety to each run. Map generation applies placement and accessibility rules to help ensure playable layouts.

## Implemented Systems

- **Procedural map generation** — randomised 7 × 7 map with placement constraints for key locations and the river.
- **Cooperative multiplayer** — networking built with Unity Netcode for GameObjects.
- **Zone-based exploration** — players can move between zones independently.
- **Shared world state** — both players interact with the same generated map and shared world data.
- **Player roles** — Gardener and Builder, with different starting tools and crafting options.
- **Inventory and resources** — item collection and inventory management, including Flowers, Branches, Coins, and crafted items.
- **Crafting** — recipes such as crafting a Bouquet from Flowers and a Log from Branches.
- **Interactive chests** — zone-based item storage and loot.
- **Map UI** — displays explored map information and player positions.
- **Zone-specific behaviour** — different zone types provide distinct interactions and gameplay challenges.

## Gameplay Loop

1. Explore the randomly generated map.
2. Search zones and collect resources.
3. Use each role's tools and crafting options.
4. Coordinate with the other player to overcome obstacles.
5. Progress towards the final objective.

## In Development

The game is still being built, so not every planned mechanic is complete. Current development priorities include:

- Completing and balancing the remaining zone mechanics.
- Expanding cooperative interactions and puzzles.
- Developing the wolf and other encounter behaviour.
- Completing the final objective, victory conditions, and match flow.
- Improving UI feedback, audiovisual presentation, and overall polish.
- Testing and refining multiplayer synchronisation.

## Technologies

- **Unity**
- **C#**
- **Unity Netcode for GameObjects**
- **Procedural Generation**
- **Object-Oriented Programming**

## Project Structure

The Unity project is located in `ConcursilloV3/ZoneGame`. Gameplay scripts, prefabs, scenes, and project settings are organised within the Unity project.

## Goal of the Project

This is a personal game-development project to gain practical experience with **Unity, C#, multiplayer networking, procedural generation, gameplay systems, and software architecture**.

## Repository

[Browse the ZoneGame project files](https://github.com/ARNII05/Unity-Personal-Projects/tree/main/ConcursilloV3/ZoneGame/Assets/MyProject)
