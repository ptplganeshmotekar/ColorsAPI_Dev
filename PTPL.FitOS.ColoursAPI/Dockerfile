#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80


FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR ../src
COPY ["PTPL.FitOS.Common/PTPL.FitOS.Common/PTPL.FitOS.Common.csproj", "PTPL.FitOS.Common/PTPL.FitOS.Common/"]
COPY ["PTPL.FitOS.DataModels/PTPL.FitOS.DataModels.csproj", "PTPL.FitOS.DataModels/"]
COPY ["PTPL.FitOS.DataContext/PTPL.FitOS.DataContext.csproj", "PTPL.FitOS.DataContext/"]
COPY ["PTPL.FitOS.Services/PTPL.FitOS.Services.csproj", "PTPL.FitOS.Services/"]
COPY ["PTPL.FitOS.ColoursAPI/PTPL.FitOS.ColoursAPI.csproj", "PTPL.FitOS.ColoursAPI/"]
#COPY ["PTPL.FitOS.SwaggerUI/PTPL.FitOS.SwaggerUI.csproj", "PTPL.FitOS.SwaggerUI/PTPL.FitOS.SwaggerUI/"]


RUN dotnet restore "PTPL.FitOS.ColoursAPI/PTPL.FitOS.ColoursAPI.csproj"
COPY . .
WORKDIR "/src/PTPL.FitOS.ColoursAPI"
RUN dotnet build "PTPL.FitOS.ColoursAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PTPL.FitOS.ColoursAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PTPL.FitOS.ColoursAPI.dll"]