set -euo pipefail
cd "$(dirname "$0")/.."

dotnet restore
buf lint src/Playoffs.Contracts
if git cat-file -e HEAD:src/Playoffs.Contracts/buf.yaml 2>/dev/null; then
  buf breaking src/Playoffs.Contracts --against ".git#ref=HEAD,subdir=src/Playoffs.Contracts"
fi
dotnet build --no-restore
dotnet format --verify-no-changes --no-restore
dotnet test --no-build
echo "check: OK"