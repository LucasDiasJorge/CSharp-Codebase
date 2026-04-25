@echo off
echo Creating GraphTraversalDemo directory and organizing files...

mkdir "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\GraphTraversalDemo" 2>nul

move "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\GRAPHTRAVERSAL_GraphTraversalDemo.csproj" "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\GraphTraversalDemo\GraphTraversalDemo.csproj"
move "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\GRAPHTRAVERSAL_Graph.cs" "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\GraphTraversalDemo\Graph.cs"
move "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\GRAPHTRAVERSAL_Program.cs" "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\GraphTraversalDemo\Program.cs"
move "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\GRAPHTRAVERSAL_README.md" "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\GraphTraversalDemo\README.md"

echo.
echo GraphTraversalDemo project organized successfully!
echo.
echo To run the project, execute:
echo cd GraphTraversalDemo
echo dotnet run
echo.
pause
