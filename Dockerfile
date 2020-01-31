FROM microsoft/dotnet:2.2-aspnetcore-runtime

ENV ASPNETCORE_ENVIRONMENT=Production

WORKDIR /server

COPY . ./

RUN dotnet publish -c Release -o publish

CMD ASPNETCORE_URLS=http://*:$PORT dotnet ExpenseTracker.Web.dll