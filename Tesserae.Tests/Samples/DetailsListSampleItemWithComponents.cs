using System;
using System.Collections.Generic;
using static Tesserae.UI;
using static Retyped.dom;
using Tesserae.Components;

namespace Tesserae.Tests.Samples
{
    public class DetailsListSampleItemWithComponents : IDetailsListItem<DetailsListSampleItemWithComponents>
    {
        public LineAwesome Icon               { get; private set; }

        public CheckBox CheckBox              { get; private set; }

        public string Name                    { get; private set; }

        public Button Button                  { get; private set; }

        public ChoiceGroup ChoiceGroup        { get; private set; }

        public Dropdown Dropdown              { get; private set; }

        public Toggle Toggle                  { get; private set; }

        public bool EnableOnListItemClickEvent => false;

        public void OnListItemClick(int listItemIndex)
        {

        }

        public int CompareTo(DetailsListSampleItemWithComponents other, string columnSortingKey)
        {
            return 0;
        }

        public DetailsListSampleItemWithComponents WithIcon(LineAwesome icon)
        {
            Icon = icon;

            return this;
        }

        public DetailsListSampleItemWithComponents WithCheckBox(CheckBox checkBox)
        {
            CheckBox = checkBox;

            return this;
        }

        public DetailsListSampleItemWithComponents WithName(string name)
        {
            Name = name;

            return this;
        }

        public DetailsListSampleItemWithComponents WithButton(Button button)
        {
            Button = button;

            return this;
        }

        public DetailsListSampleItemWithComponents WithChoiceGroup(ChoiceGroup choiceGroup)
        {
            ChoiceGroup = choiceGroup;

            return this;
        }

        public DetailsListSampleItemWithComponents WithDropdown(Dropdown dropdown)
        {
            Dropdown = dropdown;

            return this;
        }

        public DetailsListSampleItemWithComponents WithToggle(Toggle toggle)
        {
            Toggle = toggle;

            return this;
        }

        public IEnumerable<HTMLElement> Render(IList<IDetailsListColumn> columns, Func<IDetailsListColumn, Func<HTMLElement>, HTMLElement> createGridCellExpression)
        {
            yield return createGridCellExpression(columns[0], () => I(Icon));
            yield return createGridCellExpression(columns[1], () => CheckBox.Render());
            yield return createGridCellExpression(columns[2], () => Span(_(text: Name)));
            yield return createGridCellExpression(columns[3], () => Button.Render());
            yield return createGridCellExpression(columns[4], () => ChoiceGroup.Render());
            yield return createGridCellExpression(columns[5], () => Dropdown.Render());
            yield return createGridCellExpression(columns[6], () => Toggle.Render());
        }
    }
}
