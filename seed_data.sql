-- Spor Salonları
INSERT INTO [dbo].[SporSalonlari] ([Ad], [Adres], [Aciklama], [CalismaZamanlari])
VALUES 
    ('Sakarya Fitness Center', 'Sakarya, Turkiye', 'Modern fitness merkezi', '06:00-22:00'),
    ('Gold Gym Sakarya', 'Adapazarı, Sakarya', 'Profesyonel antrenörlü spor salonu', '07:00-23:00'),
    ('Power Gym Serdivan', 'Serdivan, Sakarya', 'Ağırlık ve kardiyak egzersizleri', '08:00-20:00');

-- Hizmetler
INSERT INTO [dbo].[Hizmetler] ([Ad], [Aciklama], [Ucret], [SureDakika], [SporSalonuId])
VALUES
    ('Personal Training', 'Bireysel antrenörlük hizmeti', 500, 60, 1),
    ('Yoga', 'Yoga ve esneklik eğitimi', 300, 60, 1),
    ('Zumba', 'Müzikli hareket egzersizi', 250, 45, 1),
    ('Yüzme Dersleri', 'Yüzme öğretimi', 400, 60, 2),
    ('Fitness Danışmanlığı', 'Program ve diyet planı', 600, 90, 2),
    ('Crossfit', 'Yoğun kardiyak antrenmanı', 550, 60, 3);

-- Antrenörler
INSERT INTO [dbo].[Antrenorler] ([Ad], [Soyad], [Email], [Telefon], [UzmanlikAlanlari], [MevcutBaslangicSaati], [MevcutBitisSaati], [SporSalonuId])
VALUES
    ('Ahmet', 'Yılmaz', 'ahmet@example.com', '05321234567', 'Fitness, Kuvvet Antrenmanı', '09:00', '17:00', 1),
    ('Fatma', 'Kaya', 'fatma@example.com', '05329876543', 'Yoga, Pilates', '10:00', '18:00', 1),
    ('Mehmet', 'Demir', 'mehmet@example.com', '05335555555', 'Yüzme, Su Spor', '08:00', '16:00', 2),
    ('Ayşe', 'Çetin', 'ayse@example.com', '05346666666', 'Zumba, Dans', '15:00', '21:00', 2),
    ('Serkan', 'Arslan', 'serkan@example.com', '05357777777', 'Crossfit, Kardiyak', '06:00', '14:00', 3);

-- Test Üyeleri
INSERT INTO [dbo].[Uyeler] ([Ad], [Soyad], [Email], [Telefon], [UyelikTarihi])
VALUES
    ('Ali', 'Öztürk', 'ali.ozturk@example.com', '05321111111', GETDATE()),
    ('Zeynep', 'Şahin', 'zeynep.sahin@example.com', '05322222222', GETDATE()),
    ('İbrahim', 'Vural', 'ibrahim.vural@example.com', '05323333333', GETDATE()),
    ('Sena', 'Aktaş', 'sena.aktas@example.com', '05324444444', GETDATE()),
    ('Davut', 'Koç', 'davut.koc@example.com', '05325555555', GETDATE());

-- Admin Kullanıcı (AspNetUsers)
INSERT INTO [dbo].[AspNetUsers] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnabled], [AccessFailedCount])
VALUES
    (NEWID(), 'ogrencinumarasi@sakarya.edu.tr', 'OGRENCINUMARASI@SAKARYA.EDU.TR', 'ogrencinumarasi@sakarya.edu.tr', 'OGRENCINUMARASI@SAKARYA.EDU.TR', 1, 
    'AQAAAAIAAYagAAAAEL8w8h9z1q8t7x8z9z9z9z9z9z9z9z9z9z9z9z9z9z9z9z9z9z9z9z9z9z==',
    'SECURITY_STAMP_123', NEWID(), 0, 0, 1, 0);

-- Admin Rolü
INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
VALUES
    (NEWID(), 'Admin', 'ADMIN', NEWID()),
    (NEWID(), 'Uye', 'UYE', NEWID());
