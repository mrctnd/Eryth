---
name: deploy
description: Uygulamayı production ortamına deploy eder. Build, CSS derleme ve deployment adımlarını sırayla çalıştırır.
disable-model-invocation: true
allowed-tools: Bash, Read, Grep, Glob
---

# Deploy Workflow

$ARGUMENTS ortamına deploy et (varsayılan: production).

## Adımlar

1. **Pre-deploy kontrolleri**
   - `git status` ile temiz working tree doğrula
   - Doğru branch'te olduğunu kontrol et (main veya release branch)
   - Son commit'in CI'dan geçtiğini doğrula

2. **Build ve doğrulama**
   - NuGet paketlerini geri yükle: `dotnet restore`
   - CSS derle: `npm run build-css-prod`
   - Projeyi build et: `dotnet build --configuration Release`
   - Build başarılı mı kontrol et

3. **Deploy et**
   - Publish: `dotnet publish --configuration Release --output ./publish`
   - Build artifact'larını doğrula
   - Deploy komutunu çalıştır
   - Health check ile deployment'ı doğrula

4. **Post-deploy**
   - Deployment bilgisini logla
   - Veritabanı migration'larının uygulandığını doğrula
   - Herhangi bir hata varsa rollback talimatlarını göster

## Hata Durumunda

- Build başarısız olursa deploy YAPMA
- Migration hatası varsa deploy YAPMA
- Hata mesajını açıkça raporla
