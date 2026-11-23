// 현재 Step을 관리하고, 상호작용이 보낸 성공 ID가 맞을 때만 안내 갱신·이동·다음 Step 진행을 하는 단일 진행 관리자
using System.Collections.Generic;
using GatePassVR.Guidance;
using GatePassVR.Interaction;
using GatePassVR.VR;
using UnityEngine;
using UnityEngine.Events;

namespace GatePassVR.Core
{
    public class ScenarioManager : MonoBehaviour
    {
        [SerializeField] private List<ScenarioStep> steps = new List<ScenarioStep>();
        [SerializeField] private GuideManager guideManager;
        [SerializeField] private GoalIndicatorArrow goalArrow;
        [SerializeField] private FadeMoveController fadeMove;
        [Tooltip("모든 Step을 마쳤을 때 표시할 안내")]
        [SerializeField] private string completedGuideText;

        [Tooltip("Step에 들어갈 때마다(Reset 재진입 포함) 호출")]
        [SerializeField] private UnityEvent onStepEntered;
        [Tooltip("Step에 들어갈 때 그 Step의 successId를 넘긴다(ScannerZone.AlignToScanId 등 차례 맞추기용)")]
        [SerializeField] private UnityEvent<string> onStepEnteredId;
        [SerializeField] private UnityEvent onScenarioCompleted;

        private ScenarioProgress progress;

        public int CurrentStepIndex => progress?.CurrentIndex ?? 0;
        public bool IsFinished => progress != null && progress.IsFinished;
        public ScenarioStep CurrentStep => progress == null || progress.IsFinished ? null : steps[progress.CurrentIndex];

        private void Start()
        {
            var ids = new List<string>(steps.Count);
            foreach (var step in steps)
            {
                ids.Add(step.successId);
            }

            progress = new ScenarioProgress(ids);
            EnterCurrentStep();
        }

        // 상호작용이 성공 사실을 알리는 공통 진입점. 현재 Step의 ID가 아니면 무시한다.
        public void ReportSuccess(string successId)
        {
            if (progress == null)
            {
                return;
            }

            var finishedStep = CurrentStep;
            if (!progress.TryAdvance(successId))
            {
                return;
            }

            if (finishedStep.moveDestination != null && fadeMove != null)
            {
                fadeMove.MoveTo(finishedStep.moveDestination);
            }

            EnterCurrentStep();
        }

        // PointAndHoldTarget.onHoldCompleted에 Inspector로 연결한다.
        public void ReportHoldCompleted(PointAndHoldTarget target)
        {
            if (target != null)
            {
                ReportSuccess(target.TargetId);
            }
        }

        // PlacementZone.onPlaced에 Inspector로 연결한다.
        public void ReportPlaced(PlacementZone zone)
        {
            if (zone != null)
            {
                ReportSuccess(zone.ZoneId);
            }
        }

        // HandOverZone.onHandedOver에 Inspector로 연결한다.
        public void ReportHandedOver(HandOverZone zone)
        {
            if (zone != null)
            {
                ReportSuccess(zone.ZoneId);
            }
        }

        // HandOverZone.onReturnTaken에 Inspector로 연결한다. 성공 ID는 zoneId + "_Returned".
        public void ReportReturnTaken(HandOverZone zone)
        {
            if (zone != null)
            {
                ReportSuccess(zone.ReturnTakenId);
            }
        }

        // ScannerZone.onScanned에 Inspector로 연결한다. 방금 스캔한 물건의 scanId를 성공 ID로 쓴다.
        public void ReportScanned(ScannerZone zone)
        {
            if (zone != null)
            {
                ReportSuccess(zone.LastScanId);
            }
        }

        // 현재 Step을 처음부터 다시 안내한다. 시나리오를 다 마친 뒤라면 첫 Step부터 다시 시작한다.
        public void ResetStep()
        {
            if (progress == null)
            {
                return;
            }

            if (progress.IsFinished)
            {
                progress.Restart();
            }

            EnterCurrentStep();
        }

        private void EnterCurrentStep()
        {
            var guide = guideManager != null ? guideManager : GuideManager.Instance;
            var step = CurrentStep;

            if (step == null)
            {
                if (guide != null)
                {
                    guide.SetGuide(completedGuideText);
                }

                SetGoal(null);
                onScenarioCompleted?.Invoke();
                return;
            }

            if (guide != null)
            {
                guide.SetGuide(step.guideMain, step.guideHint, step.guideVoice);
            }

            SetGoal(step.goalTarget);
            onStepEntered?.Invoke();
            onStepEnteredId?.Invoke(step.successId);
        }

        private void SetGoal(Transform target)
        {
            if (goalArrow != null)
            {
                goalArrow.SetGoal(target);
            }
        }
    }
}
