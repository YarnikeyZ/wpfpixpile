# wpfpixpile
"C# .NET 8.0 library" for 2D and 3D graphics in WPF thru the WritableBitmap.

![image](https://github.com/user-attachments/assets/abf850a1-1d30-49e5-ab1e-6c17d5e81ae2)
![image](https://github.com/user-attachments/assets/424e391d-fe10-4405-9bc9-86d9fa44110d)
![image](https://github.com/user-attachments/assets/9661705b-6417-40b7-bd0a-6de497ab8973)

# Implements
* **Matrix4x4** -  class that represents 4x4 matrix, it has many premade matrix to multiply by like: _GetTranslation_, _GetScale_, _GetRotationX_, _GetRotationY_, _GetRotationZ_, _GetLookAt_, _getPerspectiveProj_, also has _ToString_ if you want to print it;

* **Vector3D** -  class that represents 3D vector (x, y, z, w), it has: Len to get the length, Norm to normalize, DistanceTo to get distance to a 3D point, ScalarProduct to get scalar product, has ToString too;

* **PColors** -  class that contains instances of 147 default colors like Red, White, Turquoise, SlateGrey, SlateGray, DodgerBlue and others from css;

* **DrawPolygonFill** -  function that can fill any convex polygon in 2D or 3D space;

* **TakeScreenshot** -  to take a screenshot of the WritableBitmap;

* **DrawEllipse** -  draws an ellipse, can be filled or not;

* **DrawLine** -  draws a line in 2D or 3D space;

* **DrawRectangle** -  draws a rectangle in 2D or 3D space;

* **DrawPixel** -  draws a pixel in 2D or 3D space;

* **MakeColorGradient** -  generates a rainbow gradient;

* **RenderLoop** -  a looping function that is supposed to be used for running your RenderFunc (function that renders one frame on WritableBitmap);

* **UpdateCalculations** -  a looping function that is supposed to be used for running your CalculationFunc (function that calculates one frame);

* **Setup** -  is a function that gets everything ready for starting to calculate and render;

# Included Examples
* **Pong** - literally just pong for two players (image 1);

* **Shark3D** - 3D model inspecting demo, can load objects from .obj files (image 2);

* **CsSysStat** - shows the CPU time with default WPF progress bar and a graph on a bottom (image 3);
