# GatePass VR - Context Notes

결정 사항과 이유를 시간순으로 누적 기록한다. 완료된 내용을 삭제하지 않는다.

## 2026-09-04 — 퀘스트 실기기 테스트 사전 준비 (시각 피드백 + Fade 연결)

- **배경**: 퀘스트 3S가 충전 중이라 아직 연결 전. 연결하자마자 바로 의미
  있는 테스트를 할 수 있도록, 지금까지 "로직은 있지만 눈에 보이는 결과가
  없던" 두 지점을 메꿔둠.
- **결정**: `HoldProgressVisual`(신규, `Scripts/Interaction`)을 만들어
  `PointAndHoldTarget.onProgressChanged(float)`에 연결. 게이지 진행률에
  따라 오브젝트 색을 흰색→초록색으로 Lerp한다. `MaterialPropertyBlock`을
  사용해 머티리얼 애셋 자체를 복제하지 않도록 함 (CLAUDE.md §24 "동일
  오브젝트의 Material 복제를 남발하지 않는다").
  - 이 컴포넌트는 정식 UI가 아니라 **임시 디버그/검증용 시각 피드백**이다.
    Phase B의 정식 "Point & Hold Progress UI Prefab"이 생기면 이걸 대체하거나
    같이 쓸 수 있다. 지금은 헤드셋에서 눈으로 확인만 되면 충분하므로
    최소 구현만 함.
- **결정**: `SmokeTest_PointHoldFade.unity`에서
  `PointAndHoldTarget.onHoldCompleted` → `FadeMoveController.MoveTo(WaypointB)`를
  Inspector 수준에서 연결 (코드가 아니라 `UnityEditor.Events.UnityEventTools.
  AddObjectPersistentListener<Transform>`로 씬에 Persistent Listener로
  저장). `onHoldCompleted`는 `UnityEvent<PointAndHoldTarget>`이고
  `MoveTo`는 `Transform` 인자를 받아 타입이 다르지만, Unity UnityEvent의
  "Static Parameter" 방식으로 고정 인자(WaypointB)를 넘기도록 연결했다 —
  Inspector에서 수동으로 연결하는 것과 동일한 결과.
  - **이유**: `ScenarioManager`가 아직 없어서 정식 Step 전환 로직으로
    연결할 수는 없지만, 지금 단계에서 "Point & Hold 성공 → Fade 이동"이
    실제로 이어지는지 눈으로 보려면 임시로라도 연결이 필요했음. 나중에
    `ScenarioManager`가 생기면 이 Persistent Listener는 제거하고 Step
    기반 로직으로 대체할 것 (지금 연결은 SmokeTest 전용, 실제 콘텐츠
    Scene에는 적용하지 않음).
- **검증 결과 (Play Mode, Hover 이벤트 코드 직접 호출 시뮬레이션)**:
  게이지가 1.0에 도달하자 큐브 색이 정확히 초록색(RGBA 0,1,0,1)으로
  바뀌었고, 동시에 `FadeMoveController`가 자동으로 트리거되어
  `PlayerOrigin`이 `WaypointB` 위치로 이동 완료(`IsMoving=false`)까지
  확인함. Point & Hold → 시각 피드백 → Fade → 이동 전체 체인이 정상
  동작.
  - **여전히 미검증**: 이번에도 실제 손 추적/컨트롤러 Ray로 조준한 것이
    아니라 이벤트를 코드로 흉내 낸 것이다. 퀘스트 연결 후 실제로 손으로
    큐브를 가리키고 있으면 색이 서서히 변하다가 초록이 되고, 화면이
    Fade됐다가 다른 위치에 다시 뜨는지 최종 확인이 필요하다.

## 2026-09-04 — Grab 시스템 확인 (기존 XRI 기능, 코드 변경 없음)

- **확인 방법**: `SampleScene`의 "Interactables" 그룹(Unity VR Template 기본
  제공, Cube/Cylinder/Torus/Tapered/Sphere/Arch/Blaster/Spatial Panel 등 총
  10개, 전부 `XRGrabInteractable` 장착 완료 상태)을 그대로 사용. 코드는 하나도
  추가/수정하지 않았고 기존 기능이 프로젝트에서 실제로 동작하는지만 검증함
  (CLAUDE.md "기존 기능이 있으면 새로 만들지 않는다" 원칙).
  - Play Mode에서 `XRInteractionManager.SelectEnterUnconditionally(interactor,
    interactable)` / `SelectExit(...)`를 직접 호출해 "Cube Interactable"을
    `XR Origin Hands (XR Rig)`의 Right Controller `Near-Far Interactor`로
    잡고 놓는 전체 사이클을 실측함.
  - 결과: `isSelected` true/false 정상 전환, `interactorsSelecting`에 정확한
    인터랙터 등록, `movementType=VelocityTracking`에 따라 Cube가 실제
    물리적으로 인터랙터 근처까지 이동(거리 약 0.25)한 뒤 Release 시 정상
    분리됨. Grab 자체의 핵심 로직(XRGrabInteractable + XRInteractionManager)은
    정상 동작.
- **중요 발견**: 처음에 공개 API `SelectEnter(interactor, interactable)`
  (검증을 거치는 버전)를 호출했을 때는 아무 효과가 없었음. 콘솔 로그 확인
  결과 원인은 "Interactor is not registered with this XR Interaction
  Manager. The interactor component is not active and enabled." —
  즉 **Hand Tracking 데이터가 없는 상태(헤드셋 미연결, Device Simulator
  미가동)에서는 Near-Far Interactor 자체가 비활성 상태**였음. 이 프로젝트가
  `XR Origin Hands` 기반(컨트롤러가 아니라 손 추적 우선)이라 나타나는 특성.
  - 검증 자체는 등록/가능성 검사를 건너뛰는 `SelectEnterUnconditionally`로
    우회해서 실제 물리 동작까지 확인했지만, 이는 **정상적인 사용자 입력
    경로(실제 손 추적 또는 컨트롤러 Ray로 조준 → Select 버튼)를 검증한 것은
    아니다.**
  - **미검증 (다음 세션 필수)**: 실제 Quest 3S 연결 또는 XR Device
    Simulator로 손을 움직여 "자연스럽게 Hover → Select"가 되는지는 아직
    확인 못 함. Point & Hold Ray Hover와 마찬가지로 실기기/시뮬레이터
    연결 후 재확인 필요.
  - 이 세션 동안 나머지 경고(XR 오디오 드라이버, Eye Tracking/Hand Tracking
    Subsystem 없음)는 헤드셋 미연결 상태에서 기대되는 정상 로그이며 버그
    아님.
- **작업 형태**: 코드/씬 변경 없이 검증만 했으므로 별도 커밋 없이 `checklist.
  md`/`context-notes.md`만 갱신.

## 2026-09-04 — Point & Hold / Fade 스크립트 구현

- **결정**: 게이지 누적 로직을 `HoldGaugeState`(순수 C# 클래스)로 분리하고,
  `PointAndHoldTarget`(MonoBehaviour)은 XRI의 `XRBaseInteractable.hoverEntered
  /hoverExited` 이벤트를 구독해 `HoldGaugeState`를 구동만 한다.
  - **이유**: CLAUDE.md §22.1 "MonoBehaviour에 강하게 결합된 기능을 테스트하기
    위해 거대한 추상화 계층을 새로 만들지 않는다. 테스트 가능한 작은 상태
    로직만 분리한다"를 그대로 적용. Ray/Hover 판정 자체는 XR Interaction
    Toolkit(XRSimpleInteractable 등 `IXRHoverInteractable`)에 위임하고
    재구현하지 않음.
  - Hold 중단 시 정책은 "즉시 초기화"가 아니라 "감소(decay)"를 선택
    (`decayRate` 파라미터). 급격한 초기화보다 자연스러운 UX라 판단.
  - 완료(`IsCompleted`) 이후에는 `Tick()`이 더 이상 진행/감소하지 않도록
    잠가서, 같은 컴포넌트 레벨에서도 중복 성공 신호가 나가지 않게 함
    (최종 중복 방지 책임은 `ScenarioManager`에 있지만 방어적으로 1차 처리).
- **결정**: `FadeMoveController.MoveTo(destination)`는 destination의 위치는
  그대로 쓰되 회전은 `Quaternion.Euler(0, destination.eulerAngles.y, 0)`로
  Yaw만 반영한다.
  - **이유**: CLAUDE.md §13 UX 규칙 7번 "갑작스러운 카메라 회전이나 불필요한
    강제 시점 변경을 사용하지 않는다"를 코드 레벨에서 강제하기 위함. 목적지
    Transform의 Pitch/Roll이 실수로 기울어져 있어도 카메라가 그대로 따라
    기울지 않도록 방지.
- **결정**: `Assets/_GatePassVR/Scripts/GatePassVR.Runtime.asmdef`와
  `Assets/_GatePassVR/Tests/EditMode/GatePassVR.Tests.EditMode.asmdef`를
  새로 추가.
  - **이유**: 스크립트가 기본 `Assembly-CSharp`에 있으면 EditMode 테스트
    어셈블리가 참조할 수 없어 자동 테스트가 불가능함. Runtime asmdef는
    `Unity.XR.Interaction.Toolkit`을 참조.
  - **주의**: 이 asmdef는 현재 `Scripts/` 폴더 전체(Editor 하위 포함)를
    덮는다. 나중에 `Scripts/Editor`에 실제 에디터 전용 스크립트가 생기면
    그 안에 별도 Editor 전용 asmdef를 만들어 빌드에서 제외해야 한다
    (지금은 Editor 폴더가 비어 있어 문제 없음).
- **검증 결과**:
  - EditMode 테스트 `GatePassVR.Tests.EditMode.HoldGaugeStateTests` 6개 전부
    통과 (진행/감소/완료 1회 발동/완료 후 재발동 안 함/0 미만 방지/Reset).
  - Play Mode 수동 Smoke Test: `Assets/_GatePassVR/Scenes/
    SmokeTest_PointHoldFade.unity`에서 `HoldTargetCube`(XRSimpleInteractable
    + PointAndHoldTarget)의 `hoverEntered`/`hoverExited` UnityEvent를 코드로
    직접 호출해 게이지 완료(Progress=1, IsCompleted=true)와 `ResetHold()`
    이후 정상 초기화(Progress=0)를 확인. `FadeMoveController.MoveTo`
    호출 시 Fade Out이 먼저 진행되고(위치 불변, alpha만 상승) 이후 위치가
    목적지로 정확히 이동, Fade In으로 alpha가 0으로 복귀하는 순서를 실측
    확인 (position/rotation 모두 destination과 일치).
  - **미검증 (중요)**: 위 Smoke Test는 실제 컨트롤러 Ray로 조준한 것이
    아니라 `hoverEntered`/`hoverExited` UnityEvent를 코드로 직접 호출해
    시뮬레이션한 것이다. 실제 Near-Far/Ray Interactor로 조준했을 때의
    Hover 판정 자체(거리, 각도, Interaction Layer)는 아직 확인하지 않았음.
    Meta Quest 3S가 충전 중이라 실기기 검증도 못 함. 다음 세션에서 Device
    Simulator 또는 실기기로 실제 Ray 조준 테스트가 필요.
  - Play Mode 진입/종료 시 콘솔에 "XR: Error setting active audio output
    driver. Falling back to default." 경고 2건 발생 — 이 프로젝트의 XR
    오디오 출력 관련 환경 이슈로 추정되며 이번에 작성한 스크립트와는 무관.
    컴파일 오류/경고는 0건.
- **아직 하지 않은 것**: `PointAndHoldTarget.onHoldCompleted` →
  `FadeMoveController.MoveTo` 연결은 하지 않았다. 이 둘을 실제로 잇는 것은
  `ScenarioManager`/`GuideManager`가 생긴 뒤 Step 단위로 Inspector에서
  구성할 콘텐츠 조립 작업 (Phase B/C)이라 지금은 각 컴포넌트 단위로만
  완성해 둠.

## 2026-09-04 — 저장소 초기화

- **결정**: GitHub Public 저장소 `KJY-1204/GatePass_VR` 생성. `main`/`develop`
  브랜치 구성, 실제 개발은 `feature/<이름>-<기능>` 브랜치에서 진행 (CLAUDE.md §19).
  - **이유**: 팀 협업 시 Scene/Prefab 동시 수정 충돌을 피하기 위해 브랜치
    전략을 먼저 확정할 필요가 있었음.
- **결정**: `.gitignore`는 Unity 표준 패턴(`Library/`, `Temp/`, `Logs/`, `obj/`,
  `UserSettings/`, IDE 산출물, 빌드 산출물) 기반으로 작성. `*.csproj`, `*.sln`,
  `*.slnx`도 제외 (IDE가 자동 재생성하는 파일이므로).
  - **이유**: 사용자가 명시한 포함/제외 목록(Assets, Packages, ProjectSettings,
    CLAUDE.md 포함 / Library, Temp, Logs, obj, UserSettings 제외)을 그대로 따름.
- **확인 사항**: 초기 커밋 전 시크릿 패턴(API Key, Password, Private Key 등)과
  개인정보, 재배포 불가 에셋을 검사함. `Assets/` 내 콘텐츠는 전부 Unity 공식
  VR Template + XR Interaction Toolkit/XR Hands 샘플 패키지이며 유료 에셋
  스토어 콘텐츠 없음. 검색 결과 이상 없음.
- **현재 프로젝트 상태**: Unity 6000.3.10f1, VR Core 템플릿으로 초기화된
  상태. `SampleScene`, `BasicScene`은 템플릿 기본 씬이며 프로젝트 전용 Scene
  이 아직 없음. `Assets/_GatePassVR` 폴더와 커스텀 스크립트는 전혀 없는
  완전 초기 상태 — 이번이 CLAUDE.md §29 First Session Bootstrap에 해당하는
  첫 세션.

## 2026-09-04 — `_GatePassVR` 폴더 구조 생성

- **결정**: `Assets/_GatePassVR` 하위에 CLAUDE.md §15 구조(Scenes, Scripts/Core·VR·
  Interaction·Guidance·UI·Editor, Prefabs/VR·Interaction·UI·Environment, Data,
  Audio, Art/Temp, Tests/EditMode·PlayMode)를 생성. 각 빈 폴더에 `.gitkeep`
  파일을 넣음.
  - **이유**: Git은 빈 디렉터리를 추적하지 않으므로 `.gitkeep`으로 구조를
    먼저 고정. `.meta` 파일은 Unity Editor가 다음에 이 프로젝트를 열 때
    자동 생성되므로 지금은 만들지 않음 (Unity를 거치지 않고 파일시스템에서만
    작업했기 때문).
  - **주의**: 다음에 Unity Editor로 이 프로젝트를 열면 새 폴더마다 `.meta`
    파일이 자동 생성된다. 그 변경은 별도로 커밋해야 함 (Unity가 생성한
    `.meta`는 무시하지 말고 추적 대상).
- **작업 방식**: `feature/kim-project-structure` 브랜치를 `develop`에서 분기해
  작업. 사용자가 git 브랜치 작업에 익숙하지 않아 브랜치 생성·커밋·develop
  병합까지 에이전트가 대신 처리함. 이후에도 같은 패턴(브랜치 생성 → 작업 →
  커밋 → develop 병합 → push)을 기본으로 진행.

## 다음 세션이 알아야 할 것

- `plan.md`, `checklist.md`는 이번에 처음 생성됨. Phase A 항목 중 Git/GitHub
  설정, `_GatePassVR` 폴더 구조, Point & Hold(`PointAndHoldTarget`)/Fade
  (`FadeMoveController`) 스크립트, Grab 시스템(기존 XRI 기능) 확인까지 완료
  상태. 남은 것은 XR 기본 실행/Ray Interaction/Grab 전부의 **실제 입력 경로**
  (손 추적 또는 컨트롤러 Ray로 자연스럽게 조준·선택하는 것) 확인 뿐이다.
- XR 기본 실행, 실제 손 추적/컨트롤러 Ray Interaction은 아직 확인하지
  않았음 (`checklist.md`에 "미검증"으로 표기됨). Meta Quest 3S가 있으니
  충전이 끝나면 실기기로, 또는 그 전에 XR Device Simulator(`Assets/XR/...`에
  이미 설정되어 있음)로 먼저 확인할 수 있다.
- **중요**: Grab 확인 중 발견한 사실 — 이 프로젝트는 `XR Origin Hands`
  기반이라 Hand Tracking 데이터가 없으면(헤드셋 미연결, 시뮬레이터 미가동)
  Near-Far Interactor가 자동으로 비활성화된다. 즉 지금까지의 Point & Hold/
  Fade/Grab 검증은 전부 "이벤트나 API를 코드로 강제 호출"한 것이지 실제
  입력 경로로 조준·선택한 것이 아니다. 다음 세션에서 실기기 또는 Device
  Simulator(손 포즈 시뮬레이션 포함)로 이 부분을 반드시 재확인할 것.
- 폴더 구조(`Assets/_GatePassVR/...`)는 생성 완료, `.meta` 파일도 이미
  Unity Editor를 통해 정상 생성·커밋됨.
- `Assets/_GatePassVR/Scenes/SmokeTest_PointHoldFade.unity`는 Point & Hold/
  Fade 컴포넌트 검증용 최소 테스트 씬이다. `HoldTargetCube`,
  `FadeController`, `PlayerOrigin`, `WaypointA`/`WaypointB`, `FadeCanvas`로
  구성. 다음에 ScenarioManager/GuideManager를 만들 때 이 씬을 확장해
  CLAUDE.md §14.3의 전체 수직 슬라이스(Ray → Point & Hold → Fade 이동 →
  Step 성공 → Guide 갱신 → Reset)를 완성하면 된다.
- `PointAndHoldTarget.onHoldCompleted`와 `FadeMoveController.MoveTo`는
  아직 서로 연결되어 있지 않다 (의도적으로 각 컴포넌트 단위까지만 완성).
  ScenarioManager가 생기면 그 안에서 두 이벤트를 이어준다.
- 팀 담당자 배정은 CLAUDE.md §18에 이미 정의되어 있음 (김씨=메인 VR/시스템,
  이씨=콘텐츠/에셋, 조씨=예비/테스트 지원). 동일 Scene/Prefab을 동시 수정하지
  않도록 작업 전 소유자 확인.
