# GatePass VR - Checklist

체크 표시는 실제로 Unity에서 검증된 항목에만 한다. 추측으로 체크하지 않는다.
각 항목의 담당자는 CLAUDE.md §18 Team Ownership을 따른다.

## Phase A. Project and VR Foundation

- [x] Unity 프로젝트 생성 (6000.3.10f1, VR Core 템플릿). Owner: 김씨.
- [x] Git 저장소 생성, `.gitignore` 설정, GitHub Public 저장소(`GatePass_VR`) 연결. Owner: 김씨.
- [x] `main`/`develop` 브랜치 구성. Owner: 김씨.
- [x] `Assets/_GatePassVR` 공통 폴더 구조 생성 (CLAUDE.md §15). Owner: 김씨.
- [x] XR/VR 기본 설정 실기기 기본 실행 확인. Owner: 김씨. — **퀘스트 3S 컨트롤러로 실기기 확인 완료 (2026-09-04)**. `TestMap_Quest` 씬에서 정상 실행됨.
- [x] Controller Input과 Ray Interaction 동작 확인. Owner: 김씨. — **퀘스트 3S 컨트롤러로 실기기 확인 완료**. 패드 조준/홀드 정상 동작.
- [x] 퀘스트 실기기 테스트용 `TestMap_Quest.unity` 제작. Owner: 김씨. — 실제 XR Origin Prefab(`Complete XR Origin Set Up Hands Variant`) 재사용, Point & Hold 이동 패드 3개(왕복 루프: Start→GrabZone→OpenArea→Start), Grab 테스트용 테이블+오브젝트 2개 구성. Build Settings에 등록. **퀘스트 3S 컨트롤러로 실기기 테스트 완료** — 패드 조준/홀드, Grab, 이동 전부 정상 동작 확인.
- [x] Point & Hold 게이지 컴포넌트 구현 (`PointAndHoldTarget` + `HoldGaugeState`). Owner: 김씨. — 퀘스트 실기기 검증 완료.
- [x] Fade 화면 전환 구현 (`FadeMoveController`). Owner: 김씨. — 퀘스트 실기기 검증 완료.
- [x] Grab 시스템 확인 (XR Interaction Toolkit 기존 기능 활용). Owner: 김씨. — **퀘스트 3S 컨트롤러로 실기기 확인 완료**. Cube/Sphere 모두 정상적으로 잡힘.
- [x] Point & Hold 진행률 시각 피드백을 2D 도넛(Radial) 게이지로 교체 (`RadialGaugeVisual`), `onHoldCompleted` → `FadeMoveController.MoveTo` 연결 유지. Owner: 김씨. — 사용자 실기기 테스트 피드백 반영: 큐브 색상 변화 방식(`HoldProgressVisual`) 대신 시계방향으로 차오르는 도넛형 Radial360 UI로 교체. `TestMap_Quest`의 패드 3개 모두 적용. Play Mode 시뮬레이션으로 fillAmount/방향/도넛 렌더링 확인, 실제 손 입력 재확인은 다음 실기기 테스트에서.
- [x] 컨트롤러 모델 대신 VR 손 모델(PolyOne "Free VR Hands") 표시. Owner: 김씨. — 공용 Rig 프리팹(`Complete XR Origin Set Up Hands Variant`)의 `Left/Right Controller Visual` 하위 `UniversalController`(플라스틱 컨트롤러 메쉬)를 비활성화하고, 그 자리에 손 모델 본(`J_Left`/`J_Right`)을 재배치. 프리팹 공용 수정이라 `SampleScene`/`TestMap_Quest` 양쪽에 자동 반영됨(확인 완료). **미검증**: 실제 헤드셋에서 손 방향(회전값)이 자연스럽게 보이는지, 그립 애니메이션 없이 정적 포즈만으로 괜찮은지는 사용자가 실기기로 확인 필요.

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
