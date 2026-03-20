---
name: review-pr
description: Pull request'i kapsamlı olarak inceler. Kod kalitesi, güvenlik, performans ve test coverage kontrolü yapar.
disable-model-invocation: true
allowed-tools: Bash, Read, Grep, Glob, Agent
---

# Review PR Workflow

PR $ARGUMENTS incele.

## Adımlar

1. **PR bilgilerini al**
   - `gh pr view $ARGUMENTS` ile PR detaylarını oku
   - `gh pr diff $ARGUMENTS` ile değişiklikleri gör
   - PR açıklamasını ve ilgili issue'ları kontrol et

2. **Paralel inceleme için 3 subagent oluştur**

   **Agent 1 - Kod Kalitesi:**
   - C# naming conventions (PascalCase, _camelCase)
   - Kod tekrarı (DRY ihlali)
   - Gereksiz karmaşıklık
   - Error handling eksiklikleri
   - async/await doğru kullanımı
   - LINQ sorgu optimizasyonu

   **Agent 2 - Güvenlik:**
   - Input validasyonu (Data Annotations)
   - SQL injection riski (raw SQL kullanımı)
   - XSS açıkları (Html.Raw kullanımı)
   - Hardcoded secret'lar
   - [Authorize] attribute kontrolleri
   - Anti-forgery token kullanımı

   **Agent 3 - Performans & Mimari:**
   - N+1 sorgu problemi (EF Core Include)
   - Gereksiz veritabanı sorgusu
   - Soft delete filtresi uygulanmış mı
   - Service katmanı kullanımı (controller'da business logic yok mu)
   - ViewModel kullanımı

3. **Bulguları birleştir**
   - Kritik (engelleme): düzeltilmeli
   - Uyarı: düzeltilmesi önerilir
   - Öneri: iyileştirme fırsatı

4. **Özet rapor oluştur**
   - Genel değerlendirme (onayla / değişiklik iste / reddet)
   - Her bulgu için dosya:satır referansı
   - Düzeltme önerileri
