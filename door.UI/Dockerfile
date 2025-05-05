# -----------------------
# ベースランタイム（ラズパイ用 ARM64）
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 7275

# -----------------------
# SDK（ビルド用）※ ARM64
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# プロジェクトファイルを個別にコピー（順序重要）
COPY ["door.UI/door.UI.csproj", "door.UI/"]
COPY ["door.Domain/door.Domain.csproj", "door.Domain/"]
COPY ["door.Infrastructure/door.Infrastructure.csproj", "door.Infrastructure/"]

# パッケージ復元
RUN dotnet restore "door.UI/door.UI.csproj"

# ソース全体コピー
COPY . .

# パブリッシュ（静的ファイル含む成果物を生成）
WORKDIR "/src/door.UI"
RUN dotnet publish "door.UI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -----------------------
# 実行用ステージ
FROM base AS final
WORKDIR /app

# アプリケーションが 0.0.0.0:7275 でリッスンするように指定
ENV ASPNETCORE_URLS=http://0.0.0.0:7275

# パブリッシュ成果物をコピー（← wwwroot, Razor, DLL含む）
COPY --from=build /app/publish .

# 起動
ENTRYPOINT ["dotnet", "door.UI.dll"]
