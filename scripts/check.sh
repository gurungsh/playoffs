set -euo pipefail
cd "$(dirname "$0")/.."

dotnet restore
dotnet build --no-restore
dotnet format --verify-no-changes --no-restore
dotnet test --no-build
echo "check: OK"