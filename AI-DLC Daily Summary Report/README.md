# AI-DLC Daily Summary Report

Folder này chứa các báo cáo tổng hợp công việc hàng ngày khi áp dụng AI-DLC workflow.

---

## 📁 Cấu trúc Folder

```
AI-DLC Daily Summary Report/
├── README.md                           # File này - hướng dẫn sử dụng
├── 2026-03/                            # Organize theo tháng (YYYY-MM)
│   ├── 2026-03-21_Day-01/              # Mỗi ngày 1 folder (YYYY-MM-DD_Day-XX)
│   │   ├── daily-summary.md            # Tóm tắt công việc ngày (quick overview)
│   │   ├── case-study-report.md        # Báo cáo chi tiết (comprehensive)
│   │   ├── slide-deck-outline.md       # Slide deck cho presentation
│   │   ├── executive-summary.md        # Executive summary (1-pager)
│   │   ├── screenshots/                # Screenshots nếu có
│   │   └── attachments/                # Files đính kèm khác
│   ├── 2026-03-22_Day-02/
│   │   └── ...
│   └── monthly-summary.md              # Tổng kết tháng (end of month)
├── templates/                          # Templates cho các loại reports
│   ├── daily-summary-template.md
│   ├── case-study-template.md
│   ├── slide-deck-template.md
│   └── executive-summary-template.md
└── archive/                            # Archive các tháng cũ (optional)
    └── 2026-02/
```

---

## 📝 Các Loại Reports

### 1. Daily Summary (Bắt buộc mỗi ngày)
**File**: `daily-summary.md`  
**Mục đích**: Quick overview công việc trong ngày  
**Thời gian**: 5-10 phút để viết  
**Nội dung**:
- Ngày làm việc
- Thời gian làm việc (start - end)
- Tasks completed
- Key decisions made
- Blockers/Issues
- Next day plan

### 2. Case Study Report (Khi hoàn thành milestone)
**File**: `case-study-report.md`  
**Mục đích**: Comprehensive report cho learning và sharing  
**Thời gian**: 30-60 phút để viết  
**Nội dung**:
- Detailed process walkthrough
- Technical decisions & trade-offs
- Lessons learned
- DO's and DON'Ts
- Metrics & outcomes

### 3. Slide Deck Outline (Khi cần present)
**File**: `slide-deck-outline.md`  
**Mục đích**: Presentation cho team/stakeholders  
**Thời gian**: 20-30 phút để viết  
**Nội dung**:
- Slide structure với speaker notes
- Key points per slide
- Visual suggestions
- Q&A preparation

### 4. Executive Summary (Khi cần báo cáo management)
**File**: `executive-summary.md`  
**Mục đích**: 1-pager cho management/executives  
**Thời gian**: 10-15 phút để viết  
**Nội dung**:
- Key metrics
- Major achievements
- Business impact
- Next steps

---

## 🗓️ Naming Convention

### Folder Names
- **Format**: `YYYY-MM-DD_Day-XX`
- **Examples**: 
  - `2026-03-21_Day-01`
  - `2026-03-22_Day-02`
  - `2026-03-25_Day-03` (nếu skip weekend)

### File Names
- **Daily Summary**: `daily-summary.md` (fixed name)
- **Case Study**: `case-study-report.md` (fixed name)
- **Slide Deck**: `slide-deck-outline.md` (fixed name)
- **Executive Summary**: `executive-summary.md` (fixed name)

**Lý do dùng fixed names**: Dễ automation, scripting, và tìm kiếm

---

## 📊 Daily Summary Template

```markdown
# Daily Summary — YYYY-MM-DD (Day XX)

**Date**: YYYY-MM-DD  
**Day**: Day XX  
**Work Hours**: HH:MM - HH:MM (X hours)  
**Phase**: [Inception/Construction/Operations]  
**Status**: [On Track/Blocked/Ahead]

---

## 🎯 Today's Goals
- [ ] Goal 1
- [ ] Goal 2
- [ ] Goal 3

## ✅ Completed Tasks
1. **Task 1** (X hours)
   - Description
   - Outcome
   - Artifacts: [links]

2. **Task 2** (X hours)
   - Description
   - Outcome
   - Artifacts: [links]

## 🔑 Key Decisions
- Decision 1: [rationale]
- Decision 2: [rationale]

## 💡 Lessons Learned
- Lesson 1
- Lesson 2

## ⚠️ Blockers/Issues
- Issue 1: [description] → [resolution/status]
- Issue 2: [description] → [resolution/status]

## 📈 Metrics
- Time spent: X hours
- Artifacts created: X files
- Code generated: X LOC (if applicable)
- Tests written: X tests (if applicable)

## 🚀 Tomorrow's Plan
- [ ] Task 1
- [ ] Task 2
- [ ] Task 3

---

**Notes**: [Any additional notes]
```

---

## 📅 Monthly Summary Template

```markdown
# Monthly Summary — YYYY-MM

**Month**: YYYY-MM  
**Total Work Days**: XX days  
**Total Hours**: XX hours  
**Phase Progress**: [summary]

---

## 📊 Overview
- Total tasks completed: XX
- Total artifacts created: XX
- Major milestones: [list]

## 🏆 Key Achievements
1. Achievement 1
2. Achievement 2
3. Achievement 3

## 📈 Metrics Summary
| Metric | Value |
|---|---|
| Work days | XX |
| Total hours | XX |
| Artifacts | XX |
| Code generated | XX LOC |
| Tests written | XX |

## 💡 Top Lessons Learned
1. Lesson 1
2. Lesson 2
3. Lesson 3

## 🎯 Next Month Goals
- [ ] Goal 1
- [ ] Goal 2
- [ ] Goal 3

---

**Generated**: YYYY-MM-DD
```

---

## 🔄 Workflow

### Daily Workflow
1. **Morning** (5 phút):
   - Tạo folder mới cho ngày hôm nay (nếu chưa có)
   - Copy template `daily-summary.md`
   - Fill "Today's Goals"

2. **During Day**:
   - Update "Completed Tasks" khi hoàn thành
   - Note "Key Decisions" khi có
   - Log "Blockers/Issues" khi gặp

3. **End of Day** (10 phút):
   - Complete "Metrics"
   - Write "Lessons Learned"
   - Plan "Tomorrow's Plan"
   - Review và finalize

### Weekly Workflow (Optional)
- Review 5 daily summaries
- Identify patterns/trends
- Update team on progress

### Monthly Workflow
- Generate monthly summary từ daily summaries
- Archive tháng cũ (optional)
- Plan next month

---

## 🛠️ Tools & Automation (Optional)

### Script Ideas
1. **Auto-create daily folder**:
   ```bash
   # create-daily-folder.sh
   DATE=$(date +%Y-%m-%d)
   DAY_NUM=$1  # Pass day number as argument
   FOLDER="AI-DLC Daily Summary Report/$(date +%Y-%m)/${DATE}_Day-${DAY_NUM}"
   mkdir -p "$FOLDER"
   cp templates/daily-summary-template.md "$FOLDER/daily-summary.md"
   ```

2. **Generate monthly summary**:
   ```bash
   # generate-monthly-summary.sh
   # Aggregate all daily summaries in current month
   ```

3. **Search across reports**:
   ```bash
   # search-reports.sh
   grep -r "keyword" "AI-DLC Daily Summary Report/"
   ```

---

## 📌 Best Practices

### DO's
- ✅ Write daily summary mỗi ngày (consistency is key)
- ✅ Be honest về blockers và issues
- ✅ Track time spent (helps với estimation)
- ✅ Link to artifacts (aidlc-docs/, code files)
- ✅ Write lessons learned while fresh

### DON'Ts
- ❌ Không skip daily summary (even khi busy)
- ❌ Không viết quá dài (keep concise)
- ❌ Không chỉ list tasks (explain outcomes)
- ❌ Không quên plan tomorrow (helps với focus)

---

## 📚 References

**Related Folders**:
- `aidlc-docs/` — AI-DLC workflow artifacts
- `aidlc-docs/audit.md` — Complete interaction history
- `aidlc-docs/SESSION-STATE.md` — Resume guide

**External Resources**:
- AI-DLC Rules: `.kiro/aws-aidlc-rule-details/`

---

**Last Updated**: 2026-03-21  
**Maintained By**: [Your Name]
