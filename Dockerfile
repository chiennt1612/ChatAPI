FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore
 
COPY . ./
RUN dotnet publish -c Release -o out
 
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
# FROM  mcr.microsoft.com/dotnet/core
ENV TZ=Asia/Ho_Chi_Minh
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

WORKDIR /app
COPY --from=build-env /app/out ./

RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
   		libgdiplus \
        libc6-dev \
        libgdiplus \
        libx11-dev \
     && rm -rf /var/lib/apt/lists/*


EXPOSE 4021
ENTRYPOINT ["dotnet", "ChatAPI.dll"]
