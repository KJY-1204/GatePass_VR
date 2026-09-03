# GatePass VR - Context Notes

결정 사항과 이유를 시간순으로 누적 기록한다. 완료된 내용을 삭제하지 않는다.

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
  설정, `_GatePassVR` 폴더 구조만 완료 상태이고, 나머지(XR 기본 실행 확인,
  Point & Hold, Fade 등)는 전부 미착수.
- XR 기본 실행, Ray Interaction 등은 아직 실기기/에디터에서 확인하지 않았음
  (`checklist.md`에 "미검증"으로 표기됨). 다음 세션은 이 확인부터 시작해야
  CLAUDE.md §29 절차(3단계: XR 기본 실행 확인)를 따르는 것이 된다.
- 폴더 구조(`Assets/_GatePassVR/...`)는 생성 완료 (`.gitkeep`으로 빈 폴더
  고정). Unity Editor를 처음 열면 `.meta` 파일이 자동 생성되므로 그 변경을
  별도 커밋할 것.
- 팀 담당자 배정은 CLAUDE.md §18에 이미 정의되어 있음 (김씨=메인 VR/시스템,
  이씨=콘텐츠/에셋, 조씨=예비/테스트 지원). 동일 Scene/Prefab을 동시 수정하지
  않도록 작업 전 소유자 확인.
