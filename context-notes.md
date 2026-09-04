# GatePass VR - Context Notes

결정 사항과 이유를 시간순으로 누적 기록한다. 완료된 내용을 삭제하지 않는다.

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
  (`FadeMoveController`) 스크립트가 완료 상태. 남은 것은 XR 기본 실행/Ray
  Interaction의 실기기(또는 Device Simulator) 확인, Grab 시스템 확인.
- XR 기본 실행, 실제 컨트롤러 Ray Interaction은 아직 확인하지 않았음
  (`checklist.md`에 "미검증"으로 표기됨). Meta Quest 3S가 있으니 충전이
  끝나면 실기기로, 또는 그 전에 XR Device Simulator(`Assets/XR/...`에 이미
  설정되어 있음)로 먼저 확인할 수 있다.
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
