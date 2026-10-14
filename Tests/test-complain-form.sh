#!/bin/bash
set -euo pipefail
cd "$(dirname "$0")/.."
json_dll=$(find Library/PackageCache/com.unity.nuget.newtonsoft-json@* -path '*/Runtime/Newtonsoft.Json.dll' -print -quit)
if [[ -z "$json_dll" ]]; then
  echo 'Open this project in Unity first to restore the Newtonsoft.Json package.' >&2
  exit 1
fi
test_dir=$(mktemp -d)
trap 'rm -rf "$test_dir"' EXIT
cp "$json_dll" "$test_dir/Newtonsoft.Json.dll"
mcs -langversion:latest -lib:"$(pkg-config --variable=libdir mono)/mono/4.5/Facades" \
  -r:netstandard -r:System.Numerics -r:"$test_dir/Newtonsoft.Json.dll" \
  -out:"$test_dir/ComplainFormDataTests.exe" \
  Assets/Scripts/GameData/ComplainFormData.cs Tests/ComplainFormDataTests.cs
mono "$test_dir/ComplainFormDataTests.exe"
