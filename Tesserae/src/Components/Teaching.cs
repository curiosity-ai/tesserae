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
        private int _firstDelay = 500;
        private int _stepDelay = 150;

        private Dictionary<int, Action> _futureSteps = new Dictionary<int, Action>();
        private Action _completed;

        public Teaching()
        {
        }

        public Teaching RunIf(Func<bool> condition)
        {
            _condition = condition;
            if(_futureSteps.TryGetValue(0, out var start))
            {
                start();
            }
            return this;
        }

        public Teaching OnComplete(Action completed)
        {
            _completed = completed;
            return this;
        }

        public Teaching FirstDelay(int milliseconds)
        {
            _firstDelay = milliseconds;
            return this;
        }
        public Teaching StepDelay(int milliseconds)
        {
            _stepDelay= milliseconds; 
            return this;
        }

        public Teaching RunNow()
        {
            _condition = () => true;
            if (_futureSteps.TryGetValue(0, out var start))
            {
                start();
            }
            return this;
        }

        public Teaching AddStep(IComponent showFor, IComponent tooltip, TooltipAnimation animation = TooltipAnimation.ShiftToward, TooltipPlacement placement = TooltipPlacement.Top, StepType stepType = StepType.NextButton)
        {
            var thisStep = _stepCounter;
            _stepCounter++;

            Button btnNext = null;
            ProgressIndicator pi = null;

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
                    if (stepType == StepType.NextButton)
                    {
                        var text = _stepCounter > thisStep + 1 ? "Next" : "Ok";
                        var icon = _stepCounter > thisStep + 1 ? LineAwesome.ChevronRight : LineAwesome.Check;
                        btnNext = Button(text).SetIcon(icon).AlignEnd().PT(8).Primary();
                        tooltip = VStack().Children(tooltip, btnNext);
                    }
                    else
                    {
                        pi = ProgressIndicator().WS().H(4);
                        tooltip = VStack().Children(tooltip, pi.PT(8));
                    }

                    hideTooltip = ShowTooltip(showFor, tooltip, animation, placement);

                    if (stepType == StepType.NextButton)
                    {
                        btnNext.OnClick(() => MoveNext());
                    }
                    else
                    {
                        int time = 0;
                        int delay = stepType == StepType.After5seconds ? 5_000 : 10_000;
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
                if(_currentStep == thisStep && _condition is object)
                {
                    window.setTimeout((_) => Show(), thisStep == 0 ? _firstDelay : _stepDelay);
                }
                else
                {
                    _futureSteps[thisStep] = () => window.setTimeout((_) => Show(), thisStep == 0 ? _firstDelay : _stepDelay); ;
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

            //RFO: This has a key difference against .Tooltip() in that it hard-cods appendTo: document.body so it's not stuck in an element that will cut it off, see https://atomiks.github.io/tippyjs/v6/faq/

            if (animation == TooltipAnimation.None)
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: {2}, placement: {3}, delay: [{4},{5}],  trigger: 'manual', hideOnClick: false, appendTo: document.body });", element, renderedTooltip, interactive, placement.ToString(), 0, 0);
            }
            else
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: {2}, placement: {3},  animation: {4}, delay: [{5},{6}],  trigger: 'manual', hideOnClick: false, appendTo: document.body });", element, renderedTooltip, interactive, placement.ToString(), animation.ToString(), 0, 0);
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
