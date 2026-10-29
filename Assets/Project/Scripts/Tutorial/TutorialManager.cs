using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class TutorialManager : MonoBehaviour
{
    private List<TutorialStep> steps;

    [Header("UI Elements")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject handPointer;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private GameObject buyButton;

    [Inject] private TutorialProgressService _tutorialProgressService;
    [Inject] private BlockManager _blockManager;
    [Inject] private WaveManager _waveManager;
    [Inject] private PlayerRuntimeManager _playerRuntimeManager;

    private int _currentStep = 0;
    private Button _currentButtonListener;

    // 🧠 Храним ссылки, чтобы знать «кто кого» мёржит в шаге Merge
    private BlockParent _firstBlockPlaced;     // первый блок, который игрок поставил на поле
    private BlockParent _secondBlockCreated;   // второй блок, купленный позже и стоящий как цель

    #region Unity

    private void OnEnable()
    {
        BlockDragController.OnDragFinished += HandleDragFinished;

        if (_tutorialProgressService.IsTutorialCompleted)
        {
            tutorialPanel.SetActive(false);
            instructionText.gameObject.SetActive(false);
            this.enabled = false;

            return;
        }
    }

    private void OnDisable()
    {
        BlockDragController.OnDragFinished -= HandleDragFinished;
        _waveManager.OnWaveCompleted -= OnWaveCompletedOnce;
        UnsubscribeCurrent();
    }

    #endregion

    #region Tutorial Flow

    public void StartTutorial()
    {
        tutorialPanel.SetActive(true);
        instructionText.gameObject.SetActive(true);
        GenerateSteps();
        _currentStep = 0;
        handPointer.SetActive(true);
        ShowStep(_currentStep);
    }

    private void GenerateSteps()
    {
        steps = new List<TutorialStep>
        {
            // 1) Купить первый блок
            new TutorialStep
            {
                instructionText = "Tap to buy your first block",
                highlightTarget = buyButton,
                actionType = TutorialActionType.Click
            },

            // 2) Перетащить первый блок на поле
            new TutorialStep
            {
                instructionText = "Great! Now, drag it to the build area",
                highlightTarget = null,
                actionType = TutorialActionType.Drag,
                OnStepComplete = () =>
                {
                    // запоминаем ПЕРВЫЙ поставленный блок
                    _firstBlockPlaced = _blockManager.LastCreatedBlock;
                    if (_firstBlockPlaced != null)
                    {
                        _firstBlockPlaced.CanDrag = false; // после постановки — фиксируем
                    }
                }
            },

            // 3) Подождать волну (пример — можно упразднить/передвинуть при желании)
            new TutorialStep
            {
                instructionText = "Awesome! Now, let's wait for wave 1 to start. Your turret will attack automatically.",
                highlightTarget = null,
                actionType = TutorialActionType.None,
                OnStepStart = () =>
                {
                    _waveManager.StartWaves();
                    _waveManager.OnWaveCompleted += OnWaveCompletedOnce;
                },
                OnStepComplete = () =>
                {
                    _waveManager.OnWaveCompleted -= OnWaveCompletedOnce;
                    _playerRuntimeManager.TrySpend(CurrencyType.Coins, _playerRuntimeManager.GetCurrency(CurrencyType.Coins));
                    _playerRuntimeManager.AddCurrency(CurrencyType.Coins, _blockManager.BlockPrice);
                }
            },

            // 4) Купить второй блок
            new TutorialStep
            {
                instructionText = "Awesome! First wave cleared. Now let's buy another block.",
                highlightTarget = buyButton,
                actionType = TutorialActionType.Click,
                OnStepStart = () =>
                {
                    _waveManager.Freeze(); // остановим следующую волну, чтобы не мешала merge-шагу
                },
                OnStepComplete = () =>
                {
                    // сразу после покупки тут же появится второй блок
                    _secondBlockCreated = _blockManager.LastCreatedBlock;
                }
            },

            // 5) MERGE: тянем ПЕРВЫЙ на ВТОРОЙ
            new TutorialStep
            {
                instructionText = "Great! Now merge the first block onto the second one",
                highlightTarget = null, // рукой лучше указывать на активный (первый) блок
                actionType = TutorialActionType.Merge,
                OnStepStart = () =>
                {
                    if (_firstBlockPlaced != null)
                    {
                        _firstBlockPlaced.CanDrag = true;       // первый — РАЗБЛОКИРУЕМ
                        MoveHandToTarget(_firstBlockPlaced.transform.position);
                    }
                    if (_secondBlockCreated != null)
                    {
                        _secondBlockCreated.CanDrag = false;     // второй — БЛОКИРУЕМ (цель)
                    }
                }
            },
            // 6) Перетащить получившийся блок на поле и запустить волну
            new TutorialStep
            {
                instructionText = "Now drag the merged block to the build area",
                highlightTarget = null,
                actionType = TutorialActionType.Drag,
                OnStepStart = () =>
                {
                    var mergedBlock = _blockManager.LastCreatedBlock;
                    if (mergedBlock != null)
                    {
                        mergedBlock.CanDrag = true;
                        MoveHandToTarget(mergedBlock.transform.position);

                        // Проверка на существование следующего шага
                        if (_currentStep + 1 < steps.Count)
                        {
                            steps[_currentStep + 1].highlightTarget = mergedBlock.gameObject;
                        }
                    }
                },
                OnStepComplete = () =>
                {           
                    // после того как блок поставили на поле
                    var mergedBlock = _blockManager.LastCreatedBlock;
                    if (mergedBlock != null)
                    {
                        mergedBlock.CanDrag = false; // блокируем перемещение
                    }

                    // запускаем волну врагов
                    _waveManager.Unfreeze();
                }
            },
            // 7) Финальный шаг — ждём пока пушка убьёт всех врагов
            new TutorialStep
            {
                instructionText = "Now just wait and let your turret destroy all enemies!",
                highlightTarget = null,
                actionType = TutorialActionType.None
            }
        };
    }

    private void ShowStep(int index)
    {
        var step = steps[index];

        instructionText.text = step.instructionText;
        step.OnStepStart?.Invoke();

        if (step.highlightTarget != null)
        {
            MoveHandToTarget(step.highlightTarget.transform.position);
        }
        else
        {
            // если нет явной цели — можно скрыть руку
            handPointer.SetActive(false);
        }

        SubscribeToAction(step);
    }

    private void CompleteCurrentStep()
    {
        UnsubscribeCurrent();

        var step = steps[_currentStep];
        step.OnStepComplete?.Invoke();

        // Если следующий шаг — Drag, подставим свежесозданный блок (когда это нужно)
        if (_currentStep + 1 < steps.Count && steps[_currentStep + 1].actionType == TutorialActionType.Drag)
        {
            var newBlock = _blockManager.LastCreatedBlock?.gameObject;
            if (newBlock != null)
            {
                steps[_currentStep + 1].highlightTarget = newBlock;
            }
            else
            {
                Debug.LogError("Tutorial: no block found for Drag step!");
            }
        }

        _currentStep++;
        if (_currentStep >= steps.Count)
        {
            EndTutorial();
            return;
        }

        ShowStep(_currentStep);
    }

    private void EndTutorial()
    {
        _tutorialProgressService.CompleteTutorial();
        HideTutorial();
    }

    public void HideTutorial()
    {
        handPointer.SetActive(false);
        instructionText.gameObject.SetActive(false);
        tutorialPanel.SetActive(false);
        enabled = false;
    }

    #endregion

    #region Click / Drag / Merge handlers

    private void SubscribeToAction(TutorialStep step)
    {
        UnsubscribeCurrent();

        if (step.actionType == TutorialActionType.Click && step.highlightTarget != null)
        {
            var button = step.highlightTarget.GetComponent<Button>();
            if (button != null)
            {
                _currentButtonListener = button;
                button.onClick.AddListener(CompleteCurrentStep);
            }
        }
    }

    private void UnsubscribeCurrent()
    {
        if (_currentButtonListener != null)
        {
            _currentButtonListener.onClick.RemoveListener(CompleteCurrentStep);
            _currentButtonListener = null;
        }
    }

    private void HandleDragFinished(DragResult result)
    {
        if (steps == null || _currentStep >= steps.Count)
            return;

        // Шаг Drag — ждём установки на поле
        if (steps[_currentStep].actionType == TutorialActionType.Drag)
        {
            if (result.Action == DragActionType.PlaceOnBoard)
                CompleteCurrentStep();
            return;
        }

        // Шаг Merge — ждём именно Merge и именно пары (первый -> второй)
        if (steps[_currentStep].actionType == TutorialActionType.Merge)
        {
            if (result.Action != DragActionType.Merge)
                return;


            // проверяем, что слились ожидаемые блоки (порядок не важен)
            bool ok =
                (_firstBlockPlaced != null && _secondBlockCreated != null) &&
                (
                    (result.Block1 == _firstBlockPlaced && result.Block2 == _secondBlockCreated) ||
                    (result.Block2 == _firstBlockPlaced && result.Block1 == _secondBlockCreated)
                );

            if (ok)
                CompleteCurrentStep();
            else
            {
                Debug.LogError("Tutorial: merged blocks are not the expected pair");
            }
        }
    }

    private void OnWaveCompletedOnce(int waveIndex)
    {
        // завершаем шаг, где ожидаем волну (если этот шаг активен)
        if (steps[_currentStep].actionType == TutorialActionType.None /* или WaitForWave */)
        {
            CompleteCurrentStep();
        }
    }

    #endregion

    #region Hand

    private void MoveHandToTarget(Vector3 position)
    {
        handPointer.SetActive(true);
        handPointer.transform.position = position;
    }
    #endregion
}
