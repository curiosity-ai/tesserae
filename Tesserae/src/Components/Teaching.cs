using System;
using System.Collections.Generic;
using System.Text;
using Tesserae.HTML;
using static Tesserae.UI;
using static H5.Core.dom;
using System.Threading.Tasks;

namespace Tesserae
{
    public class Teaching
    {
        public enum StepType
        {
            NextButton,
            After5seconds,
            After10seconds
        }
        private Func<bool> _condition;
        private int _stepCounter = 0;
        private int _currentStep = 0;

        private Dictionary<int, Action> _futureSteps = new Dictionary<int, Action>();

        public Teaching()
        {
        }

        public Teaching RunIf(Func<bool> condition)
        {
            _condition = condition;
            return this;
        }

        public Teaching RunNow()
        {
            _condition = () => true;
            return this;
        }

        public Teaching AddStep(IComponent showFor, IComponent tooltip, TooltipAnimation animation = TooltipAnimation.ShiftToward, TooltipPlacement placement = TooltipPlacement.Top, StepType stepType = StepType.NextButton)
        {
            var thisStep = _stepCounter;
            _stepCounter++;

            Button btnNext = null;
            ProgressIndicator pi = null;

            if(stepType == StepType.NextButton)
            {
                btnNext = Button("Next").SetIcon(LineAwesome.ChevronRight).AlignEnd().PT(8);
                tooltip = VStack().Children(tooltip, btnNext);
            }
            else
            {
                pi = ProgressIndicator().WS().H(4);
                tooltip = VStack().Children(tooltip, pi.PT(8));
            }


            Action hideTooltip = null;

            void MoveNext()
            {
                hideTooltip?.Invoke();
                _currentStep++;

                if (_futureSteps.TryGetValue(_currentStep, out var action))
                {
                    action();
                }
            }

            void Show()
            {
                if (_condition())
                {
                    hideTooltip = ShowTooltip(showFor, tooltip, animation, placement);

                    if (stepType == StepType.NextButton)
                    {
                        btnNext.OnClick(() => MoveNext());
                    }
                    else
                    {
                        int delay = stepType == StepType.After5seconds ? 5_000 : 10_000;
                        int time = 0;

                        Func<Task> countdown = async () => {
                            while (time < delay)
                            {
                                await Task.Delay(150);
                                time += 150;
                                pi.Progress(time, delay);
                            }

                            MoveNext();
                        };

                        countdown().FireAndForget();
                    }
                }
            }

            showFor.WhenMounted(() =>
            {
                if(_currentStep == thisStep)
                {
                    Show();
                }
                else
                {
                    _futureSteps[thisStep] = () => Show();
                }
            });

            return this;
        }

        private static Action ShowTooltip(IComponent showFor, IComponent tooltip, TooltipAnimation animation, TooltipPlacement placement)
        {
            bool interactive = true;
            var renderedTooltip = UI.DIV(tooltip.Render());
            renderedTooltip.style.display = "block";
            renderedTooltip.style.overflow = "hidden";
            renderedTooltip.style.textOverflow = "ellipsis";
            document.body.appendChild(renderedTooltip);

            var (element, _) = Stack.GetCorrectItemToApplyStyle(showFor);

            if (element.HasOwnProperty("_tippy"))
            {
                H5.Script.Write("{0}._tippy.destroy();", element);
            }

            if (animation == TooltipAnimation.None)
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: {2}, placement: {3}, delay: [{4},{5}] });", element, renderedTooltip, interactive, placement.ToString(), 0, 0);
            }
            else
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: {2}, placement: {3},  animation: {4}, delay: [{5},{6}] });", element, renderedTooltip, interactive, placement.ToString(), animation.ToString(), 0, 0);
            }

            H5.Script.Write("{0}._tippy.show();", element); //Shows it imediatelly

            // 2020-10-05 DWR: Sometimes a tooltip will be attached to an element that is removed from the DOM and then the tooltip is left hanging, orphaned. 
            return () =>
            {
                // 2020-10-05 DWR: I presume that have to check this property before trying to kill it in case it's already been tidied up
                if (element.HasOwnProperty("_tippy"))
                {
                    H5.Script.Write("{0}._tippy.destroy();", element);
                }
            };
        }
    }
}
