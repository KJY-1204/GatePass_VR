# GatePass VR - Context Notes

결정 사항과 이유를 시간순으로 누적 기록한다. 완료된 내용을 삭제하지 않는다.

## 2026-09-04 — 손가락 구부림(그립) 애니메이션을 절차적으로 구현

- **배경**: PolyOne 손 에셋에 그립/트리거 반응 애니메이션이 없다는 걸
  지난 세션에 확인했고, 사용자가 이번에 손가락 구부림을 만들어달라고
  요청. 애니메이션 클립을 새로 만드는 대신, 이미 있는 본 계층을 코드로
  직접 회전시키는 절차적(procedural) 방식을 선택.
- **본 축 규칙 발견**: `Free Pack - VR Hands ( Rigged )` 프리팹의 모든
  손가락 관절(엄지 포함)은 **로컬 +Y축이 항상 자식 본(손끝) 방향**을
  향하도록 모델링되어 있고, 대기 포즈에서 이미 로컬 X축으로 몇 도씩
  살짝 굽어있는 상태였다(예: `HandIndex2` = 3.35°, `HandThumb2` = 8.69°).
  이 패턴으로 "로컬 X축 회전 = 구부림 축"이라는 결론을 내렸고, 왼손/오른손
  둘 다 (본의 로컬 좌표계가 서로 미러링되어 있음에도) **양수 X 회전을
  더하면 동일하게 안쪽으로 구부러지는 것**을 스크린샷으로 직접 확인해
  검증함(양손을 인위적으로 떨어뜨려 놓고 각각 확인).
- **결정**: `FingerCurlAnimator`(`Scripts/VR`)를 만들어 `(bone, curlAngle)`
  쌍의 배열을 받고, `Awake()`에서 각 본의 "편 상태" 로컬 회전을 캐싱한 뒤
  `SetCurl(0~1)`로 `openRotation * AngleAxis(curlAngle * curl, Vector3.right)`
  를 적용한다. 축을 컴포넌트 파라미터로 노출하지 않고 코드에 고정한 이유는
  이 리그의 모든 관절이 동일한 축 규칙을 따르는 게 확인됐기 때문
  (Simplicity First — 불필요한 설정 노출 안 함).
  - 손가락당 4관절(1~4) × 5손가락 × 양손 = 총 40개 항목을 Prefab Stage에서
    `SerializedObject`로 한 번에 구성함. 각도는 관절별로 70/90/90/60도로
    차등을 둬서(끝 마디는 덜 굽게) 좀 더 자연스러운 주먹 모양이 나오게 함.
  - `HandGripInputDriver`(`Scripts/VR`)가 `InputDevices.GetDeviceAtXRNode`로
    Grip/Trigger 값을 읽어 `Max(grip, trigger)`를 `FingerCurlAnimator.SetCurl`에
    넘긴다. `Left Controller`/`Right Controller` 오브젝트에 각각 붙임.
    (XRI의 Select/Activate Input Action을 재사용하지 않고 XR 디바이스
    값을 직접 읽은 이유: 별도 Input Action 에셋 구조를 몰라도 되고,
    그립 시각화 전용으로 목적이 분명해서 최소 의존성으로 유지.)
- **검증 결과 (Play Mode, `SetCurl()` 직접 호출)**: 0(펴짐)/0.5(절반)/1(완전
  주먹) 전부 스크린샷으로 확인. 절반은 손가락이 자연스럽게 반쯤 굽은
  모양, 완전 주먹은 뭉툭한 주먹 실루엣으로 잘 나옴. 콘솔 에러 없음.
  **미검증**: 실제 컨트롤러의 Grip/Trigger를 눌렀을 때 `HandGripInputDriver`가
  정상적으로 값을 읽어오는지는 실기기에서만 확인 가능 (에디터에는 XR
  디바이스가 연결되어 있지 않음).
- **작업 중 발견한 무관한 이슈와 처리**: `TestMap_Quest.unity`를 저장할 때
  `Complete XR Origin Set Up Hands Variant` 프리팹 내부의 Tooltip UI
  (Affordance Callout들의 `Tooltip Canvas`, TextMeshPro 기반 Content Size
  Fitter 사용)의 `m_SizeDelta`가 저장할 때마다 0으로 잘못 직렬화되는
  현상을 발견함. `LayoutRebuilder.ForceRebuildLayoutImmediate`로 강제
  재계산해도 저장 시점에 따라 다시 0으로 돌아가는 등 일관성이 없었음 —
  아마도 TextMeshPro Content Size Fitter가 화면에 한 번도 렌더링되지
  않은 상태에서 크기를 잘못 계산하는 문제로 추정.
  - **이번 작업과 무관하다고 판단해 조사를 중단**하고, `TestMap_Quest.unity`를
    `git checkout`으로 마지막 커밋 상태로 되돌린 뒤 Unity에 다시 로드시켰다.
    손가락 구부림 기능은 전부 프리팹 쪽 변경이라 씬 파일을 되돌려도
    영향 없음(재확인 완료 — 프리팹 상속을 통해 정상적으로 존재).
  - **다음에 이 프로젝트에서 Tooltip/Affordance Callout UI를 건드릴 일이
    있으면** 이 `m_SizeDelta` 직렬화 불안정 현상을 먼저 의심해볼 것.
    지금 당장 기능에 영향은 없어 보이지만(Tooltip 자체가 일반 플레이에서
    거의 노출 안 되는 보조 UI), 나중에 문제가 되면 TextMeshPro 폰트 로딩
    타이밍이나 Content Size Fitter 설정을 점검해야 할 수 있음.

## 2026-09-04 — 컨트롤러 모델을 PolyOne VR 손 모델로 교체

- **배경**: 사용자가 `Assets/PolyOne/Free VR Hands` 에셋(무료 VR 손 모델)을
  다운로드해두고, 컨트롤러 모델 대신 손이 보이게 해달라고 요청.
- **에셋 구조 확인**: `Free Pack - VR Hands ( Rigged ).prefab` 하나에 왼손
  (`Root/J_Left/...`)과 오른손(`Root/J_Right/...`) 본 계층이 모두 들어있고,
  `SM_HandsVR_Male`이라는 SkinnedMeshRenderer 하나가 양손을 동시에 렌더링한다
  (왼손/오른손 프리팹이 분리되어 있지 않음).
  - **중요한 한계**: 이 에셋의 Animator Controller(`Free Pack - VR Hands (
    Rigged ).controller`)에는 파라미터가 하나도 없고 State도 Idle 하나뿐 —
    **그립/트리거에 반응하는 손가락 구부림 애니메이션이 전혀 없다.** 같은
    폴더의 `Free VR Hands_Controler.controller`는 Angry/Happy/Jump 등
    캐릭터 애니메이션용 스텁이라 VR 손 용도로 쓸 수 없음. 사용자에게 미리
    안내함 — 지금은 항상 고정된 대기 포즈로만 보인다.
- **결정**: 공용 Rig 프리팹 `Assets/VRTemplateAssets/Prefabs/Setup/Complete
  XR Origin Set Up Hands Variant.prefab`을 Prefab Stage에서 직접 수정.
  - `Free Pack - VR Hands ( Rigged )` 프리팹을 인스턴스화한 뒤 즉시
    `PrefabUtility.UnpackPrefabInstance(..., Completely)`로 언팩(중첩
    프리팹 상태로 두면 본만 다른 계층으로 재배치할 때 제약이 많아서).
  - `J_Left`를 `Left Controller Visual` 밑으로, `J_Right`를 `Right
    Controller Visual` 밑으로 재배치(로컬 포지션 0,0,0, 로컬 회전은
    데모 씬에서 쓰던 원래 바인드 포즈 값 그대로 사용:
    `J_Left = (0.06, 86.87, 179.76)`, `J_Right = (359.94, 93.13, 359.76)`).
  - `SM_HandsVR_Male`(렌더러)은 `Camera Offset` 밑 `VR Hands Visual`
    컨테이너에 그대로 둠 — SkinnedMeshRenderer는 `bones` 배열 참조로만
    변형되므로 렌더러 자신의 위치는 시각적 결과에 영향 없음.
  - 기존 `UniversalController`(플라스틱 컨트롤러 메쉬)는 삭제하지 않고
    `SetActive(false)`로 비활성화만 함 — 나중에 되돌리기 쉽도록.
  - 빈 `Root` 래퍼 오브젝트(본을 옮기고 나서 자식이 없어짐)는 정리해서
    삭제함.
  - 이 프리팹은 공용 Rig라서 **`SampleScene`과 `TestMap_Quest` 양쪽에
    자동으로 반영됨**을 확인함 (두 씬 모두 로드해서 `UniversalController`
    비활성화, 손 본 존재 확인, Console 에러 없음 확인).
- **회전값 튜닝 관련 중요 참고**: Scene View 스크린샷으로 몇 차례
  시도해봤는데, 원래 바인드 포즈 값을 그대로 쓰면 손이 "몸통 옆에 자연스럽게
  늘어뜨린" 자세로 보이고(형태 자체는 멀쩡함), 컨트롤러를 쥐듯 앞으로 뻗은
  자세는 아니다. 손끝이 아래나 옆을 향할 수 있음. X축으로 -90도 정도
  틀어보는 시도는 손 모양이 이상하게(펼쳐진 자세로) 나와서 되돌렸다.
  - **다음 세션/사용자가 할 일**: 실제 헤드셋을 쓰고 컨트롤러를 든 상태에서
    `J_Left`/`J_Right`의 로컬 회전값(Inspector, `Complete XR Origin Set Up
    Hands Variant` 프리팹을 열어서 `Left/Right Controller Visual` 하위)을
    직접 눈으로 보면서 미세 조정하는 게 스크린샷 추측보다 훨씬 빠르고
    정확하다. 스크린샷만으로는 "그립처럼 자연스러운지"를 판단하기 어려움.
  - Editor 상에서는 Left/Right Controller가 트래킹 데이터 없이 같은
    위치(원점 근처)에 겹쳐 있어서 스크린샷엔 손 하나만 보이는 것처럼
    보였음 — 이건 버그가 아니라 에디터에 실제 트래킹 소스가 없어서 생기는
    정상적인 현상. 실기기에서는 왼손/오른손이 각자 컨트롤러 위치로
    정상적으로 떨어져 보일 것으로 예상됨(다음 실기기 테스트에서 확인).
- **라이선스 확인 필요**: `Assets/PolyOne/Free VR Hands`는 사용자가 직접
  다운로드해 프로젝트에 넣은 에셋이라 라이선스 조건(재배포/상업적 사용
  가능 여부 등)은 확인하지 않았다. CLAUDE.md §16.5(외부 에셋 라이선스
  불명확 시 후보 목록만 기록) 원칙상, 최종 빌드/배포 전에 라이선스 조건을
  한 번 확인해두는 게 안전하다.
- **참고**: `Assets/XR/Settings/OpenXRPackageSettings.asset`가 이번에도
  또 fileID만 바뀌는 형태로 수정되어 있었음 (지난번과 동일한 현상, 저번에
  이미 한 번 커밋했었는데 Unity를 다시 쓰니 또 발생). 이번 커밋 범위와
  무관해서 포함하지 않고 그대로 둠.

## 2026-09-04 — Quest Link 연결 끊김은 케이블/하드웨어 이슈 (코드 문제 아님)

- **참고**: 퀘스트 3S 실기기 테스트 중 PC-헤드셋 연결이 중간에 끊기는
  현상이 있었음. 사용자가 케이블이 오래돼서 그런 것 같다고 판단함 (Unity
  프로젝트나 XR 설정 문제로 보지 않음). Point & Hold, Fade, Grab 자체의
  동작에는 문제가 없었고 연결 끊김만 있었던 것.
  - **다음 세션 참고**: 앞으로 비슷한 연결 끊김이 재현되면 먼저 케이블/
    Air Link 무선 연결 상태부터 의심할 것 — Unity XR 설정이나 스크립트를
    바로 의심해서 불필요하게 디버깅하지 않는다. 반복되면 케이블 교체나
    Air Link(무선) 전환을 사용자에게 제안하는 정도로 충분.
  - 이 세션에서 `Assets/XR/Settings/OpenXRPackageSettings.asset`가
    커밋되지 않은 상태로 변경되어 있었는데(대화 시작 시점 `git status`에도
    이미 잡혀 있었음), 이는 Quest Link 연결 시 OpenXR 런타임 활성화
    정보가 갱신되며 생기는 변경으로 추정됨. 이번 작업과 무관해서 건드리지
    않고 그대로 둠 — 다음 세션에서 커밋할지 말지는 실제 내용을 확인하고
    판단할 것.

## 2026-09-04 — 퀘스트 3S 실기기 테스트 통과 + 게이지를 Radial 도넛으로 교체

- **중요 이정표**: 사용자가 `TestMap_Quest` 씬을 Meta Quest 3S 컨트롤러로
  직접 테스트함. 패드 조준·홀드, Grab(Cube/Sphere), Fade 이동까지 전부
  "깔끔하게 잘 된다"고 확인. 지금까지 세션 내내 "이벤트를 코드로 흉내 낸
  것이라 미검증"이라고 반복 기록했던 부분이 드디어 **실제 손 입력 경로로
  검증 완료**됨. Point & Hold, Fade, Grab 세 핵심 시스템 모두 M1(VR Core)
  마일스톤의 상호작용 요건을 실기기에서 충족.
- **결정**: 사용자 피드백에 따라 Point & Hold 게이지 표현 방식을 교체함.
  - 기존: `HoldProgressVisual`이 큐브 자체의 색을 흰색→초록으로 Lerp.
  - 변경: 신규 `RadialGaugeVisual`(`Scripts/Interaction`)이 World Space
    Canvas 위 `Image`를 `Image.Type.Filled` + `FillMethod.Radial360` +
    `fillOrigin=Top` + `fillClockwise=true`로 설정해 시계 방향으로 차오르는
    2D 도넛(고리) 모양 게이지를 표시. 각 패드의 `onHoldCompleted`는 그대로
    유지하고, `onProgressChanged` 리스너만 `HoldProgressVisual.SetProgress`
    → `RadialGaugeVisual.SetProgress`로 교체 (컴포넌트 자체도 제거).
  - **도넛 스프라이트는 외부 아트 없이 코드로 런타임 생성**(256x256 텍스처,
    반지름 0.34~0.48 구간만 알파 255, 나머지 알파 0). 정적 캐시로 모든
    게이지 인스턴스가 텍스처 하나를 공유해 복제하지 않는다 (CLAUDE.md §24
    "Material/텍스처 복제 남발 금지"). 실제 아트가 들어오면 Inspector에서
    `Image.sprite`를 직접 지정해 대체 가능하도록 구조는 그대로 둠.
  - 게이지 링 오브젝트는 패드의 자식으로 넣지 않고 별도 루트 오브젝트로
    배치했다. 패드가 비균등 스케일(0.8, 0.05, 0.8)이라 자식으로 넣으면
    링이 눌린 타원으로 왜곡되기 때문. 대신 패드 위치 + (0, 0.06, 0)에
    독립적으로 배치하고 회전만 `(-90,0,0)`으로 줘서 바닥에 눕혀 위를
    보게 했다.
- **중요한 작업 실수와 교훈**: 처음에 리스너 교체(`RemovePersistentListener`)와
  `HoldProgressVisual` 컴포넌트 삭제(`DestroyImmediate`)를 **Play Mode
  상태에서 실행**했다가, Stop 이후 전부 원래대로 되돌아간 것을 뒤늦게
  발견함. **Unity는 Play Mode 중 씬에 가한 변경을 Stop 시 자동으로
  버린다** — 이건 이미 알고 있던 사실이지만 이번엔 스크립트 편집이 아니라
  "Persistent Listener 재배선" 같은 씬 데이터 편집도 똑같이 적용된다는
  것을 놓쳤다. Edit Mode로 돌아와서 (`EditorApplication.isPlaying`로
  확인 후) 다시 작업해서 해결함.
  - **다음부터 적용할 규칙**: `execute_code`로 씬 GameObject/Component를
    변경하는 모든 작업(컴포넌트 추가/삭제, Persistent Listener 배선,
    프로퍼티 값 변경 등)은 반드시 `EditorApplication.isPlaying == false`
    상태에서 하고, 실행 후 저장(`manage_scene action=save`)까지 마쳐야
    한다. Play Mode 안에서의 같은 작업은 오직 "시뮬레이션 검증"용으로만
    쓰고 절대 최종 결과로 착각하면 안 된다.
- **검증 결과 (Play Mode, 여전히 이벤트 코드 호출 방식)**: 도넛 게이지가
  `fillAmount`을 `PointAndHoldTarget.Progress`와 정확히 일치시켜 시계
  방향으로 차오르고, 완료 시 기존과 동일하게 Fade+이동까지 이어지는 것을
  스크린샷과 값 확인으로 재검증함. 큐브 자체는 이제 색이 바뀌지 않고
  중립 상태 유지 (`SetPropertyBlock(null)`로 이전 초록색 잔상 제거).

## 2026-09-04 — `TestMap_Quest.unity` 제작 (퀘스트 실기기 테스트용 맵)

- **배경**: 기존 `SmokeTest_PointHoldFade.unity`는 실제 XR Rig 없이 순수
  로직 검증용으로 만든 씬이라(가짜 `PlayerOrigin` Transform, 일반 Camera)
  그대로 퀘스트에 빌드해도 헤드셋 트래킹이 전혀 동작하지 않는다. 퀘스트
  연결 시 실제로 걸어 다니며 테스트할 "맵"이 별도로 필요해서 새로 제작.
- **결정**: XR Rig를 직접 다시 구성하지 않고, 프로젝트에 이미 있는 프리팹
  두 개를 그대로 인스턴스화해서 재사용했다.
  - `Assets/VRTemplateAssets/Prefabs/Setup/Complete XR Origin Set Up Hands
    Variant.prefab` — `SampleScene`의 "XR Origin Hands (XR Rig)"가 바로 이
    프리팹의 인스턴스였음을 확인하고 동일하게 사용.
  - `Assets/VRTemplateAssets/Prefabs/Setup/Hands Permissions Manager.prefab`
    — Android 런타임 손 추적 권한 요청 처리.
  - **이유**: CLAUDE.md "기존 XR Rig 구조가 있으면 먼저 읽고 존중한다",
    "같은 기능이 이미 있으면 새로 만들기보다 기존 구현을 최소 수정한다"를
    그대로 적용. 손수 XR Origin을 새로 조립하면 Near-Far Interactor, Gaze
    Assistance, Hand Subsystem 연결 등을 처음부터 다시 맞춰야 해서 위험이
    크고 불필요함.
- **맵 구성** (`Assets/_GatePassVR/Scenes/TestMap_Quest.unity`):
  - `Floor`(15×15 Plane), `Directional Light`.
  - `WaypointStart`(0,0,-5) — XR Origin 스폰 위치와 동일.
  - Point & Hold 이동 패드 3개로 왕복 루프 구성: `Pad_ToGrabZone`(시작
    지점 근처) → `WaypointGrabZone`(-5,0,2) → `Pad_ToOpenArea` →
    `WaypointOpenArea`(5,0,2) → `Pad_BackToStart` → `WaypointStart`로 복귀.
    각 패드는 `PointAndHoldTarget` + `HoldProgressVisual`을 갖고
    `onHoldCompleted`가 `FadeController(FadeMoveController).MoveTo`를
    직접 호출하도록 연결(연결 방식은 이전 세션의 `UnityEventTools.
    AddObjectPersistentListener` 방식과 동일).
  - `GrabTable` + `GrabCube`/`GrabSphere`(Rigidbody + `XRGrabInteractable`,
    `movementType=VelocityTracking`, `throwOnDetach=true`) — `WaypointGrabZone`
    도착 지점 바로 옆에 배치해서 이동 직후 바로 Grab 테스트 가능.
  - `FadeCanvas`/`FadeController`는 기존 SmokeTest 씬과 동일한 패턴
    (Screen Space Overlay Canvas + CanvasGroup).
- **Build Settings**: `manage_build(action=scenes)`로 `SampleScene`과
  `TestMap_Quest`를 Build Settings에 등록. 등록 직후에는 메모리에만 반영되고
  `ProjectSettings/EditorBuildSettings.asset`에 디스크로 저장되지 않아서
  `AssetDatabase.SaveAssets()`를 명시적으로 호출해야 했음 — 다음에 이 툴로
  빌드 씬 목록을 바꿀 때는 반드시 SaveAssets까지 해야 git에 반영된다는 점
  기억할 것.
  - 현재 빌드 타겟은 여전히 Windows64다. Quest 테스트는 Android APK
    빌드가 아니라 **Meta Quest Link/Air Link로 PC 빌드를 헤드셋에 스트리밍**
    하는 방식을 전제로 준비했다 (더 가볍고 빠른 반복 테스트 방법). 실제
    Android/Quest Standalone APK Build 전환은 별도 작업이며 아직 하지
    않았음 (CLAUDE.md §23 "Android Build Support가 설치된 환경에서만
    검증").
- **검증 결과 (Play Mode, Hover/Select 이벤트 코드 직접 호출 시뮬레이션)**:
  Start → GrabZone → OpenArea → Start 전체 루프에서 `XR Origin Hands (XR
  Rig)`의 위치가 각 Waypoint와 정확히 일치하는 것을 확인. `GrabCube`를
  `XRInteractionManager.SelectEnterUnconditionally`/`SelectExit`로 잡고
  놓는 것도 정상 동작. 콘솔에는 헤드셋 미연결 시 나오는 기존 경고
  (오디오 드라이버, Eye/Hand Tracking Subsystem 없음) 외 에러 없음.
  - **미검증 (다음 세션 필수)**: 지금까지와 마찬가지로 이 검증도 이벤트를
    코드로 흉내 낸 것이다. 퀘스트를 연결해서 실제로 걸어 다니며 손으로
    패드를 가리키고 오브젝트를 잡는 것은 아직 확인 못 했다. **이 씬이
    바로 그 실기기 테스트를 위해 만든 맵이다.**

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

- `plan.md`, `checklist.md`는 이번에 처음 생성됨. **Phase A는 사실상 완료
  상태**: Git/GitHub, `_GatePassVR` 폴더 구조, Point & Hold(`PointAndHoldTarget`
  + `RadialGaugeVisual`), Fade(`FadeMoveController`), Grab(기존 XRI 기능)
  전부 퀘스트 3S 컨트롤러로 실기기 검증까지 완료됨(2026-09-04). 남은 Phase A
  잔여 항목은 없고, 다음은 Phase B(`ScenarioManager`/`GuideManager` 등)로
  넘어가면 된다.
- `Assets/_GatePassVR/Scenes/TestMap_Quest.unity`가 실기기 테스트에 실제로
  사용된 검증된 씬이다 (Build Settings에도 등록됨). Point & Hold 이동 3곳,
  Fade 전환, Grab까지 한 번에 확인 가능. 앞으로 새 상호작용 시스템을 만들
  때도 이 씬에 추가해서 실기기로 확인하는 흐름을 유지하면 된다.
- Point & Hold 게이지 시각 표현은 `RadialGaugeVisual`(시계방향 도넛 Radial
  게이지)로 최종 확정. `HoldProgressVisual`(색상 변화 방식)은 `SmokeTest_
  PointHoldFade.unity`에는 아직 남아있지만 `TestMap_Quest`에서는 제거됨 —
  두 시각화 방식이 씬마다 다르게 남아있다는 점 인지할 것. 통일하고 싶으면
  `SmokeTest_PointHoldFade`도 `RadialGaugeVisual`로 맞추면 된다.
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
