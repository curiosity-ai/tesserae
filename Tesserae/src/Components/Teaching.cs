using System;
using System.Collections.Generic;
using System.Text;
using static Tesserae.UI;
using static H5.Core.dom;
using System.Threading.Tasks;

namespace Tesserae
{
    /// <summary>
    /// A component for creating onboarding or instructional walkthroughs.
    /// </summary>
    [H5.Name("tss.Teaching")]
    public class Teaching
    {
        /// <summary>
        /// Represents the type of step in a teaching walkthrough.
        /// </summary>
        public enum StepType
        {
            /// <summary>The step remains until the 'Next' button is clicked.</summary>
            NextButton,
            /// <summary>The step remains for 5 seconds.</summary>
            After5seconds,
            /// <summary>The step remains for 10 seconds.</summary>
            After10seconds
        }
        private Func<bool> _condition;
        private int        _stepCounter = 0;
        private int        _currentStep = 0;
        private int        _firstDelay  = 500;
        private int        _stepDelay   = 150;

        private Dictionary<int, Action> _futureSteps = new Dictionary<int, Action>();
        private Action                  _completed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Teaching"/> class.
        /// </summary>
        public Teaching()
        {
        }

        /// <summary>
        /// Runs the teaching walkthrough if the condition is met.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>The current instance.</returns>
        public Teaching RunIf(Func<bool> condition)
        {
            _condition = condition;

            if (_futureSteps.TryGetValue(0, out var start))
            {
                start();
            }
            return this;
        }

        /// <summary>
        /// Attaches a handler to the completed event.
        /// </summary>
        /// <param name="completed">The handler.</param>
        /// <returns>The current instance.</returns>
        public Teaching OnComplete(Action completed)
        {
            _completed = completed;
            return this;
        }

        /// <summary>
        /// Sets the initial delay before the walkthrough starts.
        /// </summary>
        /// <param name="milliseconds">The delay in milliseconds.</param>
        /// <returns>The current instance.</returns>
        public Teaching FirstDelay(int milliseconds)
        {
            _firstDelay = milliseconds;
            return this;
        }
        /// <summary>
        /// Sets the delay between steps.
        /// </summary>
        /// <param name="milliseconds">The delay in milliseconds.</param>
        /// <returns>The current instance.</returns>
        public Teaching StepDelay(int milliseconds)
        {
            _stepDelay = milliseconds;
            return this;
        }

        /// <summary>
        /// Runs the teaching walkthrough immediately.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Teaching RunNow()
        {
            _condition = () => true;

            if (_futureSteps.TryGetValue(0, out var start))
            {
                start();
            }
            return this;
        }

        /// <summary>
        /// Adds a step to the teaching walkthrough.
        /// </summary>
        /// <param name="showFor">The component to anchor the tooltip to.</param>
        /// <param name="tooltip">The tooltip content.</param>
        /// <param name="animation">The tooltip animation.</param>
        /// <param name="placement">The tooltip placement.</param>
        /// <param name="stepType">The step type.</param>
        /// <returns>The current instance.</returns>
        public Teaching AddStep(IComponent showFor, IComponent tooltip, TooltipAnimation animation = TooltipAnimation.ShiftToward, TooltipPlacement placement = TooltipPlacement.Top, StepType stepType = StepType.NextButton)
        {
            var thisStep = _stepCounter;
            _stepCounter++;

            Button            btnNext = null;
            ProgressIndicator pi      = null;

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
                if (_condition() && showFor.IsMounted())
                {
                    if (stepType == StepType.NextButton)
                    {
                        var text = _stepCounter > thisStep + 1 ? "Next" : "Ok";
                        var icon = _stepCounter > thisStep + 1 ? UIcons.AngleRight : UIcons.Check;
                        btnNext = Button(text).SetIcon(icon).AlignEnd().PT(8).Primary();
                        tooltip = VStack().Children(tooltip, btnNext);
                    }
                    else
                    {
                        pi      = ProgressIndicator().WS().H(4);
                        tooltip = VStack().Children(tooltip, pi.PT(8));
                    }

                    hideTooltip = ShowTooltip(showFor, tooltip, animation, placement, hideOnClick: stepType != StepType.NextButton);

                    if (stepType == StepType.NextButton)
                    {
                        btnNext.OnClick(() => MoveNext());
                    }
                    else
                    {
                        int time  = 0;
                        int delay = stepType == StepType.After5seconds ? 5_000 : 10_000;

                        Func<Task> countdown = async () =>
                        {
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
                if (_currentStep == thisStep && _condition is object)
                {
                    window.setTimeout((_) => Show(), thisStep == 0 ? _firstDelay : _stepDelay);
                }
                else
                {
                    _futureSteps[thisStep] = () => window.setTimeout((_) => Show(), thisStep == 0 ? _firstDelay : _stepDelay);
                    ;
                }
            });

            return this;
        }

        private static Action ShowTooltip(IComponent showFor, IComponent tooltip, TooltipAnimation animation, TooltipPlacement placement, bool hideOnClick)
        {
            bool interactive = true;

            var renderedTooltip = UI.DIV(tooltip.Render());
            renderedTooltip.style.display      = "block";
            renderedTooltip.style.overflow     = "hidden";
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
                H5.Script.Write("tippy({0}, { content: {1}, interactive: {2}, placement: {3}, delay: [{4},{5}],  trigger: 'manual', hideOnClick: {6}, appendTo: document.body });", element, renderedTooltip, interactive, placement.ToString(), 0, 0, hideOnClick);
            }
            else
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: {2}, placement: {3},  animation: {4}, delay: [{5},{6}],  trigger: 'manual', hideOnClick: {7}, appendTo: document.body });", element, renderedTooltip, interactive, placement.ToString(), animation.ToString(), 0, 0, hideOnClick);
            }

            H5.Script.Write("{0}._tippy.show();", element); //Shows it imediatelly

            // 2020-10-05 DWR: Sometimes a tooltip will be attached to an element that is removed from the DOM and then the tooltip is left hanging, orphaned. 
            Action hide = () =>
            {
                // 2020-10-05 DWR: I presume that have to check this property before trying to kill it in case it's already been tidied up
                if (element.HasOwnProperty("_tippy"))
                {
                    H5.Script.Write("{0}._tippy.destroy();", element);
                }
            };

            showFor.WhenRemoved(() => hide());

            return hide;
        }
    }
}