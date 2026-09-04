# GatePass VR - Checklist

체크 표시는 실제로 Unity에서 검증된 항목에만 한다. 추측으로 체크하지 않는다.
각 항목의 담당자는 CLAUDE.md §18 Team Ownership을 따른다.

## Phase A. Project and VR Foundation

- [x] Unity 프로젝트 생성 (6000.3.10f1, VR Core 템플릿). Owner: 김씨.
- [x] Git 저장소 생성, `.gitignore` 설정, GitHub Public 저장소(`GatePass_VR`) 연결. Owner: 김씨.
- [x] `main`/`develop` 브랜치 구성. Owner: 김씨.
- [x] `Assets/_GatePassVR` 공통 폴더 구조 생성 (CLAUDE.md §15). Owner: 김씨.
- [ ] XR/VR 기본 설정 실기기(또는 시뮬레이터) 기본 실행 확인. Owner: 김씨. — 미검증.
- [ ] Controller Input과 Ray Interaction 동작 확인. Owner: 김씨. — 미검증.
- [x] Point & Hold 게이지 컴포넌트 구현 (`PointAndHoldTarget` + `HoldGaugeState`). Owner: 김씨.
- [x] Fade 화면 전환 구현 (`FadeMoveController`). Owner: 김씨.
- [x] Grab 시스템 확인 (XR Interaction Toolkit 기존 기능 활용). Owner: 김씨. — Play Mode에서 `XRInteractionManager` 실제 파이프라인으로 Grab/Release 확인. 단, Hand Tracking 미연결 상태에서는 Near-Far Interactor가 자동 비활성화되므로 실기기/시뮬레이터 연결 후 재확인 필요.
- [x] Point & Hold 진행률 시각 피드백 (`HoldProgressVisual`) + `onHoldCompleted` → `FadeMoveController.MoveTo` 연결. Owner: 김씨. — 퀘스트 실기기 테스트를 눈으로 확인 가능하게 하기 위한 사전 준비. SmokeTest 씬에서 Play Mode 시뮬레이션으로 전체 흐름(게이지 완료 → 초록색 → Fade → 이동) 확인 완료. 실제 손 추적 입력으로는 아직 미검증.

## Phase B. Reusable Interaction and Guidance

- [ ] `ScenarioManager` 구현 (Step 진행, 중복 성공 방지, 전체/단계 Reset). Owner: 김씨.
- [ ] `ScenarioStep` 데이터 구조 정의. Owner: 김씨.
- [ ] `GuideManager` 구현 (안내 텍스트/음성, 재안내 타이밍 5초/10초 규칙). Owner: 김씨.
- [ ] `HighlightController` 구현. Owner: 김씨.
- [ ] Placement Zone 기본 구조 구현. Owner: 이씨. 김씨 구조 검토.
- [ ] Hand-over 시스템 구현 (`HandOverZone`). Owner: 김씨.
- [ ] Scanner 기능 구현 (`ScannerZone`, 순서 검증). Owner: 김씨.
- [ ] `ResetController` 구현 (분실/오배치/진행불능 복구). Owner: 김씨.
- [ ] 공통 기능 Prefab 제작. Owner: 김씨.
- [ ] 가이드 UI 구성과 표시 제어 (`GuideManager` 단일 진입점). Owner: 김씨.
- [ ] Point & Hold Progress UI Prefab 제작. Owner: 김씨.

## Phase C. Content Assembly

- [ ] 공항 기본 공간 구성 (체크인 → 보안검색 → 게이트 → 입국심사 → EXIT). Owner: 이씨.
- [ ] 공항 에셋 목록과 라이선스/출처 정리. Owner: 이씨.
- [ ] 오브젝트, 표지판, 이동 포인트 배치. Owner: 이씨.
- [ ] 안내문/표지 문구 확정 및 UI 반영. Owner: 이씨 (문구), 김씨 (UI 반영).
- [ ] 안내 음성/효과음 정리 및 시스템 연결. Owner: 이씨 (자료), 김씨 (연결).
- [ ] NPC 정적 배치와 상호작용 지점 설정 (Animation 없음). Owner: 이씨.
- [ ] STEP 1~9 START→EXIT 연결. Owner: 김씨. 이씨 콘텐츠 적용.

## Phase D. Verification and Release

- [ ] 기능 단위 반복 테스트. Owner: 김씨(주도), 이씨(교차), 조씨(지원).
- [ ] 전체 START→EXIT 완주 테스트. Owner: 김씨·이씨·조씨 공동.
- [ ] Quest 성능/Build 점검. Owner: 김씨(주도), 이씨·조씨(실기기).
- [ ] 주요 코드/VR/UI 버그 수정. Owner: 김씨.
- [ ] 단순 배치/Collider/Trigger 버그 수정. Owner: 이씨.
- [ ] 기술 설정/빌드 방법 문서화. Owner: 김씨.
- [ ] 에셋/테스트 결과 정리. Owner: 이씨.
- [ ] GatePass VR v1.0 최종 Build와 백업. Owner: 김씨(주도), 이씨·조씨(검증).

## 마일스톤 게이트 (CLAUDE.md §21)

- [ ] M1. VR Core — Point & Hold, Grab, Place, Hand-over, Scanner, Guide, Step 진행 동작.
- [ ] M2. Departure — 공항 입장부터 탑승 Gate까지 출국 흐름 플레이 가능.
- [ ] M3. Full Flow — START→EXIT 전체 완주 (목표: 2026-10-16).
- [ ] M4. Alpha — 환경/정적 NPC/Sound/UI 통합, NPC Animation 불필요.
- [ ] M5. Release Candidate — 신규 기능 종료, 안정화 전환.
- [ ] M6. v1.0 — 최종 Build/백업/문서 (목표: 2026-11-20).
