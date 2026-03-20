#!/bin/bash
# PostToolUse hook: Dosya düzenlemesi sonrası otomatik format kontrolü
# Kullanım: settings.json > hooks > PostToolUse > matcher: "Edit|Write"

INPUT=$(cat)
FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // empty')

if [ -z "$FILE_PATH" ]; then
  exit 0
fi

# C# dosyaları için dotnet format (eğer kuruluysa)
if echo "$FILE_PATH" | grep -E '\.cs$' > /dev/null; then
  dotnet format --include "$FILE_PATH" 2>/dev/null
fi

# JavaScript ve CSS dosyaları için Prettier (eğer kuruluysa)
if echo "$FILE_PATH" | grep -E '\.(js|css)$' > /dev/null; then
  npx prettier --write "$FILE_PATH" 2>/dev/null
fi

exit 0
