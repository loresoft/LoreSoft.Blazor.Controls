& dotnet publish LoreSoft.Blazor.Controls.sln --configuration Release
Copy-Item -Path ".\samples\Sample.ClientSide\bin\Release\netstandard2.0\publish\Sample.ClientSide\dist\*" -Destination ".\docs\" -recurse -Force
Copy-Item -Path ".\samples\Sample.ClientSide\bin\Release\netstandard2.0\publish\wwwroot\_content\*" -Destination ".\docs\_content" -recurse -Force