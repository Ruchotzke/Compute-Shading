# Compute Shading Experiments
### Several experiments in compute shading with unity's shaderlab compute shaders.

## Technologies Used
- Unity 2020 / ShaderLab

## Experiment - Slime Molds
![slime mold simulation](https://github.com/Ruchotzke/Compute-Shading/blob/main/docs/Slime%20Mold.gif)
The slime mold simulation is a very large up scaling of the ant pheromone pathfinding algorithm. Each point is a single agent, and each agent is responsible for moving forward, and turning to follow the paths of it's fellow agents. Over time, the pheromone trails the agents leave behind dissipates, leading to new paths being found.

## Experiment - Voronoi Noise
![basic voronoi](https://github.com/Ruchotzke/Compute-Shading/blob/main/docs/Voronoi.gif)
Voronoi noise is a common noise texture in computer graphics. The texture is broken into regions, where each region is colored according to the closest point. It's a straightforward texture, and a perfect noise to be parallelized on the GPU.

![voronoi with distance](https://github.com/Ruchotzke/Compute-Shading/blob/main/docs/voronoi_molecules.gif)
Voronoi noise can also be extended, in my case I brightly color the closest points to the region center. When the distance modifier is strengthened, it makes the impression of molecules.
The distance modifier can also be changed to use the more generic Minkowski distance, where both euclidean distance and Manhattan distance can be modulated between.
