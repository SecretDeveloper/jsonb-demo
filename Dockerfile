# ----- Stage 1: Build the .NET console app -----
  FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
  WORKDIR /app

  # Copy project files
  COPY jsonbdemo.csproj .
  RUN dotnet restore

  # Copy the rest of the source code
  COPY . .
  RUN dotnet publish -c Release -o out

  # ----- Stage 2: Create the runtime image -----
  FROM mcr.microsoft.com/dotnet/runtime:7.0
  WORKDIR /app

  # Copy the compiled output from the build stage
  COPY --from=build /app/out .
  ENTRYPOINT ["dotnet", "XmlToJsonConsoleApp.dll"]
