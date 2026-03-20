---
name: fix-issue
description: GitHub issue'yu analiz eder, kök nedeni bulur, düzeltir ve PR oluşturur.
disable-model-invocation: true
allowed-tools: Bash, Read, Write, Edit, Grep, Glob, Agent
---

# Fix Issue Workflow

GitHub issue $ARGUMENTS düzelt.

## Adımlar

1. **Issue detaylarını oku**
   - `gh issue view $ARGUMENTS` ile issue bilgilerini al
   - Beklenen davranış ve mevcut davranışı anla
   - İlgili label'ları ve assignee'leri kontrol et

2. **Analiz et**
   - İlgili Controller, Service, View, Model dosyalarını bul
   - Kök nedeni tespit et
   - Etki alanını değerlendir (hangi katmanlar etkilenecek?)

3. **Düzeltmeyi uygula**
   - Feature branch oluştur: `git checkout -b fix/issue-$ARGUMENTS`
   - Minimal ve odaklı değişiklikler yap
   - Mevcut kod stiline uy (C#: PascalCase, interface prefix I)

4. **Doğrula**
   - Projeyi build et: `dotnet build`
   - Varsa testleri çalıştır: `dotnet test`
   - Uygulamayı çalıştırıp manuel kontrol et

5. **Commit ve PR**
   - Conventional Commits formatında commit: `fix: kısa açıklama (#issue-no)`
   - Branch'i push et
   - `gh pr create` ile PR oluştur
   - PR açıklamasında issue'yu referans ver: `Fixes #$ARGUMENTS`
