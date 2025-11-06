# Frog Spawner System

Sistem spawner untuk kodok yang otomatis menghilang setelah 30-50 detik dan meningkatkan jumlah spawn 30% di malam hari.

## Komponen

### 1. FrogAI (Modified)
Script AI untuk kodok dengan sistem lifetime baru.

**Fitur Baru:**
- **Lifetime System**: Kodok akan otomatis menghilang setelah durasi tertentu (30-50 detik)
- **Spawn Method**: Method `Spawn()` untuk dipanggil oleh spawner
- **Despawn Callback**: Notifikasi ke spawner saat kodok menghilang

**Settings:**
- `minLifetime`: Waktu hidup minimum (default: 30 detik)
- `maxLifetime`: Waktu hidup maksimum (default: 50 detik)

### 2. FrogSpawner (New)
Script spawner dengan object pooling dan sistem nighttime bonus.

**Fitur:**
- **Object Pooling**: Efisien dengan reuse object kodok
- **Spawn Points**: Spawn di titik-titik yang ditentukan
- **Nighttime Bonus**: Otomatis meningkatkan spawn 30% di malam hari (18:00 - 06:00)
- **Dynamic Max Concurrent**: Jumlah maksimum kodok berubah sesuai waktu

**Settings:**
- `frogPrefab`: Prefab kodok (harus punya FrogAI component)
- `spawnPoints`: List transform untuk spawn points
- `baseMaxConcurrent`: Jumlah max concurrent di siang hari (default: 5)
- `spawnIntervalRange`: Interval spawn (default: 8-15 detik)
- `lifeTimeRange`: Durasi hidup kodok (default: 30-50 detik)
- `spawnRadiusNearPoint`: Radius random spawn di sekitar spawn point
- `nightStartHour`: Jam mulai malam (default: 18 = 6 PM)
- `nightEndHour`: Jam selesai malam (default: 6 = 6 AM)
- `nighttimeSpawnBonus`: Persentase bonus spawn malam (default: 0.3 = 30%)

## Cara Setup

### 1. Setup Frog Prefab
1. Buat prefab kodok dengan component **FrogAI**
2. Pastikan sudah setup animator, sprite renderer, dan tilemap reference
3. Set lifetime settings di inspector (opsional, default sudah 30-50 detik)

### 2. Setup Frog Spawner
1. Buat GameObject baru di scene (nama: "Frog Spawner")
2. Tambahkan component **FrogSpawner**
3. Assign frog prefab ke field `Frog Prefab`
4. Buat spawn points:
   - Buat empty GameObject untuk setiap spawn point (contoh: "Frog Spawn Point 1", "Frog Spawn Point 2", dll)
   - Posisikan di lokasi yang diinginkan (biasanya di dekat air atau area lembab)
   - Tambahkan spawn points ke list `Spawn Points` di FrogSpawner

### 3. Setup TimeManager
Pastikan **TimeManager** sudah ada di scene untuk sistem nighttime detection.

## Cara Kerja

1. **Spawning**:
   - FrogSpawner akan spawn kodok secara berkala (setiap 8-15 detik)
   - Spawn di salah satu spawn point secara random
   - Kodok akan muncul dengan offset random di sekitar spawn point

2. **Lifetime**:
   - Setiap kodok punya lifetime 30-50 detik (random)
   - Setelah lifetime habis, kodok otomatis despawn
   - Kodok kembali ke object pool untuk reuse

3. **Nighttime Bonus**:
   - Sistem auto-detect waktu dari TimeManager
   - Jika jam 18:00 - 06:00 (malam): spawn rate +30%
   - Contoh: Siang = max 5 kodok, Malam = max 6-7 kodok

## Debug & Visualization

### FrogAI Gizmos (Editor Only)
- **Green Wire Cube**: Current tile position
- **Label**: Menampilkan state, tile position, next hop time, dan remaining lifetime

### FrogSpawner Gizmos (Editor Only)
- **Yellow/Cyan Wire Sphere**: Spawn points (kuning = siang, cyan = malam)
- **Spawn Point Label**: Menampilkan "Spawn Point" dan status DAY/NIGHT
- **Center Label**: Info spawner (alive count, max concurrent, nighttime status)

## Contoh Penggunaan

```csharp
// Jika ingin spawn kodok secara manual (testing):
FrogAI frog = GetFrogFromPool();
float lifetime = Random.Range(30f, 50f);
frog.gameObject.SetActive(true);
frog.Spawn(spawnPosition, lifetime, OnFrogDespawn);
```

## Tips & Catatan

1. **Spawn Points**:
   - Letakkan spawn points di area yang sesuai (dekat air, kolam, area lembab)
   - Gunakan multiple spawn points untuk area yang lebih luas
   - Jarak antar spawn points minimal 2-3 tiles

2. **Performance**:
   - Object pooling sudah diimplementasikan untuk performa optimal
   - Adjust `baseMaxConcurrent` sesuai kebutuhan (terlalu banyak = lag)

3. **Nighttime Settings**:
   - Adjust `nightStartHour` dan `nightEndHour` sesuai game design
   - Adjust `nighttimeSpawnBonus` untuk kontrol spawn rate (0.3 = 30%, 0.5 = 50%, dll)

4. **Lifetime**:
   - Lifetime range (30-50 detik) sudah sesuai request
   - Bisa diubah per-prefab jika ingin kodok dengan lifetime berbeda

## Integrasi dengan System Lain

- **TimeManager**: Untuk detection siang/malam
- **TileManager**: Untuk pathfinding kodok
- **PlantManager**: Kodok akan makan tanaman jika ada di tile yang sama

## Troubleshooting

**Q: Kodok tidak spawn?**
- Pastikan frog prefab sudah di-assign
- Pastikan ada spawn points di list
- Check console untuk error/warning

**Q: Nighttime bonus tidak bekerja?**
- Pastikan TimeManager ada di scene
- Check inspector FrogSpawner, field `Is Nighttime` harus berubah sesuai waktu
- Verify jam di TimeManager (gunakan debug field)

**Q: Kodok tidak menghilang?**
- Check FrogAI Inspector, field `Min Lifetime` dan `Max Lifetime` harus terisi
- Pastikan Spawn() method dipanggil dengan parameter lifetime yang benar
- Check console untuk error di coroutine

---
**Created**: 2025-11-06
**Author**: Claude Code Assistant
