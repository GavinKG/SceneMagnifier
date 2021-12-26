# Off-axis projection demo: Scene Magnifier

![banner](banner.png)

A tiny demo showcasing how a custom off-axis projection matrix can be used to build a "scene magnifier". You can scroll to zoom, LMB drag to pan the view, just like an image viewer, but with loseless image and infinite zoom level (like a vector graphics). Different from adjusting camera rotation and FOV angle, using a off-axis projection matrix to zoom in will not change the "perspective relationship" of the original shot (like on the top-left image).

This method can be extented to build a display array (like on a stage), with each screen being rendered by different computer using the same scene, but with different projection matrix (this custom projection matrix can also be used to cull objects!), to average the computational load.

This method can also be used to make a super-resolution screenshot tool, by dividing the screen into several smaller grids and use the custom projection matrices to capture them individually (Nvidia Ansel already did that!). This approach avoids hitting GPU texture size limit, which is 16384x16384 on dx11 and dx12, when allocating huge render targets.

