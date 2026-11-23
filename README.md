# 📜 GAME DESIGN DOCUMENT (GDD)

## **Judul Game:** Albal

**Genre:** Top-Down 2D Action, Survival, Tower Defense 

**Platform:** PC 

**Target Durasi:** 10 Menit (600 detik) per sesi 

**Bahasa:** English

**Currency:** 💎 **Essence**

---

## 1. 🎯 GAME OVERVIEW

"Albal" adalah game survival aksi pertahanan di mana pemain melindungi **God Statue** selama 10 menit dari gelombang monster yang semakin kuat, diakhiri dengan pertarungan Boss. Pemain berperan ganda: sebagai pejuang garis depan (Pedang/Busur) dan komandan pertahanan (Tower, Tembok, Guard).

**Core Objective:** Bertahan 10 menit, jaga HP Patung di atas 0, dan kalahkan Boss "The Destroyer".

---

## 2. 🎮 CORE GAMEPLAY LOOP

```
┌─ FASE AWAL (0-4 menit) ──────────────────────────────┐
│  • Bunuh Stacker → kumpulkan Essence                  │
│  • Upgrade Patung ke Lv 2 → buka Wilayah 2            │
│  • Bangun Tower & Wall pertama                        │
└───────────────────────────────────────────────────────┘
                        ↓

┌─ FASE MENENGAH (4-8 menit) ──────────────────────────┐
│  • Waltroy & Deasor mulai muncul                      │
│  • Rekrut Guard Dalam & Luar                          │
│  • Unlock Skill 1 & 2                                 │
│  • Upgrade Patung ke Lv 5-7                           │
└───────────────────────────────────────────────────────┘
                        ↓

┌─ FASE AKHIR (8-10 menit) ────────────────────────────┐
│  • Gelombang sangat padat                             │
│  • Gunakan Skill 3 & 4                                │
│  • Patung mencapai Lv 8-10                            │
│  • Siapkan pertahanan untuk Boss                       │
└───────────────────────────────────────────────────────┘
                        ↓

┌─ BOSS FIGHT (menit 10+) ─────────────────────────────┐
│  • Boss muncul dengan 8 minion                        │
│  • 3 fase pertempuran                                 │
│  • Kalahkan Boss untuk VICTORY!                       │
└───────────────────────────────────────────────────────┘
```

---

## 3. 🕹️ CONTROLS

| Input                  | Aksi                                                |
| ---------------------- | --------------------------------------------------- |
| **WASD**               | Bergerak                                            |
| **Mouse**              | Mengarahkan karakter                                |
| **Klik Kiri**          | Serangan dasar                                      |
| **Klik Kanan (tahan)** | Sword: Block (0.5 stamina/detik) · Bow: Charge Shot |
| **Scroll Mouse**       | Ganti senjata (Pedang ↔ Busur)                      |
| **E**                  | Interaksi (Patung, Tower, Wall, Guard, Teleport)    |
| **1, 2, 3, 4**         | Aktifkan Skill                                      |
| **ESC**                | Tutup menu                                          |

---

## 4. 🧑‍🦰 PLAYER CHARACTER (PAWN)

### **Stat Dasar**

| Stat               | Nilai                              |
| ------------------ | ---------------------------------- |
| **HP**             | 50                                 |
| **Stamina**        | 20                                 |
| **Mana**           | Mulai 0, max bertambah +1 per kill |
| **Movement Speed** | 2                                  |
| **Size**           | 30                                 |
| **Arrow Cooldown** | 4 detik (240 frame)                |

### **Stamina Cost**

| Aksi                      | Cost                    |
| ------------------------- | ----------------------- |
| Sword normal attack       | 1                       |
| Bow normal shot           | 1                       |
| Bow charged shot          | 2                       |
| Sword block               | 0.5/detik               |
| Skill 3 Sword (Berserker) | **GRATIS** selama aktif |

### **Damage Output — Pedang (Sword)**

| Aksi                | Damage | Catatan                |
| ------------------- | ------ | ---------------------- |
| Basic Attack        | **1**  | Cost 1 stamina         |
| Skill 1 (Dash)      | **2**  | Per hit saat menabrak  |
| Skill 3 (Berserker) | **4**  | Speed ×4, stamina FREE |

### **Damage Output — Busur (Bow)**

| Aksi                  | Damage           | Catatan                          |
| --------------------- | ---------------- | -------------------------------- |
| Basic Shot            | **1**            | Cost 1 stamina + 1 panah         |
| Charged Shot          | **2**            | Tahan klik kanan, cost 2 stamina |
| Skill 1 (Triple Shot) | **4 per panah**  | 3 panah menyebar (total 12)      |
| Skill 2 (Lock-On)     | **2× damage**    | Auto-aim ke musuh terdekat       |
| Skill 3 (Clone Mode)  | **2 per attack** | 1 utama + 2 clone                |

### **Mekanik Khusus**

- **Respawn**: Setelah mati, respawn di dekat Patung dengan 3 detik invincible
- **Kematian**: -30 HP ke Patung
- **Healing Zone**: Dalam radius Patung, regen 0.3 HP/frame (jika zona tidak ternoda)

---

## 5. 🗿 PATUNG DEWA (STATUE)

### **HP per Level**

| Level | HP Max | Bonus Fitur           |
| ----- | ------ | --------------------- |
| 1     | 50     | Awal game             |
| 2     | 60     | Wilayah 2 terbuka     |
| 3     | 70     | Guard Dalam + Skill 1 |
| 4     | 80     | Wilayah 3 terbuka     |
| 5     | 90     | Guard Luar + Skill 2  |
| 6     | 100    | —                     |
| 7     | 110    | Skill 3               |
| 8     | 120    | Teleport Pods         |
| 9     | 130    | Skill 4               |
| 10    | 140    | Guard Perimeter       |

### **Upgrade Cost (Essence)**

| Level Target | Cost | Syarat                   |
| ------------ | ---- | ------------------------ |
| Lv 2         | 20   | —                        |
| Lv 3         | 25   | Semua Wall W1 selesai    |
| Lv 4         | 40   | 3 Guard Dalam aktif      |
| Lv 5         | 50   | Semua Wall W2 selesai    |
| Lv 6         | 60   | 6 Guard Luar aktif       |
| Lv 7         | 70   | 100 monster dibunuh      |
| Lv 8         | 90   | Boss kalah ATAU 150 kill |
| Lv 9         | 110  | Boss kalah ATAU 200 kill |
| Lv 10        | 140  | Boss kalah DAN 250 kill  |

### **Zona Heal**

- Radius: 120
- Regen Player: 0.3 HP/frame
- **Ternoda** jika ada musuh di dalam → heal berhenti

---

## 6. 🛡️ GUARD SYSTEM

### **3 Jenis Guard**

| Guard                  | Level | Cost | HP  | Speed | Damage |
| ---------------------- | ----- | ---- | --- | ----- | ------ |
| **Guard Dalam** 🛡️    | Lv 3  | 20e  | 100 | 2     | 1      |
| **Guard Luar** 🛡️     | Lv 5  | 25e  | 150 | 2     | 1      |
| **Guard Perimeter** ⚔️ | Lv 10 | 30e  | 200 | 2     | 1      |

### **Damage Guard dalam Berbagai Mode**

| Mode                        | Damage                         |
| --------------------------- | ------------------------------ |
| Normal Attack               | 1                              |
| Purging Mode                | **1** (tetap, tidak ada bonus) |
| Guard Empower (Skill 4 Bow) | **4** (×4 selama 6 detik)      |

### **State Machine Guard**

```

PATROL → (HP < 30%) → RETREATING → (tiba di Patung) → HEALING

   ↑                                                        │

   └──────────── RETURNING (HP > 70%) ◀────────────────────┘

                          │

                          └─(zona ternoda)─▶ PURGING

```

### **Zona Patroli**

- **Guard Dalam**: Berpatroli di Wilayah 1
- **Guard Luar**: Berpatroli di Ring antara Wilayah 1 & 2
- **Guard Perimeter**: Berpatroli di garis luar Wilayah 2

---

## 7. 👹 MONSTER SYSTEM

### **Stat Monster**

| Monster        | HP  | Speed | Warna  | Peran    | Essence |
| -------------- | --- | ----- | ------ | -------- | ------- |
| **Stacker** 👹 | 3   | 1.0   | Merah  | Swarm    | 1       |
| **Waltroy** 🏰 | 10  | 0.5   | Coklat | Siege    | 2       |
| **Deasor** 🗡️ | 2   | 2.0   | Ungu   | Assassin | 2       |

### **Damage Output Monster**

| Monster     | ke Statue | ke Wall/Tower | ke Player | ke Guard |
| ----------- | --------- | ------------- | --------- | -------- |
| **Stacker** | **2**     | 1             | 1         | 1        |
| **Waltroy** | 1         | **4**         | 1         | 1        |
| **Deasor**  | 1         | 1             | **4**     | **4**    |

### **AI Behavior**

- **Healing**: HP < 30% → cari pohon (Red/Magic/Arcana) untuk makan HP
- **Retreat**: HP < 30% + ada player/guard dekat → kabur dengan speed **1.5×**
- **Mind Control** (Skill 4 Sword): Jadi sekutu, damage ×1.5

### **Pohon yang Dimakan Monster**

- ✅ Red Tree (HP drop 1)
- ❌ Purple Tree (tidak dimakan — hanya mana)
- ✅ Magic Tree (HP drop 2, mana tetap utuh)
- ✅ Arcana Tree (HP drop 3, mana tetap utuh)

---

## 8. 💀 BOSS — "THE DESTROYER"

### **Stat Dasar**

| Stat           | Nilai         |
| -------------- | ------------- |
| HP             | 20            |
| Movement Speed | 0.5           |
| Size           | 80            |
| Melee Damage   | 10            |
| Projectile     | **TIDAK ADA** |
| Enrage         | **TIDAK ADA** |

### **3 Fase Boss**

| Fase       | HP     | Summon                | Attack Rate |
| ---------- | ------ | --------------------- | ----------- |
| **Fase 1** | >60%   | 2 Stacker             | Normal      |
| **Fase 2** | 30-60% | 1 Stacker + 1 Waltroy | Normal      |
| **Fase 3** | <30%   | 1 Waltroy + 1 Deasor  | **×2**      |

### **Spawn Behavior**

- Target utama: **Patung** (di semua fase)
- Saat spawn: dikelilingi **8 minion**:
  - 4 Stacker
  - 2 Waltroy
  - 2 Deasor
- Boss **TIDAK BISA RETREAT**

---

## 9. 🏰 DEFENSE STRUCTURES

### **Tower**

| Stat             | Nilai          |
| ---------------- | -------------- |
| HP               | **40**         |
| Damage per Arrow | **1**          |
| Attack Range     | 300            |
| Attack Rate      | 50 frame       |
| Cost             | **20 essence** |
| Per Wilayah      | 4 (di sudut)   |

### **Wall Segment**

| Stat               | Nilai                                   |
| ------------------ | --------------------------------------- |
| HP                 | 100                                     |
| Cost               | **20 essence**                          |
| Segmen per Wilayah | 8                                       |
| Syarat Build       | Semua Tower di wilayah tersebut selesai |

### **Spike Trap**

| Stat            | Nilai                                        |
| --------------- | -------------------------------------------- |
| Damage          | **1**                                        |
| Cost            | **5 essence**                                |
| Slowmo Duration | 120 frame                                    |
| Max Durability  | 10 trigger                                   |
| Radius          | 35                                           |
| Cooldown        | 45 frame                                     |
| Muncul Otomatis | Di celah tembok setelah semua Tower dibangun |

---

## 10. 🌳 ENVIRONMENT (POHON)

### **Jenis Pohon**

| Jenis           | HP  | HP Drop | Mana Drop | Tier | Warna | Respawn   |
| --------------- | --- | ------- | --------- | ---- | ----- | --------- |
| **Red Tree**    | 5   | 1       | 0         | 1    | Merah | 30 detik  |
| **Purple Tree** | 5   | 0       | 3         | 1    | Ungu  | 30 detik  |
| **Magic Tree**  | 9   | 2       | 2         | 2    | Biru  | 60 detik  |
| **Arcana Tree** | 12  | 3       | 3         | 3    | Emas  | 120 detik |

### **Distribusi Spawn per Wilayah**

- Red: 15 pohon
- Purple: 15 pohon
- Magic: 4 pohon
- Arcana: 2 pohon

### **Mekanik**

- Pohon tumbuh kembali setelah dihancurkan
- Pemain bisa menebang dengan attack
- Monster bisa "memakan" pohon untuk heal
- Monster TIDAK akan makan Purple Tree

---

## 11. 🌀 TELEPORT SYSTEM

### **Teleport Pods**

| Stat          | Nilai                |
| ------------- | -------------------- |
| Build Cost    | **20 essence**       |
| Teleport Cost | **10 essence**       |
| Jumlah Pod    | 3 (satu per wilayah) |
| Unlock        | Level Patung 8       |

### **Mekanik**

- Klik Pod → masuk **Teleport Select Mode**
- Kamera zoom out (0.45×)
- WASD untuk geser kamera
- Klik Pod tujuan → konfirmasi teleport
- ESC untuk batal

---

## 12. 💎 ECONOMY (ESSENCE)

### **Sumber Essence**

| Sumber | Essence |
|---|---|
| Stacker kill | 1 |
| Waltroy kill | 2 |
| Deasor kill | 2 |

### **Pengeluaran Essence**

| Item | Cost |
|---|---|
| Tower | 20 |
| Wall Segment | 20 |
| Guard Dalam (Lv 3) | 20 |
| Guard Luar (Lv 5) | 25 |
| Guard Perimeter (Lv 10) | 30 |
| Teleport Pod (build) | 20 |
| Teleportasi | 10 |
| Spike Trap | 5 |
| Upgrade Patung Lv 2 | 20 |
| Upgrade Patung Lv 3 | 25 |
| Upgrade Patung Lv 4 | 40 |
| Upgrade Patung Lv 5 | 50 |
| Upgrade Patung Lv 6 | 60 |
| Upgrade Patung Lv 7 | 70 |
| Upgrade Patung Lv 8 | 90 |
| Upgrade Patung Lv 9 | 110 |
| Upgrade Patung Lv 10 | 140 |

---

## 13. ⚡ SKILL SYSTEM

### **Skill 1 — Unlock di Lv 3 (Cost: 5 Mana)**

| Weapon | Efek |
|---|---|
| **Sword** | **Dash**: Menyeruduk ke arah kursor, damage 2 |
| **Bow** | **Triple Shot**: 3 panah menyebar, masing-masing damage 4 |

### **Skill 2 — Unlock di Lv 5 (Cost: 20 Mana)**

| Weapon | Efek |
|---|---|
| **Sword** | **Time Slash**: AoE 360px, memperlambat musuh 80% selama 6 detik |
| **Bow** | **Lock-On**: Panah berikutnya auto-aim, damage ×2 |

### **Skill 3 — Unlock di Lv 7 (Cost: 20 Mana)**

| Weapon | Efek |
|---|---|
| **Sword** | **Berserker**: Damage 4/hit, speed ×4, stamina FREE, 5 detik |
| **Bow** | **Clone Mode**: Attack speed ×2, 2 panah clone tambahan, damage 2/attack |

### **Skill 4 — Unlock di Lv 9 (Cost: 50 Mana)**

| Weapon | Efek |
|---|---|
| **Sword** | **Mind Control**: Mengubah musuh dalam radius 180 jadi sekutu (damage ×1.5), 6 detik |
| **Bow** | **Guard Empower**: Guard dalam radius 180 dapat damage ×4 & attack speed ×2, 6 detik |

---

## 14. 🧠 ADAPTIVE AI SYSTEM

Game memantau 3 skor untuk menyesuaikan kesulitan secara dinamis:

| Metrik | Cara Kerja | Efek |
|---|---|---|
| **Turtle Score** | Diam di Patung + bergantung Guard | Monster spawn lebih cepat, Waltroy lebih banyak |
| **Guard Score** | Rasio kill Guard vs total | Deasor muncul lebih sering |
| **Neglect Score** | HP Patung rendah + monster dekat | Threat level naik |

### **Safety Net**

- Jika HP Player < 25% → threat level turun 50%
- Jika HP Patung < 25% → threat level turun 50%
- Mencegah spiral kematian

### **Monster Quality Buff**

- Monster yang di-spawn mendapat HP, Damage, dan Speed yang dimultiplier
- Maksimal buff: 1.5× stats dasar

---

## 15. 🖥️ UI / UX

### **HUD Elements**

- **Top Bar**: Bar HP Patung Dewa
- **Boss Bar**: Muncul saat Boss aktif
- **Bottom Center**: Timer (MM:SS), berubah merah di menit 9+
- **Bottom Left**: Gauge melingkar (Stamina/HP/Mana) + Info senjata, Essence, Panah
- **Bottom Right**: 8 kotak Skill (4 Sword + 4 Bow) dengan indikator cooldown

### **Auto-Close Menu Patung**

- Tahan WASD selama **4 detik** → menu otomatis tertutup
- Klik mouse (kiri/kanan) atau scroll → menu langsung tertutup
- Progress bar visual muncul saat menahan WASD
- Warning di detik ke-2 dan ke-3

### **Interact Prompt**

- Ikon "E" melayang di atas objek yang bisa diinteraksi
- Menampilkan cost dalam essence

---

## 16. 🛠️ ADMIN PANEL

| Cheat | Efek |
|---|---|
| ❤️ God Mode | HP Player & Statue full terus |
| 💎 Infinite Essence | Essence tak terbatas |
| ⚡ Infinite Stamina/Mana | Stamina & Mana full |
| 🏗️ Free Build | Semua bangunan gratis |
| ⏱️ No Cooldown | Skill tanpa cooldown |
| 🚫 Stop Spawn | Monster berhenti spawn |
| 🔓 Unlock All | Semua skill terbuka |
| 🗿 Max Level | Patung langsung Lv 10 |
| 💎 +1,000 Essence | Tambah essence |
| 💧 +100 Mana | Tambah mana |
| ❤️ Heal Statue | Full HP Patung |
| 💊 Heal Player | Full HP/Stamina/Mana |
| 🏰 Build All | Bangun semua struktur |
| 💀 Kill All | Semua monster mati |
| 💀 Spawn Boss | Munculkan Boss manual |
| ⚰️ Kill Boss | Bunuh Boss |

---

## 17. 🎯 VICTORY & GAME OVER

### **Victory Conditions**

- Boss HP mencapai 0

### **Game Over Conditions**

- HP Patung mencapai 0

### **Stats yang Ditampilkan**

- Waktu bermain
- Total kills (Stacker/Waltroy/Deasor)
- Death count
- Essence terkumpul
- Max Mana
- Level Patung

---

## 18. 📊 TABEL KILLS (Hit yang Dibutuhkan)

### **Pawn dengan Sword (damage 1)**

| Target | HP | Hit |
|---|---|---|
| Stacker | 3 | 3 hit |
| Waltroy | 10 | 10 hit |
| Deasor | 2 | 2 hit |
| Boss | 20 | 20 hit |

### **Pawn dengan Bow Charge (damage 2)**

| Target | HP | Hit |
|---|---|---|
| Stacker | 3 | 2 shots |
| Waltroy | 10 | 5 shots |
| Deasor | 2 | **1 shot** ✅ |
| Boss | 20 | 10 shots |

### **Pawn saat Berserker (damage 4)**

| Target | HP | Hit |
|---|---|---|
| Stacker | 3 | **1 hit** ✅ |
| Waltroy | 10 | 3 hits |
| Deasor | 2 | **1 hit** ✅ |
| Boss | 20 | 5 hits |

### **Tower (damage 1)**

| Target | HP | Hit |
|---|---|---|
| Stacker | 3 | 3 arrows |
| Waltroy | 10 | 10 arrows |
| Deasor | 2 | 2 arrows |
| Boss | 20 | 20 arrows |

### **Spike Trap (damage 1)**

| Target | HP | Trigger |
|---|---|---|
| Semua monster | - | 1 trigger ✅ |

---

## 19. 🏃 KECEPATAN RELATIF

| Entitas | Speed | Catatan |
|---|---|---|
| **Pawn (Berserker)** | 8.0 | 4× speed saat skill 3 aktif |
| **Pawn (Dash)** | 18.0 | Skill 1 sword |
| **Deasor** | 2.0 | Tercepat di monster biasa |
| **Pawn** | 2.0 | Sama cepat dgn Deasor |
| **Guard** | 2.0 | Sama cepat dgn Pawn |
| **Guard (retreat)** | 3.0 | 1.5× saat mundur |
| **Monster (retreat)** | 1.5 | 1.5× speed saat kabur |
| **Stacker** | 1.0 | Lambat |
| **Waltroy** | 0.5 | Paling lambat |
| **Boss** | 0.5 | Lambat |

---

## 20. 💥 DAMAGE KE PLAYER

| Sumber | Damage | Catatan |
|---|---|---|
| Stacker | 1 | Per hit |
| Waltroy | 1 | Per hit |
| Deasor | **4** | ⚠️ Sangat sakit |
| Boss (melee) | 10 | ⚠️ 5 hit kill |
| Kematian Player | -30 HP Statue | Otomatis |

---

## 21. 🎨 VISUAL INDICATORS

### **Monster State Icons**

- 🌳 — Monster sedang makan pohon (healing)
- 💨 — Monster sedang kabur (retreating)
- 😇 — Monster under Mind Control

### **Guard State Visuals**

- Lingkaran hijau — Sedang retreat/heal/return
- Lingkaran merah — Purging mode
- ⚡x4 — Under Guard Empower

### **Patung Zone**

- Kuning berkedip — Zona aman (heal aktif)
- Merah berkedip — Zona ternoda (heal berhenti)

---

## 22. 📝 CATATAN TEKNIS

- **Collision**: Circle-to-Circle & Point-to-Segment
- **Slowmo Scale**: 0.15× saat menu terbuka
- **Auto-close Timer**: 240 frame (4 detik real-time)

---
