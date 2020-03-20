FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["ExpenseTracker.Web/ExpenseTracker.Web.csproj", "ExpenseTracker.Web/"]
COPY ["ExpenseTracker.Common/ExpenseTracker.Common.csproj", "ExpenseTracker.Common/"]
COPY ["ExpenseTracker.Data/ExpenseTracker.Data.csproj", "ExpenseTracker.Data/"]
COPY ["ExpenseTracker.Biz/ExpenseTracker.Biz.csproj", "ExpenseTracker.Biz/"]
RUN dotnet restore "ExpenseTracker.Web/ExpenseTracker.Web.csproj"
COPY . .
WORKDIR "/src/ExpenseTracker.Web"
RUN dotnet build "ExpenseTracker.Web.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ExpenseTracker.Web.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENV ASPNETCORE_ENVIRONMENT Production
CMD ASPNETCORE_URLS=http://*:$PORT dotnet ExpenseTracker.Web.dll