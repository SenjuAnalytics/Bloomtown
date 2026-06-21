# Internal Testing Plan - Bloomtown

## Tujuan Testing

Mengevaluasi apakah sistem yang sudah dibangun (Social Standing, Daily Rhythm, Agency, Aktivitas Harian, Personal Milestone, dll) terasa **menyenangkan, bermakna, dan tidak monoton** ketika dimainkan secara utuh dalam jangka waktu yang lebih panjang (minimal 15–20 hari in-game).

---

## Struktur Testing

### Fase 1: Hari 1 – 7 (Onboarding & First Impression)

**Tujuan:** Merasakan pengalaman bermain sebagai player baru.

**Hal yang Perlu Diperhatikan:**

- Apakah player baru mudah memahami apa yang harus dilakukan di hari pertama?
- Apakah `status` command dan `help` sudah cukup membantu?
- Bagaimana rasanya melakukan aktivitas harian di hari 1–3?
- Apakah pilihan `start calm` / `start active` terasa berguna?
- Catat hal-hal yang membingungkan atau kurang nyaman di hari awal.

**Target:** Mainkan minimal 5–7 hari in-game.

---

### Fase 2: Hari 8 – 15 (Daily Loop & Progression)

**Tujuan:** Mengevaluasi apakah rutinitas harian terasa menarik dalam jangka menengah.

**Hal yang Perlu Diperhatikan:**

- Apakah aktivitas harian mulai terasa repetitif?
- Apakah Personal Milestone memberikan rasa progresi yang cukup?
- Bagaimana rasanya naik ke tier Respected? Apakah terasa worth it?
- Apakah Daily Rhythm dan pilihan agency memberikan dampak yang terasa?
- Aktivitas mana yang paling sering dilakukan? Mengapa?
- Apakah desa terasa hidup atau masih statis?
- Apakah kamu masih ingin melanjutkan bermain setelah hari ke-10?

**Target:** Mainkan hingga minimal 12–15 hari in-game.

---

### Fase 3: Evaluasi Keseluruhan (Setelah Hari 15+)

Jawab pertanyaan berikut setelah mencapai minimal 15 hari in-game:

1. Bagian mana yang paling kamu nikmati selama bermain?
2. Bagian mana yang paling membosankan atau repetitif?
3. Apakah ada sistem yang terasa kurang jelas atau kurang bermanfaat?
4. Apakah kamu merasa ada tujuan/progresi saat bermain?
5. Apa yang paling ingin ditambahkan atau diperbaiki?

---

## Area Prioritas untuk Diperhatikan

| Prioritas | Area | Alasan |
|-----------|------|--------|
| Tinggi | Daily Loop & Aktivitas Harian | Apakah terasa repetitif setelah beberapa hari? |
| Tinggi | Daily Rhythm & Agency | Apakah pilihan agency terasa bermakna? |
| Tinggi | Tier Respected | Apakah tier ini terasa rewarding? |
| Sedang | Personal Milestone | Apakah memberikan rasa progresi? |
| Sedang | Emotional Bond dengan NPC | Apakah hubungan dengan NPC terasa bermakna? |
| Sedang | Village Reactivity | Apakah desa terasa hidup? |

---

## Cara Mencatat Feedback

### Format per Sesi (isi setelah setiap sesi bermain)

```markdown
## Sesi Testing — [Tanggal]
**Tester:** [nama]
**Hari in-game:** [X] · Fase: [1 / 2 / 3]
**Durasi sesi:** [~X menit]

### Ringkasan hari ini
- Aktivitas utama:
- Agency dipakai: [ya/tidak — calm / active / focused break / rest early / push through / wind down]
- Milestone baru: [nama atau "tidak ada"]
- Standing tier: [Stranger / Known / Respected / Well-liked]

### Skor cepat (1–5)
| Area                            | Skor | Catatan 1 kalimat |
|--------------------------------|------|-------------------|
| Onboarding / kejelasan         |      |                   |
| Daily loop (repetitif?)        |      |                   |
| Daily rhythm & agency          |      |                   |
| Personal milestone             |      |                   |
| Social standing / Respected    |      |                   |
| Emotional bond NPC             |      |                   |
| Village reactivity             |      |                   |
| Ingin lanjut besok?            |      |                   |

### Yang terasa bagus
-

### Yang membingungkan / tidak nyaman
-

### Bug / hal teknis
-

### Ide perbaikan (opsional)
-
```

---

### Format per Fase (isi sekali saat fase selesai)

```markdown
## Evaluasi Fase [1 / 2 / 3] — Hari [X–Y]
**Tester:** [nama]

### Pertanyaan fase
1.
2.

### Temuan utama (maks. 3)
1.
2.
3.

### Keputusan
- [ ] Lanjut ke fase berikutnya
- [ ] Perlu perbaikan dulu di: ___

### Area prioritas tinggi — status
| Area                    | Lulus / Gagal / Netral | Bukti singkat |
|-------------------------|------------------------|---------------|
| Daily loop              |                        |               |
| Daily rhythm & agency   |                        |               |
| Tier Respected          |                        |               |
```

---

### Format Evaluasi Akhir (Fase 3, hari 15+)

```markdown
## Evaluasi Akhir — Hari [X]+
**Tester:** [nama] · Total sesi: [N]

1. **Paling dinikmati:**
2. **Paling membosankan / repetitif:**
3. **Sistem kurang jelas / kurang berguna:**
4. **Rasa tujuan & progresi (1–5):** [ ] — bukti: milestone ___, standing ___, bond ___
5. **Prioritas perbaikan:**
   - P1 (harus):
   - P2 (nice):
   - P3 (nanti):
```

---

## Checklist Command per Fase

### Fase 1 — Hari 1–7

| Hari | Coba minimal | Perhatikan |
|------|----------------|------------|
| 1 | `help`, `status`, `greet elsie`, `daily`, `rhythm` | Apakah Getting Started + tip di `status` cukup? |
| 2 | `chat locals`, `sit bench`, nap / sleep | Apakah Nearby Activities terbaca di `status`? |
| 3–4 | community help (`help garden`, dll.), gather | Apakah recovery mendominasi tanpa arah? |
| 5–7 | `start calm` **vs** `start active`, `place` furniture | Apakah agency terasa berbeda? Milestone muncul di `status`? |

**Sinyal gagal fase 1:** Player bingung >10 menit tanpa tahu langkah berikutnya, atau `help`/`status` tidak pernah dipakai karena tidak membantu.

---

### Fase 2 — Hari 8–15

| Target | Cara mendekati | Cek di `status` |
|--------|----------------|-----------------|
| First Furnishing | `place` furniture | Home + Personal Milestones |
| Comfortable Nest | comfort ≥ 21 | Home |
| Helping Hand | 3× community help | Village Standing + Personal Milestones |
| Close Friend | greet/talk/gift focus NPC | Social Standing |
| Respected Neighbor | 2 focus close friends | Social Standing tier |
| Steady Rhythm | agency di 3 hari berbeda | Daily Rhythm + Personal Milestones |
| Village Regular | 4 daily activities di ≥3 hari | `chat locals`, `tend public garden`, `practice workshop` |

**Pertanyaan wajib hari 10:** *Apakah saya masih ingin lanjut besok?* — catat ya/tidak + alasannya.

**Sinyal gagal fase 2:** Loop = recovery saja; milestone tidak terasa; Respected hanya grind tanpa hubungan.

---

## Matriks Prioritas (Lulus / Gagal)

| Prioritas | Area | Lulus jika… | Gagal jika… |
|-----------|------|-------------|-------------|
| **Tinggi** | Daily loop | Hari 10+ masih ada variasi aktivitas yang *dipilih* | Hanya nap/sleep/bench tanpa motivasi |
| **Tinggi** | Daily rhythm | Agency mengubah cara main hari itu | `rhythm` diabaikan setelah hari 3 |
| **Tinggi** | Respected | Naik tier terasa rewarding + ada reaksi desa/NPC | Greet tanpa rasa kedekatan |
| **Sedang** | Personal milestone | Progress terlihat; 1–2 milestone terasa bermakna | Tidak muncul / tidak diperhatikan |
| **Sedang** | Emotional bond | ≥1 NPC terasa dekat | Semua interaksi terasa sama |
| **Sedang** | Village reactivity | Ada momen desa “menyadari” player | Desa statis meski hari tinggi |

---

## Referensi Alur 15–20 Hari (fleksibel)

| Hari | Fokus |
|------|-------|
| 1–2 | orientasi: `help`, `status`, greet, `daily`, `rhythm` |
| 3–5 | komunitas + sosial: help, `chat locals` |
| 6–8 | rumah + bond: furniture, dekatkan 1 focus NPC |
| 9–12 | variasi agency + daily activities di lokasi berbeda |
| 13–15 | kejar Respected + 2–3 personal milestone |
| 16–20 | evaluasi fase 3 |

---

## Exit Criteria Internal Testing

- [ ] ≥15 hari in-game tercatat
- [ ] ≥5 sesi log (format per sesi) terisi
- [ ] Evaluasi fase 1, 2, dan 3 selesai
- [ ] Semua area **prioritas tinggi** punya skor + bukti konkret
- [ ] Daftar P1 perbaikan disusun dari temuan, bukan asumsi