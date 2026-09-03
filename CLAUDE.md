# CLAUDE.md

Behavioral guidelines to reduce common LLM coding mistakes. Merge with project-specific instructions as needed.

**Tradeoff:** These guidelines bias toward caution over speed. For trivial tasks, use judgment.

## 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

## 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

## 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

## 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.

## 5. No Closing Colons (Korean Output)

**End Korean sentences with a period, not a colon.**

When the user writes in Korean, your output is also Korean:
- Don't end sentences with `:` even if the next line is a list or example.
- LLMs trained on English docs leak the colon habit into Korean. Catch it.
- The test: every Korean sentence terminator should be `.`, `?`, or `!` — not `:`.
- Colons are fine inside code, key-value pairs, or labels. Not as sentence enders.

## 6. File Header Comments in Korean

**First line of every new source file: a one-line Korean comment stating its role.**

When creating a new file:
- TypeScript/JavaScript: `// 사용자 인증 상태를 관리하는 Context Provider`
- Python: `# KIS API 호출을 비동기로 래핑하는 클라이언트`
- SQL: `-- 일별 집계 결과를 저장하는 머티리얼라이즈드 뷰`
- Place it directly under required directives (`'use client'`, `'use server'`, shebang).
- Skip config files (`*.config.ts`, `package.json`, etc.).

Why: agents read files selectively, not whole codebases. A one-line Korean header gives instant context so the next session (human or agent) can navigate without re-reading the entire file.

## 7. Plan + Checklist + Context Notes

**Before any non-trivial task, produce three artifacts. Don't start coding without them.**

- **Plan** — what we're building and why.
- **Checklist** (`checklist.md`) — concrete tasks as checkboxes. Tick as you go.
- **Context Notes** (`context-notes.md`) — decisions made during the work and the reasoning behind them. Append continuously.

If the user gives only a plan and asks you to start coding, stop and ask: "Should I create the checklist and context notes first?" The next session — yours or someone else's — needs the notes to pick up where you left off without re-deriving every decision.

## 8. Run Tests Before Marking Complete

**If you touched code, run the tests before saying "done".**

- `npm test`, `pytest`, `cargo test`, whatever the project uses — run it.
- If tests pass, report results. If they fail, fix and re-run.
- No test setup? At minimum, verify the project builds/compiles.
- Run tests proactively, before the user signals "끝", "완료", "다 됐어" — not after.

This is the step LLMs skip most often. Treat it as non-negotiable.

## 9. Semantic Commits

**Commit when one logical change is complete. Don't wait for the user to ask.**

- The test: "Can I describe this commit in one sentence?" If yes, commit. If no, the changes are still mixed — split them.
- Good: "auth 미들웨어 추가". Bad: "auth 추가하고 UI도 고치고 버그도 수정" (split into 3).
- Don't accumulate 20 unrelated edits and lose the ability to roll back individually.
- Don't commit just to commit — meaningful units only.

Note: For solo prototypes or throwaway scripts, group commits loosely if it slows you down. The point is reversibility, not ceremony.

## 10. Read Errors, Don't Guess

**Read the actual error/log line. Don't pattern-match from memory.**

When something fails:
- Read the full error message and stack trace.
- Check the actual log output, not what you assume it should say.
- Don't apply a "common fix" before confirming the cause.
- If unclear, add a print/log to verify state — then fix.

This is the step LLMs skip most often after "run tests". They guess from error keywords and apply the most-recent-pattern fix. That's how a one-line bug becomes a three-file refactor.

---

**These guidelines are working if:** fewer unnecessary changes in diffs, fewer rewrites due to overcomplication, and clarifying questions come before implementation rather than after mistakes.

---

# GatePass VR Project Harness

이 절은 GatePass VR 프로젝트 전용 지침이다. 위의 공통 지침을 유지하면서 아래 규칙을 함께 적용한다.

## 11. Source of Truth and Priority

GatePass VR 작업에서 기준 문서의 우선순위는 다음과 같다.

1. 현재 사용자의 명시적 요청.
2. `GatePass VR 업무관리표 최종`의 최종 업무 범위, 담당자, 협업 규칙.
3. `GatePass_VR_세부개발기획서_초안(1).docx`의 기능 요구사항, 플레이 흐름, 일정, 완료 기준.
4. 이 `CLAUDE.md`의 공통 개발 지침.

문서 간 충돌이 있으면 상위 우선순위를 따른다. 특히 세부개발기획서에는 NPC Animation이 중요 기능으로 기재되어 있지만 최종 업무관리표에서는 NPC Animation을 개발 범위에서 제외하므로, GatePass VR v1.0에서는 NPC Animation을 구현하지 않는다. NPC는 정적인 안내·심사 역할과 상호작용 지점만 제공한다.

범위를 임의로 확장하지 않는다. 실제 공항 1:1 재현, 자유로운 전체 공항 탐험, 복잡한 NPC AI, 음성인식 입국 인터뷰, 멀티플레이, 온라인 기능, 실제 항공편 연동, 수하물 무게 게임, 위험물 탐지 게임, 여권 위조 판별, 세관 신고 시뮬레이션, 비행 시뮬레이션, 대규모 군중 NPC는 v1.0에서 제외한다.

## 12. Project Contract

프로젝트명은 `GatePass VR`이다.

- 엔진은 Unity 6.3 LTS를 사용한다.
- 플랫폼은 Meta Quest 3, Pico 4 계열 Standalone VR과 PC VR을 목표로 한다.
- 장르는 초보자용 1인칭 공항 출입국 안내·체험 시뮬레이션이다.
- 권장 체험 시간은 약 10~15분이다.
- 최종 완료 목표는 2026년 11월 20일이다.
- 2026년 10월 16일까지 START에서 EXIT까지 전체 흐름을 완주 가능하게 만드는 것을 우선한다.
- 2026년 11월은 신규 기능 추가보다 테스트, 최적화, 버그 수정, 빌드 안정화에 집중한다.

핵심 사용자 경험은 다음 한 문장으로 판단한다.

> 어디로 가야 하는지 안다 → 무엇을 해야 하는지 안다 → 실제 여행에서 덜 당황한다.

## 13. Non-Negotiable UX Rules

다음 규칙은 구현 편의 때문에 변경하지 않는다.

1. 자유 스틱 이동을 기본 이동 방식으로 사용하지 않는다.
2. 이동은 `Point & Hold → 게이지 충전 → Fade → 목적지 이동`을 기본으로 한다.
3. 한 번에 하나의 핵심 행동만 안내한다.
4. 잘못된 행동을 Game Over로 처리하지 않는다.
5. Highlight, 화살표, 반복 안내, 힌트, Reset으로 사용자를 다시 올바른 흐름으로 유도한다.
6. 각 단계에는 진행 불능 상태를 복구할 수 있는 Reset 또는 복구 수단이 있어야 한다.
7. 갑작스러운 카메라 회전이나 불필요한 강제 시점 변경을 사용하지 않는다.
8. 현실적인 복잡성보다 사용자가 출국·입국 순서를 이해하는 것을 우선한다.
9. 약 5초간 진행이 없으면 목표를 더 명확히 Highlight할 수 있어야 한다.
10. 약 10초 이상 진행하지 못하면 안내 음성 또는 힌트를 다시 제공할 수 있어야 한다.

## 14. Harness Engineering Execution Loop

GatePass VR의 모든 비단순 작업은 아래 루프로 진행한다. 코드를 먼저 쓰고 나중에 검증 계획을 만드는 방식을 금지한다.

### 14.1 작업 시작 전 산출물

프로젝트 루트에 아래 세 파일이 없으면 먼저 만든다.

- `plan.md` — 이번 작업에서 무엇을 만들고 왜 만드는지, 성공 조건과 검증 방법.
- `checklist.md` — 업무를 실제 실행 단위 체크박스로 분해한 목록.
- `context-notes.md` — 결정한 구조, 이유, 주의점, 다음 세션이 알아야 할 정보를 누적 기록.

기존 파일이 있으면 덮어쓰지 않고 갱신한다.

### 14.2 한 작업의 표준 루프

1. 현재 브랜치와 변경 파일을 확인한다.
2. 작업과 관련된 기존 코드, Prefab, Scene만 읽는다.
3. `checklist.md`에서 이번 작업 하나를 선택한다.
4. 완료 기준을 테스트 가능한 문장으로 다시 적는다.
5. 가장 작은 구현으로 기능을 만든다.
6. Unity 컴파일 오류를 0개로 만든다.
7. 가능한 경우 EditMode 또는 PlayMode 테스트를 실행한다.
8. VR 상호작용은 테스트 Scene 또는 실제 Scene에서 최소 1회 수동 Smoke Test한다.
9. 실패하면 실제 Console/로그를 읽고 원인을 수정한 뒤 같은 검증을 다시 실행한다.
10. 성공하면 `checklist.md`를 체크하고 `context-notes.md`에 중요한 결정만 기록한다.
11. 한 문장으로 설명 가능한 논리 단위로 Commit한다.
12. develop 병합 후 Unity를 다시 열어 컴파일과 핵심 실행을 재검증한다.

테스트를 실행하지 않았거나 Unity 컴파일을 확인하지 않았다면 완료라고 말하지 않는다. 테스트 환경이나 빌드 모듈이 없어 실행할 수 없는 검증은 `미검증`으로 명확히 기록한다.

### 14.3 수직 슬라이스 우선

시스템을 전부 만든 뒤 콘텐츠를 한꺼번에 붙이지 않는다. 다음처럼 작은 수직 슬라이스를 먼저 완성한다.

`Ray → Point & Hold → Fade 이동 → Step 성공 → Guide 갱신 → Reset 확인`

그 다음 상호작용 슬라이스를 완성한다.

`Grab → Hand-over 또는 Place/Scan → Step 성공 → Guide 갱신 → Reset 확인`

각 슬라이스가 실제 Scene에서 동작한 뒤 같은 패턴을 다른 단계로 확장한다.

## 15. Unity Project Structure

기존 프로젝트 구조가 이미 있으면 무리하게 재배치하지 않는다. 새 프로젝트를 초기화할 때는 아래 정도의 단순한 구조를 권장한다.

```text
Assets/
└─ _GatePassVR/
   ├─ Scenes/
   ├─ Scripts/
   │  ├─ Core/
   │  ├─ VR/
   │  ├─ Interaction/
   │  ├─ Guidance/
   │  ├─ UI/
   │  └─ Editor/
   ├─ Prefabs/
   │  ├─ VR/
   │  ├─ Interaction/
   │  ├─ UI/
   │  └─ Environment/
   ├─ Data/
   ├─ Audio/
   ├─ Art/
   │  └─ Temp/
   └─ Tests/
      ├─ EditMode/
      └─ PlayMode/
```

외부 Asset Store 에셋은 가능하면 `_GatePassVR` 밖의 원래 폴더를 유지하고 직접 수정하지 않는다. 필요한 경우 Wrapper Prefab을 만들어 프로젝트 전용 설정을 적용한다.

## 16. Core Architecture

세부개발기획서가 요구한 공통 Step 기반 구조를 사용한다. Scene마다 서로 다른 진행 코드를 복사해 만들지 않는다.

### 16.1 최소 핵심 구성

다음 역할을 최소 단위로 구현한다.

- `ScenarioManager` — 현재 Step, 성공 처리, 다음 Step, 전체 Reset, 단계 Reset을 관리한다.
- `ScenarioStep` 또는 동등한 Step 데이터 구조 — Guide Text, Guide Audio, Target, Interaction Type, Success Condition, Next Step을 가진다.
- `GuideManager` — 현재 단계의 안내 텍스트, 음성, 재안내 타이밍을 관리한다.
- `HighlightController` — 현재 목표 오브젝트를 시각적으로 강조한다.
- `PointAndHoldTarget` — Ray Hover 시간을 누적해 게이지를 채우고 성공 이벤트를 발생시킨다.
- `FadeMoveController` — Fade Out, XR Origin 이동, Fade In 순서를 담당한다.
- `PlacementZone` — 지정 물체가 올바른 위치에 놓였는지 판단한다.
- `HandOverZone` — 여권 등 지정 물체가 전달 지점에 들어오면 성공 처리한다.
- `ScannerZone` — Passport 또는 Boarding Pass의 스캔 성공을 처리한다.
- `ResetController` — 현재 단계에서 잃어버린 물체, 잘못된 배치, 진행 불능 상태를 복구한다.

기존 Unity/XR Interaction Toolkit 기능으로 해결되는 Grab, Ray, Socket 기능을 불필요하게 다시 구현하지 않는다. 프로젝트에 이미 사용 중인 XR 방식이 있으면 그것을 우선한다.

### 16.2 상호작용 성공 규칙

상호작용 컴포넌트는 다음 Step을 직접 결정하지 않는다. 상호작용은 자신의 성공 사실과 필요한 식별자만 `ScenarioManager`에 전달하고, 실제 진행 순서는 `ScenarioManager`가 판단한다.

같은 성공 이벤트가 중복 호출되어 Step이 두 번 넘어가지 않도록 현재 Step 검증과 1회 성공 보호를 둔다.

### 16.3 Inspector 중심 설정

후반 콘텐츠 적용 단계에서 코드를 복사하지 않고 Inspector 설정으로 진행 순서를 조정할 수 있어야 한다. 단, 지나치게 범용적인 노드 에디터나 자체 비주얼 스크립팅 시스템은 만들지 않는다.

## 17. Scene and Flow Contract

Scene은 성능과 편의에 따라 합칠 수 있지만 플레이 흐름의 논리 단계는 유지한다.

1. 공항 로비에서 체크인 카운터로 이동한다.
2. 여권을 집고 직원에게 건넨 뒤 수하물을 지정 위치에 올리고 탑승권을 확인한다.
3. 소지품을 Security Tray에 넣고 보안검색 구역을 통과한다.
4. 여권과 탑승권을 Scanner에 순서대로 인식한다.
5. 올바른 Gate를 확인하고 Point & Hold로 이동한다.
6. Fade와 안내방송으로 비행을 대체하고 현지 공항에 도착한다.
7. Immigration의 General Line으로 이동한다.
8. 여권을 심사관에게 전달하고 반환받는다. NPC Animation은 구현하지 않는다.
9. Baggage Claim을 지나 EXIT Point & Hold 성공 후 완료 화면으로 이동한다.

`START → STEP 1~9 → EXIT`의 전체 연속 완주가 프로젝트의 가장 중요한 통합 테스트다.

## 18. Team Ownership

업무 소유권은 최종 업무관리표를 따른다.

### 김씨

메인 VR·시스템·UI 개발과 프로젝트 통합을 담당한다.

- Unity 프로젝트 구조와 Git 기본 설정.
- XR/VR 기본 설정.
- Controller Input과 Ray Interaction.
- Point & Hold와 Fade 이동.
- Grab, Hand-over, Scanner.
- Step Manager와 Guide Manager.
- Highlight와 힌트 연결.
- 공통 기능 Prefab.
- 가이드 UI 구성과 표시 제어.
- 전체 Scenario 통합.
- Quest Build와 성능 점검.
- 주요 코드, VR, UI 버그 수정.
- 최종 Build와 기술 설정 문서화.

### 이씨

콘텐츠·에셋·서브 개발을 담당한다.

- 공항 기본 공간 구성.
- 에셋 조사, 정리, 배치.
- 표지판과 이동 포인트 배치.
- Placement Zone 기본 구조와 오브젝트 적용.
- Collider와 Trigger 설정.
- 안내 문구와 표지 문구 정리.
- 오디오 자료 정리와 적용.
- NPC 정적 배치와 상호작용 지점 설정.
- 단순 배치 버그 수정.
- 에셋과 테스트 결과 정리.
- 김씨 기능의 콘텐츠 적용과 교차 테스트.

### 조씨

고정 주담당이 아닌 예비 인력으로 운영한다.

- 반복 Play Test.
- 에셋과 오브젝트 정리 지원.
- Prefab과 기능 적용 보조.
- Collider와 Trigger 단순 수정 지원.
- 버그 재현과 검증.
- 실기기 테스트.
- 일정 지연이나 테스트 집중 시 추가 지원.

담당자가 아닌 사람이 같은 Scene 또는 Prefab을 동시에 수정하지 않는다. 반드시 소유자를 정한 뒤 작업한다.

## 19. Git Collaboration Harness

기본 흐름은 다음과 같다.

`Pull → 개인 브랜치 작업 → 검증 → Commit → Push → develop 병합 → Unity 실행 확인`

### 19.1 브랜치 예시

```text
develop
feature/kim-point-hold
feature/kim-guide-ui
feature/lee-airport-layout
feature/lee-placement-zone
fix/kim-step-reset
fix/lee-collider
```

브랜치 이름은 예시이며 기존 저장소 규칙이 있으면 기존 규칙을 따른다.

### 19.2 Scene과 Prefab 충돌 방지

- 같은 `.unity` Scene을 두 사람이 동시에 수정하지 않는다.
- 같은 핵심 Prefab을 두 사람이 동시에 수정하지 않는다.
- Scene 작업자는 작업 시작 전 담당 Scene을 명확히 알린다.
- 공통 Prefab 변경이 필요하면 김씨가 변경하거나 변경 목적을 먼저 공유한다.
- 머지 충돌을 억지로 텍스트 편집해 해결하지 않는다. Unity YAML 구조를 이해하지 못한 상태에서 Scene/Prefab 충돌을 수동 병합하는 것을 금지한다.
- 병합 후 누락된 Script, Missing Prefab, Missing Reference, Console Error를 확인한다.

## 20. Master Implementation Order

아래 순서는 기능 위험도를 먼저 줄이기 위한 기본 순서다. 업무관리표의 실제 상태와 담당자 일정이 더 최신이면 그 상태를 우선한다.

### Phase A. Project and VR Foundation

- [ ] Unity 프로젝트 생성과 공통 폴더 구조 확정. Owner: 김씨.
- [ ] Git 저장소, 브랜치, Unity용 `.gitignore` 확인. Owner: 김씨.
- [ ] XR/VR 기본 설정과 실기기 기본 실행 확인. Owner: 김씨.
- [ ] Controller Input과 Ray Interaction 확인. Owner: 김씨.
- [ ] Point & Hold 이동 시스템 구현. Owner: 김씨.
- [ ] Fade 화면 전환 구현. Owner: 김씨.
- [ ] Grab 시스템 확인. Owner: 김씨.

### Phase B. Reusable Interaction and Guidance

- [ ] Placement Zone 기본 구조 구현. Owner: 이씨. 김씨 구조 검토.
- [ ] Hand-over 시스템 구현. Owner: 김씨.
- [ ] Scanner 기능 구현. Owner: 김씨.
- [ ] Step Manager 구현. Owner: 김씨.
- [ ] Guide Manager 구현. Owner: 김씨.
- [ ] Highlight와 힌트 기능 구현. Owner: 김씨.
- [ ] 공통 기능 Prefab 제작. Owner: 김씨.
- [ ] 가이드 UI 구성과 표시 제어. Owner: 김씨.

### Phase C. Content Assembly

- [ ] 공항 기본 공간 구성. Owner: 이씨.
- [ ] 공항 에셋 목록과 라이선스/출처 정리. Owner: 이씨.
- [ ] 오브젝트, 표지판, 이동 포인트 배치. Owner: 이씨.
- [ ] 안내문과 표지 문구 확정. Owner: 이씨. 김씨 UI 반영.
- [ ] 안내 음성과 효과음 정리. Owner: 이씨. 김씨 시스템 연결.
- [ ] NPC 정적 배치와 상호작용 지점 설정. Owner: 이씨.
- [ ] 출국과 입국 흐름을 START부터 EXIT까지 연결. Owner: 김씨. 이씨 콘텐츠 적용.

### Phase D. Verification and Release

- [ ] 기능 단위 반복 테스트. Owner: 김씨. 이씨 교차 테스트. 조씨 지원.
- [ ] 전체 플레이 완주 테스트. Owner: 김씨·이씨·조씨 공동.
- [ ] Quest 성능과 Build 점검. Owner: 김씨. 이씨·조씨 실기기 테스트.
- [ ] 주요 코드, VR, UI 버그 수정. Owner: 김씨.
- [ ] 단순 배치, Collider, Trigger 버그 수정. Owner: 이씨.
- [ ] 기술 설정과 빌드 방법 문서화. Owner: 김씨.
- [ ] 에셋과 테스트 결과 정리. Owner: 이씨.
- [ ] GatePass VR v1.0 최종 Build와 백업. Owner: 김씨. 이씨·조씨 검증.

## 21. Milestone Gates

마일스톤은 날짜만 맞추는 것이 아니라 아래 기능 게이트를 통과해야 완료로 본다.

### M1. VR Core

Point & Hold, Grab, Place, Hand-over, Scanner, Guide, Step 진행이 동작한다.

### M2. Departure

공항 입장부터 탑승 Gate까지 출국 흐름을 플레이할 수 있다.

### M3. Full Flow

START에서 EXIT까지 출국과 입국 전체를 막힘 없이 완주할 수 있다.

### M4. Alpha

환경, 정적 NPC, Sound, UI가 제품 형태로 들어가고 주요 기능이 연결된다. NPC Animation은 요구하지 않는다.

### M5. Release Candidate

신규 기능 개발을 종료하고 치명적 버그 수정과 안정화 중심으로 전환한다.

### M6. v1.0

최종 Build, 프로젝트 백업, 재빌드 가능한 기본 문서가 존재한다.

## 22. Test Harness

테스트는 자동 검증과 VR 수동 검증을 함께 사용한다.

### 22.1 자동 검증 대상

가능한 범위에서 Unity Test Framework를 사용한다.

- Scenario가 올바른 Step에서만 성공하는지.
- 성공 이벤트가 중복 호출되어도 Step이 두 번 진행되지 않는지.
- Reset 후 현재 Step과 목표 상태가 복구되는지.
- Placement Zone이 잘못된 오브젝트를 성공 처리하지 않는지.
- Hand-over Zone이 지정 오브젝트만 성공 처리하는지.
- Scanner가 요구 순서를 지키는지.
- Guide가 Step 변경 시 올바른 텍스트로 갱신되는지.

MonoBehaviour에 강하게 결합된 기능을 테스트하기 위해 거대한 추상화 계층을 새로 만들지 않는다. 테스트 가능한 작은 상태 로직만 분리한다.

### 22.2 PlayMode Smoke Test

최소 Smoke Test Scene에서 아래 흐름을 확인한다.

1. Ray가 목표를 가리킨다.
2. Point & Hold 게이지가 정상 충전된다.
3. Hold를 중단하면 정책에 맞게 게이지가 초기화 또는 감소한다.
4. 성공 시 Fade가 시작된다.
5. 목적지 이동 후 Fade가 끝난다.
6. 다음 Guide가 표시된다.
7. Reset을 눌러도 진행 불능 상태가 남지 않는다.

### 22.3 전체 수동 Acceptance Test

최종적으로 다음을 모두 확인한다.

- 외부 도움 없이 STEP 1부터 STEP 9까지 완료할 수 있다.
- 자유 스틱 이동 없이 전체 체험이 가능하다.
- 현재 목적지와 다음 행동이 항상 명확하다.
- 여권, 탑승권, 짐 상호작용이 반복 테스트에서도 안정적이다.
- 잘못된 행동 후에도 진행 불능 상태가 되지 않는다.
- 각 단계 Reset 또는 복구 기능이 동작한다.
- Quest 3에서 심각한 프레임 저하가 없다.
- 화면 전환에 갑작스러운 카메라 회전이 없다.
- START부터 EXIT까지 치명적 오류 없이 연속 완주한다.

## 23. Unity Verification Commands

프로젝트에 CI 스크립트가 아직 없다면 먼저 Unity Editor 내부 테스트 실행을 사용한다. 자동화 환경이 준비되면 Unity 실행 경로를 하드코딩하지 말고 `UNITY_PATH` 같은 환경 변수로 받는다.

Windows PowerShell 예시는 다음과 같다.

```powershell
& $env:UNITY_PATH -batchmode -nographics -quit `
  -projectPath . `
  -runTests -testPlatform EditMode `
  -testResults TestResults/EditMode.xml `
  -logFile Logs/EditMode.log

& $env:UNITY_PATH -batchmode -nographics -quit `
  -projectPath . `
  -runTests -testPlatform PlayMode `
  -testResults TestResults/PlayMode.xml `
  -logFile Logs/PlayMode.log
```

명령이 실패하면 먼저 `Logs/*.log`의 실제 오류를 읽는다. 일반적인 Unity 오류를 추측해서 여러 파일을 동시에 고치지 않는다.

Android/Quest Build는 Android Build Support와 필요한 SDK가 설치된 환경에서만 검증한다. 해당 환경이 없으면 `Quest Build 미검증`이라고 기록하고 성공으로 간주하지 않는다.

## 24. Performance Rules for Standalone VR

성능 최적화는 기능이 완주 가능한 뒤 집중하되, 초기에 명백히 비싼 구조를 만들지는 않는다.

- 불필요한 실시간 조명과 고비용 Shadow를 피한다.
- 동일 오브젝트의 Material 복제를 남발하지 않는다.
- Update에서 매 프레임 Find 계열 검색을 하지 않는다.
- 안내와 상호작용 이벤트는 이벤트 기반으로 처리한다.
- Physics Layer와 Interaction Layer를 목적에 맞게 분리한다.
- Collider는 필요한 범위로 단순하게 유지한다.
- 공항 장식 오브젝트는 기능 완성 전까지 임시 Cube와 단순 Mesh를 허용한다.
- 실제 에셋 교체 후 Draw Call, Overdraw, 조명, Collider, 메모리 사용을 다시 확인한다.

특정 FPS 수치나 그래픽 품질을 근거 없이 새 요구사항으로 만들지 않는다. 최종 기준은 실제 Quest 3에서 심각한 프레임 저하 없이 10~15분 체험이 가능한지다.

## 25. UI Harness

UI는 김씨 담당이다. UI 구현은 다음 규칙을 지킨다.

- 화면당 핵심 안내 1개.
- 보조 조작 안내는 핵심 문구보다 시각적으로 약하게 표시.
- 작은 원거리 텍스트보다 큰 표지판과 명확한 아이콘을 우선.
- 현재 Step의 목표와 UI가 항상 일치하도록 `GuideManager`를 단일 진입점으로 사용.
- Scene Script가 UI Text를 직접 여기저기 수정하지 않도록 한다.
- Point & Hold Progress UI는 재사용 가능한 Prefab으로 만든다.
- UI 변경 후 VR 시야에서 실제 크기와 가독성을 확인한다.

## 26. Reset and Recovery Harness

Reset은 마지막에 붙이는 부가기능이 아니라 각 상호작용의 완료 조건에 포함한다.

각 Step을 구현할 때 최소한 다음을 답할 수 있어야 한다.

- 사용자가 필요한 물체를 떨어뜨리거나 멀리 던지면 어떻게 복구하는가.
- 잘못된 Zone에 놓으면 어떻게 다시 시도하는가.
- Step 성공 전에 다음 목표를 건드리면 어떻게 무시하는가.
- 성공 이벤트가 두 번 들어오면 어떻게 중복 진행을 막는가.
- Scene Reload 없이 현재 Step을 되돌릴 수 있는가.

Reset이 구현되지 않은 Step은 기능 완료로 체크하지 않는다.

## 27. Definition of Done

개별 업무는 아래 조건을 모두 만족해야 `완료`로 표시할 수 있다.

1. 업무관리표의 완료 기준을 충족한다.
2. Unity Console에 해당 변경으로 인한 Compile Error가 없다.
3. 관련 자동 테스트가 있으면 통과한다.
4. 자동 테스트가 없는 VR 상호작용은 수동 Smoke Test를 통과한다.
5. Reset 또는 실패 복구 경로가 필요한 기능이면 복구를 검증한다.
6. 변경한 Prefab과 Scene에 Missing Reference가 없다.
7. `checklist.md`와 `context-notes.md`가 현재 상태를 반영한다.
8. 한 논리 변경 단위로 Commit되어 있다.
9. develop 병합 후 다시 실행해 문제가 없는지 확인한다.

## 28. Agent Guardrails Specific to GatePass VR

- 사용자가 요구하지 않은 미니게임을 추가하지 않는다.
- 공항을 예쁘게 만드는 작업을 핵심 기능보다 먼저 하지 않는다.
- 임시 Cube와 Placeholder UI로 기능을 검증하는 것을 허용한다.
- Scene별 전용 진행 코드를 복사해 만들지 않는다.
- 한 번에 여러 시스템을 대규모로 리팩터링하지 않는다.
- 패키지 버전을 임의로 바꾸지 않는다.
- 기존 XR Rig 또는 XR Origin 구조가 있으면 먼저 읽고 존중한다.
- 같은 기능이 이미 있으면 새로 만들기보다 기존 구현을 최소 수정한다.
- 에셋 라이선스가 불명확하면 프로젝트에 포함하지 않고 후보 목록만 기록한다.
- 사용자가 제공하지 않은 유료 에셋 구매를 전제로 구현하지 않는다.
- 실제 기기 검증 없이 Quest 또는 Pico에서 동작한다고 단정하지 않는다.
- `완료`, `끝`, `정상 작동`이라고 쓰기 전에 테스트 결과 또는 실행 근거를 확인한다.

## 29. First Session Bootstrap

새 Unity 프로젝트에서 이 파일을 처음 읽은 에이전트는 다음 순서로 시작한다.

1. 현재 Unity 버전, 프로젝트 루트, Package 상태, Git 상태를 읽는다.
2. `plan.md`, `checklist.md`, `context-notes.md`를 생성하거나 기존 내용을 확인한다.
3. 업무관리표의 현재 상태를 체크리스트에 옮기되 이미 완료된 항목을 추측해서 체크하지 않는다.
4. Unity용 `.gitignore`, Visible Meta Files, Force Text 설정 여부를 확인한다.
5. XR 기본 실행과 Ray Interaction을 먼저 확인한다.
6. 최소 테스트 Scene에서 `Point & Hold → Fade 이동 → Step 진행 → Guide 변경 → Reset` 수직 슬라이스를 완성한다.
7. 검증이 끝난 뒤에만 체크인, 보안검색, 출국심사 등 실제 공항 콘텐츠를 붙이기 시작한다.

첫 세션의 목표는 공항 전체를 만드는 것이 아니다. 공통 시스템 하나가 재사용 가능한 형태로 실제 VR에서 동작하고, 다음 세션이 그대로 이어서 작업할 수 있는 하네스를 확보하는 것이다.

