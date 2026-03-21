# Cấu Trúc Folder — AI-DLC Daily Summary Report

**Last Updated**: 2026-03-21

---

## 📁 Current Structure

```
AI-DLC Daily Summary Report/
├── README.md                           # ✅ Hướng dẫn sử dụng (đọc đầu tiên)
├── STRUCTURE.md                        # ✅ File này - overview cấu trúc
│
├── 2026-03/                            # 📅 Tháng 03/2026
│   ├── 2026-03-21_Day-01/              # 📂 Day 01 (2026-03-21)
│   │   ├── daily-summary.md            # ✅ Quick overview (5-10 min read)
│   │   ├── case-study-report.md        # ✅ Comprehensive report (30-60 min read)
│   │   ├── slide-deck-outline.md       # ✅ Presentation (32 slides, 60 min)
│   │   └── executive-summary.md        # ✅ 1-pager for management
│   │
│   ├── 2026-03-22_Day-02/              # 📂 Day 02 (future)
│   │   └── ...
│   │
│   └── monthly-summary.md              # 📊 Tổng kết tháng (end of month)
│
├── templates/                          # 📝 Templates cho reports
│   ├── daily-summary-template.md       # ✅ Template cho daily summary
│   ├── case-study-template.md          # 🔜 TODO
│   ├── slide-deck-template.md          # 🔜 TODO
│   └── executive-summary-template.md   # 🔜 TODO
│
└── archive/                            # 📦 Archive các tháng cũ (optional)
    └── 2026-02/                        # 🔜 Future
```

---

## 📊 File Types & Purpose

### 1. Daily Summary (Bắt buộc mỗi ngày)
- **File**: `daily-summary.md`
- **Size**: ~5-10 KB
- **Time to write**: 10-15 phút
- **Audience**: Self, Team
- **Purpose**: Quick daily recap
- **Sections**:
  - Today's Goals
  - Completed Tasks
  - Key Decisions
  - Lessons Learned
  - Blockers/Issues
  - Metrics
  - Tomorrow's Plan

### 2. Case Study Report (Khi hoàn thành milestone)
- **File**: `case-study-report.md`
- **Size**: ~200-300 KB
- **Time to write**: 30-60 phút
- **Audience**: Team, Future reference
- **Purpose**: Comprehensive learning document
- **Sections**:
  - Project Overview
  - Workflow Details
  - Technical Decisions
  - Lessons Learned
  - DO's and DON'Ts
  - Metrics & Outcomes
  - Recommendations

### 3. Slide Deck Outline (Khi cần present)
- **File**: `slide-deck-outline.md`
- **Size**: ~50-100 KB
- **Time to write**: 20-30 phút
- **Audience**: Team, Stakeholders, Management
- **Purpose**: Presentation material
- **Format**: 
  - Slide content
  - Visual suggestions
  - Speaker notes
  - Q&A preparation

### 4. Executive Summary (Khi báo cáo management)
- **File**: `executive-summary.md`
- **Size**: ~10-20 KB
- **Time to write**: 10-15 phút
- **Audience**: Management, Executives
- **Purpose**: High-level overview
- **Sections**:
  - Executive Overview
  - Key Metrics
  - Major Achievements
  - Business Impact
  - Next Steps

---

## 🗓️ Naming Conventions

### Folder Names
- **Format**: `YYYY-MM-DD_Day-XX`
- **Examples**:
  - `2026-03-21_Day-01` ✅
  - `2026-03-22_Day-02` ✅
  - `2026-03-25_Day-03` ✅ (skip weekend OK)

### File Names (Fixed)
- `daily-summary.md` (always same name)
- `case-study-report.md` (always same name)
- `slide-deck-outline.md` (always same name)
- `executive-summary.md` (always same name)

**Why fixed names?**
- Easy to find
- Easy to automate
- Consistent structure

---

## 📈 Growth Pattern

### Daily Growth
```
Day 01: 4 files (~350 KB)
Day 02: 4 files (~50 KB)   # Typically smaller (just daily work)
Day 03: 4 files (~50 KB)
...
Day 30: 4 files (~50 KB)
```

### Monthly Growth
```
Month 1: ~30 days × 4 files = ~120 files (~2 MB)
Month 2: ~30 days × 4 files = ~120 files (~2 MB)
...
```

### Archive Strategy
- Keep current month + last 2 months in main folder
- Move older months to `archive/` folder
- Compress archived months (optional)

---

## 🔄 Daily Workflow

### Morning (5 phút)
1. Create new folder: `YYYY-MM-DD_Day-XX`
2. Copy template: `templates/daily-summary-template.md`
3. Rename to: `daily-summary.md`
4. Fill "Today's Goals"

### During Day
- Update "Completed Tasks" as you go
- Note "Key Decisions" when made
- Log "Blockers/Issues" immediately

### End of Day (10-15 phút)
1. Complete "Metrics"
2. Write "Lessons Learned"
3. Plan "Tomorrow's Plan"
4. Review and finalize
5. Commit to git (optional)

---

## 🛠️ Automation Ideas

### 1. Auto-create Daily Folder
```bash
#!/bin/bash
# create-daily-folder.sh

DATE=$(date +%Y-%m-%d)
MONTH=$(date +%Y-%m)
DAY_NUM=$1  # Pass as argument: ./create-daily-folder.sh 02

FOLDER="AI-DLC Daily Summary Report/${MONTH}/${DATE}_Day-${DAY_NUM}"
mkdir -p "$FOLDER"

# Copy template
cp "AI-DLC Daily Summary Report/templates/daily-summary-template.md" \
   "$FOLDER/daily-summary.md"

# Replace placeholders
sed -i "s/YYYY-MM-DD/$DATE/g" "$FOLDER/daily-summary.md"
sed -i "s/Day XX/Day $DAY_NUM/g" "$FOLDER/daily-summary.md"

echo "✅ Created: $FOLDER"
```

### 2. Generate Monthly Summary
```bash
#!/bin/bash
# generate-monthly-summary.sh

MONTH=$(date +%Y-%m)
FOLDER="AI-DLC Daily Summary Report/${MONTH}"

# Aggregate all daily summaries
cat "$FOLDER"/*/daily-summary.md > "$FOLDER/monthly-summary-raw.md"

# TODO: Parse and summarize
echo "✅ Generated monthly summary for $MONTH"
```

### 3. Search Across Reports
```bash
#!/bin/bash
# search-reports.sh

KEYWORD=$1
grep -r "$KEYWORD" "AI-DLC Daily Summary Report/" \
  --include="*.md" \
  --exclude-dir="templates"
```

---

## 📊 Statistics (Current)

### Day 01 (2026-03-21)
- **Files**: 4 files
- **Total Size**: ~350 KB
- **Work Hours**: 7.5 hours
- **Artifacts Created**: 35 documents
- **User Interactions**: 34 queries
- **Phases Completed**: Inception (100%) + Construction Unit 1 (80%)

### Templates
- **Files**: 1 template (daily-summary)
- **TODO**: 3 templates (case-study, slide-deck, executive-summary)

---

## 🎯 Best Practices

### DO's
- ✅ Write daily summary every day (consistency)
- ✅ Be honest about blockers
- ✅ Track time spent (helps estimation)
- ✅ Link to artifacts (aidlc-docs/, code)
- ✅ Write lessons learned while fresh
- ✅ Plan tomorrow before leaving

### DON'Ts
- ❌ Don't skip daily summary (even when busy)
- ❌ Don't write too long (keep concise)
- ❌ Don't just list tasks (explain outcomes)
- ❌ Don't forget to commit (if using git)

---

## 🔗 Related Folders

### AI-DLC Workflow Artifacts
- `aidlc-docs/inception/` — Inception phase artifacts
- `aidlc-docs/construction/` — Construction phase artifacts
- `aidlc-docs/operations/` — Operations phase (future)
- `aidlc-docs/audit.md` — Complete interaction history
- `aidlc-docs/SESSION-STATE.md` — Resume guide

### Project Code
- `src/` — Application code (future)
- `tests/` — Test code (future)
- `docs/` — Additional documentation (future)

---

## 📝 TODO

### Templates to Create
- [ ] `case-study-template.md`
- [ ] `slide-deck-template.md`
- [ ] `executive-summary-template.md`

### Automation Scripts
- [ ] `create-daily-folder.sh`
- [ ] `generate-monthly-summary.sh`
- [ ] `search-reports.sh`
- [ ] `archive-old-months.sh`

### Documentation
- [ ] Add examples for each report type
- [ ] Create video tutorial (optional)
- [ ] Document git workflow (optional)

---

**Maintained By**: [Your Name]  
**Last Updated**: 2026-03-21  
**Version**: 1.0
