#!/usr/bin/env bash
# Compliance Guardian Portal - Local Dev Startup Script
# Usage: bash portal/start.sh

set -e

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
BACKEND_DIR="$ROOT_DIR/backend/src/Portal.Api"
FRONTEND_DIR="$ROOT_DIR/frontend"

echo "========================================="
echo " Compliance Guardian Portal - Local Dev"
echo "========================================="

# Check prerequisites
command -v dotnet &>/dev/null || { echo "ERROR: .NET 8 SDK not found. Install from https://dotnet.microsoft.com/download"; exit 1; }
command -v node &>/dev/null || { echo "ERROR: Node.js not found. Install from https://nodejs.org"; exit 1; }

# Install frontend deps if needed
if [ ! -d "$FRONTEND_DIR/node_modules" ]; then
  echo "[Frontend] Installing dependencies..."
  (cd "$FRONTEND_DIR" && npm install)
fi

# Restore backend
echo "[Backend] Restoring NuGet packages..."
(cd "$ROOT_DIR/backend/src" && dotnet restore Portal.sln)

# Launch backend in background
echo "[Backend] Starting Portal.Api on http://localhost:5000 ..."
(cd "$BACKEND_DIR" && dotnet run --urls http://localhost:5000) &
BACKEND_PID=$!

# Launch frontend
echo "[Frontend] Starting Vite dev server on http://localhost:3000 ..."
(cd "$FRONTEND_DIR" && npm run dev) &
FRONTEND_PID=$!

echo ""
echo "Portal is running:"
echo "  Frontend : http://localhost:3000"
echo "  Backend  : http://localhost:5000"
echo "  Swagger  : http://localhost:5000/swagger"
echo ""
echo "Press Ctrl+C to stop all services."

# Wait and clean up on exit
trap "kill $BACKEND_PID $FRONTEND_PID 2>/dev/null; echo 'Stopped.'" EXIT
wait
