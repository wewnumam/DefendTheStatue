# Albal — Playtest Checklist

**Build:** Unity 6 / `Assets/@Game/Scenes/Gameplay.unity` (Play mode) atau build `Builds/`
**Tester:** ______________  **Tanggal:** __________  **Build/commit:** __________
**Durasi 1 sesi:** 10 menit + boss (target 600 ± 60 detik)

Cara pakai: centang kalau sesuai. Kalau tidak sesuai → tulis ID item di tabel bug (§9) + screenshot/video.
Jangan lapor sebagai bug kalau ada di §8 (Known limitations).

---

## 1. Smoke test (2 menit)

- [ ] Scene gameplay load tanpa error merah di Console
- [ ] Player muncul di dalam Wilayah 1, kamera mengikuti mulus
- [ ] Timer jalan dari 00:00, bar HP Patung penuh (50/50), level `Lv.1`
- [ ] Monster mulai spawn dalam ~3 detik
- [ ] Bisa menyerang, membunuh Stacker, dapat +1 Essence
- [ ] Tekan **E** dekat Patung → panel upgrade terbuka

---

## 2. Kontrol (GDD §3)

| Aksi | Input | Cek |
|---|---|---|
| Gerak 8 arah | WASD | - [ ] tanpa drift, diagonal tidak lebih cepat |
| Aim | Mouse | - [ ] player menghadap kursor, tidak ada lag terasa |
| Serangan dasar | Klik kiri | - [ ] repeat saat ditahan, kena monster/tree/boss |
| Block (sword) | Tahan klik kanan | - [ ] stamina turun 0.5/detik, berhenti di 0 |
| Charge shot (bow) | Tahan klik kanan, lepas | - [ ] 1 panah keluar, damage 2, stamina -2 |
| Ganti senjata | Scroll | - [ ] pedang ⇄ busur, tidak bisa saat Clone Mode aktif |
| Interaksi | E | - [ ] patung / tower / wall / guard / teleport |
| Skill | 1 2 3 4 | - [ ] sesuai senjata di tangan |
| Tutup menu | ESC | - [ ] panel tertutup, game lanjut normal |
| Admin panel | F1 | - [ ] hanya muncul di debug build (§7) |

---

## 3. Player & ekonomi (GDD §4, §12)

- [ ] HP awal 50, stamina 20, mana 0 — angka di HUD cocok
- [ ] Mana **max** naik +1 per kill, bukan isi otomatis penuh
- [ ] Panah kembali +1 tiap 4 detik, max = level patung
- [ ] Mati → Patung −30 HP, respawn dekat patung, 3 detik invincible (tidak bisa diserang)
- [ ] Di dalam zona heal: HP regen saat tidak ada musuh di zona
- [ ] Ada musuh di zona → heal berhenti, zona berkedip merah
- [ ] Essence bertambah: Stacker 1, Waltroy 2, Deasor 2
- [ ] Beli tanpa cukup essence → ditolak, tidak ada yang terbangun

---

## 4. Patung & progresi (GDD §5)

- [ ] Upgrade Lv 2 (20e) → Wilayah 2 terbuka, HP max 60
- [ ] Lv 3 (25e) **diblokir** sampai semua 8 wall W1 selesai
- [ ] Lv 3 → fondasi Guard Dalam muncul (3 slot) + Skill 1 aktif
- [ ] Lv 4 (40e) **diblokir** sampai 3 Guard Dalam direkrut
- [ ] Lv 5 (50e) → Wilayah 3 terbuka, fondasi Guard Luar, Skill 2 aktif
- [ ] Lv 6 **diblokir** sampai 6 Guard Luar direkrut
- [ ] Lv 7 → Skill 3 aktif; tombol menampilkan alasan blokir (100 kill)
- [ ] Lv 8 → pod teleport muncul (3)
- [ ] Lv 9 → Skill 4 aktif
- [ ] Lv 10 → fondasi Guard Perimeter muncul, HP max 140
- [ ] HP max tiap level: 50/60/70/80/90/100/110/120/130/140
- [ ] Biaya upgrade: 20/25/40/50/60/70/90/110/140
- [ ] Panel Patung: auto-close setelah WASD 4 detik, progress bar muncul, warning di detik 2 & 3
- [ ] Klik mouse / scroll / ESC juga menutup panel

---

## 5. Bangunan (GDD §9)

- [ ] Tower: 4 per wilayah di sudut, cost 20e, HP 40, damage 1, range 300, menembak otomatis
- [ ] Wall: 8 segmen per wilayah, cost 20e, HP 100, **baru bisa** setelah 4 tower wilayah itu selesai
- [ ] Wall W2 butuh semua wall W1 selesai
- [ ] Wall memblokir gerak player & monster; celah di 4 sisi tetap bisa dilewati
- [ ] Tower/wall hancur → slot bisa dibangun ulang
- [ ] Spike trap: 1 per celah, cost 5e, damage 1, 10 trigger lalu hilang, slow 2 detik, cooldown singkat
- [ ] Prompt **E** muncul di objek yang bisa dibangun, menampilkan harga

---

## 6. Guard, monster, pohon (GDD §6, §7, §10)

**Guard**
- [ ] Guard Dalam patroli Wilayah 1, Guard Luar di ring antara W1–W2, Perimeter di luar W2
- [ ] Guard Luar tidak masuk ke Wilayah 1
- [ ] Guard HP <30% → retreat ke patung → heal → kembali patroli (lingkaran hijau)
- [ ] Zona ternoda → guard mode purging (lingkaran merah)
- [ ] Skill 4 bow: guard dalam radius dapat ⚡x4 damage & attack speed ×2 selama 6 detik
- [ ] Guard mati hilang dari scene, slot tetap terpakai? → catat hasil sebenarnya

**Monster**
- [ ] Stacker (merah, HP 3, cepat), Waltroy (coklat, HP 10, lambat, serang wall/tower 4 damage), Deasor (ungu, HP 2, cepat, damage 4 ke player/guard)
- [ ] Monster menyerang Patung 2/1/1 (Stacker/Waltroy/Deasor)
- [ ] Monster HP <30% → cari pohon untuk makan; di bawah tekanan player/guard → kabur 1.5×
- [ ] Monster **tidak** makan Purple Tree
- [ ] Makan Magic/Arcana memulihkan HP lebih besar dari Red
- [ ] Skill 4 sword: monster dalam radius jadi sekutu (😇), damage ×1.5, kembali normal setelah 6 detik

**Pohon**
- [ ] 4 jenis dengan warna beda: Red, Purple, Magic, Arcana (tier 2 & 3 punya tanda)
- [ ] Bisa ditebang dengan serangan, HP sesuai tabel (5/5/9/12)
- [ ] Respawn 30/30/60/120 detik dengan animasi tumbuh
- [ ] Pohon tumbuh baru tiap ±60 detik di wilayah yang terbuka
- [ ] Pohon memblokir gerak player

---

## 7. Skill (GDD §13)

- [ ] Skill 1 (Lv 3, 5 mana): sword dash damage 2 / bow triple shot 4 per panah
- [ ] Skill 2 (Lv 5, 20 mana): sword time slash radius 360, musuh melambat 80% 6 detik / bow lock-on damage ×2
- [ ] Skill 3 (Lv 7, 20 mana): sword berserker damage 4, speed ×4, stamina gratis 5 detik / bow clone mode attack ×2 + 2 clone
- [ ] Skill 4 (Lv 9, 50 mana): sword mind control / bow guard empower, radius 180, 6 detik
- [ ] Skill terkunci → kotak HUD menunjukkan 🔒 + level unlock
- [ ] Cooldown per senjata: pakai skill di pedang tidak mengunci skill busur
- [ ] Mana kurang → cast ditolak, ada umpan balik
- [ ] Effect berakhir tepat waktu (tidak nyangkut setelah 6 detik)

---

## 8. Boss, hasil, UI, admin

**Boss (GDD §8)**
- [ ] Boss spawn di menit 10:00, bar boss muncul di HUD
- [ ] Spawn dengan 8 minion: 4 Stacker, 2 Waltroy, 2 Deasor
- [ ] HP 20, speed 0.5, damage melee 10, **tanpa projectile**, **tanpa enrage**
- [ ] Fase 1 (>60%) summon 2 Stacker; Fase 2 (30–60%) 1 Stacker + 1 Waltroy; Fase 3 (<30%) 1 Waltroy + 1 Deasor + attack rate ×2
- [ ] Boss selalu menuju Patung, tidak pernah retreat
- [ ] Boss mati → layar VICTORY + statistik
- [ ] Patung 0 HP → GAME OVER, statistik tampil (waktu, kill per jenis, death, essence, max mana, level patung)
- [ ] Main Lagi / restart mengembalikan semua state (HP, level, bangunan, monster, timer)

**UI (GDD §15)**
- [ ] Bar HP patung, timer MM:SS (merah mulai menit 9), gauge stamina/HP/mana, essence, panah
- [ ] 8 kotak skill (4 pedang + 4 busur) terlihat, status cooldown/aktif jelas
- [ ] Ikon status monster (makan / kabur / mind control) dan lingkaran status guard
- [ ] Prompt E menampilkan harga

**Admin panel (GDD §16) — F1, debug build**
- [ ] 6 toggle: God Mode, Infinite Essence, Infinite Stamina/Mana, Free Build, No Cooldown, Stop Spawn
- [ ] 10 aksi: Unlock All, Max Level, +1,000 Essence, +100 Mana, Heal Statue, Heal Player, Build All, Kill All, Spawn Boss, Kill Boss
- [ ] God Mode: player & patung tidak berkurang HP
- [ ] Max Level → Lv 10, semua wilayah terbuka, fondasi guard muncul
- [ ] Unlock All → 4 skill langsung bisa dipakai di Lv 1
- [ ] Build All → semua tower/wall/trap di wilayah terbuka langsung terbangun, essence tidak berkurang
- [ ] Tutup panel (F1) → semua cheat mati, efek aksi tetap ada
- [ ] Panel tidak muncul di build non-debug

---

## 9. Adaptif & performa (GDD §14)

- [ ] Diam di patung beberapa menit → spawn makin cepat / Waltroy lebih sering
- [ ] Kill didominasi guard → Deasor lebih sering muncul
- [ ] HP player/patung <25% → tekanan turun (tidak makin brutal)
- [ ] Stat monster tidak melebihi 1.5× nilai dasar
- [ ] 60 FPS stabil saat 40+ monster + tower menembak (catat FPS terendah)
- [ ] Tidak ada memory naik terus / objek nyangkut setelah restart

---

## 10. Known limitations (jangan dilaporkan sebagai bug)

- Gauge melingkar HUD belum final (masih placeholder)
- Countdown per kotak skill masih kasar / belum real-time
- Ikon status monster (makan / kabur / mind control) belum ada artnya
- Guard direkrut manual dari fondasi (bukan otomatis)
- Tidak ada save/load; restart = reload scene
- Admin panel hanya di build dengan `IsDebugBuild = 1`
- Cursor kustom masih WIP

---

## 11. Format laporan bug

| Field | Isi |
|---|---|
| ID | B-001 |
| Item checklist | mis. §5 Wall |
| Severity | Blocker / Major / Minor / Cosmetic |
| Build + commit | |
| Langkah reproduksi | 1. 2. 3. |
| Hasil sebenarnya | |
| Hasil diharapkan + rujukan GDD | mis. GDD §9 "Wall HP 100" |
| Bukti | screenshot / video / log Console |
| Frekuensi | selalu / kadang (x/10) |

**Severity**
- **Blocker** — tidak bisa main / crash / progresi macet
- **Major** — mekanik inti salah (damage, gate upgrade, boss, ekonomi)
- **Minor** — angka/UI salah tapi game tetap jalan
- **Cosmetic** — teks, warna, posisi

---

## 12. Sign-off

| Bagian | Lulus | Gagal | Catatan |
|---|---|---|---|
| 1 Smoke | | | |
| 2 Kontrol | | | |
| 3 Player & ekonomi | | | |
| 4 Patung & progresi | | | |
| 5 Bangunan | | | |
| 6 Guard/monster/pohon | | | |
| 7 Skill | | | |
| 8 Boss/UI/admin | | | |
| 9 Adaptif & performa | | | |

**Total bug:** Blocker __ · Major __ · Minor __ · Cosmetic __
**Rekomendasi:** ☐ layak lanjut ke polish  ☐ perlu perbaikan sebelum playtest berikutnya
