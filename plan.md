# GatePass VR - Plan

## 무엇을 만드는가

초보자용 1인칭 공항 출입국 안내·체험 VR 시뮬레이션. Unity 6.3 LTS 기반,
Meta Quest 3 / Pico 4 Standalone 및 PC VR 대상. 체험 시간 약 10~15분.

핵심 사용자 경험 (판단 기준 문장):
> 어디로 가야 하는지 안다 → 무엇을 해야 하는지 안다 → 실제 여행에서 덜 당황한다.

## 왜 만드는가

실제 공항 재현이나 자유 탐험형 시뮬레이션이 아니라, 출입국 절차의 순서와
행동을 짧은 시간 안에 명확하게 학습시키는 것이 목적이다. 복잡한 NPC AI,
음성인식, 멀티플레이 등은 범위 밖이다 (CLAUDE.md §11 참고).

## 현재 상태 (2026-09-04 기준)

- Unity 6.3 LTS (6000.3.10f1) 프로젝트, VR Core 템플릿 기반 초기화 완료.
- Packages: XR Interaction Toolkit 3.5.1, XR Hands 1.8.1, OpenXR, URP 등 설치됨.
- Scenes: `SampleScene`, `BasicScene` (템플릿 기본 씬, 프로젝트 전용 씬 아님).
- `Assets/XR`, `Assets/XRI` 설정 존재 (XR 기본 러너/시뮬레이터 설정 완료 추정, 미검증).
- 커스텀 스크립트, `_GatePassVR` 폴더, ScenarioManager 등 핵심 아키텍처는
  아직 전혀 없음 — 이번 세션이 첫 구현 세션.
- Git: `main`/`develop` 브랜치로 GitHub Public 저장소 생성 및 초기 커밋 완료.

## 목표 마일스톤 순서

CLAUDE.md §20 Master Implementation Order를 따른다. 이번 세션은
**Phase A (Project and VR Foundation)** 부터 시작한다.

1. Phase A — 프로젝트/VR 기초 (폴더 구조, XR 확인, Ray Interaction, Point & Hold, Fade, Grab).
2. Phase B — 재사용 가능한 상호작용/안내 시스템 (Placement Zone, Hand-over, Scanner, Step/Guide Manager, Highlight, 공통 Prefab, 가이드 UI).
3. Phase C — 콘텐츠 조립 (공항 공간, 표지판, NPC 정적 배치, START~EXIT 연결).
4. Phase D — 검증/릴리스 (통합 테스트, 성능, 버그 수정, 최종 빌드).

## 성공 조건과 검증 방법

- Unity Console에 Compile Error 0개.
- 첫 수직 슬라이스: `Ray → Point & Hold → Fade 이동 → Step 성공 → Guide 갱신 → Reset 확인`
  이 최소 테스트 Scene에서 실제로 동작.
- 이후 상호작용 슬라이스: `Grab → Hand-over/Place/Scan → Step 성공 → Guide 갱신 → Reset 확인`.
- 각 기능은 CLAUDE.md §27 Definition of Done을 만족해야 완료로 표시.
- 자동 테스트가 가능한 로직(Step 진행, 중복 성공 방지, Reset 복구 등)은
  EditMode 테스트로 검증하고, VR 상호작용은 수동 Smoke Test로 검증한다.

## 범위 제외 (재확인)

NPC Animation, 실제 공항 1:1 재현, 자유 전체 탐험, 복잡한 NPC AI, 음성인식
입국 인터뷰, 멀티플레이/온라인, 실제 항공편 연동, 수하물 무게 게임, 위험물
탐지 게임, 여권 위조 판별, 세관 신고 시뮬레이션, 비행 시뮬레이션, 대규모
군중 NPC.

## 현재 작업 (2026-11-23) — ResetController와 Point & Hold 재사용 버그 수정

context-notes 2026-11-23에 기록한 버그 3건을 고친다. ScenarioManager는 이번
범위가 아니다(사용자 결정).

완료 기준 (테스트 가능한 문장):

1. 완료된 `HoldGaugeState`를 `Reset()`하면 다시 채워서 완료 이벤트를 한 번 더
   낼 수 있다 → EditMode 테스트.
2. `PointAndHoldTarget.ResetHold()` 뒤에도 Ray가 계속 가리키고 있으면 게이지가
   다시 찬다(hover 상태 유지) → TestMap_Quest Play 확인.
3. `TestMap_Quest`에서 Pad 3개를 한 바퀴 돈 뒤 Pad_ToGrabZone을 다시 가리키면
   다시 이동한다(이동 완료 시 Pad 재활성화) → 시뮬레이터 Play 확인.
4. `ResetController.ResetCurrent()`가 등록된 Pad 게이지, Grab 물건 위치(잡고
   있어도 놓게 하고 원위치), PlacementZone 판정을 한 번에 되돌린다 → Play 확인.
5. 컨트롤러 B/Y(secondaryButton)로 `ResetCurrent()`가 실행된다 → 입력 시뮬레이션
   Play 확인. 실기기 확인은 미검증으로 남긴다.
6. Compile Error 0개, 기존 EditMode 테스트 포함 전체 통과.

## 현재 작업 (2026-11-23) — ScenarioManager·ScenarioStep (Phase B, M1 선행)

상호작용은 성공 사실과 ID만 알리고 진행 순서는 ScenarioManager가 정한다(§16.2).
TestMap_Quest Pad에 직접 연결된 MoveTo·SetMainText·SetGoal을 Step 데이터로 옮긴다.

구조:
- `ScenarioProgress` (순수 C#): 성공 ID 목록, 현재 인덱스, 맞는 ID만 1회 진행, 끝 처리, 처음부터 다시.
- `ScenarioStep` (Serializable): stepId, successId, 안내 메인·힌트·음성, 화살표 목표, 성공 시 이동 목적지(선택).
- `ScenarioManager`: Step 진입 시 `GuideManager.SetGuide` + `GoalIndicatorArrow.SetGoal` + onStepEntered,
  성공 시 이동 목적지가 있으면 `FadeMoveController.MoveTo` 후 다음 Step. `ReportSuccess(string)`,
  `ReportHoldCompleted(PointAndHoldTarget)`, `ReportPlaced(PlacementZone)` 진입점. `ResetStep()`은 현재 Step
  재진입(끝났으면 첫 Step부터).

완료 기준:
1. 현재 Step의 ID가 아닌 성공 보고는 무시된다 → EditMode 테스트 + Play(다른 Pad Hold 시 이동 없음).
2. 같은 성공이 두 번 와도 한 Step만 진행된다 → EditMode 테스트.
3. 마지막 Step 뒤에는 보고가 무시되고 완료 이벤트가 1회 난다 → EditMode 테스트.
4. TestMap_Quest가 Step 데이터만으로 기존과 같은 순서·문구·화살표·이동을 한다 → 시뮬레이터 Play.
5. B 버튼 Reset 시 현재 Step 안내가 다시 표시되고, 완료 후 Reset 시 첫 Step부터 다시 시작한다 → Play.
6. Compile Error 0, EditMode 전체 통과.
