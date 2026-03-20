#!/bin/bash
# PreToolUse hook: Tehlikeli komutları engeller
# Kullanım: settings.json > hooks > PreToolUse > matcher: "Bash"

INPUT=$(cat)
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

if [ -z "$COMMAND" ]; then
  exit 0
fi

# Tehlikeli komutları engelle
if echo "$COMMAND" | grep -iE '\b(rm -rf /|DROP DATABASE|DROP TABLE|TRUNCATE|DELETE FROM .* WHERE 1|git push --force origin main|git reset --hard HEAD~)\b' > /dev/null; then
  echo "ENGELLENDI: Tehlikeli komut tespit edildi: $COMMAND" >&2
  exit 2
fi

# appsettings.json ve secret dosyalarını cat etmeyi engelle (secret sızıntısı)
if echo "$COMMAND" | grep -E 'cat.*appsettings\.(Production|Staging)\.json|cat.*\.env|echo.*\$[A-Z_]*KEY|echo.*\$[A-Z_]*SECRET|echo.*ConnectionString' > /dev/null; then
  echo "ENGELLENDI: Secret dosya/değişken okuma girişimi" >&2
  exit 2
fi

# EF Core tehlikeli komutları
if echo "$COMMAND" | grep -iE 'dotnet ef database drop' > /dev/null; then
  echo "ENGELLENDI: Veritabanı silme girişimi" >&2
  exit 2
fi

exit 0
