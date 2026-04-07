#!/bin/sh
set -e
PORT="${PORT:-8080}"
export ASPNETCORE_URLS="http://0.0.0.0:${PORT}"
exec dotnet BankProductsRegistry.Api.dll
