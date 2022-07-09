# RainWorldShadow-URP
## What is this?
Rain World's 2D screen-space shadow effect implemented in Unity Universal Rendering Pipeline. This implementation is based on the [tweets](https://twitter.com/joar_lj/status/1525445116789497856?t=E0TvPUclpmWs7LO9pV-xhQ&amp;s=19) made by Rain World's developer Joar Jakobsson.  

**Result**  

https://user-images.githubusercontent.com/8101387/178085659-091bb77c-6cf8-49b1-b13b-010d1721d0bf.mp4



## Technique overview
As mentioned in the previous section, most of the implementation details are the same as the one described in the tweets. For the original implementation, Joar has already explained it perfectly in his original tweets. Therefore I recommend that you read the tweets directly :P

However here is my understandings anyways: 
* First of all, you need to prepare a "depth texture" for the background. It will be used to calculate the "sample point" in the 2nd step later.  
1. Render foreground (dynamic) objects to the frame buffer. Foreground objects are the ones that could cast shadows.
2. Render background objects' color to the frame buffer. But instead of just render the color/texture directly, the background shader gets how deep the background pixel is in the background using the depth texture mentioned earlier. Using the depth as the offset, we calculate a sample point to sample the frame buffer pixel. If the sampled pixel is not transparent, it means there is something in the foreground that should cast shadow on the background pixel. If that's the case, we multiple the background color with 0.5f so it looks darker.

And here is my implementation: 
1. Render foreground objects to the frame buffer. Foreground objects are the ones that could cast shadows. In my implementation, instead of rendering the colors directly to the frame buffer, these objects are rendered to an external render texture.
2. Render background objects' depth to a depth texture.
3. Render background objects' color to the frame buffer like how it's done in the original implementation.

The benefit of my implementation are that the background obejcts could also be dynamic (As shown in my result video, the rotating background fan). The downside is that there is an extra pass in the process.

Other than the differences between the rendering passes, I've also implemented semi-transparent shadows with this system. 
![image](https://user-images.githubusercontent.com/8101387/178085218-8cb4ebdd-58bf-4b38-943a-db8658a68505.png)  

## Known Limitations
1. Since this is a screen-space effects, Off-screen foreground objects cannot cast shadows. Due to how the levels are designed in Rain World (static camera + one-screen-sized level), this was not a problem in the game. The possible solution is to render the foreground color buffer in a slightly bigger size.  
![image](https://user-images.githubusercontent.com/8101387/178085134-3ec4a2d3-6d6f-43b8-bcfb-bf18737c84e4.png)  
2. Depth texture is generated with the geometry of the object, the alpha value is not taken into consideration. This could possibly be solved by adding an extra pass that renders the alpha value buffer and uses that to cutoff depth texture.  
![image](https://user-images.githubusercontent.com/8101387/178085157-7ff03583-fca7-43a6-a8af-7c30e2de7405.png)
3. No anti-aliasing on the shadows, which is actually perfect if it's a pixel art game.  

https://user-images.githubusercontent.com/8101387/178085706-a628d059-c49c-4ebf-8066-743d13d87c0c.mp4



## Relevant Assets
All the assets/code that are relevant to the effect are located in these folders:
* Assets/Settings/URP-Peformant.asset & Assets/Settings/URP-Peformant-Renderer.asset  
* Assets/GameData/Rendering
* Assets/Scripts/Rendering
* Assets/Shaders
* Assets/Materials

## Bonus  
The assets are taken from one of my school projects at CGL. I wrote the procedural tail animation that is used on the ghost character.

## Credits
* Joar Jakobsson:  
  For sharing the implementation details in RW.
* Maria Eom:  
  For making the ghost character sprites for the school project.

