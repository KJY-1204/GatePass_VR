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
- [x] 컨트롤러 모델 대신 VR 손 모델(PolyOne "Free VR Hands") 표시. Owner: 김씨. — 공용 Rig 프리팹(`Complete XR Origin Set Up Hands Variant`)의 `Left/Right Controller Visual` 하위 `UniversalController`(플라스틱 컨트롤러 메쉬)를 비활성화하고, 그 자리에 손 모델 본(`J_Left`/`J_Right`)을 재배치. 프리팹 공용 수정이라 `SampleScene`/`TestMap_Quest` 양쪽에 자동 반영됨(확인 완료). **미검증**: 실제 헤드셋에서 손 방향(회전값)이 자연스럽게 보이는지는 사용자가 실기기로 확인 필요.
  - **2026-09-11 추가 수정 (미검증)**: 사용자가 실제 Quest 화면에서 손이 전혀 안 보인다고 보고. `SM_HandsVR_Male`(SkinnedMeshRenderer)의 `updateWhenOffscreen`이 꺼져 있었는데, 이 렌더러는 정적 컨테이너(`VR Hands Visual`, `Camera Offset` 바로 밑)에 있고 실제로 손을 구부리는 본(`J_Left`/`J_Right`)은 별도 계층(`Left/Right Controller`)에 있어 렌더러 자기 위치와 본 위치가 다르다. `CameraYOffset=1.36`이 Editor(트래킹 없음)에서는 `Camera Offset`에 그대로 적용돼 렌더러와 본이 우연히 비슷한 높이에 있었지만, 실기기(Floor 트래킹)에서는 이 오프셋이 0으로 리셋되며 렌더러 컨테이너가 바닥 높이로 내려가 본과 크게 벌어질 가능성이 큼 — `updateWhenOffscreen=false`면 한 번 화면 밖으로 판정된 뒤 바운즈가 갱신을 멈춰 계속 안 보이는 현상이 생길 수 있음. `updateWhenOffscreen=true`로 수정. Play Mode에서 렌더러 컨테이너를 강제로 1.36m 아래로 옮겨도 본 위치에서 손이 정상 렌더링되는 것은 확인했으나, 이 특정 "한 번 오프스크린 판정되면 고정" 버그 자체를 Editor에서 완전히 재현하지는 못함 — **실제 Quest 헤드셋 재확인 필요**.
  - **2026-09-11 추가 수정 2 (미검증)**: 손은 보이는데 "오른손이 왼손처럼 보이고, 손이 아래로, 손등은 오른쪽으로" 향한다고 보고. 사용자가 알려준 목표 자세(손가락→UI창 방향, 손등→위)를 기준으로, 실측(자식 본의 실제 월드 포지션)으로 각 손의 로컬 축을 구해서 `J_Left.localRotation=(90,180,0)`, `J_Right.localRotation=(84.5629,71.5030,78.3345)`로 재계산해 프리팹에 반영함(자세한 계산 과정은 `context-notes.md` 참고). Editor에서 실측 좌표와 스크린샷(정확히 위/특정 각도에서 촬영)으로 양손 다 손등-위·손가락-앞·엄지 마주보는 정상 대칭 쌍이 되는 것을 확인함. **실제 Quest 헤드셋 재확인 필요**.
  - **2026-09-11 추가 수정 3 (미검증, 확신도 낮음)**: 실기기 테스트 결과 위 수정 2가 틀렸음 — "오른손은 뒤집어야 하고, 이동/회전 시 손목이 꺾인다"고 재보고. `J_Right`의 손등/손바닥 축 판단을 뒤집어서 `J_Right.localRotation=(275.44, 251.50, 281.67)`로 재조정(손가락 축 기준 180도 롤). **손목 꺾임 현상은 원인 미확인** — Point & Hold 텔레포트 직후에만 생기는지, 항상 생기는지 구분 필요(다음에 사용자에게 재질문할 것). 이 리그가 팔뚝/팔꿈치 관절 없이 컨트롤러에 손을 직결한 구조라 근본적 한계일 가능성도 있음.
  - **2026-09-11 롤백 (미검증)**: 손목 꺾임을 "고무줄처럼 늘어남"이라고 구체적으로 설명함 — 바인드 포즈에서 너무 먼 회전값이 원인일 가능성이 높다고 판단. 사용자가 "방향 정확도보다 자연스러운 움직임"을 우선한다고 답해서, `J_Left`/`J_Right`를 이번 세션 이전 원래 값(`J_Left=(0.06,86.87,179.76)`, `J_Right=(359.94,93.13,359.76)`)으로 **완전히 롤백**함. "오른손이 왼손처럼 보이는" 원래 방향 문제는 아직 미해결 상태로 남아있음 — 다음엔 원래 값 근처에서 최소 변화로만 조정하는 방식으로 재시도할 것(자세한 교훈은 `context-notes.md` 참고).
  - **2026-09-11 최소 변경 시도 (미검증)**: 롤백 후 원래 문제("손가락 아래, 손등 오른쪽, 양손이 왼손 같음")가 그대로 재현됨을 사용자가 확인. `J_Left`/`J_Right`의 Z축이 정확히 180도 차이난다는 걸 발견(`179.76` vs `359.76`)하고, X/Y는 그대로 두고 `J_Right`의 Z만 180도 추가(`→179.76`)하는 최소 변경만 시도함. **실기기 확인 필요**.
  - **2026-09-11 근본 원인 수정 (미검증, 확신도 높음)**: 회전값만 바꿔서는 "양손이 왼손"인 문제가 절대 안 풀리는 이유를 찾음 — `Right Controller Visual`의 `-1` X 미러 스케일이 이미 스스로 올바르게 미러링된 오른손 메쉬 데이터를 **또 한 번 미러링해서 원래(왼손과 같은 모양)로 되돌리고 있었음**(이중 미러 = 미러 없음). `J_Right.localScale=(-1,1,1)`로 부모 미러를 상쇄(`lossyScale=(1,1,1)`)하고, 손가락 구부림 방향 실측(스크린샷 대신)으로 손등/손바닥 축을 다시 확인해서 `J_Right.localRotation=(275.44,71.50,101.67)`, `J_Left.localRotation=(90,180,0)`으로 설정. 실측 결과 양손 손가락 방향 정확히 일치, 엄지 방향 정확히 반대(진짜 미러 쌍) 확인, 스크린샷도 일치. 자세한 계산은 `context-notes.md` 참고. **실기기 확인 필요** — 특히 고무줄 현상이 이번엔 재발하지 않는지.
  - **2026-09-11 손목 고무줄 현상은 별개 원인 (미검증)**: 방향/모양은 고쳐졌지만 손목 늘어남은 그대로 재현됨 — 회전/미러와 무관한, 메쉬 스키닝(정점 절반 이상이 다중 본 블렌딩) 한계로 판단. 사용자 제안대로 `Left/Right Controller`(리지드) 밑에 `LeftWristCap`/`RightWristCap`(단순 Sphere, 손 메쉬와 동일 머티리얼)을 붙여서 손목 이음새를 가림. **실기기 확인 필요**.
  - **2026-09-11 캡을 구→캡슐(팔뚝 스텁)로 확장 (미검증)**: 구가 너무 작았다는 사용자 피드백("회전하면 손목이 늘어나고 손목을 자연스럽게 없앨 수는 없을까")에 따라, 캡을 더 크고 뒤로 뻗는 캡슐 형태(`localScale=(0.06,0.09,0.06)`, 손목 안쪽까지 겹치도록 위치 조정)로 교체. 작업 중 재질이 조용히 기본값으로 되돌아가는 버그를 발견해 재적용함(자세한 내용은 `context-notes.md`). 씬 파일에 기능적으로 무해한 낡은 오버라이드 4줄이 남아있는데 정상임(YAML 직접 수정 안 함). **실기기 확인 필요**.
  - **2026-09-11 팔뚝 스텁 제거 (사용자 지시)**: 손목 늘어남 문제를 더 쫓지 않고 캡슐 스텁을 완전히 삭제함. **손목 늘어남 문제는 미해결 상태로 남음.**
- [x] 손가락 구부림(그립) 애니메이션 구현 (`FingerCurlAnimator` + `HandGripInputDriver`). Owner: 김씨. — PolyOne 손 에셋에 그립 애니메이션이 없어서 절차적으로 대체: 손가락 관절(5개 손가락 × 4관절 × 양손 = 40개)을 로컬 X축으로 굽혀 펴짐↔주먹 사이를 보간. 컨트롤러의 Grip/Trigger 입력값(둘 중 큰 값)으로 구동. Play Mode에서 `SetCurl(0)/(0.5)/(1)` 직접 호출로 시각 확인(스크린샷) — 펴짐/절반 굽힘/완전 주먹 전부 자연스럽게 보임. **미검증**: 실제 컨트롤러 Grip/Trigger를 눌렀을 때 실기기에서 확인 필요.

## Phase B. Reusable Interaction and Guidance

- [ ] `ScenarioManager` 구현 (Step 진행, 중복 성공 방지, 전체/단계 Reset). Owner: 김씨.
- [ ] `ScenarioStep` 데이터 구조 정의. Owner: 김씨.
- [x] `GuideManager` 구현 (안내 텍스트/음성, 재안내 타이밍 5초/10초 규칙). Owner: 김씨. — `GuideManager`(MonoBehaviour, 단일 진입점) + `GuideReguideTimer`(순수 C# 상태 로직, EditMode 테스트 5개 통과). `SetGuide(main, hint, voice)`/`ReportProgress()`/`ClearGuide()` API. 5초 무반응 시 `onNoProgressShort`(향후 HighlightController 연동용 이벤트만 노출), 10초 무반응 시 `onNoProgressLong` + 안내 음성 재생. `TestMap_Quest`에서 Play Mode로 실제 5초/10초 타이머 발동, 텍스트 갱신, ReportProgress 리셋까지 전부 검증 완료.
- [ ] `HighlightController` 구현. Owner: 김씨. — `GuideManager.onNoProgressShort` 이벤트에 연동하면 됨(이미 노출됨).
- [x] 목표 방향 화살표 (`GoalIndicatorArrow`) 구현. Owner: 김씨. — `RadialGaugeVisual`과 동일한 절차적 스프라이트 생성 패턴. 카메라 정면 기준 목표까지의 수평 방향(좌우)만 계산해서 화면 위 화살표 아이콘을 Z축 회전시킴. `SetGoal(Transform)`으로 목표 갱신, null이면 자동 숨김. `GuideHUD` Prefab에 포함되어 공용으로 재사용 가능. `TestMap_Quest`에서 `GuideManager.initialMainText`/`GoalIndicatorArrow.initialGoal`로 시작 상태 설정 + 패드 3개의 `onHoldCompleted`에 다음 목표 텍스트/화살표 갱신을 추가 연결(Point & Hold 완료 시 다음 목표로 자동 전환). Play Mode에서 이벤트를 코드로 직접 호출해 텍스트/화살표 전환 확인, EditMode 테스트 11개 통과. **실기기에서 실제 패드 조준으로 확인 필요**. 이 하드와이어링은 `ScenarioManager`/`ScenarioStep`이 생기면 Step 데이터 기반으로 교체해야 함.
  - **2026-09-11 개선**: 화살표를 단순 삼각형 → 몸통+화살촉 모양으로 변경. 위치를 패널 안 구석 → 패널 위쪽 바깥으로 이동. `GuideUIFollow.localOffset`을 눈높이 아래(`y=-0.1`)에서 위(`y=0.35`)로 올려서 안내 패널이 컨트롤러 조준 영역(정면~하단)을 덜 가리게 함. 공용 프리팹의 `Affordance Callouts Left/Right`(Grab/Turn 등 컨트롤러 버튼 안내 툴팁)를 비활성화. Play Mode 스크린샷으로 확인, EditMode 테스트 11개 통과. **실기기에서 실제 조준선이 안 가려지는지 확인 필요**.
- [ ] Placement Zone 기본 구조 구현. Owner: 이씨. 김씨 구조 검토.
- [ ] Hand-over 시스템 구현 (`HandOverZone`). Owner: 김씨.
- [ ] Scanner 기능 구현 (`ScannerZone`, 순서 검증). Owner: 김씨.
- [ ] `ResetController` 구현 (분실/오배치/진행불능 복구). Owner: 김씨.
- [ ] 공통 기능 Prefab 제작. Owner: 김씨.
- [x] 가이드 UI 구성과 표시 제어 (`GuideManager` 단일 진입점). Owner: 김씨. — `GuideHUD` Prefab(`Prefabs/UI/GuideHUD.prefab`): 카메라 추종 World Space Canvas(`GuideUIFollow`), 메인 안내 텍스트(크게)/보조 힌트 텍스트(약하게) 2단 구성 + 반투명 배경. `TestMap_Quest`에 인스턴스 배치해 실제 Play Mode에서 한글 렌더링/레이아웃 확인. **미해결**: 임시 한글 폰트(`MalgunGothic SDF`, DynamicOS)는 Windows 전용 — Quest/Android 빌드에서는 한글이 깨질 것. 최종 빌드 전 반드시 임베디드 한글 폰트(OFL 라이선스 등)로 교체 필요.
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
