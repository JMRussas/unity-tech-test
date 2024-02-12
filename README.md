#Justin Russas Comments:

1. Adding, Clearing, Creating a maze, etc:
  There is an editor window loaded from Window->Pathfinding->GridEditor
  Clicking on Nodes:  Clicking on the nodes will toggle the walkable status
  Generate Maze: Will fill the plane with a maze
  Clear Plane:  Will remove all obstacles
  Fill Plane:  Will completely fill the plane with obstacles

Color Coding:
Walkable areas: Green
Non-Walkable areas: Red

3. There are no obstacles by default on the playing surface.

4. Widgets can be toggle on and off to look at the walkable areas and non walkable areas beyond the obstacles placed on the map.

# Unity Pathfinding Take-Home Project

Welcome to the Pocket Worlds Unity Pathfinding Take-Home Project! In this repository, we'd like you to demonstrate your engineering skills by creating a small Unity project that implements pathfinding. While Unity provides a built-in NavMesh system for pathfinding, this test will challenge you to create your own custom pathfinding solution for a 2D grid.

This project will serve as the primary jumping off point for our technical interviews.

## Project Description
We've already put together a project that has the foundations of what you need to get started. Please use it as a starting point and build your solution on top of it.

Your task is to build a Unity project that meets the following requirements:

1. **Pathfinding Implementation**: Develop a custom pathfinding system without using Unity's NavMesh system. You are free to choose any pathfinding algorithm you prefer, such as A* or Dijkstra's algorithm.

3. **2D Grid**: Implement the pathfinding on a 2D grid where each cell represents either an impassable obstacle or a clear area. You have the flexibility to determine how this grid data is populated. Options include creating an editor tool to author it, automatically generating it from scene geometry, procedural generation, or any other method of your choice.

4. **Path Smoothing**: Pathfinding on a 2D grid can result in stair-stepped paths that look unnatural when followed directly. Your character should use a method to smooth out the path following to appear more natural, similar to how a human would move.

## Getting Started
To begin the project, follow these steps:

1. Clone this repository to your local machine:

   ```shell
   git clone https://github.com/pocketzworld/unity-tech-test.git

2. Create a Unity project or use an existing one.

3. Build the custom pathfinding system within your Unity project according to the project requirements outlined above.

4. Implement player control, allowing the character to follow the calculated path when the player clicks on the screen.

5. Ensure the path follows a natural-looking trajectory by implementing path smoothing.

6. Test your project thoroughly to ensure it meets the specified requirements.

7. Document your code, providing clear comments and explanations of your implementation choices.

8. Beyond this, feel free to add whatever other bells and whistles to the project you'd like. This is your opportunity to show off what you can do.

## Submission Guidelines
When you have completed the project, please follow these guidelines for submission:

1. Commit and push your code to your GitHub repository.

2. Update this README with any additional instructions, notes, or explanations regarding your implementation, if necessary.

3. Provide clear instructions on how to run and test your project.

4. Share the repository URL with the hiring team or interviewer.

## Additional Information

Feel free to be creative in how you approach this project. Your solution will be evaluated based on code quality, efficiency, and how well it meets the specified requirements.

Good luck, and we look forward to seeing your Unity pathfinding project! If you have any questions or need clarifications, please reach out to us.
