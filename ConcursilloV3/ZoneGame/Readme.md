# Hello, Grandma! — Cooperative 2D Adventure

A **2D cooperative exploration and survival game** developed with **Unity and C#**. Designed for two players, the game combines procedural map generation, multiplayer networking, resource management, crafting, and enemy AI.

Players must explore a mysterious forest, collect resources, overcome obstacles, and work together to deliver a bouquet of flowers to Grandma while avoiding a dangerous wolf.

> **Status:** In development. Core gameplay systems are being implemented and refined.

## Overview

Each match generates a new **7 × 7 grid-based world**, consisting of 49 interconnected zones. The map layout and the distribution of resources and special elements change between matches.

Both players share the same world but can explore different areas independently. They must coordinate their actions, manage their individual inventories, and use their respective roles to progress.

The main objective is to craft a bouquet of flowers and deliver it to Grandma's House. Along the way, players must cross rivers, gather resources, interact with different zones, and avoid the wolf.

Exploration also offers optional challenges, such as finding three Ancient Relics scattered throughout the map to unlock a special chest in the Ruins.

## Implemented Systems

- **Procedural Map Generation** — Randomised 7 × 7 map with placement constraints for special zones and gameplay elements.
- **Cooperative Multiplayer** — Multiplayer networking implemented with Unity Netcode for GameObjects.
- **Zone-based Exploration** — Players can move independently between interconnected zones.
- **Shared World State** — Both players interact with the same generated world and its shared elements.
- **Player Roles** — Gardener and Builder, each with exclusive crafting abilities and different starting equipment.
- **Inventory System** — Individual player inventories with limited capacity and item management.
- **Item Transfer and Storage** — Players can deposit and retrieve items through shared chests.
- **Crafting System** — Role-specific recipes, including Bouquets from Flowers and Logs from Branches.
- **Interactive Chests** — Chests provide item storage and loot, with special rewards available in the Ruins.
- **Trading System** — Players can buy and sell items using Coins in the Village.
- **Wolf AI** — Enemy behaviour with prioritised objectives, pathfinding, player detection, chasing, and item stealing.
- **Buff System** — Special items provide temporary or passive gameplay advantages.
- **Ancient Relics** — Three randomly distributed relics are required to unlock the special Ruins chest.
- **Zone-specific Mechanics** — Different zone types provide unique interactions, resources, obstacles, and challenges.
- **Map and Radar UI** — Role-specific tools provide information about the surrounding areas and the wolf's proximity.

## Gameplay Loop

1. Explore the randomly generated forest.
2. Search zones, interact with objects, and collect resources.
3. Manage inventories and store items in shared chests.
4. Use role-specific crafting abilities to create essential items.
5. Avoid the wolf and use available resources to survive.
6. Cooperate to overcome obstacles and progress through the map.
7. Craft the bouquet and reach Grandma's House together.

Players can also explore the Ruins and collect Ancient Relics to unlock additional rewards.

## Key Gameplay Mechanics

### Player Roles

- **Gardener:** Specialises in Flowers, can craft the Bouquet, and starts with a Radar.
- **Builder:** Specialises in construction, can craft Logs to cross rivers, and starts with a Map.

Only the corresponding role can craft its exclusive items.

### Wolf AI

The wolf moves around the map following a priority-based behaviour system:

1. Return to its den when it needs to store stolen items.
2. Search for Meat.
3. Detect and chase players.
4. Move randomly when no higher-priority objective is available.

When it catches a player, it can steal a random number of items from their inventory before returning to its den.

### Ancient Relics and Ruins

Three Ancient Relics are randomly distributed throughout eligible zones. Players can collect them and use them to unlock the special chest in the Ruins.

The chest contains Coins and a random selection of special rewards, including the Agility Potion, Luck Potion, and Ancestral Necklace.

### Buffs and Special Items

Special items provide additional gameplay effects:

- **Agility Potion:** Increases movement speed.
- **Luck Potion:** Provides a chance to prevent the wolf from stealing items after catching a player.
- **Ancestral Necklace:** Provides protection against the wolf and the Swamp.

## In Development

The project is still under development. Further work includes:

- Refining and balancing gameplay mechanics.
- Improving UI feedback and audiovisual presentation.
- Testing multiplayer synchronisation and edge cases.
- Refining the overall gameplay experience.
- Completing and polishing the remaining gameplay and match-flow elements.

## Technologies

- **Unity 6.3**
- **C#**
- **Unity Netcode for GameObjects**
- **Unity Transport**
- **Procedural Generation**
- **Object-Oriented Programming**
- **Pathfinding (BFS)**

## Project Structure

The Unity project is located in:

`ConcursilloV3/ZoneGame`

Gameplay scripts, prefabs, scenes, and project settings are organised within the Unity project.

## Goal of the Project

This is a personal game-development project focused on gaining practical experience with **Unity, C#, multiplayer networking, procedural generation, AI behaviour, gameplay systems, and software architecture**.

It also serves as an opportunity to design and develop interconnected systems that work together in a cooperative multiplayer environment.

## Repository

[Browse the ZoneGame project files](https://github.com/ARNII05/Unity-Personal-Projects/tree/main/ConcursilloV3/ZoneGame)