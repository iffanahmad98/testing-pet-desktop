# 🎁 Gift System Documentation

Sistem gift yang dapat di-klik oleh player untuk mendapatkan random reward berdasarkan ukuran gift (Small, Medium, Large).

---

## 📋 Features

- ✅ 3 ukuran gift: **Small**, **Medium**, **Large**
- ✅ Sistem reward berbasis **probability** (fully customizable)
- ✅ Berbagai jenis reward: **Coin**, **Food Pack**, **Medicine**, **Golden Ticket**, **Decoration**, **Bonus 2x Coins**, **Empty (convert to coins)**
- ✅ Click detection menggunakan **OnMouseDown** dan **IPointerClickHandler**
- ✅ Visual feedback: **sprite change**, **particle effects**, **sound effects**, **bounce animation**
- ✅ Floating text notification
- ✅ Auto cleanup setelah dibuka
- ✅ ChickenAI integration: auto spawn gift dengan limit maksimal 4 gift

---

## 📊 Reward Probability Table

### Small Gift
| Reward Type       | Probability | Amount Range    |
|-------------------|-------------|-----------------|
| Coin              | 60%         | 50 – 150        |
| Food Pack         | 20%         | 1 (Feed×1)      |
| Medicine          | 6%          | 1               |
| Bonus 2x Coins    | 4%          | 50 – 150 (×2)   |
| Decoration (rare) | 1%          | 1               |
| Empty → Coins     | 8.5%        | 25 – 75         |
| Golden Ticket     | 0.5%        | 1               |
| **TOTAL**         | **100%**    |                 |

### Medium Gift
| Reward Type       | Probability | Amount Range    |
|-------------------|-------------|-----------------|
| Coin              | 55%         | 150 – 500       |
| Food Pack         | 22%         | 3 (Feed×3)      |
| Medicine          | 8%          | 1               |
| Bonus 2x Coins    | 6%          | 150 – 500 (×2)  |
| Empty → Coins     | 5%          | 75 – 250        |
| Decoration (rare) | 3%          | 1               |
| Golden Ticket     | 1%          | 1               |
| **TOTAL**         | **100%**    |                 |

### Large Gift
| Reward Type       | Probability | Amount Range    |
|-------------------|-------------|-----------------|
| Coin              | 45%         | 500 – 2000      |
| Food Pack         | 22%         | 5 (Feed×5)      |
| Medicine          | 12%         | 1               |
| Bonus 2x Coins    | 8%          | 500 – 2000 (×2) |
| Decoration (rare) | 6%          | 1               |
| Empty → Coins     | 5%          | 250 – 1000      |
| Golden Ticket     | 2%          | 1               |
| **TOTAL**         | **100%**    |                 |

---

## 🛠️ Setup Guide

### Step 1: Create Gift Prefabs

1. **Create 3 Empty GameObjects** untuk Small, Medium, Large gift
2. **Add Components**:
   - `SpriteRenderer` - untuk visual gift
   - `Collider2D` (Box/Circle) - untuk click detection
   - `GiftItem` script

3. **Configure GiftItem**:
   - Pilih `Gift Size` (Small/Medium/Large)
   - Set `Bounce Scale` = 1.2 (animasi scale saat dibuka)
   - Set `Bounce Duration` = 0.3
   - Set `Auto Destroy After Open` = true
   - Set `Destroy Delay` = 2 seconds

### Step 2: Create Reward Tables

1. **Right-click di Project** → Create → **Gift** → **Reward Table**
2. **Create 3 Reward Tables**:
   - `SmallGiftRewardTable`
   - `MediumGiftRewardTable`
   - `LargeGiftRewardTable`

3. **Quick Setup (Recommended)**:
   - Select reward table asset
   - Di Inspector, assign ItemData references:
     - Feed Item
     - Medicine Item
     - Golden Ticket Item
     - Decoration Item
   - Click button **"Setup Small Gift"** / **"Setup Medium Gift"** / **"Setup Large Gift"**
   - Reward list akan otomatis terisi sesuai probability table!

4. **Assign Reward Table** ke prefab:
   - Buka gift prefab
   - Di `GiftItem` component, assign corresponding reward table

### Step 3: Setup Items (ItemData)

Pastikan Anda sudah membuat ItemData ScriptableObjects untuk:
- **Feed** (Food Pack)
- **Medicine**
- **Golden Ticket**
- **Decoration** (rare item)

Cara membuat ItemData:
1. Right-click → Create → **Inventory** → **Item**
2. Configure item properties (itemId, displayName, icon, itemType, dll)
3. Save di folder `Resources/Items/`

### Step 4: Setup ChickenAI

1. **Select ChickenAI GameObject** di scene
2. **Configure Gift Drop Settings**:
   - Assign `Small Gift Prefab`
   - Assign `Medium Gift Prefab`
   - Assign `Large Gift Prefab`
   - Set `Small Gift Chance` = 70%
   - Set `Medium Gift Chance` = 25%
   - Set `Large Gift Chance` = 5%
   - Set `Drop Interval Minutes` = 5 (setiap 5 menit)
   - Set `Max Gift Count` = 4
   - Set `Gift Spawn Offset` = (0, 0, 0)

3. **Play & Test!**

---

## 🎮 Usage

### Player Interaction
- **Klik gift** di scene untuk membuka
- Gift akan:
  1. Play bounce animation
  2. Give reward ke player
  3. Show floating text notification
  4. Auto destroy setelah 2 detik

### Reward Distribution
Reward diberikan langsung ke player:
- **Coins** → `MagicalGarden.Farm.CoinManager.Instance.AddCoins(amount)`
- **Items** → `InventoryManager.Instance.AddItem(itemData, amount)`

**IMPORTANT:** Gift system menggunakan **Farm.CoinManager** (Instance-based), bukan static CoinManager!

---

## 📝 Manual Configuration (Advanced)

Jika tidak ingin menggunakan Quick Setup, Anda bisa manually configure reward list:

1. **Select Reward Table**
2. **Expand "Rewards" list**
3. **Add reward entries** dengan:
   - Reward Type
   - Drop Chance (%)
   - Min/Max Amount
   - Item Data (jika reward berupa item)

**IMPORTANT**: Total probability harus **= 100%**!

Gunakan button **"Validate Probabilities"** untuk check.

---

## 🔧 Customization

### Custom Reward Types
Edit `GiftRewardType.cs` untuk menambah reward type baru:
```csharp
public enum GiftRewardType
{
    Coin,
    FoodPack,
    Medicine,
    // ... tambahkan reward type baru di sini
    NewRewardType
}
```

Lalu update `GiftItem.GiveReward()` untuk handle reward type baru.

### Custom Probability Presets
Edit `GiftRewardPresets.cs` untuk mengubah default probability values.

### Custom Animation
Modify `GiftItem.BounceAnimation()` atau tambahkan animator untuk efek visual yang lebih menarik.

### Custom Notification UI
Replace `GiftItem.SpawnFloatingText()` dengan UI notification system yang lebih fancy (misal: DOTween, TextMeshPro dengan fade-out, dll).

---

## 🐛 Troubleshooting

### Gift tidak bisa diklik
- Pastikan ada `Collider2D` di GameObject
- Check Layer collision matrix (Layer vs Layer)
- Pastikan EventSystem ada di scene (untuk UI interaction)

### **Coin didapatkan tapi UI tidak berubah** ⚠️
**PENYEBAB:** `Farm.CoinManager.Instance` NULL atau tidak ada di scene!

**Solusi:**
1. **Check apakah CoinManager ada di scene FarmGame:**
   - Hierarchy → cari GameObject dengan component `CoinManager` (namespace: MagicalGarden.Farm)
   - Pastikan GameObject tersebut **ACTIVE**

2. **Add GiftSystemDebugger untuk debug:**
   - Create GameObject baru
   - Add component `GiftSystemDebugger`
   - Check "Log On Start"
   - Play scene

3. **Check Console Log:**
   ```
   ========== GIFT SYSTEM DEBUG ==========
   ✓ Farm.CoinManager.Instance: OK
     Current Coins: 5000
   ```

   Jika muncul error:
   ```
   ✗ Farm.CoinManager.Instance: NULL!
     Solution: Pastikan ada GameObject dengan CoinManager component di scene FarmGame
   ```

4. **Buka Gift:**
   Harusnya muncul:
   ```
   [Gift] Coin Reward: +100 coins | Before: 5000 → After: 5100  ✓ OK
   ```

5. **Cek UI Coin** - Seharusnya berubah! ✓

**Test Manual:**
- Select `GiftSystemDebugger` GameObject
- Right-click component → **"Test Add 100 Coins"**
- Check console & UI coin display

### Reward tidak diberikan
- Check `Farm.CoinManager.Instance` ada di scene FarmGame (bukan static CoinManager!)
- Check `InventoryManager.Instance` ada di scene
- Check ItemData sudah di-assign dengan benar di reward table
- Check console log untuk error messages (sekarang lebih detail!)

### Probability tidak 100%
- Gunakan button "Validate Probabilities" di Inspector
- Adjust drop chance values hingga total = 100%

### ChickenAI tidak spawn gift
- Check gift prefab sudah di-assign
- Check `Drop Interval Minutes` > 0
- Check scene sudah playing dan ChickenAI active

---

## 📂 File Structure

```
Assets/Game/Scripts/
├── Runtime/Farm/Gift/
│   ├── GiftSize.cs                 # Enum ukuran gift
│   ├── GiftRewardType.cs           # Enum jenis reward
│   ├── GiftRewardData.cs           # Data class untuk reward config
│   ├── GiftRewardTable.cs          # ScriptableObject reward table
│   ├── GiftRewardPresets.cs        # Helper preset configurations
│   ├── GiftItem.cs                 # Main gift item script
│   └── README.md                   # This file
└── Editor/
    └── GiftRewardTableEditor.cs    # Custom editor untuk reward table
```

---

## 🎯 Example Workflow

1. Player sedang bermain di FarmGame scene
2. ChickenAI berjalan-jalan dan idle
3. **Setiap 5 menit**, ChickenAI spawn 1 gift (random size berdasarkan probability)
4. Gift muncul di posisi ChickenAI
5. Player **klik gift**
6. Gift terbuka dengan animasi
7. Player mendapat random reward:
   - Misal: **Medium Gift** → dapat **3× Feed** (22% chance)
   - Atau: **Large Gift** → dapat **1500 Coins** (45% chance)
8. Floating text muncul: **"+3x Feed"** atau **"+1500 Coins"**
9. Gift otomatis hilang setelah 2 detik
10. Jika sudah ada **4 gift** di scene, gift tertua akan dihapus saat spawn gift baru

---

## ✅ Checklist Setup

- [ ] Buat 3 gift prefabs dengan SpriteRenderer dan Collider2D
- [ ] Tambahkan GiftItem script ke setiap prefab
- [ ] Buat 3 reward tables (Small, Medium, Large)
- [ ] Setup reward tables dengan Quick Setup buttons
- [ ] Buat ItemData untuk Feed, Medicine, Golden Ticket, Decoration
- [ ] Assign reward tables ke gift prefabs
- [ ] Setup ChickenAI dengan gift prefabs
- [ ] Configure animation settings (bounce scale, duration)
- [ ] Test di Play mode
- [ ] Validate probability total = 100%

---

**Happy Gift Hunting! 🎁✨**
