---
name: db-migrate
description: EF Core migration oluşturur ve çalıştırır. Schema değişikliği, rollback planı ve veri bütünlüğü kontrolü dahil.
disable-model-invocation: true
allowed-tools: Bash, Read, Write, Edit, Grep, Glob
---

# Database Migration Workflow (EF Core)

$ARGUMENTS için veritabanı migration oluştur.

## Adımlar

1. **Mevcut schema'yı incele**
   - `Migrations/` dizinindeki mevcut migration'ları oku
   - `Data/ApplicationDbContext.cs` DbSet'lerini ve OnModelCreating konfigürasyonunu incele
   - İlgili Model entity'lerini bul (`Models/`)
   - Mevcut ilişkileri (foreign key, cascade/restrict) anla

2. **Model değişikliğini uygula**
   - `Models/` altındaki entity sınıfını güncelle veya yeni entity oluştur
   - Navigation property'leri tanımla
   - Gerekli enum'ları `Models/Enums/` altında oluştur
   - `ApplicationDbContext`'e yeni DbSet ekle (gerekiyorsa)
   - `OnModelCreating`'de ilişki ve constraint konfigürasyonu yap

3. **Migration oluştur**
   - `dotnet ef migrations add $ARGUMENTS` komutu ile migration oluştur
   - Oluşan migration dosyasını incele (Up ve Down method'ları)
   - Index'leri düşün (sık sorgulanan alanlar)
   - NOT NULL constraint'leri için default değer belirle
   - Soft delete pattern'ına uy (DeletedAt, CreatedAt, UpdatedAt)

4. **Veri bütünlüğü**
   - Mevcut veriyi kontrol et
   - Büyük tablolarda migration etkisini değerlendir

5. **Migration'ı uygula**
   - `dotnet ef database update` ile migration'ı çalıştır
   - Schema'nın doğru oluştuğunu doğrula
   - `dotnet build` ile projenin derlenebildiğini kontrol et

## ÖNEMLİ

- Production'da çalışan migration'ları asla değiştirme
- Her zaman yeni migration dosyası oluştur
- Destructive migration'lar (tablo/kolon silme) için dikkatli ol
- Soft delete pattern'ına uy: `DeletedAt` nullable DateTime alanı
- Timestamp'ler: `CreatedAt` ve `UpdatedAt` alanlarını ekle
