FROM registry.yx.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY ./*.csproj ./NuGet.Config ./*.props ./*.sln ./
# RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done
RUN dotnet restore -r linux-x64
COPY ./ ./
RUN dotnet publish --no-restore -c Release -r linux-x64 --no-self-contained -o out EsSyncService.csproj

FROM registry.yx.com/dotnet/aspnet-ttf:7.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 5600
ENV DOTNET_PRINT_TELEMETRY_MESSAGE=false
ENV ASPNETCORE_URLS="http://*:5600"
ENTRYPOINT ["dotnet", "EsSyncService.dll"]