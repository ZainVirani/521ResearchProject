# 521ResearchProject
Research project for 521.

This project is a study on A* pathfinding through time.

## Context
The game is a simple maze with a start and end goal and discrete movement. Every now and then the environment will randomly change causing the calculated AI path to be recalculated and it may therefore become less efficient. The AI has access to previous recorded time states and can choose to rewind back to that state if it's current state is unsatisfactory.

## Purpose
To determine if given an option to rewind the game state, an agent will be more efficient in a dynamic environment.
