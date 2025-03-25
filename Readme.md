**A simple and fast collection of video game libraries targeting .net standard 2.1.**

[!NOTE]
This repository is in a really early stage and is far from being production ready. Breaking changes in the API are expected to come. So use at your own risk.

## Overview
This is not an engine, nor a rendering library. This is merely a set of tools and data structures to help you implement your game logic that canbe easily integrated in any tech stack as long as it supports .Net Standard 2.1. 

## Design Goals (Big ideas, does not necessarily represent the current state of the library)
### Performance
- Encourage a Data Oriented approach
- Avoids generating garbage and generaly does not allocate memory behind your back

### Modularity
- Each module is independent and only references the Core Module
- Take what you need, pay only for what you use


### Easy to integrate
- No external dependencies
- Avoids retaining state and does not take over the control flow of your program
- Does not requires tons of refactoring or boilerplate when adding it to your project
- Framework-like features are opt-in

## Modules

### Core
- Data oriented entity management framework
- Common Data structures and algorithms
- Handy extensions

### Physics
- Collision detection between primitive 2D shapes
- Shape overlaps
- Shape casting

### AI
- Grid based, any angle pathfinding using Lazy theta algorithm

## Roadmap
FlipLib is currently built concurrently with a internal game project. This means that features are implemented as they are needed in the game and regularly promoted to the library and features that are not required by the game are not a priority for us. However here are some ideas for the further development of the library.

### Memory Module
- Custom Arena allocator allowing for GC untracked memory allocation

### Physics
- Broad phase collision detection using spatial partitionning
- Convex polygon collision detection

### AI
- Grid constraint pathfinding
- Visibility graph and Navigation Mesh
- StateMachine
